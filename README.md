# Inventory Bag System

---

<a name="русский"></a>

# 🇷🇺 Русский

<div align="center">

**[ [🇬🇧 Switch to English](#english) ]**

</div>

> [!WARNING]
> ## ⚠️ Совместимость версий Unity
>
> Этот проект использует **Unity 6.4**, и Zenjex также обновлён под эту версию.
> Открытие в любой версии Unity ниже **6.4** может привести к нестабильной работе Zenjex.
>
> Если вы открываете проект в другой версии Unity - рассмотрите замену внутренней версии Zenjex на актуальную:
> **https://github.com/antonprv/Zenjex/releases**

## Содержание

1. [Обзор проекта](#обзор-проекта)
2. [Стек технологий](#стек-технологий)
3. [Архитектурный обзор](#архитектурный-обзор)
4. [Игровой автомат состояний (GSM)](#игровой-автомат-состояний-gsm)
5. [Внедрение зависимостей: Zenjex + Reflex](#внедрение-зависимостей-zenjex--reflex)
6. [Статические данные](#статические-данные)
7. [Модель инвентаря](#модель-инвентаря)
8. [Архитектура UI: MVP + MVVM](#архитектура-ui-mvp--mvvm)
9. [Система Drag-and-Drop](#система-drag-and-drop)
10. [Загрузка ассетов (Addressables)](#загрузка-ассетов-addressables)
11. [Реактивность: R3](#реактивность-r3)
12. [Асинхронность: UniTask](#асинхронность-unitask)
13. [Инструменты редактора](#инструменты-редактора)
14. [Тестирование](#тестирование)
15. [Соглашения по именованию](#соглашения-по-именованию)
16. [Структура папок](#структура-папок)

---

## Обзор проекта

Система инвентаря в жанре Merge-puzzle для мобильных устройств. Игрок перетаскивает предметы по сетке нестандартной формы, объединяет одинаковые предметы в предметы более высокого уровня, управляет слотами на панели быстрого доступа. Каждый уровень имеет собственную конфигурацию сетки и стартовый набор предметов.

**Ключевые игровые механики:**
- Сетка произвольной формы (не прямоугольная) - задаётся через `BagConfig`
- Многоячеечные предметы (L-образные, T-образные и т.д.) с полноразмерными иконками
- Merge: два одинаковых предмета → предмет следующего уровня
- Слоты на панели быстрого доступа с анимацией возврата
- Несколько уровней с разными конфигурациями сетки и стартовыми наборами
- Главное меню → Уровень → Главное меню (полный lifecycle без утечек)

---

## Стек технологий

| Технология | Роль |
|---|---|
| **Unity 6** | Движок |
| **C# 9** | Язык |
| **Reflex** | IoC-контейнер (низкоуровневый) |
| **Zenjex** | Надстройка над Reflex (Zenject-совместимый API) |
| **UniTask** | Структурная асинхронность без аллокаций |
| **R3** | Реактивные расширения (замена UniRx) |
| **Addressables** | Управление ассетами и сценами |
| **LeanTween** | Анимации UI |
| **NSubstitute** | Мокирование в тестах |
| **NUnit** | Тест-фреймворк |

---

## Архитектурный обзор

Проект использует слоистую архитектуру, где каждый слой знает только о слоях ниже:

```
┌─────────────────────────────────────────────────────┐
│                      View                           │  ← MonoBehaviour, визуализация
├─────────────────────────────────────────────────────┤
│                    ViewModel                        │  ← Реактивное состояние UI
├─────────────────────────────────────────────────────┤
│                    Presenter                        │  ← Медиация View ↔ Model
├─────────────────────────────────────────────────────┤
│               Model / Domain Services               │  ← Бизнес-логика, pure C#
├─────────────────────────────────────────────────────┤
│          Infrastructure / Static Data               │  ← Addressables, DI, FSM
└─────────────────────────────────────────────────────┘
```

**Ключевые принципы:**
- **ScriptableObject никогда не инжектируются через DI** - только загружаются через `IAssetLoader` → `IStaticDataService`
- **Views никогда не знают о Model** - только о своём ViewModel
- **Presenter не хранит состояние** - только медиирует события между Model и View
- **Модель - чистый C#** без зависимости от Unity (кроме `Vector2Int`)

---

## Реализация щаблона "Состояние" - Game State Machine

`GameStateMachine` управляет жизненным циклом всей игры. Состояния регистрируются как `AsTransient` в DI-контейнере и создаются заново при каждом переходе.

```
Bootstrap → PreloadAssets → MainMenu ←→ LoadLevel → GameLoop
                                ↑                       |
                                └───────────────────────┘
                                    (возврат в меню)
```

### Состояния

**`BootstrapState`**
- Инициализирует Addressables (`InitializeAsync`)
- Устанавливает `targetFrameRate = 60`, `vSyncCount = 0` для стабильного FPS на мобиле
- Загружает сцену `Initial` (пустая Bootstrap-сцена)
- Переходит в `PreloadAssetsState`

**`PreloadAssetsState`**
- Параллельно загружает все манифесты статических данных (`WhenAll`)
- Предзагружает все иконки предметов через `IAssetsPreloader`
- Отображает прогресс на экране загрузки: `0→30%` - данные, `30→100%` - иконки
- Переходит в `MainMenuState`

**`MainMenuState`**
- Загружает сцену `MainMenu`
- Ищет `MainMenuView` в сцене через `FindFirstObjectByType`
- Подписывается на кнопки уровней и передаёт payload `levelName` в `LoadLevelState`

**`LoadLevelState`** - самый сложный, 6 шагов с отслеживанием прогресса:
1. `0→10%` - `UIFactory.WarmUp()` (кэш префабов)
2. `10→30%` - `LevelData.LoadForLevelAsync(levelName)` (BagConfig + ItemPreset)
3. `30→35%` - `InitializeModelServices()` (создание сетки)
4. `35→80%` - `ISceneLoader.LoadAsync()` (загрузка сцены, polling `PercentComplete`)
5. `80→95%` - `UIFactory.CreateUIRoot/Canvas/Hud`
6. `95→100%` - скрытие шторки загрузки

Передаёт `HudView` как payload в `GameLoopState`.

**`GameLoopState`**
- Получает `HudView` как payload - подписывается на кнопку «Назад в меню»
- Вызывает `StartupItemsService.PlaceStartupItems()`
- При возврате в меню вызывает `UIFactory.Cleanup()` перед переходом

### Дизайн: payload через GSM

Состояния, требующие данных, реализуют `IGamePayloadedState<T>`:

```csharp
// LoadLevelState принимает имя уровня:
_gsm.Enter<LoadLevelState, string>(SceneAddresses.Level1Address);

// GameLoopState принимает ссылку на HudView:
_gsm.Enter<GameLoopState, HudView>(_hudView);
```

Это позволяет передавать данные между состояниями без глобального состояния.

---

## Внедрение зависимостей: Zenjex + Reflex

**Reflex** - низкоуровневый IoC-контейнер. **Zenjex** - надстройка с Zenject-совместимым API (`AsSingle`, `AsTransient`, `BindInterfacesAndSelf`).

### Паттерны регистрации

```csharp
// Синглтон через интерфейс
builder.Bind<IAssetLoader>().To<AssetLoader>().AsSingle();

// Синглтон под несколькими интерфейсами + конкретным типом
builder.Bind<GridInventoryService>().BindInterfacesAndSelf().AsSingle();

// Транзиент - новый экземпляр при каждом Resolve
builder.Bind<IBagViewModel>().To<BagViewModel>().AsTransient();
```

### Почему `BagViewModel` - `AsTransient`

`BagViewModel` в конструкторе кэширует `GridSize`, `ActiveCells`, `BottomSlotCount` из `IBagConfigSubservice`. Так как конфигурация сетки меняется от уровня к уровню, ViewModel должен пересоздаваться при каждом входе на уровень. `UIFactory` вызывает `Container.Resolve<IBagViewModel>()` каждый раз - получает свежий экземпляр с данными текущего уровня. При выходе с уровня `UIFactory.Cleanup()` явно диспозит старый ViewModel.

### Паттерн Construct() вместо [Zenjex]

Views не используют `[Zenjex]`-инжекцию напрямую - вместо этого `UIFactory` вызывает `view.Construct(viewModel)`. Это гарантирует, что View инициализируется только после того, как Model-сервисы уже готовы:

```csharp
// UIFactory.CreateGameplayUIAsync():
var bagViewModel = _container.Resolve<IBagViewModel>(); // ← уже с данными уровня
bagView.Construct(bagViewModel, _assetLoader);           // ← только теперь
```

---

## Статические данные

### Архитектура двух уровней

```
IStaticDataService
├── IItemDataSubservice       - глобальный каталог предметов (загружается один раз)
├── ILevelStaticDataService   - манифесты уровней (загружаются один раз, резолвятся по уровню)
└── IBagConfigSubservice      - live-proxy к CurrentBagConfig текущего уровня
```

### Глобальные данные (загружаются в `PreloadAssetsState`)

**`ItemManifest`** - словарь `ItemId → AssetReference<ItemConfig>`. `ItemDataSubservice` загружает все конфиги параллельно и кэширует их. Иконки предзагружаются по Addressables-лейблу `Preload_UI`.

### Данные уровня (загружаются в `LoadLevelState`)

**`LevelBagManifest`** - словарь `levelName → AssetReference<BagConfig>`. Задаёт форму сетки, размер ячеек, количество слотов для каждого уровня.

**`LevelItemPresetManifest`** - словарь `levelName → AssetReference<LevelItemPreset>`. `LevelItemPreset` содержит список `Entry { AssetReference<ItemConfig> Item; int Count }`. После загрузки пресета `LevelStaticDataService` **сразу загружает все `ItemConfig` из него параллельно** - это необходимо потому что `AssetReference.Asset` не заполняется автоматически, его нужно явно загрузить через Addressables.

### `LevelBagConfigSubservice` - live-proxy

`IBagConfigSubservice` реализован как прокси, который **не кэширует данные** - каждое обращение к свойству делегируется в `ILevelStaticDataService.CurrentBagConfig`:

```csharp
public Vector2Int GridSize => _levelData.CurrentBagConfig?.GridSize ?? Vector2Int.zero;
```

Это гарантирует, что смена уровня автоматически отражается на всех потребителях без какого-либо «refresh-шага».

### Редакторы манифестов

Все манифесты имеют кастомные Editor-инспекторы на базе `ManifestEditorBase<TManifest, TData, TKey>`:
- Кнопка **AutoFill from Assets** - автоматически сканирует проект и добавляет все ассеты нужного типа
- **SceneDropdownKeyDrawer** - использует `InspectorUtils.GetAllScenes()` для автодополнения по именам сцен
- **ManualSaveEditor** - изменения применяются только после явного нажатия «Save», что предотвращает случайные правки

---

## Модель инвентаря

### `GridInventory` - чистая логика

`GridInventory` - pure C# класс без Unity-зависимостей. Хранит три структуры:

```csharp
HashSet<Vector2Int>                 _activeCells;   // какие ячейки существуют
Dictionary<Vector2Int, InventoryItem> _occupiedCells; // что стоит в ячейке
List<InventoryItem>                 _items;         // все предметы
```

Поддерживает произвольную форму сетки - активные ячейки задаются через `BagConfig.Shape` (список Vector2Int). Ячейки вне Shape просто не входят в `_activeCells`.

### `InventoryItem`

Пара `(ItemConfig config, Vector2Int origin)`. `ItemConfig.Shape` определяет какие ячейки занимает предмет относительно origin. Метод `GetBoundsSize()` возвращает размер bounding box в ячейках - используется для вычисления размера иконки.

### Merge-логика

```
CanMerge: dragged.Config == target.Config && dragged.Config.CanMerge
         && CanPlace(mergeResult, target.Origin, target)

Merge: TryRemove(a) + TryRemove(b) → new InventoryItem(MergeResult, b.Origin) → TryPlace
```

**Важный нюанс:** при начале перетаскивания предмет уже удалён из сетки (`TryRemove` в `StartDragFromBag`). Поэтому `GridInventoryService.Merge` после `_grid.Merge()` обязан вручную стрелять `_onItemRemoved` для обоих предметов и `_onItemPlaced` для результата - иначе `BagView` не узнает что нужно обновить иконки.

---

## Архитектура UI: MVP + MVVM

Два паттерна применяются в разных ролях:

**MVP** (Model → Presenter → View):
- `BagPresenter` медиирует между `GridInventoryService` и View-слоем
- Добавляет UI-специфичную логику (highlight-запросы по группам ячеек)
- Не хранит состояние - только проксирует события и команды

**MVVM** (ViewModel ↔ View):
- `BagViewModel`, `CellViewModel`, `DragIconViewModel` - реактивные состояния для View
- `BagView` и `CellView` подписываются на `Observable<T>` через R3

### Иерархия UI объектов

```
UI_Root (пустой контейнер, создаётся UIFactory.CreateUIRoot)
├── PUI_BagCanvas
│   ├── GridRoot        - ячейки (CellView)
│   ├── IconsRoot       - иконки предметов поверх ячеек
│   ├── BottomSlotsRoot - слоты быстрого доступа
│   └── DragIconRoot    - иконка перетаскиваемого предмета
└── PUI_Hud
    └── ReturnButton    - кнопка «Назад в меню»
```

`UIFactory.Cleanup()` уничтожает `UI_Root` - все дочерние объекты уничтожаются вместе с ним. Одновременно диспозятся `BagViewModel` и `BottomSlotsViewModel`.

### Иконки предметов

Иконки не привязаны к отдельным ячейкам. Каждый предмет получает отдельный `Image` GameObject в `IconsRoot`, размер которого равен bounding box предмета:

```csharp
rt.sizeDelta = new Vector2(
    bounds.x * _step - _spacing,
    bounds.y * _step - _spacing);
```

Это решает проблему клиппинга для многоячеечных предметов.

---

## Система Drag-and-Drop

### Поток событий

```
CellView.OnPointerDown
    → DragDropPresenter.StartDragFromBag(cellCoord, screenPos)
        → BagPresenter.TryRemove(item)        ← предмет удаляется из модели
        → DragDropService.StartDrag(item, ...)
        → DragIconViewModel.Show(sprite, pos, bounds)
            → DragIconView.Construct отображает иконку

CellView.OnPointerEnter (во время drag)
    → DragDropPresenter.HandlePointerEnterCell(coord)
        → BagPresenter.CanMerge / CanPlace → highlight

CellView.OnDrop
    → DragDropPresenter.HandleDropOnCell(cellCoord)
        → CanMerge?  → BagPresenter.Merge(dragged, target)
        → CanPlace?  → BagPresenter.TryPlace(dragged)
        → иначе     → DragDropService.CancelDrag() → возврат

```

### Анимация возврата в слот

`DragIconViewModel.FlyTo(targetScreenPosition)` → `DragIconView` анимирует иконку через LeanTween к координатам слота. Позиции слотов читаются через `SlotPositionProviderLocator.Instance` - статический локатор, который регистрирует `BottomSlotsView` при создании. Этот паттерн позволяет `DragDropPresenter` получать позиции без прямой зависимости от View-объекта.

### Анимация Merge

После мёрджа `GridInventoryService.Merge` стреляет события в строгом порядке:
1. `_onItemRemoved(a)` - уничтожить иконку перетаскиваемого предмета
2. `_onItemRemoved(b)` - уничтожить иконку цели
3. `_onItemPlaced(merged)` - создать иконку результата (`SpawnIconAsync`)
4. `_onItemsMerged(result)` → `OnMergeAnimation` → `BagView.PlayMergeAnimationAsync`

`PlayMergeAnimationAsync` ждёт 1 кадр (`UniTask.DelayFrame(1)`) чтобы `SpawnIconAsync` успел создать GameObject, затем запускает LeanTween-анимацию: instant scale 1.4 → `setEaseOutElastic` до 1.0 + alpha flash + cell punch.

---

## Загрузка ассетов (Addressables)

### `AssetLoader` - кэширующая обёртка

Два словаря:
- `_completedHandles` - `key → handle` для cache-hit (возврат Result без Addressables)
- `_calledHandles` - `key → [handles]` для корректного `Release` в `Cleanup()`

```csharp
// LoadAsync по AssetReference использует GUID как ключ кэша:
string key = reference.AssetGUID;
if (TryGetCached<T>(key, out var cached)) return cached;
return await RunWithCache(key, Addressables.LoadAssetAsync<T>(reference));
```

Это гарантирует что повторные вызовы с одинаковым ассетом мгновенно возвращают кэшированный результат без нагрузки на Addressables.

### Загрузка сцен

`AddressableSceneLoader` использует `LoadSceneMode.Single` - Unity сама выгружает текущую сцену при загрузке новой. Явный `Addressables.Release` на `SceneInstance` хендлах **не вызывается** - это вызывало ошибку `Unloading the last loaded scene is not supported`.

Прогресс загрузки сцены передаётся через `IProgress<float>` с polling `handle.PercentComplete` на каждом `UniTask.Yield`.

---

## Реактивность: R3

R3 (следующее поколение UniRx) используется для реактивных связей между слоями:

```csharp
// В GridInventoryService - события модели:
private readonly Subject<InventoryItem> _onItemPlaced = new();
public Observable<InventoryItem> OnItemPlaced => _onItemPlaced;

// В BagView - подписка:
_bagViewModel.OnItemPlaced
    .Subscribe(item => SpawnIconAsync(item).Forget())
    .AddTo(_disposables);
```

Все подписки добавляются в `CompositeDisposable`, который диспозится в `OnDestroy` или `UIFactory.Cleanup()`. Это предотвращает утечки памяти при смене уровней.

---

## Асинхронность: UniTask

`UniTask` используется вместо `async Task` для:
- Отсутствия аллокаций (работает через `PlayerLoop`)
- Поддержки `CancellationToken` во всех состояниях GSM
- `UniTaskVoid` для fire-and-forget методов в MonoBehaviour

Паттерн отмены в состояниях:
```csharp
public void Exit()
{
    _cts?.Cancel();
    _cts?.Dispose();
    _cts = null;
}
```

Каждый `await` перед продолжением проверяет `ct.IsCancellationRequested` - это предотвращает выполнение операций после смены состояния.

---

## Инструменты редактора

### `ManifestEditorBase<TManifest, TData, TKey>`

Универсальная базовая система для кастомных инспекторов манифестов. Предоставляет:
- **AutoFill** - сканирует все ассеты типа `TData`, добавляет новые, обновляет изменившиеся пути
- Поддержку `DictionaryData<K,V>` через `IForceSerialization`
- Расширяемые key-drawer'ы через `ICustomKeyDrawer`

### `SceneDropdownKeyDrawer`

Рисует ключи словаря как выпадающий список сцен, используя `InspectorUtils.GetAllScenes()`. Кнопка «Refresh Scene List» обновляет список без перезапуска редактора.

### `BagConfigEditor` / `ItemConfigEditor`

Интерактивная сетка в Inspector: клик по ячейке добавляет/убирает её из Shape предмета или BagConfig. Использует `ManualSaveEditor` - изменения не применяются до явного нажатия «Save».

### `SceneSwitcherOverlay`

Toolbar overlay с выпадающим списком сцен для быстрого переключения без Build Settings.

---

## Тестирование

### EditMode тесты

**`GridInventoryTests`** - 20+ тестов чистой логики `GridInventory`:
- Размещение, удаление, merge
- Произвольные формы сетки
- Edge cases: выход за границы, занятые ячейки, merge на несовместимых предметах

**`ViewModel тесты`** (`BagViewModelTests`, `CellViewModelTests`, `DragIconViewModelTests`, `BottomSlotsViewModelTests`):
- Все зависимости мокируются через NSubstitute
- Тестируются реактивные события через `Subject<T>`

**`PresenterTests`** - тесты `DragDropPresenter` с полным мок-окружением.

### PlayMode тесты

**`MvpMvvmIntegrationTests`** - интеграционные тесты всего стека Model → Presenter → ViewModel:
- `IBagConfigSubservice` мокируется - тесты не зависят от ScriptableObject ассетов
- Проверяется корректность реактивных цепочек при drag, drop, merge, cancel

---

## Соглашения по именованию

Задокументированы в `Code/Documentation/NamingConvention.cs` с кастомным Editor (`NamingConventionEditor`):

| Паттерн | Пример |
|---|---|
| Интерфейсы | `IGridInventoryService` |
| Private поля | `_gridInventory` |
| Константы | `BagCanvasAddress` |
| Addressable адреса префабов | `PUI_BagCanvas` |
| Addressable адреса статических данных | `BagConfig`, `ItemManifest` |
| Addressable адреса сцен | `Level1`, `MainMenu` |
| ScriptableObject ассеты | `BagConfig`, `ItemConfig_Sword_Lv1` |

---

## Структура папок

```
Assets/Code/
├── Common/              - утилиты, кастомные типы, расширения
│   ├── CustomTypes/     - DictionaryData, HashSetData, Vector-типы
│   ├── Extensions/      - UniTask, Array, Functional расширения
│   ├── FastMath/        - Burst-оптимизированная математика
│   └── Logging/         - IGameLog, GameLogger
│
├── Data/
│   └── StaticData/
│       ├── Configs/     - BagConfig, ItemConfig
│       ├── Manifests/   - ItemManifest, LevelBagManifest, LevelItemPresetManifest
│       └── LevelItemPreset.cs
│
├── Editor/              - кастомные инспекторы, инструменты редактора
│   ├── Common/Manifests - ManifestEditorBase, ManualSaveEditor, Drawers
│   ├── Config/          - BagConfigEditor, ItemConfigEditor
│   └── Tools/           - SceneSwitcher, QuickLook
│
├── Infrastructure/
│   ├── AssetManagement/ - AssetLoader, IAssetLoader, адреса
│   ├── AssetsPreloader/ - прогресс-tracked предзагрузка иконок
│   ├── Installer/       - GameInstaller (DI-корень), InstallerFactory
│   ├── Loading/         - LoadingCurtain, ILoadScreen
│   ├── Services/
│   │   ├── SceneLoader/ - AddressableSceneLoader, SceneAddresses
│   │   └── StaticData/  - IStaticDataService, сабсервисы
│   └── StateMachine/    - GSM, States, Factory
│
├── Model/
│   ├── Core/            - GridInventory, InventoryItem (pure C#)
│   └── Services/        - GridInventoryService, BottomSlotsService, DragDropService
│
├── Presenter/           - BagPresenter, BottomSlotsPresenter, DragDropPresenter
│
├── Tests/
│   ├── EditMode/        - unit-тесты GridInventory, ViewModels, Presenters
│   └── PlayMode/        - интеграционные MVP+MVVM тесты
│
├── UI/
│   ├── Factory/         - IUIFactory, UIFactory
│   ├── Hud/             - HudView
│   └── MainMenu/        - MainMenuView, IMainMenuView
│
├── View/                - BagView, CellView, BottomSlotsView, DragIconView
│
└── ViewModel/           - BagViewModel, CellViewModel, BottomSlotsViewModel, DragIconViewModel
```

---

<a name="english"></a>

# 🇬🇧 English

<div align="center">

**[ [🇷🇺 Switch to Russian](#русский) ]**

</div>

> [!WARNING]
> ## ⚠️ Unity Version Compatibility
>
> This project uses **Unity 6.4**, and Zenjex is also updated to this version.
> Opening it in any Unity version **lower than 6.4** may cause Zenjex to become unstable.
>
> If you open this project in another Unity version, consider replacing the internal Zenjex version with this one:
> **https://github.com/antonprv/Zenjex/releases**

## Table of Contents

1. [Project Overview](#project-overview)
2. [Technology Stack](#technology-stack)
3. [Architecture Overview](#architecture-overview)
4. [Game State Machine (GSM)](#game-state-machine-gsm)
5. [Dependency Injection: Zenjex + Reflex](#dependency-injection-zenjex--reflex)
6. [Static Data](#static-data)
7. [Inventory Model](#inventory-model)
8. [UI Architecture: MVP + MVVM](#ui-architecture-mvp--mvvm)
9. [Drag-and-Drop System](#drag-and-drop-system)
10. [Asset Loading (Addressables)](#asset-loading-addressables)
11. [Reactivity: R3](#reactivity-r3)
12. [Async: UniTask](#async-unitask)
13. [Editor Tools](#editor-tools)
14. [Testing](#testing)
15. [Naming Conventions](#naming-conventions)
16. [Folder Structure](#folder-structure)

---

## Project Overview

A merge-puzzle inventory system for mobile devices. The player drags items across a custom-shaped grid, merges identical items into higher-level ones, and manages a quick-access slot panel. Each level has its own grid configuration and starting item set.

**Key gameplay mechanics:**
- Arbitrary-shaped grid (non-rectangular) - defined via `BagConfig`
- Multi-cell items (L-shaped, T-shaped, etc.) with full bounding-box icons
- Merge: two identical items → next-level item
- Quick-access slots with fly-back animations
- Multiple levels with different grid configurations and starting loadouts
- Main menu → Level → Main menu full lifecycle without memory leaks

---

## Technology Stack

| Technology | Role |
|---|---|
| **Unity 6** | Engine |
| **C# 9** | Language |
| **Reflex** | IoC container (low-level) |
| **Zenjex** | Reflex wrapper (Zenject-compatible API) |
| **UniTask** | Structural async without allocations |
| **R3** | Reactive Extensions (UniRx successor) |
| **Addressables** | Asset and scene management |
| **LeanTween** | UI animations |
| **NSubstitute** | Test mocking |
| **NUnit** | Test framework |

---

## Architecture Overview

The project uses a layered architecture where each layer only knows about layers below it:

```
┌─────────────────────────────────────────────────────┐
│                      View                           │  ← MonoBehaviour, rendering
├─────────────────────────────────────────────────────┤
│                    ViewModel                        │  ← Reactive UI state
├─────────────────────────────────────────────────────┤
│                    Presenter                        │  ← View ↔ Model mediation
├─────────────────────────────────────────────────────┤
│               Model / Domain Services               │  ← Business logic, pure C#
├─────────────────────────────────────────────────────┤
│          Infrastructure / Static Data               │  ← Addressables, DI, FSM
└─────────────────────────────────────────────────────┘
```

**Key principles:**
- **ScriptableObjects are never DI-injected** - only loaded via `IAssetLoader` → `IStaticDataService`
- **Views never know about the Model** - only about their ViewModel
- **Presenters hold no state** - they only mediate events between Model and View
- **The Model is pure C#** with no Unity dependency (except `Vector2Int`)

---

## Game State Machine (GSM)

`GameStateMachine` manages the entire game lifecycle. States are registered as `AsTransient` in the DI container and are created fresh on every transition.

```
Bootstrap → PreloadAssets → MainMenu ←→ LoadLevel → GameLoop
                                ↑                       |
                                └───────────────────────┘
                                      (return to menu)
```

### States

**`BootstrapState`**
- Initializes Addressables (`InitializeAsync`)
- Sets `targetFrameRate = 60`, `vSyncCount = 0` for stable mobile FPS
- Loads the `Initial` scene (empty bootstrap scene)
- Transitions to `PreloadAssetsState`

**`PreloadAssetsState`**
- Loads all static data manifests in parallel (`WhenAll`)
- Preloads all item icons via `IAssetsPreloader`
- Displays progress on the loading screen: `0→30%` - data, `30→100%` - icons
- Transitions to `MainMenuState`

**`MainMenuState`**
- Loads the `MainMenu` scene
- Finds `MainMenuView` in the scene via `FindFirstObjectByType`
- Subscribes to level buttons and passes the `levelName` payload to `LoadLevelState`

**`LoadLevelState`** - the most complex, 6 steps with progress tracking:
1. `0→10%` - `UIFactory.WarmUp()` (prefab cache)
2. `10→30%` - `LevelData.LoadForLevelAsync(levelName)` (BagConfig + ItemPreset)
3. `30→35%` - `InitializeModelServices()` (grid allocation)
4. `35→80%` - `ISceneLoader.LoadAsync()` (scene load, polling `PercentComplete`)
5. `80→95%` - `UIFactory.CreateUIRoot/Canvas/Hud`
6. `95→100%` - hiding the loading curtain

Passes `HudView` as payload to `GameLoopState`.

**`GameLoopState`**
- Receives `HudView` as payload - subscribes to the «Return to Menu» button
- Calls `StartupItemsService.PlaceStartupItems()`
- On returning to menu calls `UIFactory.Cleanup()` before transitioning

### Design: payload via GSM

States that require data implement `IGamePayloadedState<T>`:

```csharp
// LoadLevelState receives a level name:
_gsm.Enter<LoadLevelState, string>(SceneAddresses.Level1Address);

// GameLoopState receives a HudView reference:
_gsm.Enter<GameLoopState, HudView>(_hudView);
```

This allows data to be passed between states without global state.

---

## Dependency Injection: Zenjex + Reflex

**Reflex** is the low-level IoC container. **Zenjex** is the wrapper with a Zenject-compatible API (`AsSingle`, `AsTransient`, `BindInterfacesAndSelf`).

### Registration patterns

```csharp
// Singleton via interface
builder.Bind<IAssetLoader>().To<AssetLoader>().AsSingle();

// Singleton under multiple interfaces + concrete type
builder.Bind<GridInventoryService>().BindInterfacesAndSelf().AsSingle();

// Transient - new instance on every Resolve
builder.Bind<IBagViewModel>().To<BagViewModel>().AsTransient();
```

### Why `BagViewModel` is `AsTransient`

`BagViewModel` caches `GridSize`, `ActiveCells`, and `BottomSlotCount` from `IBagConfigSubservice` in its constructor. Since the grid configuration changes between levels, the ViewModel must be recreated on every level entry. `UIFactory` calls `Container.Resolve<IBagViewModel>()` each time - it gets a fresh instance with the current level's data. On level exit, `UIFactory.Cleanup()` explicitly disposes the old ViewModel.

### Construct() pattern instead of [Zenjex]

Views don't use `[Zenjex]` injection directly - instead `UIFactory` calls `view.Construct(viewModel)`. This guarantees the View is only initialized after the Model services are ready:

```csharp
// UIFactory.CreateGameplayUIAsync():
var bagViewModel = _container.Resolve<IBagViewModel>(); // ← already has level data
bagView.Construct(bagViewModel, _assetLoader);           // ← only now
```

---

## Static Data

### Two-tier architecture

```
IStaticDataService
├── IItemDataSubservice       - global item catalogue (loaded once)
├── ILevelStaticDataService   - level manifests (loaded once, resolved per level)
└── IBagConfigSubservice      - live-proxy to CurrentBagConfig of the current level
```

### Global data (loaded in `PreloadAssetsState`)

**`ItemManifest`** - a `ItemId → AssetReference<ItemConfig>` dictionary. `ItemDataSubservice` loads all configs in parallel and caches them. Icons are preloaded via the Addressables label `Preload_UI`.

### Level data (loaded in `LoadLevelState`)

**`LevelBagManifest`** - a `levelName → AssetReference<BagConfig>` dictionary. Defines the grid shape, cell size, and slot count for each level.

**`LevelItemPresetManifest`** - a `levelName → AssetReference<LevelItemPreset>` dictionary. `LevelItemPreset` contains a list of `Entry { AssetReference<ItemConfig> Item; int Count }`. After loading the preset, `LevelStaticDataService` **immediately loads all `ItemConfig` entries in parallel** - this is required because `AssetReference.Asset` is not populated automatically; it must be explicitly loaded through Addressables.

### `LevelBagConfigSubservice` - live proxy

`IBagConfigSubservice` is implemented as a proxy that **does not cache data** - every property access delegates to `ILevelStaticDataService.CurrentBagConfig`:

```csharp
public Vector2Int GridSize => _levelData.CurrentBagConfig?.GridSize ?? Vector2Int.zero;
```

This guarantees that a level change is automatically reflected to all consumers without any "refresh step".

### Manifest editors

All manifests have custom Editor inspectors based on `ManifestEditorBase<TManifest, TData, TKey>`:
- **AutoFill button** - scans the project for all assets of type `TData`, adds new ones, updates changed paths
- **SceneDropdownKeyDrawer** - uses `InspectorUtils.GetAllScenes()` for scene-name autocomplete
- **ManualSaveEditor** - changes are only applied after explicitly clicking "Save", preventing accidental edits

---

## Inventory Model

### `GridInventory` - pure logic

`GridInventory` is a pure C# class with no Unity dependencies. It maintains three data structures:

```csharp
HashSet<Vector2Int>                   _activeCells;   // which cells exist
Dictionary<Vector2Int, InventoryItem> _occupiedCells; // what's in each cell
List<InventoryItem>                   _items;         // all placed items
```

Supports arbitrary grid shapes - active cells are defined via `BagConfig.Shape` (a list of Vector2Int offsets). Cells outside the Shape are simply not in `_activeCells`.

### `InventoryItem`

A pair of `(ItemConfig config, Vector2Int origin)`. `ItemConfig.Shape` defines which cells the item occupies relative to its origin. `GetBoundsSize()` returns the bounding box size in cells - used to compute icon size.

### Merge logic

```
CanMerge: dragged.Config == target.Config && dragged.Config.CanMerge
         && CanPlace(mergeResult, target.Origin, target)

Merge: TryRemove(a) + TryRemove(b) → new InventoryItem(MergeResult, b.Origin) → TryPlace
```

**Important nuance:** when a drag begins, the item is already removed from the grid (`TryRemove` in `StartDragFromBag`). Therefore `GridInventoryService.Merge` after `_grid.Merge()` must manually fire `_onItemRemoved` for both items and `_onItemPlaced` for the result - otherwise `BagView` won't know to update the icons.

---

## UI Architecture: MVP + MVVM

Two patterns are used in different roles:

**MVP** (Model → Presenter → View):
- `BagPresenter` mediates between `GridInventoryService` and the View layer
- Adds UI-specific logic (highlight requests across groups of cells)
- Holds no state - only proxies events and commands

**MVVM** (ViewModel ↔ View):
- `BagViewModel`, `CellViewModel`, `DragIconViewModel` - reactive state for Views
- `BagView` and `CellView` subscribe to `Observable<T>` via R3

### UI object hierarchy

```
UI_Root (empty container, created by UIFactory.CreateUIRoot)
├── PUI_BagCanvas
│   ├── GridRoot        - cells (CellView)
│   ├── IconsRoot       - item icons rendered above cells
│   ├── BottomSlotsRoot - quick-access slots
│   └── DragIconRoot    - dragged item icon
└── PUI_Hud
    └── ReturnButton    - «Return to Menu» button
```

`UIFactory.Cleanup()` destroys `UI_Root` - all children are destroyed with it. `BagViewModel` and `BottomSlotsViewModel` are disposed at the same time.

### Item icons

Icons are not tied to individual cells. Each item gets a dedicated `Image` GameObject in `IconsRoot`, sized to the item's bounding box:

```csharp
rt.sizeDelta = new Vector2(
    bounds.x * _step - _spacing,
    bounds.y * _step - _spacing);
```

This solves the clipping problem for multi-cell items.

---

## Drag-and-Drop System

### Event flow

```
CellView.OnPointerDown
    → DragDropPresenter.StartDragFromBag(cellCoord, screenPos)
        → BagPresenter.TryRemove(item)        ← item removed from model
        → DragDropService.StartDrag(item, ...)
        → DragIconViewModel.Show(sprite, pos, bounds)
            → DragIconView displays the icon

CellView.OnPointerEnter (during drag)
    → DragDropPresenter.HandlePointerEnterCell(coord)
        → BagPresenter.CanMerge / CanPlace → highlight

CellView.OnDrop
    → DragDropPresenter.HandleDropOnCell(cellCoord)
        → CanMerge?  → BagPresenter.Merge(dragged, target)
        → CanPlace?  → BagPresenter.TryPlace(dragged)
        → otherwise → DragDropService.CancelDrag() → return
```

### Return-to-slot animation

`DragIconViewModel.FlyTo(targetScreenPosition)` → `DragIconView` animates the icon via LeanTween to the slot's screen coordinates. Slot positions are read through `SlotPositionProviderLocator.Instance` - a static locator that registers `BottomSlotsView` at creation time. This pattern lets `DragDropPresenter` access positions without a direct dependency on a View object.

### Merge animation

After a merge, `GridInventoryService.Merge` fires events in strict order:
1. `_onItemRemoved(a)` - destroy the dragged item's icon
2. `_onItemRemoved(b)` - destroy the target item's icon
3. `_onItemPlaced(merged)` - create the result icon (`SpawnIconAsync`)
4. `_onItemsMerged(result)` → `OnMergeAnimation` → `BagView.PlayMergeAnimationAsync`

`PlayMergeAnimationAsync` waits 1 frame (`UniTask.DelayFrame(1)`) for `SpawnIconAsync` to create the GameObject, then launches a LeanTween animation: instant scale to 1.4 → `setEaseOutElastic` to 1.0 + alpha flash + cell punch.

---

## Asset Loading (Addressables)

### `AssetLoader` - caching wrapper

Two dictionaries:
- `_completedHandles` - `key → handle` for cache-hits (returns Result without Addressables roundtrip)
- `_calledHandles` - `key → [handles]` for correct `Release` in `Cleanup()`

```csharp
// LoadAsync by AssetReference uses GUID as cache key:
string key = reference.AssetGUID;
if (TryGetCached<T>(key, out var cached)) return cached;
return await RunWithCache(key, Addressables.LoadAssetAsync<T>(reference));
```

This guarantees that repeated calls with the same asset instantly return the cached result with no Addressables overhead.

### Scene loading

`AddressableSceneLoader` uses `LoadSceneMode.Single` - Unity unloads the current scene automatically when loading a new one. Explicit `Addressables.Release` on `SceneInstance` handles is **never called** - doing so caused the `Unloading the last loaded scene is not supported` error.

Scene load progress is passed via `IProgress<float>` by polling `handle.PercentComplete` on each `UniTask.Yield`.

---

## Reactivity: R3

R3 (the next generation of UniRx) is used for reactive bindings between layers:

```csharp
// In GridInventoryService - model events:
private readonly Subject<InventoryItem> _onItemPlaced = new();
public Observable<InventoryItem> OnItemPlaced => _onItemPlaced;

// In BagView - subscription:
_bagViewModel.OnItemPlaced
    .Subscribe(item => SpawnIconAsync(item).Forget())
    .AddTo(_disposables);
```

All subscriptions are added to a `CompositeDisposable` that is disposed in `OnDestroy` or `UIFactory.Cleanup()`. This prevents memory leaks on level transitions.

---

## Async: UniTask

`UniTask` is used instead of `async Task` for:
- Zero allocations (works through `PlayerLoop`)
- `CancellationToken` support in all GSM states
- `UniTaskVoid` for fire-and-forget methods in MonoBehaviours

Cancellation pattern in states:
```csharp
public void Exit()
{
    _cts?.Cancel();
    _cts?.Dispose();
    _cts = null;
}
```

Every `await` checks `ct.IsCancellationRequested` before continuing - this prevents operations from running after a state transition.

---

## Editor Tools

### `ManifestEditorBase<TManifest, TData, TKey>`

A universal base system for custom manifest inspectors. Provides:
- **AutoFill** - scans all assets of type `TData`, adds new ones, updates changed paths
- Support for `DictionaryData<K,V>` via `IForceSerialization`
- Extensible key drawers via `ICustomKeyDrawer`

### `SceneDropdownKeyDrawer`

Renders dictionary keys as a scene-name dropdown using `InspectorUtils.GetAllScenes()`. The «Refresh Scene List» button updates the list without restarting the Editor.

### `BagConfigEditor` / `ItemConfigEditor`

An interactive grid in the Inspector: clicking a cell adds/removes it from the item's Shape or BagConfig. Uses `ManualSaveEditor` - changes are not applied until «Save» is clicked explicitly.

### `SceneSwitcherOverlay`

A toolbar overlay with a scene dropdown for quick switching without going through Build Settings.

---

## Testing

### EditMode tests

**`GridInventoryTests`** - 20+ tests of pure `GridInventory` logic:
- Placement, removal, merge
- Arbitrary grid shapes
- Edge cases: out-of-bounds, occupied cells, merge on incompatible items

**ViewModel tests** (`BagViewModelTests`, `CellViewModelTests`, `DragIconViewModelTests`, `BottomSlotsViewModelTests`):
- All dependencies mocked via NSubstitute
- Reactive events tested through `Subject<T>`

**`PresenterTests`** - `DragDropPresenter` tests with a full mock environment.

### PlayMode tests

**`MvpMvvmIntegrationTests`** - integration tests of the full Model → Presenter → ViewModel stack:
- `IBagConfigSubservice` is mocked - tests don't depend on ScriptableObject assets
- Verifies correctness of reactive chains during drag, drop, merge, cancel

---

## Naming Conventions

Documented in `Code/Documentation/NamingConvention.cs` with a custom Editor (`NamingConventionEditor`):

| Pattern | Example |
|---|---|
| Interfaces | `IGridInventoryService` |
| Private fields | `_gridInventory` |
| Constants | `BagCanvasAddress` |
| Addressable prefab addresses | `PUI_BagCanvas` |
| Addressable static data addresses | `BagConfig`, `ItemManifest` |
| Addressable scene addresses | `Level1`, `MainMenu` |
| ScriptableObject assets | `BagConfig`, `ItemConfig_Sword_Lv1` |

---

## Folder Structure

```
Assets/Code/
├── Common/              - utilities, custom types, extensions
│   ├── CustomTypes/     - DictionaryData, HashSetData, Vector types
│   ├── Extensions/      - UniTask, Array, Functional extensions
│   ├── FastMath/        - Burst-optimized math
│   └── Logging/         - IGameLog, GameLogger
│
├── Data/
│   └── StaticData/
│       ├── Configs/     - BagConfig, ItemConfig
│       ├── Manifests/   - ItemManifest, LevelBagManifest, LevelItemPresetManifest
│       └── LevelItemPreset.cs
│
├── Editor/              - custom inspectors, editor tools
│   ├── Common/Manifests - ManifestEditorBase, ManualSaveEditor, Drawers
│   ├── Config/          - BagConfigEditor, ItemConfigEditor
│   └── Tools/           - SceneSwitcher, QuickLook
│
├── Infrastructure/
│   ├── AssetManagement/ - AssetLoader, IAssetLoader, addresses
│   ├── AssetsPreloader/ - progress-tracked icon preloading
│   ├── Installer/       - GameInstaller (DI root), InstallerFactory
│   ├── Loading/         - LoadingCurtain, ILoadScreen
│   ├── Services/
│   │   ├── SceneLoader/ - AddressableSceneLoader, SceneAddresses
│   │   └── StaticData/  - IStaticDataService, subservices
│   └── StateMachine/    - GSM, States, Factory
│
├── Model/
│   ├── Core/            - GridInventory, InventoryItem (pure C#)
│   └── Services/        - GridInventoryService, BottomSlotsService, DragDropService
│
├── Presenter/           - BagPresenter, BottomSlotsPresenter, DragDropPresenter
│
├── Tests/
│   ├── EditMode/        - unit tests for GridInventory, ViewModels, Presenters
│   └── PlayMode/        - MVP+MVVM integration tests
│
├── UI/
│   ├── Factory/         - IUIFactory, UIFactory
│   ├── Hud/             - HudView
│   └── MainMenu/        - MainMenuView, IMainMenuView
│
├── View/                - BagView, CellView, BottomSlotsView, DragIconView
│
└── ViewModel/           - BagViewModel, CellViewModel, BottomSlotsViewModel, DragIconViewModel
```
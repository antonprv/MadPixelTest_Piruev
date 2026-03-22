namespace BagFight.Infrastructure.AssetManagement
{
  /// <summary>
  /// Строковые адреса Addressable-ассетов.
  /// Значения должны совпадать с полем "Address" в Addressables Groups окне Unity.
  /// </summary>
  public static class BagAssetAddresses
  {
    // ── UI ────────────────────────────────────────────────────────────────────
    public const string LoadingCurtain = "UI/LoadingCurtain";
    public const string BagCanvas      = "UI/BagCanvas";

    // ── Items (label для пакетной загрузки) ───────────────────────────────────
    // Все иконки предметов помечаются лейблом "ItemIcon" в Addressables Groups.
    // AssetsPreloader загружает их одним вызовом LoadAssetsAsync по этому лейблу.
    public const string ItemIconLabel = "ItemIcon";
  }
}

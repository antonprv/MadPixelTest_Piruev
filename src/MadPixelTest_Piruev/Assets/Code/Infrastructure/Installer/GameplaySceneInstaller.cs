// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Reflex.Core;

using Zenjex.Extensions.Core;
using Zenjex.Extensions.SceneContext;

namespace Code.Infrastructure.Installer
{
  /// <summary>
  /// Scene-scoped DI installer for the gameplay scene.
  ///
  /// BagConfig and ItemManifest are no longer bound here — they are loaded
  /// from Addressables by StaticDataService (BagConfigSubservice / ItemDataSubservice)
  /// before gameplay starts, and exposed via IBagConfigSubservice / IItemDataSubservice.
  ///
  /// This installer remains as the correct extension point for any future
  /// scene-specific bindings (e.g. per-level enemy spawner configs, camera rigs, etc.)
  /// that are serialized in the scene rather than loaded from Addressables.
  /// </summary>
  public class GameplaySceneInstaller : SceneInstaller
  {
    public override void InstallBindings(ContainerBuilder builder)
    {
      // Scene-specific bindings go here.
    }
  }
}

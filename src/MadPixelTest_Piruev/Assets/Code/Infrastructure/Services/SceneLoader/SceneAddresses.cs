// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

namespace Code.Infrastructure.SceneLoader
{
  /// <summary>
  /// Addressable keys for all scenes in the project.
  /// Must match the "Address" field in Addressables Groups window.
  ///
  /// To add a new scene:
  ///   1. Add it to Build Settings
  ///   2. Mark as Addressable in the Inspector
  ///   3. Add a constant here
  /// </summary>
  public static class SceneAddresses
  {
    /// <summary>
    /// Initial empty scene for bootstrap
    /// </summary>
    public const string InitialAddress = "Initial";

    public const string Level1Address = "Level1";
    public const string Level2Address = "Level2";
    public const string MainMenuAddress = "MainMenu";
  }
}

// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.
using UnityEngine;
[UnityEngine.CreateAssetMenu(fileName = "NamingConvention", menuName = "Documentation/Naming Convention")]
public class NamingConvention : UnityEngine.ScriptableObject
{
  [Header("Naming Convention")]
  [TextArea(25, 35)]
  public string conventionText =
      "<b><size=16>Prefab Naming</size></b>\n\n" +
      "This naming convention serves as a practical middle ground between Unity and Unreal Engine conventions. " +
      "The prefixes act as quick visual hints to help team members and developers instantly understand what type of " +
      "prefab they're working with, without needing to open it first.\n\n" +
      "<b><size=14>Prefix Guide</size></b>\n\n" +
      "<b>P_</b> - Standard Prefab\n" +
      "A basic prefab containing any general game object or component setup.\n\n" +
      "<b>P_V_</b> - Prefab Variant\n" +
      "A variant of an existing prefab with modified properties or components.\n\n" +
      "<b>PA_</b> - Prefab Actor\n" +
      "A prefab representing an interactive game entity or character (borrowed from Unreal's \"Actor\" concept).\n\n" +
      "<b>PAV_</b> - Prefab Actor Variant\n" +
      "A variant of an Actor prefab with customized settings.\n\n" +
      "<b>PP_</b> - Prefab Pawn\n" +
      "A prefab for controllable characters or entities (using Unreal's \"Pawn\" terminology for player-controllable objects).\n\n" +
      "<b>PAI_</b> - Prefab AI-Controlled Pawn\n" +
      "A prefab specifically for AI-controlled characters, making it clear this entity uses autonomous behavior.\n\n" +
      "<b>PUI_</b> - Prefab UI\n" +
      "A prefab containing user interface elements.\n\n" +
      "<b><size=16>UI Element Suffixes</size></b>\n\n" +
      "UI elements within prefabs use descriptive suffixes to identify their function and type at a glance. " +
      "These suffixes are appended to the end of GameObject names within UI prefabs.\n\n" +
      "<b>_CNT</b> - Container\n" +
      "An empty container object used for layout and organization, similar to a <div> in HTML. " +
      "Typically holds other UI elements and helps structure the interface hierarchy.\n" +
      "Example: MainMenu_CNT, InventoryPanel_CNT\n\n" +
      "<b>_BTN</b> - Button\n" +
      "A button object that handles user input and interactions. Usually contains a Button component " +
      "along with visual elements and text.\n" +
      "Example: StartGame_BTN, CloseWindow_BTN\n\n" +
      "<b>_BG</b> - Background\n" +
      "A background object that provides visual backing for UI panels, windows, or other elements. " +
      "Often contains an Image component for rendering sprites or solid colors.\n" +
      "Example: Panel_BG, Window_BG\n\n" +
      "<b>_TXT</b> - Text\n" +
      "A text display object, usually a TextMeshPro component. Used for labels, titles, descriptions, " +
      "and any other text content in the UI.\n" +
      "Example: PlayerName_TXT, ScoreLabel_TXT\n\n" +
      "<b>_IMG</b> - Image\n" +
      "A pure visual element containing only an image component without interactive logic. " +
      "Used for icons, decorative elements, sprites, and other graphical content that doesn't require user interaction.\n" +
      "Example: PlayerAvatar_IMG, ItemIcon_IMG, Decoration_IMG\n\n" +
      "<i>Note: These prefixes and suffixes are organizational tools to improve workflow efficiency. They give you an at-a-glance " +
      "understanding of a prefab's purpose and contents, saving time during development and collaboration. " +
      "When combined, they create clear, self-documenting names like 'PUI_InventoryWindow' containing objects such as " +
      "'Header_CNT', 'CloseButton_BTN', 'Background_BG', and 'Title_TXT'.</i>";
}

using System;
using UnityEngine;
using UnityEngine.Events;

using lasthourdev_UI;  // Your UI namespace



/// <summary>
/// Example component script attached to the MainMenu panel.
/// </summary>

public class MainMenuPanel : UIPanel
{
    // Add your main menu specific logic here.
}






/// <summary>
/// This script demonstrates how to use the UIManager and UIPanel system in a simple and clear way.
/// Attach this script to any GameObject in your scene to try out the UIManager features.
/// </summary>
public class UIManagerTutorialExample : MonoBehaviour
{
    private void Start()
    {
        // --- 1. Show a panel by its type (no ID) ---
        // This will show the MainMenu panel.
        UIPanel mainMenuPanel = UIManager.Instance.ShowPanel(UIType.MainMenu);
        // The panel is now active and visible.

        // --- 2. Show a panel by type with a specific ID ---
        // This allows multiple instances of the same panel type.
        UIPanel settingsPanelUser1 = UIManager.Instance.ShowPanel(UIType.Settings, "User1");
        UIPanel settingsPanelUser2 = UIManager.Instance.ShowPanel(UIType.Settings, "User2");

        // --- 3. Subscribe to receive data sent to a specific panel ---
        // Here we subscribe to data events for the Settings panel with ID "User1".
        UIManager.Instance.SubscribeToPanelData(UIType.Settings, "User1", OnSettingsDataReceived);

        // --- 4. Send data to a specific panel ---
        // We send some settings data to the Settings panel "User1".
        SettingsData newSettings = new SettingsData { Volume = 0.75f, Brightness = 0.6f };
        UIManager.Instance.SendDataToPanel("Settings_User1", newSettings);

        // --- 5. Check if a panel is active ---
        // Check if MainMenu panel is currently active (no ID needed).
        bool isMainMenuActive = UIManager.Instance.IsPanelActive(UIType.MainMenu);

        // Check if Settings panel with ID "User2" is active.
        bool isSettingsUser2Active = UIManager.Instance.IsPanelActive(UIType.Settings, "User2");

        // --- 6. Get a component from a panel ---
        // Get the MainMenuPanel component from the MainMenu panel.
        MainMenuPanel mainMenuComponent = UIManager.Instance.GetPanelComponent<MainMenuPanel>(UIType.MainMenu);

        // Get the SettingsPanelController component from Settings panel "User1".
        SettingsPanelController settingsController = UIManager.Instance.GetPanelComponent<SettingsPanelController>(UIType.Settings, "User1");

        // --- 7. Hide panels ---
        // Hide the MainMenu panel.
        UIManager.Instance.HidePanel(UIType.MainMenu);

        // Hide the Settings panel with ID "User2".
        UIManager.Instance.HidePanel(UIType.Settings, "User2");

        // Hide all Settings panels (all IDs).
        UIManager.Instance.HidePanel(UIType.Settings);

        // --- 8. Destroy panels ---
        // Destroy a specific panel by getting it first.
        UIPanel panelToDestroy = UIManager.Instance.GetPanelComponent<UIPanel>(UIType.Settings, "User1");
        if (panelToDestroy != null)
        {
            UIManager.Instance.DestroyPanel(panelToDestroy);
        }

        // Destroy all Inventory panels.
        UIManager.Instance.DestroyPanels(UIType.Inventory);

        // --- 9. Hide all active panels globally ---
        UIManager.Instance.HideAllPanels();

        // --- 10. Unsubscribe from panel data events ---
        UIManager.Instance.UnsubscribeFromPanelData(UIType.Settings, "User1", OnSettingsDataReceived);
    }

    /// <summary>
    /// This method is called when data is sent to the Settings panel with ID "User1".
    /// </summary>
    /// <param name="data">The data object sent to the panel.</param>
    private void OnSettingsDataReceived(object data)
    {
        if (data is SettingsData settings)
        {
            // Here you can update the UI or internal state with the received settings.
            ApplySettings(settings);
        }
    }

    /// <summary>
    /// Example method to apply settings data.
    /// </summary>
    /// <param name="settings">Settings data to apply.</param>
    private void ApplySettings(SettingsData settings)
    {
        // Implement your logic to apply volume, brightness, etc.
        // For example, update audio volume or screen brightness.
    }
}

/// <summary>
/// Example data class to send to panels.
/// </summary>
[Serializable]
public class SettingsData
{
    public float Volume;
    public float Brightness;
}

/// <summary>
/// Example component script attached to the Settings panel prefab.
/// </summary>
public class SettingsPanelController : MonoBehaviour
{
    /// <summary>
    /// Update the UI elements based on the settings data.
    /// </summary>
    public void UpdateSettingsUI(SettingsData data)
    {
        // Update sliders, toggles, or other UI elements here.
    }
}


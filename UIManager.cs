using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace lasthourdev_UI
{
    /// <summary>
    /// Types of UI panels in the application
    /// </summary>
    public enum UIType
    {
        None,
        MainMenu,
        Settings,
        Inventory,
        Shop,
        Pause,
        GameOver,
        Victory,
        Dialogue,
        Notification,
        Loading
    }

    public abstract class UIPanel : MonoBehaviour
    {
        [SerializeField] private UIType panelType;
        [SerializeField] private bool destroyOnHide = false;
        [SerializeField] private string panelId = ""; // Added panel ID for distinguishing panels of same type

        // Events for panel lifecycle
        public UnityEvent OnShowBegin = new UnityEvent();
        public UnityEvent OnShowComplete = new UnityEvent();
        public UnityEvent OnHideBegin = new UnityEvent();
        public UnityEvent OnHideComplete = new UnityEvent();

        public UIType PanelType => panelType;
        public bool DestroyOnHide => destroyOnHide;

        // Unique identifier for panel - combines type and optional ID
        public string UniqueId => string.IsNullOrEmpty(panelId) ? panelType.ToString() : $"{panelType}_{panelId}";

        protected virtual void Awake()
        {
            // Register with UI Manager if present in scene
            if (UIManager.HasInstance)
            {
                UIManager.Instance.RegisterPanel(this);
            }
        }

        public virtual void Show()
        {
            OnShowBegin.Invoke();
            gameObject.SetActive(true);
            OnShowComplete.Invoke();
        }

        public virtual void Hide()
        {
            OnHideBegin.Invoke();
            gameObject.SetActive(false);
            OnHideComplete.Invoke();

            if (destroyOnHide && UIManager.HasInstance)
            {
                UIManager.Instance.DestroyPanel(this);
            }
        }
    }

    /// <summary>
    /// Data container for UI panel configuration
    /// </summary>
    [Serializable]
    public class UIPanelData
    {
        public UIType Type;
        public GameObject Prefab;

    }

    /// <summary>
    /// Global UI management system that handles showing, hiding, and managing UI panels
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        #region Singleton
        private static UIManager _instance;

        public static bool HasInstance => _instance != null;

        public static UIManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    // Try to find existing UIManager in scene
                    _instance = FindObjectOfType<UIManager>();

                    if (_instance == null)
                    {
                        GameObject managerObject = new GameObject("UIManager");
                        _instance = managerObject.AddComponent<UIManager>();
                        Debug.Log("UIManager created automatically");
                    }
                }
                return _instance;
            }
        }
        #endregion

        [SerializeField] private List<UIPanelData> panelPrefabs = new List<UIPanelData>();
        [SerializeField] private Transform panelContainer;

        // Changed to use uniqueId as key (combination of type and optional ID)
        private Dictionary<string, UIPanel> registeredPanels = new Dictionary<string, UIPanel>();

        // Store panels by type for quicker lookups of all panels of a specific type
        private Dictionary<UIType, List<UIPanel>> panelsByType = new Dictionary<UIType, List<UIPanel>>();

        // For sending data between panels
        public class PanelDataEvent : UnityEvent<object> { }
        private Dictionary<string, PanelDataEvent> panelDataEvents = new Dictionary<string, PanelDataEvent>();

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;

            // If no panel container set, use this transform
            if (panelContainer == null)
            {
                panelContainer = transform;
            }

            // Find and register any panels already in the scene
            FindScenePanels();
        }

        private void FindScenePanels()
        {
            UIPanel[] scenePanels = FindObjectsOfType<UIPanel>(true);
            foreach (UIPanel panel in scenePanels)
            {
                RegisterPanel(panel);
                // Initially hide all panels
                if (panel.gameObject.activeSelf)
                {
                    panel.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Register a panel with the UI manager
        /// </summary>
        public void RegisterPanel(UIPanel panel)
        {
            if (panel == null) return;

            string uniqueId = panel.UniqueId;
            UIType panelType = panel.PanelType;

            // Register in the main dictionary
            if (registeredPanels.ContainsKey(uniqueId))
            {
                Debug.LogWarning($"Panel with unique ID {uniqueId} already registered. Replacing previous reference.");
            }

            registeredPanels[uniqueId] = panel;

            // Also register in type-based lookup
            if (!panelsByType.TryGetValue(panelType, out List<UIPanel> typePanels))
            {
                typePanels = new List<UIPanel>();
                panelsByType[panelType] = typePanels;
            }

            if (!typePanels.Contains(panel))
            {
                typePanels.Add(panel);
            }
        }

        /// <summary>
        /// Shows a UI panel of the specified type
        /// </summary>
        public UIPanel ShowPanel(UIType panelType, object data = null)
        {
            return ShowPanel(panelType, "", data);
        }

        /// <summary>
        /// Shows a UI panel of the specified type with an optional ID
        /// </summary>
        public UIPanel ShowPanel(UIType panelType, string panelId, object data = null)
        {
            string uniqueId = string.IsNullOrEmpty(panelId) ? panelType.ToString() : $"{panelType}_{panelId}";
            UIPanel panel = GetOrCreatePanel(panelType, panelId);

            if (panel != null)
            {
                // Send data before showing if available
                if (data != null)
                {
                    SendDataToPanel(uniqueId, data);
                }

                panel.Show();
            }

            return panel;
        }

        /// <summary>
        /// Shows a UI panel and returns a specific component
        /// </summary>
        public T ShowPanel<T>(UIType panelType, object data = null) where T : Component
        {
            UIPanel panel = ShowPanel(panelType, data);
            return panel != null ? panel.GetComponent<T>() : null;
        }

        /// <summary>
        /// Shows a UI panel with ID and returns a specific component
        /// </summary>
        public T ShowPanel<T>(UIType panelType, string panelId, object data = null) where T : Component
        {
            UIPanel panel = ShowPanel(panelType, panelId, data);
            return panel != null ? panel.GetComponent<T>() : null;
        }

        /// <summary>
        /// Hides all panels of the specified type
        /// </summary>
        public void HidePanel(UIType panelType)
        {
            if (panelsByType.TryGetValue(panelType, out List<UIPanel> panels))
            {
                foreach (UIPanel panel in new List<UIPanel>(panels))
                {
                    if (panel != null && panel.gameObject.activeSelf)
                    {
                        panel.Hide();
                    }
                }
            }
        }

        /// <summary>
        /// Hides a specific panel by its unique ID
        /// </summary>
        public void HidePanel(UIType panelType, string panelId)
        {
            string uniqueId = string.IsNullOrEmpty(panelId) ? panelType.ToString() : $"{panelType}_{panelId}";

            if (registeredPanels.TryGetValue(uniqueId, out UIPanel panel) && panel != null)
            {
                panel.Hide();
            }
        }

        /// <summary>
        /// Destroys a panel
        /// </summary>
        public void DestroyPanel(UIPanel panel)
        {
            if (panel == null) return;

            string uniqueId = panel.UniqueId;
            UIType panelType = panel.PanelType;

            // Remove from dictionaries
            if (registeredPanels.ContainsKey(uniqueId))
            {
                registeredPanels.Remove(uniqueId);
            }

            if (panelsByType.TryGetValue(panelType, out List<UIPanel> typePanels))
            {
                typePanels.Remove(panel);
            }

            Destroy(panel.gameObject);
        }

        /// <summary>
        /// Destroys all panels of the specified type
        /// </summary>
        public void DestroyPanels(UIType panelType)
        {
            if (panelsByType.TryGetValue(panelType, out List<UIPanel> panels))
            {
                foreach (UIPanel panel in new List<UIPanel>(panels))
                {
                    if (panel != null)
                    {
                        DestroyPanel(panel);
                    }
                }
            }
        }

        /// <summary>
        /// Hide all active panels
        /// </summary>
        public void HideAllPanels()
        {
            foreach (var panel in new List<UIPanel>(registeredPanels.Values))
            {
                if (panel != null && panel.gameObject.activeSelf)
                {
                    panel.Hide();
                }
            }
        }

        /// <summary>
        /// Checks if any panel of the specified type is currently active
        /// </summary>
        public bool IsPanelActive(UIType panelType)
        {
            if (panelsByType.TryGetValue(panelType, out List<UIPanel> panels))
            {
                foreach (UIPanel panel in panels)
                {
                    if (panel != null && panel.gameObject.activeSelf)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if a specific panel by ID is currently active
        /// </summary>
        public bool IsPanelActive(UIType panelType, string panelId)
        {
            string uniqueId = string.IsNullOrEmpty(panelId) ? panelType.ToString() : $"{panelType}_{panelId}";

            return registeredPanels.TryGetValue(uniqueId, out UIPanel panel) &&
                   panel != null &&
                   panel.gameObject.activeSelf;
        }

        /// <summary>
        /// Get a component from the first active panel of specified type
        /// </summary>
        public T GetPanelComponent<T>(UIType panelType) where T : Component
        {
            if (panelsByType.TryGetValue(panelType, out List<UIPanel> panels))
            {
                foreach (UIPanel panel in panels)
                {
                    if (panel != null)
                    {
                        T component = panel.GetComponent<T>();
                        if (component != null)
                        {
                            return component;
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Get a component from a specific panel by ID
        /// </summary>
        public T GetPanelComponent<T>(UIType panelType, string panelId) where T : Component
        {
            string uniqueId = string.IsNullOrEmpty(panelId) ? panelType.ToString() : $"{panelType}_{panelId}";

            if (registeredPanels.TryGetValue(uniqueId, out UIPanel panel) && panel != null)
            {
                return panel.GetComponent<T>();
            }
            return null;
        }

        /// <summary>
        /// Get all active panels of a specific type
        /// </summary>
        public List<UIPanel> GetActivePanels(UIType panelType)
        {
            List<UIPanel> activePanels = new List<UIPanel>();

            if (panelsByType.TryGetValue(panelType, out List<UIPanel> panels))
            {
                foreach (UIPanel panel in panels)
                {
                    if (panel != null && panel.gameObject.activeSelf)
                    {
                        activePanels.Add(panel);
                    }
                }
            }

            return activePanels;
        }

        /// <summary>
        /// Send data to a specific panel by unique ID
        /// </summary>
        public void SendDataToPanel(string uniqueId, object data)
        {
            if (!panelDataEvents.TryGetValue(uniqueId, out PanelDataEvent dataEvent))
            {
                dataEvent = new PanelDataEvent();
                panelDataEvents[uniqueId] = dataEvent;
            }

            dataEvent.Invoke(data);
        }

        /// <summary>
        /// Send data to all panels of a specific type
        /// </summary>
        public void SendDataToPanelType(UIType panelType, object data)
        {
            if (panelsByType.TryGetValue(panelType, out List<UIPanel> panels))
            {
                foreach (UIPanel panel in panels)
                {
                    if (panel != null)
                    {
                        SendDataToPanel(panel.UniqueId, data);
                    }
                }
            }
        }

        /// <summary>
        /// Subscribe to data events for a panel by unique ID
        /// </summary>
        public void SubscribeToPanelData(string uniqueId, UnityAction<object> callback)
        {
            if (!panelDataEvents.TryGetValue(uniqueId, out PanelDataEvent dataEvent))
            {
                dataEvent = new PanelDataEvent();
                panelDataEvents[uniqueId] = dataEvent;
            }

            dataEvent.AddListener(callback);
        }

        /// <summary>
        /// Subscribe to data events for a panel
        /// </summary>
        public void SubscribeToPanelData(UIType panelType, string panelId, UnityAction<object> callback)
        {
            string uniqueId = string.IsNullOrEmpty(panelId) ? panelType.ToString() : $"{panelType}_{panelId}";
            SubscribeToPanelData(uniqueId, callback);
        }

        /// <summary>
        /// Unsubscribe from data events for a panel by unique ID
        /// </summary>
        public void UnsubscribeFromPanelData(string uniqueId, UnityAction<object> callback)
        {
            if (panelDataEvents.TryGetValue(uniqueId, out PanelDataEvent dataEvent))
            {
                dataEvent.RemoveListener(callback);
            }
        }

        /// <summary>
        /// Unsubscribe from data events for a panel
        /// </summary>
        public void UnsubscribeFromPanelData(UIType panelType, string panelId, UnityAction<object> callback)
        {
            string uniqueId = string.IsNullOrEmpty(panelId) ? panelType.ToString() : $"{panelType}_{panelId}";
            UnsubscribeFromPanelData(uniqueId, callback);
        }

        /// <summary>
        /// Gets or creates a panel of the specified type
        /// </summary>
        private UIPanel GetOrCreatePanel(UIType panelType, string panelId = "")
        {
            string uniqueId = string.IsNullOrEmpty(panelId) ? panelType.ToString() : $"{panelType}_{panelId}";

            // Return existing panel if available
            if (registeredPanels.TryGetValue(uniqueId, out UIPanel panel) && panel != null)
            {
                return panel;
            }

            // Otherwise, create a new one from prefab
            UIPanelData data = panelPrefabs.Find(d => d.Type == panelType);
            if (data == null || data.Prefab == null)
            {
                Debug.LogWarning($"No prefab found for panel type {panelType}");
                return null;
            }

            GameObject panelObject = Instantiate(data.Prefab, panelContainer);
            panel = panelObject.GetComponent<UIPanel>();

            if (panel == null)
            {
                Debug.LogError($"Prefab for {panelType} does not have a UIPanel component!");
                Destroy(panelObject);
                return null;
            }

            // If a custom ID was provided, we need to set it on the panel
            if (!string.IsNullOrEmpty(panelId))
            {
                // Using reflection to set the private panelId field
                var fieldInfo = typeof(UIPanel).GetField("panelId", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (fieldInfo != null)
                {
                    fieldInfo.SetValue(panel, panelId);
                }
                else
                {
                    Debug.LogWarning($"Could not set panelId on {panelType}. Consider making panelId property settable.");
                }
            }

            // Register the panel after setting its ID
            RegisterPanel(panel);
            return panel;
        }
    }
}
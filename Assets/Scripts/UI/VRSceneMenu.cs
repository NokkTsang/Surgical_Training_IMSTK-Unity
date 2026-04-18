using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Floating VR navigation menu that lets the user switch between surgical training scenes
/// using the left controller ray. Toggle with the left Y button or Tab key.
/// Attach to an empty GameObject in each scene.
/// 
/// Setup:
///   1. In Unity Editor menu: Tools > Setup Scene Menu > Add All Device Scenes to Build
///   2. Tools > Setup Scene Menu > Create VRSceneMenu in Current Scene
///   3. Make sure your XR Rig has an XR Ray Interactor on the left controller
///   4. Press Play, then press Y (Quest) or Tab (keyboard) to toggle the menu
/// </summary>
public class VRSceneMenu : MonoBehaviour
{
    [Header("Menu Settings")]
    [Tooltip("Offset from the left controller where the panel appears")]
    public Vector3 menuOffset = new Vector3(0.15f, 0.1f, 0.3f);

    [Tooltip("Scale of the floating menu canvas")]
    public float menuScale = 0.001f;

    [Tooltip("Width of the canvas in pixels")]
    public float canvasWidth = 600f;

    [Tooltip("Height of the canvas in pixels")]
    public float canvasHeight = 820f;

    [Header("Appearance")]
    public Color panelColor = new Color(0.08f, 0.08f, 0.10f, 0.95f);           // Deep charcoal
    public Color buttonColor = new Color(0.14f, 0.16f, 0.22f, 1f);             // Slate blue-grey
    public Color buttonHoverColor = new Color(0.20f, 0.50f, 0.65f, 1f);        // Teal accent
    public Color currentSceneColor = new Color(0.16f, 0.55f, 0.45f, 1f);       // Surgical green
    public Color textColor = new Color(0.92f, 0.93f, 0.96f, 1f);               // Soft white
    public Color accentColor = new Color(0.30f, 0.70f, 0.85f, 1f);             // Cyan accent
    public Color subtitleColor = new Color(0.55f, 0.58f, 0.68f, 1f);           // Muted lavender

    [Header("References (Auto-detected if empty)")]
    public Transform leftController;

    private struct SceneEntry
    {
        public string displayName;
        public string sceneName;
    }

    private GameObject menuRoot;
    private bool isMenuVisible = false;
    private Text loadingText;
    private List<Button> sceneButtons = new List<Button>();
    private string currentlyLoading = "";
    private Font uiFont;

    private readonly SceneEntry[] scenes = new SceneEntry[]
    {
        new SceneEntry { displayName = "Suturing",                       sceneName = "Suturing" },
        new SceneEntry { displayName = "Tissue Contact",                 sceneName = "TissueContact" },
        new SceneEntry { displayName = "PBD Thin Tissue Contact",        sceneName = "PbdThinTissueContact" },
        new SceneEntry { displayName = "Grasping",                       sceneName = "Grasping" },
        new SceneEntry { displayName = "Connective Tissue Burn & Tear",  sceneName = "Connective Tissue Burn And Tear" },
        new SceneEntry { displayName = "Rigid Controller",               sceneName = "RigidController" },
        new SceneEntry { displayName = "Virtual Coupling",               sceneName = "Virtual Coupling" },
        new SceneEntry { displayName = "Rigid Controller (VRPN)",        sceneName = "RigidControllerVrpn" },
    };

    void Start()
    {
        uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (uiFont == null)
            uiFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

        if (leftController == null)
            TryAutoDetectLeftController();

        BuildMenu();

        // Place menu at world origin (0,0,0) initially
        transform.position = Vector3.zero;

        menuRoot.SetActive(false);
    }

    void Update()
    {
        bool toggle = false;

        if (Input.GetKeyDown(KeyCode.Tab))
            toggle = true;
        if (Input.GetKeyDown(KeyCode.JoystickButton4)) // Y button on Quest
            toggle = true;

        if (toggle)
            ToggleMenu();

        if (isMenuVisible && leftController != null)
            UpdateMenuPosition();
    }

    public void ToggleMenu()
    {
        isMenuVisible = !isMenuVisible;
        menuRoot.SetActive(isMenuVisible);

        if (isMenuVisible && leftController != null)
            UpdateMenuPosition();
    }

    private void UpdateMenuPosition()
    {
        menuRoot.transform.position = leftController.position
            + leftController.forward * menuOffset.z
            + leftController.up * menuOffset.y
            + leftController.right * menuOffset.x;

        Camera cam = Camera.main;
        if (cam != null)
        {
            Vector3 lookDir = menuRoot.transform.position - cam.transform.position;
            if (lookDir.sqrMagnitude > 0.001f)
                menuRoot.transform.rotation = Quaternion.LookRotation(lookDir);
        }
    }

    private void TryAutoDetectLeftController()
    {
        // Search by XR node via InputDevices (no XRI assembly dependency)
        var leftHandDevices = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.LeftHand, leftHandDevices);

        foreach (var name in new[] { "LeftHand Controller", "Left Controller", "LeftHand" })
        {
            var obj = GameObject.Find(name);
            if (obj != null)
            {
                leftController = obj.transform;
                return;
            }
        }

        if (Camera.main != null)
            leftController = Camera.main.transform;
    }

    // ?�?�?�?�?�?�?�?�?�?�?�?�?�?�?� UI Construction ?�?�?�?�?�?�?�?�?�?�?�?�?�?�?�

    private void BuildMenu()
    {
        // World Space Canvas
        GameObject canvasObj = new GameObject("VRSceneMenuCanvas");
        canvasObj.transform.SetParent(transform);

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10;

        canvasObj.AddComponent<GraphicRaycaster>();

        // Try to add TrackedDeviceGraphicRaycaster if XRI is available
        var trackedRaycasterType = System.Type.GetType(
            "UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster, Unity.XR.Interaction.Toolkit");
        if (trackedRaycasterType != null)
            canvasObj.AddComponent(trackedRaycasterType);

        RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(canvasWidth, canvasHeight);
        canvasRect.localScale = Vector3.one * menuScale;

        // Root panel
        menuRoot = CreatePanel(canvasObj.transform, "MenuPanel", panelColor);
        RectTransform rootRect = menuRoot.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        VerticalLayoutGroup layout = menuRoot.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(25, 25, 25, 20);
        layout.spacing = 10;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        // Title
        CreateText(menuRoot.transform, "Title", "\u2695  Surgical Training", 38, FontStyle.Bold, accentColor, 55);
        CreateText(menuRoot.transform, "Subtitle", "Select Training Scenario", 22, FontStyle.Normal,
            subtitleColor, 30);

        // Divider — gradient accent line
        CreateDivider(menuRoot.transform, accentColor * 0.6f);

        // Scene Buttons
        string currentScene = SceneManager.GetActiveScene().name;
        foreach (var scene in scenes)
        {
            bool isCurrent = (scene.sceneName == currentScene);
            CreateSceneButton(menuRoot.transform, scene, isCurrent);
        }

        // Loading indicator
        CreateSpacer(menuRoot.transform, 8);
        GameObject loadObj = CreateText(menuRoot.transform, "Loading", "", 20, FontStyle.Italic,
            accentColor, 25);
        loadingText = loadObj.GetComponent<Text>();

        // Close hint
        CreateText(menuRoot.transform, "Hint", "Press  [Y]  or  [Tab]  to close", 16, FontStyle.Normal,
            new Color(0.40f, 0.42f, 0.50f, 0.6f), 25);
    }

    private GameObject CreatePanel(Transform parent, string name, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        Image img = obj.AddComponent<Image>();
        img.color = color;
        return obj;
    }

    private GameObject CreateText(Transform parent, string name, string content,
        int fontSize, FontStyle style, Color color, float height)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        Text text = obj.AddComponent<Text>();
        text.text = content;
        text.font = uiFont;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.color = color;
        text.alignment = TextAnchor.MiddleCenter;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;

        LayoutElement le = obj.AddComponent<LayoutElement>();
        le.preferredHeight = height;

        return obj;
    }

    private void CreateDivider(Transform parent, Color color = default)
    {
        if (color == default) color = new Color(0.3f, 0.4f, 0.7f, 0.5f);

        GameObject div = new GameObject("Divider");
        div.transform.SetParent(parent, false);

        Image img = div.AddComponent<Image>();
        img.color = color;

        LayoutElement le = div.AddComponent<LayoutElement>();
        le.preferredHeight = 1;
    }

    private void CreateSpacer(Transform parent, float height)
    {
        GameObject sp = new GameObject("Spacer");
        sp.transform.SetParent(parent, false);
        LayoutElement le = sp.AddComponent<LayoutElement>();
        le.preferredHeight = height;
    }

    private void CreateSceneButton(Transform parent, SceneEntry scene, bool isCurrent)
    {
        Color bgColor = isCurrent ? currentSceneColor : buttonColor;

        GameObject btnObj = new GameObject("Btn_" + scene.sceneName);
        btnObj.transform.SetParent(parent, false);

        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = bgColor;

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor = bgColor;
        cb.highlightedColor = isCurrent
            ? new Color(0.20f, 0.65f, 0.50f)
            : buttonHoverColor;
        cb.pressedColor = new Color(0.12f, 0.35f, 0.50f);
        cb.disabledColor = new Color(0.13f, 0.42f, 0.35f, 0.85f);
        cb.fadeDuration = 0.08f;
        btn.colors = cb;

        LayoutElement le = btnObj.AddComponent<LayoutElement>();
        le.preferredHeight = 60;

        // Button label
        GameObject textObj = new GameObject("Label");
        textObj.transform.SetParent(btnObj.transform, false);

        Text label = textObj.AddComponent<Text>();
        label.text = isCurrent ? ("\u25B6  " + scene.displayName + "   \u2714") : ("    " + scene.displayName);
        label.font = uiFont;
        label.fontSize = 24;
        label.fontStyle = isCurrent ? FontStyle.Bold : FontStyle.Normal;
        label.color = isCurrent ? new Color(0.85f, 1f, 0.92f) : textColor;
        label.alignment = TextAnchor.MiddleCenter;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 0);
        textRect.offsetMax = new Vector2(-10, 0);

        if (isCurrent)
        {
            btn.interactable = false;
        }
        else
        {
            string sn = scene.sceneName;
            btn.onClick.AddListener(() => LoadScene(sn));
        }

        sceneButtons.Add(btn);
    }

    // ?�?�?�?�?�?�?�?�?�?�?�?�?�?�?� Scene Loading ?�?�?�?�?�?�?�?�?�?�?�?�?�?�?�

    private void LoadScene(string sceneName)
    {
        if (!string.IsNullOrEmpty(currentlyLoading)) return;

        foreach (var btn in sceneButtons)
            btn.interactable = false;

        currentlyLoading = sceneName;
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        loadingText.text = "Loading " + sceneName + "...";
        yield return null;

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        if (op == null)
        {
            loadingText.text = "Failed! Add scenes via:\nTools > Setup Scene Menu";
            currentlyLoading = "";
            foreach (var btn in sceneButtons)
                btn.interactable = true;
            yield break;
        }

        while (!op.isDone)
        {
            float pct = Mathf.Clamp01(op.progress / 0.9f) * 100f;
            loadingText.text = "Loading " + sceneName + "... " + pct.ToString("F0") + "%";
            yield return null;
        }
    }
}

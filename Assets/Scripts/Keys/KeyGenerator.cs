using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class KeyGenerator : MonoBehaviour
{
    public static KeyGenerator Instance { get; private set; }
    public Material keyMaterialTemplate;
    public Transform[] spawnPoints;
    public float overallScale = 0.25f;
    public float slantX = 18f;
    public float slantZ = 10f;
    public float rotateSpeed = 80f;
    string CurrentScene => SceneManager.GetActiveScene().name;

    readonly System.Collections.Generic.List<GameObject> _tutorialShapeLabels = new System.Collections.Generic.List<GameObject>();
    readonly System.Collections.Generic.Dictionary<KeyHeadShape, GameObject> _tutorialLabelByShape =
        new System.Collections.Generic.Dictionary<KeyHeadShape, GameObject>();

    [HideInInspector] public KeyHeadShape correctShape;
    [HideInInspector] public KeyColorType correctColor;
    [HideInInspector] public bool correctKeySpinning = true;
    [HideInInspector] public List<string> generatedClues = new List<string>();
    const KeyHeadShape TutorialCorrectShape = KeyHeadShape.Square;
    const KeyColorType TutorialCorrectColor = KeyColorType.Yellow;

    // GetShapeClues(): [0] = Level 1, [1] and [2] = Level 2 boxes
    const int ShapeClueIndexLevel1 = 0;
    const int ShapeClueIndexLevel2A = 1;
    const int ShapeClueIndexLevel2B = 2;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    void Start()
    {
        Debug.Log("KeyGenerator Start");
        if (spawnPoints == null || spawnPoints.Length == 0) return;

        if (CurrentScene == "Tutorial-1")
            GenerateTutorial1();
        else if (CurrentScene == "Level1")
            GenerateLane1();
        else if (CurrentScene == "Level2")
            GenerateLane2();
    }

    void GenerateTutorial1()
    {
        correctShape = TutorialCorrectShape;
        correctColor = TutorialCorrectColor;

        KeyHeadShape[] shapes = (KeyHeadShape[])Enum.GetValues(typeof(KeyHeadShape));
        KeyColorType color = TutorialCorrectColor;
        int count = Mathf.Min(spawnPoints.Length, shapes.Length);
        Quaternion keyRot = Quaternion.Euler(slantX, 0f, slantZ);
        int teeth = 3;
        // same color but different shapes
        for (int i = 0; i < count; i++)
        {
            if (spawnPoints[i] == null) continue;
            CreateKeyObject(
                $"Tutorial1_Key_{i}_{shapes[i]}",
                spawnPoints[i].position,
                keyRot,
                shapes[i],
                color,
                teeth,
                spinning: true);
        }
        // shape labels above the tutorial keys
        ShowTutorialShapeLabels(true);
    }

    void ShowTutorialShapeLabels(bool show)
    {
        if (show && _tutorialShapeLabels.Count == 0)
            CreateTutorialShapeLabels();

        foreach (var lbl in _tutorialShapeLabels)
            if (lbl != null) lbl.SetActive(show);
    }

    void CreateTutorialShapeLabels()
    {
        KeyHeadShape[] shapes = (KeyHeadShape[])System.Enum.GetValues(typeof(KeyHeadShape));
        int count = Mathf.Min(spawnPoints != null ? spawnPoints.Length : 0, shapes.Length);

        Color labelColor = new Color(1f, 0.84f, 0f);

        for (int i = 0; i < count; i++)
        {
            if (spawnPoints == null || i >= spawnPoints.Length || spawnPoints[i] == null)
                continue;

            Vector3 labelPos = spawnPoints[i].position + Vector3.up * 1.4f;

            KeyHeadShape shape = shapes[i];
            string shapeName = shape.ToString().ToUpperInvariant();

            GameObject go = new GameObject($"TutorialShapeLabel_{shapeName}");
            go.transform.position = labelPos;

            Canvas c = go.AddComponent<Canvas>();
            c.renderMode   = RenderMode.WorldSpace;
            c.sortingOrder = 15;

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.sizeDelta  = new Vector2(160f, 40f);
            rt.localScale = Vector3.one * 0.008f;

            go.AddComponent<UnityEngine.UI.CanvasScaler>();

            GameObject bg = new GameObject("BG");
            bg.transform.SetParent(go.transform, false);
            RectTransform bgRt = bg.AddComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;
            UnityEngine.UI.Image img = bg.AddComponent<UnityEngine.UI.Image>();
            img.color = new Color(0.05f, 0.05f, 0.05f, 0.85f);

            GameObject textGO = new GameObject("Label");
            textGO.transform.SetParent(go.transform, false);
            RectTransform tRt = textGO.AddComponent<RectTransform>();
            tRt.anchorMin = Vector2.zero;
            tRt.anchorMax = Vector2.one;
            tRt.offsetMin = new Vector2(6f, 2f);
            tRt.offsetMax = new Vector2(-6f, -2f);

            TMPro.TextMeshProUGUI tmp = textGO.AddComponent<TMPro.TextMeshProUGUI>();
            tmp.text      = shapeName;
            tmp.fontSize  = 22;
            tmp.color     = labelColor;
            tmp.fontStyle = TMPro.FontStyles.Bold;
            tmp.alignment = TMPro.TextAlignmentOptions.Center;

            go.AddComponent<BillboardLabel>();

            _tutorialShapeLabels.Add(go);
            if (!_tutorialLabelByShape.ContainsKey(shape))
                _tutorialLabelByShape.Add(shape, go);
        }
    }

    public void OnKeyCollected(KeyHeadShape collectedShape)
    {
        bool tutorialLike = CurrentScene == "Tutorial-1" || CurrentScene == "Level1";
        if (!tutorialLike) return;

        if (_tutorialLabelByShape.TryGetValue(collectedShape, out var label) && label != null)
            label.SetActive(false);

        if (CurrentScene == "Tutorial-1" && TutorialManager.Instance != null)
            TutorialManager.Instance.OnKeyCollected();
    }

    void GenerateLane1()
    {
        KeyHeadShape[] shapes = (KeyHeadShape[])Enum.GetValues(typeof(KeyHeadShape));
        correctColor = KeyColorType.Yellow;
        correctShape = shapes[Random.Range(0, shapes.Length)];
        var shapeLines = GetShapeClues(correctShape);
        generatedClues = new List<string> { shapeLines[ShapeClueIndexLevel1] };
        int teeth = 3;
        Quaternion keyRot = Quaternion.Euler(slantX, 0f, slantZ);
        int count = Mathf.Min(spawnPoints.Length, shapes.Length);
        for (int i = 0; i < count; i++)
        {
            if (spawnPoints[i] == null) continue;
            CreateKeyObject($"Level1_Key_{i}_{shapes[i]}",
                spawnPoints[i].position, keyRot, shapes[i], correctColor, teeth, spinning: true);
        }
        if (KeyInventory.Instance != null)
            KeyInventory.Instance.requiredKeyCount = count;
        ShowTutorialShapeLabels(true);
        ClueBoxGenerator.SpawnForActiveScene(generatedClues);

        PersistToGameManager();
        Debug.Log($"[Level1] Correct key: {correctColor} {correctShape}");
    }

    void GenerateLane2()
    {
        KeyHeadShape[] shapes = (KeyHeadShape[])Enum.GetValues(typeof(KeyHeadShape));
        correctColor = KeyColorType.Yellow;
        correctShape = shapes[Random.Range(0, shapes.Length)];
        // Two clue boxes: indices 1 and 2 (ClueBoxGenerator.LANE2_PLACEMENTS).
        var shapeLines = GetShapeClues(correctShape);
        generatedClues = new List<string> { shapeLines[ShapeClueIndexLevel2A], shapeLines[ShapeClueIndexLevel2B] };
        int teeth = 3;
        Quaternion keyRot = Quaternion.Euler(slantX, 0f, slantZ);
        int count = Mathf.Min(spawnPoints.Length, shapes.Length);
        for (int i = 0; i < count; i++)
        {
            if (spawnPoints[i] == null) continue;
            CreateKeyObject($"Level2_Key_{i}_{shapes[i]}",
                spawnPoints[i].position, keyRot, shapes[i], correctColor, teeth, spinning: true);
        }
        if (KeyInventory.Instance != null)
            KeyInventory.Instance.requiredKeyCount = count;
        ShowTutorialShapeLabels(false);

        ClueBoxGenerator.SpawnForActiveScene(generatedClues);
        PersistToGameManager();
        Debug.Log($"[Level2] Correct key: {correctColor} {correctShape}");
    }

    string[] GetColorClues(KeyColorType color)
    {
        switch (color)
        {
            case KeyColorType.Green:
                return new[] { "Nature's favorite shade holds the answer.", "The forest whispers the answer." };
            case KeyColorType.Yellow:
                return new[] { "The color of caution reveals the path.", "Bright as the sun, the answer glows." };
            case KeyColorType.Blue:
                return new[] { "Look to the endless horizon.", "Deep as the ocean, the answer flows." };
            case KeyColorType.White:
                return new[] { "Winter’s untouched canvas holds the answer.", "Pure as snow, the answer awaits."};
            default:
                return new[] { "Look carefully at the colors.", "One color holds the answer." };
        }
    }

    string[] GetShapeClues(KeyHeadShape shape)
    {
        switch (shape)
        {
            case KeyHeadShape.Circle:
                return new[]
                {
                    "Key shape that never breaks into corners",
                    "The path closes on itself—no corner to hide behind.",
                    "Every direction from the center is the same distance."
                };
            case KeyHeadShape.Square:
                return new[]
                {
                    "Key shape where all sides agree",
                    "straight edges, equal corners.",
                    "Turn four times and you face home again."
                };
            case KeyHeadShape.Capsule:
                return new[]
                {
                    "Key shape that is neither sharp nor whole.",
                    "Straight in the middle, rounded at the ends—like a pill.",
                    "Not a ball, not a bar alone: two half-moons joined by a hallway."
                };
            case KeyHeadShape.Cross:
                return new[]
                {
                    "Key shape where two paths meet",
                    "One line crosses another; four arms point away from the middle.",
                    "Where a vertical meets a horizontal and neither wins."
                };
            default:
                return new[]
                {
                    "Look carefully at the shapes.",
                    "One shape holds the answer.",
                    "One shape holds the answer."
                };
        }
    }

    void PersistToGameManager()
    {
        if (GameManager.Instance == null) return;

        if (CurrentScene == "Level1")
        {
            GameManager.Instance.levelCorrectShape = correctShape.ToString();
            GameManager.Instance.levelCorrectColor = correctColor.ToString();
        }
        else if (CurrentScene == "Level2")
        {
            GameManager.Instance.lane2Clues = new List<string>(generatedClues);
            // Lane 3 door answer = lane1's color + lane2's shape
            GameManager.Instance.finalAnswer = $"{GameManager.Instance.levelCorrectColor} {correctShape}";
        }
    }

    GameObject CreateKeyObject(string name, Vector3 worldPos, Quaternion rot, KeyHeadShape shape, KeyColorType colorType, int teeth, bool spinning = true)
    {
        GameObject root = new GameObject(name);
        root.transform.position = worldPos;
        root.transform.rotation = rot;
        root.transform.localScale = Vector3.one * overallScale;
        KeySpinY spin = root.AddComponent<KeySpinY>();
        spin.speed = spinning ? rotateSpeed : 0f;
        KeyItem item = root.AddComponent<KeyItem>();
        item.shape = shape;
        item.color = colorType;
        item.spinning = spinning;
        SphereCollider pickup = root.AddComponent<SphereCollider>();
        pickup.isTrigger = true;
        pickup.radius = 1f;

        GameObject shaft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        shaft.name = "Shaft";
        shaft.transform.SetParent(root.transform, false);
        shaft.transform.localRotation = Quaternion.identity;
        shaft.transform.localScale = new Vector3(0.12f, 0.95f, 0.12f);
        Destroy(shaft.GetComponent<Collider>());

        float teethBaseY = -0.85f;
        for (int t = 0; t < teeth; t++)
        {
            GameObject tooth = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tooth.name = $"Tooth_{t}";
            tooth.transform.SetParent(root.transform, false);
            tooth.transform.localScale = new Vector3(0.18f, 0.10f, 0.10f);
            tooth.transform.localPosition = new Vector3(0.12f, teethBaseY + t * 0.12f, 0f);
            Destroy(tooth.GetComponent<Collider>());
        }

        float headY = 0.95f;
        CreateHeadShape(root.transform, shape, headY);
        Color c = ToUnityColor(colorType);
        Material template = keyMaterialTemplate != null ? keyMaterialTemplate : GetDefaultMaterial();
        foreach (var r in root.GetComponentsInChildren<Renderer>())
        {
            Material mat = template != null ? new Material(template) : new Material(r.sharedMaterial);
            mat.SetColor("_BaseColor", c);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", c * 0.6f);
            r.material = mat;
        }
        return root;
    }

    void CreateHeadShape(Transform parent, KeyHeadShape shape, float headY)
    {
        if (shape == KeyHeadShape.Circle) CreateCircleHead(parent, headY);
        else if (shape == KeyHeadShape.Square) CreateSquareHead(parent, headY);
        else if (shape == KeyHeadShape.Capsule) CreateCapsuleHead(parent, headY);
        else if (shape == KeyHeadShape.Cross) CreateCrossHead(parent, headY);
    }

    void CreateCircleHead(Transform parent, float headY)
    {
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        head.name = "Head_Circle";
        head.transform.SetParent(parent, false);
        head.transform.localRotation = Quaternion.Euler(90, 0, 0);
        head.transform.localScale = new Vector3(0.76f, 0.05f, 0.76f);
        head.transform.localPosition = new Vector3(0, headY, 0);
        Destroy(head.GetComponent<Collider>());
    }

    void CreateSquareHead(Transform parent, float headY)
    {
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Cube);
        head.name = "Head_Square";
        head.transform.SetParent(parent, false);
        head.transform.localScale = new Vector3(0.76f, 0.07f, 0.76f);
        head.transform.localPosition = new Vector3(0, headY, 0);
        Destroy(head.GetComponent<Collider>());
    }

    void CreateCapsuleHead(Transform parent, float headY)
    {
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        head.name = "Head_Capsule";
        head.transform.SetParent(parent, false);
        head.transform.localRotation = Quaternion.Euler(0, 0, 90);
        head.transform.localScale = new Vector3(0.46f, 0.40f, 0.46f);
        head.transform.localPosition = new Vector3(0, headY, 0);
        Destroy(head.GetComponent<Collider>());
    }

    void CreateCrossHead(Transform parent, float headY)
    {
        GameObject a = GameObject.CreatePrimitive(PrimitiveType.Cube);
        a.name = "Cross_A";
        a.transform.SetParent(parent, false);
        a.transform.localScale = new Vector3(0.76f, 0.07f, 0.34f);
        a.transform.localPosition = new Vector3(0, headY, 0);
        Destroy(a.GetComponent<Collider>());

        GameObject b = GameObject.CreatePrimitive(PrimitiveType.Cube);
        b.name = "Cross_B";
        b.transform.SetParent(parent, false);
        b.transform.localScale = new Vector3(0.34f, 0.07f, 0.76f);
        b.transform.localPosition = new Vector3(0, headY, 0);
        Destroy(b.GetComponent<Collider>());
    }

    Color ToUnityColor(KeyColorType c)
    {
        switch (c)
        {
            case KeyColorType.Green:  return new Color(0.30f, 0.69f, 0.31f);
            case KeyColorType.Yellow: return new Color(1f, 0.84f, 0f);
            case KeyColorType.Blue:   return new Color(0.0f, 0.75f, 1.0f);
            case KeyColorType.White:  return new Color(0.93f, 0.93f, 0.96f);
            default:                  return Color.white;
        }
    }

    Material GetDefaultMaterial()
    {
        var fromResources = Resources.Load<Material>("KeyMaterial");
        if (fromResources != null) return fromResources;
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        return shader != null ? new Material(shader) : null;
    }

    T[] ShuffleShapes<T>(T[] arr) => Shuffle(arr);
    T[] ShuffleColors<T>(T[] arr) => Shuffle(arr);

    T[] Shuffle<T>(T[] arr)
    {
        for (int i = arr.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (arr[i], arr[j]) = (arr[j], arr[i]);
        }
        return arr;
    }
}

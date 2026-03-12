using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ClueBoxGenerator : MonoBehaviour
{
    public float boxWidth  = 0.5f;
    public float boxHeight = 0.4f;
    public float boxDepth  = 0.06f;

    public Color boxColor  = Color.black;
    public Color trimColor = Color.gray;

    private static readonly BoxPlacement[] LANE2_PLACEMENTS = new BoxPlacement[]
    {
        new BoxPlacement(new Vector3(-2.77f, 1.5f,  12.0f), Quaternion.Euler(0f, 270f, 0f)),
        new BoxPlacement(new Vector3( 2.77f, 1.5f, 23.0f), Quaternion.Euler(0f,  90f, 0f)),
    };

    private static readonly BoxPlacement[] LANE3_PLACEMENTS = new BoxPlacement[]
    {
        new BoxPlacement(new Vector3(2.77f, 1.5f,  8.0f), Quaternion.Euler(0f,  90f, 0f)),
        new BoxPlacement(new Vector3(-2.77f, 1.5f, 22.0f), Quaternion.Euler(0f, 270f, 0f)),
        new BoxPlacement(new Vector3( 2.77f, 1.5f, 32.0f), Quaternion.Euler(0f, 90f, 0f)),
    };

    private string currentScene;

    void Start()
    {
        currentScene = SceneManager.GetActiveScene().name;

        if      (currentScene == "Level1-Lane2") StartCoroutine(WaitAndSpawn());
        else if (currentScene == "Level1-Lane3") SpawnClues(LANE3_PLACEMENTS, GetLane3Clues(), "Lane3");
    }

    System.Collections.IEnumerator WaitAndSpawn()
    {
        yield return null;
        yield return null;
        SpawnClues(LANE2_PLACEMENTS, GetLane2Clues(), "Lane2");
    }

    void SpawnClues(BoxPlacement[] placements, List<string> clues, string prefix)
    {
        int count = Mathf.Min(clues.Count, placements.Length);
        for (int i = 0; i < count; i++)
            CreateClueBox(prefix + "_Clue" + i, placements[i], clues[i], i);
    }

    GameObject CreateClueBox(string name, BoxPlacement p, string clueText, int index)
    {
        GameObject root = new GameObject(name);
        root.transform.position = p.position;
        root.transform.rotation = p.rotation;

        GameObject body = MakePrimitive(root, "BoxBody", PrimitiveType.Cube,
            new Vector3(boxWidth, boxHeight, boxDepth),
            Vector3.zero, boxColor, keepCollider: true);

        MakePrimitive(root, "BorderTop", PrimitiveType.Cube,
            new Vector3(boxWidth + 0.02f, 0.015f, boxDepth + 0.01f),
            new Vector3(0f, boxHeight * 0.5f + 0.0075f, 0f),
            trimColor, keepCollider: false);
        MakePrimitive(root, "BorderBot", PrimitiveType.Cube,
            new Vector3(boxWidth + 0.02f, 0.015f, boxDepth + 0.01f),
            new Vector3(0f, -boxHeight * 0.5f - 0.0075f, 0f),
            trimColor, keepCollider: false);
        MakePrimitive(root, "BorderL", PrimitiveType.Cube,
            new Vector3(0.015f, boxHeight + 0.02f, boxDepth + 0.01f),
            new Vector3(-boxWidth * 0.5f - 0.0075f, 0f, 0f),
            trimColor, keepCollider: false);
        MakePrimitive(root, "BorderR", PrimitiveType.Cube,
            new Vector3(0.015f, boxHeight + 0.02f, boxDepth + 0.01f),
            new Vector3(boxWidth * 0.5f + 0.0075f, 0f, 0f),
            trimColor, keepCollider: false);

        AttachFaceLabel(root, index);

        ClueBox cb = root.AddComponent<ClueBox>();
        cb.clueText        = clueText;
        cb.clueIndex       = index;
        cb.interactionRange = 4.0f;

        return root;
    }

    void AttachFaceLabel(GameObject root, int index)
    {
        GameObject canvasGO = new GameObject("FaceLabel");
        canvasGO.transform.SetParent(root.transform, false);
        canvasGO.transform.localPosition = new Vector3(0f, 0f, -(boxDepth * 0.5f + 0.002f));
        canvasGO.transform.localRotation = Quaternion.identity;
        canvasGO.layer = 5;

        Canvas c = canvasGO.AddComponent<Canvas>();
        c.renderMode   = RenderMode.WorldSpace;
        c.sortingOrder = 15;

        RectTransform cr = canvasGO.GetComponent<RectTransform>();
        cr.sizeDelta  = new Vector2(220f, 90f);
        cr.localScale = Vector3.one * 0.002f;

        canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();

        GameObject bgGO = new GameObject("FaceBG");
        bgGO.transform.SetParent(canvasGO.transform, false);
        bgGO.transform.SetAsFirstSibling();
        bgGO.layer = 5;
        RectTransform bgRect = bgGO.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = bgRect.offsetMax = Vector2.zero;
        var bgImg = bgGO.AddComponent<UnityEngine.UI.Image>();
        bgImg.color = new Color(0.08f, 0.06f, 0.04f, 0.95f);

        GameObject numGO = new GameObject("ClueNum");
        numGO.transform.SetParent(canvasGO.transform, false);
        numGO.layer = 5;
        RectTransform nr = numGO.AddComponent<RectTransform>();
        nr.anchorMin = new Vector2(0f, 0.54f); nr.anchorMax = new Vector2(1f, 1f);
        nr.offsetMin = new Vector2(6f, 2f);    nr.offsetMax = new Vector2(-6f, -2f);
        TextMeshProUGUI numTMP = numGO.AddComponent<TextMeshProUGUI>();
        numTMP.text               = "CLUE " + (index + 1);
        numTMP.fontSize           = 22;
        numTMP.color              = Color.yellow;
        numTMP.alignment          = TextAlignmentOptions.Bottom;
        numTMP.fontStyle          = FontStyles.Bold;
        numTMP.textWrappingMode   = TextWrappingModes.NoWrap;

        GameObject divGO = new GameObject("Divider");
        divGO.transform.SetParent(canvasGO.transform, false);
        divGO.layer = 5;
        RectTransform dr = divGO.AddComponent<RectTransform>();
        dr.anchorMin = new Vector2(0.08f, 0.50f); dr.anchorMax = new Vector2(0.92f, 0.52f);
        dr.offsetMin = dr.offsetMax = Vector2.zero;
        divGO.AddComponent<UnityEngine.UI.Image>().color = Color.yellow;

        GameObject txtGO = new GameObject("ClickLabel");
        txtGO.transform.SetParent(canvasGO.transform, false);
        txtGO.layer = 5;
        RectTransform tr = txtGO.AddComponent<RectTransform>();
        tr.anchorMin = new Vector2(0f, 0.04f); tr.anchorMax = new Vector2(1f, 0.50f);
        tr.offsetMin = new Vector2(6f, 2f);    tr.offsetMax = new Vector2(-6f, -2f);
        TextMeshProUGUI clickTMP = txtGO.AddComponent<TextMeshProUGUI>();
        clickTMP.text               = "Click to see clue";
        clickTMP.fontSize           = 17;
        clickTMP.color              = Color.white;
        clickTMP.alignment          = TextAlignmentOptions.Top;
        clickTMP.fontStyle          = FontStyles.Normal;
        clickTMP.textWrappingMode   = TextWrappingModes.NoWrap;
    }

    private List<string> GetLane2Clues()
    {
        KeyGenerator keyGen = FindFirstObjectByType<KeyGenerator>();
        if (keyGen?.generatedClues?.Count > 0)
            return new List<string>(keyGen.generatedClues);
        if (GameManager.Instance?.lane2Clues?.Count > 0)
            return new List<string>(GameManager.Instance.lane2Clues);
        return new List<string>
        {
            "Look carefully at the shapes.",
            "One shape holds the answer."
        };
    }

    private List<string> GetLane3Clues() => new List<string>
    {
        "Combine clues from Lane 1 and Lane 2.",
        "The colour of the key helped you unlock the door in Lane 1",
        "Key shape that helped you unlock the door in Lane 2"
    };

    static Material _litMaterial;

    static Material GetLitMaterial()
    {
        if (_litMaterial != null) return _litMaterial;
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        _litMaterial = shader != null ? new Material(shader) : null;
        return _litMaterial;
    }

    private GameObject MakePrimitive(GameObject parent, string n, PrimitiveType type,
        Vector3 scale, Vector3 localPos, Color color, bool keepCollider = false)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        go.name = n;
        go.transform.SetParent(parent.transform, false);
        go.transform.localScale    = scale;
        go.transform.localPosition = localPos;
        if (!keepCollider) Destroy(go.GetComponent<Collider>());
        Renderer r = go.GetComponent<Renderer>();
        if (r != null)
        {
            var mat = GetLitMaterial();
            if (mat != null) r.sharedMaterial = mat;
            r.material.color = color;
        }
        return go;
    }

    private struct BoxPlacement
    {
        public Vector3    position;
        public Quaternion rotation;
        public BoxPlacement(Vector3 pos, Quaternion rot) { position = pos; rotation = rot; }
    }
}
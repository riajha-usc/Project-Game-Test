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

    public static void SpawnForActiveScene(List<string> clues = null)
    {
        ClueBoxGenerator clueGen = FindFirstObjectByType<ClueBoxGenerator>();
        if (clueGen == null)
        {
            var go = new GameObject("_ClueGen");
            clueGen = go.AddComponent<ClueBoxGenerator>();
            clueGen.boxWidth = 0.7f;
            clueGen.boxHeight = 0.6f;
        }

        string sceneName = SceneManager.GetActiveScene().name;
        clueGen.SpawnForScene(sceneName, clues);
    }

    private static readonly BoxPlacement[] LANE2_PLACEMENTS = new BoxPlacement[]
    {
        new BoxPlacement(new Vector3(-2.766f, 1.5f, 17.0f), Quaternion.Euler(0f, 270f, 0f)),
        new BoxPlacement(new Vector3( 2.77f, 1.45f, 35.5f), Quaternion.Euler(0f,  90f, 0f)),
    };

    private static readonly BoxPlacement[] LANE3_PLACEMENTS = new BoxPlacement[]
    {
        new BoxPlacement(new Vector3(2.77f, 1.5f,  8.0f), Quaternion.Euler(0f,  90f, 0f)),
        new BoxPlacement(new Vector3(-2.77f, 1.5f, 22.0f), Quaternion.Euler(0f, 270f, 0f)),
        new BoxPlacement(new Vector3( 2.77f, 1.5f, 32.0f), Quaternion.Euler(0f, 90f, 0f)),
    };
    private static readonly BoxPlacement[] LEVEL1_PLACEMENT = new BoxPlacement[]
    {
        new BoxPlacement(new Vector3(2.01f, 1.8f, 13.7f), Quaternion.Euler(0f, 0f, 0f)),
    };

    public void SpawnForScene(string sceneName, List<string> clues = null)
    {
        if (FindFirstObjectByType<ClueBox>() != null) return;

        if (sceneName == "Level1")
        {
            string clue = (clues != null && clues.Count > 0) ? clues[0] : null;
            SpawnLevel1Clue(clue);
        }
        else if (sceneName == "Level2")
        {
            var lane2Clues = (clues != null && clues.Count > 0) ? clues : GetLane2Clues();
            SpawnClues(LANE2_PLACEMENTS, lane2Clues, "Lane2");
        }
        else if (sceneName == "Level3")
        {
            SpawnClues(LANE3_PLACEMENTS, GetLane3Clues(), "Lane3");
        }
    }

    public GameObject SpawnLevel1Clue(string clueTextOverride = null)
    {
        string clue = string.IsNullOrEmpty(clueTextOverride) ? GetLevel1Clue() : clueTextOverride;
        return CreateClueBox("Level1_Clue0", LEVEL1_PLACEMENT[0].position, LEVEL1_PLACEMENT[0].rotation, clue, 0);
    }

    void SpawnClues(BoxPlacement[] placements, List<string> clues, string prefix)
    {
        int count = Mathf.Min(clues.Count, placements.Length);
        for (int i = 0; i < count; i++)
            CreateClueBox(prefix + "_Clue" + i, placements[i].position, placements[i].rotation, clues[i], i);
    }

    public GameObject CreateClueBox(string name, Vector3 position, Quaternion rotation, string clueText, int index)
    {
        GameObject root = new GameObject(name);
        root.transform.position = position;
        root.transform.rotation = rotation;

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
        cb.interactionRange = 2.0f;

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
        cr.sizeDelta  = new Vector2(240f, 140f);
        cr.localScale = Vector3.one * 0.0026f;

        canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();

        GameObject borderGO = new GameObject("Border");
        borderGO.transform.SetParent(canvasGO.transform, false);
        borderGO.transform.SetAsFirstSibling();
        borderGO.layer = 5;
        RectTransform borderRect = borderGO.AddComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.offsetMin = borderRect.offsetMax = Vector2.zero;
        borderGO.AddComponent<UnityEngine.UI.Image>().color = new Color(1f, 0.84f, 0f, 0.92f);

        GameObject bgGO = new GameObject("FaceBG");
        bgGO.transform.SetParent(canvasGO.transform, false);
        bgGO.layer = 5;
        RectTransform bgRect = bgGO.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = new Vector2(3f, 3f);
        bgRect.offsetMax = new Vector2(-3f, -3f);
        bgGO.AddComponent<UnityEngine.UI.Image>().color = new Color(0.07f, 0.05f, 0.03f, 0.97f);

        GameObject numGO = new GameObject("ClueNum");
        numGO.transform.SetParent(canvasGO.transform, false);
        numGO.layer = 5;
        RectTransform nr = numGO.AddComponent<RectTransform>();
        nr.anchorMin = Vector2.zero;
        nr.anchorMax = Vector2.one;
        nr.offsetMin = new Vector2(10f, 6f);
        nr.offsetMax = new Vector2(-10f, -6f);
        TextMeshProUGUI numTMP = numGO.AddComponent<TextMeshProUGUI>();
        numTMP.text             = "CLUE " + (index + 1);
        numTMP.fontSize         = 38;
        numTMP.color            = new Color(1f, 0.84f, 0f);
        numTMP.alignment        = TextAlignmentOptions.Center;
        numTMP.fontStyle        = FontStyles.Bold;
        numTMP.textWrappingMode = TextWrappingModes.NoWrap;

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

    private string GetLevel1Clue()
    {
        KeyGenerator keyGen = FindFirstObjectByType<KeyGenerator>();
        if (keyGen?.generatedClues != null && keyGen.generatedClues.Count > 0)
            return keyGen.generatedClues[0];
        return "Look carefully at the shapes.";
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
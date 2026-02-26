using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Door : MonoBehaviour
{
    public string sceneToLoad;
    public string spawnID;
    public string currentScene;

    public float clueSize = 0.45f; // size of the clue
    public Vector3 circlePosition = new Vector3(-2.9f, 0f, 0f); // position of the clue for circle
    public Vector3 squarePosition = new Vector3(-2.9f, 0f, 0f); // position of the clue for square
    public Vector3 capsulePosition = new Vector3(-2.9f, 0f, 0f); // position of the clue for capsule
    public Vector3 crossPosition = new Vector3(-2.9f, 0f, 0f); // position of the clue for cross
    public int maxWrongAttempts = 1;
    int wrongAttempts = 0;
    GameObject clueRoot;
    public GameObject gameOverScreen;

    void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.solvedScenes.Contains(currentScene)) {
            GetComponent<Collider>().enabled = false;
            return;   
        }
        InvokeRepeating(nameof(TryDrawClue), 0f, 0.1f);
    }

    void TryDrawClue()
    {
        if (!KeyAnswer.hasValue) return;
        //if (KeyInventory.Instance == null || !KeyInventory.Instance.HasAllKeys()) return;
        CancelInvoke(nameof(TryDrawClue));
        DrawClue2D(KeyAnswer.shape, KeyAnswer.color);
    }

    void DrawClue2D(KeyHeadShape shape, KeyColorType color)
    {
        // this is not needed just keeping it
        Transform old = transform.Find("DoorClueRoot");
        if (old != null) Destroy(old.gameObject);

        clueRoot = new GameObject("DoorClueRoot");
        clueRoot.transform.SetParent(transform, false);

        Vector3 pos;
        switch (shape)
        {
            case KeyHeadShape.Circle:  pos = circlePosition;  break;
            case KeyHeadShape.Square:  pos = squarePosition;  break;
            case KeyHeadShape.Capsule: pos = capsulePosition; break;
            case KeyHeadShape.Cross:   pos = crossPosition;   break;
            default:                   pos = squarePosition;  break;
        }

        clueRoot.transform.localPosition = pos;
        clueRoot.transform.localRotation = Quaternion.identity;
        clueRoot.transform.localScale = Vector3.one;

        // create the 2D sticker shape
        GameObject clue = CreateStickerShape(shape);
        clue.name = "DoorClue";
        clue.transform.SetParent(clueRoot.transform, false);
        clue.transform.localRotation = Quaternion.identity;
        clue.transform.localScale = Vector3.one * clueSize;

        Material m = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        Color col = ToUnityColor(color);
        m.SetColor("_BaseColor", col);

        foreach (var rr in clue.GetComponentsInChildren<Renderer>(true))
        {
            rr.material = m;
            rr.material.renderQueue = 4000;
        }
    }

    GameObject CreateStickerShape(KeyHeadShape shape)
    {
        if (shape == KeyHeadShape.Circle)
        {
            GameObject g = new GameObject("Circle2D");
            g.AddComponent<MeshFilter>().mesh = CreateDiscMesh(32);
            g.AddComponent<MeshRenderer>();
            return g;
        }

        if (shape == KeyHeadShape.Square)
        {
            GameObject g = new GameObject("Square2D");
            g.AddComponent<MeshFilter>().mesh = CreateQuadMesh();
            g.AddComponent<MeshRenderer>();
            return g;
        }

        if (shape == KeyHeadShape.Capsule)
        {
            GameObject g = new GameObject("Capsule2D");
            g.AddComponent<MeshFilter>().mesh = CreateCapsuleMesh(1f, 0.45f, 32);
            g.AddComponent<MeshRenderer>();
            return g;
        }

        if (shape == KeyHeadShape.Cross)
        {
            GameObject root = new GameObject("Cross2D");

            GameObject a = new GameObject("CrossArm1");
            a.AddComponent<MeshFilter>().mesh = CreateQuadMesh();
            a.AddComponent<MeshRenderer>();
            a.transform.SetParent(root.transform, false);
            a.transform.localScale = new Vector3(1f, 0.30f, 1.0f);

            GameObject b = new GameObject("CrossArm2");
            b.AddComponent<MeshFilter>().mesh = CreateQuadMesh();
            b.AddComponent<MeshRenderer>();
            b.transform.SetParent(root.transform, false);
            b.transform.localScale = new Vector3(1f, 1.0f, 0.30f);

            return root;
        }

        GameObject fallback = new GameObject("Fallback2D");
        fallback.AddComponent<MeshFilter>().mesh = CreateQuadMesh();
        fallback.AddComponent<MeshRenderer>();
        return fallback;
    }

    Mesh CreateQuadMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[]
        {
            new Vector3(0f, -0.5f, -0.5f),
            new Vector3(0f,  0.5f, -0.5f),
            new Vector3(0f,  0.5f,  0.5f),
            new Vector3(0f, -0.5f,  0.5f)
        };
        mesh.triangles = new int[]
        {
            0, 1, 2,  0, 2, 3,
            0, 2, 1,  0, 3, 2
        };
        mesh.RecalculateNormals();
        return mesh;
    }

    Mesh CreateDiscMesh(int segments)
    {
        Mesh mesh = new Mesh();
        Vector3[] verts = new Vector3[segments + 1];
        verts[0] = Vector3.zero;
        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            verts[i + 1] = new Vector3(0f, Mathf.Sin(angle) * 0.5f, Mathf.Cos(angle) * 0.5f);
        }

        int[] tris = new int[segments * 6];
        for (int i = 0; i < segments; i++)
        {
            int next = (i + 1) % segments + 1;
            tris[i * 6]     = 0;
            tris[i * 6 + 1] = i + 1;
            tris[i * 6 + 2] = next;
            tris[i * 6 + 3] = 0;
            tris[i * 6 + 4] = next;
            tris[i * 6 + 5] = i + 1;
        }

        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        return mesh;
    }

    Mesh CreateCapsuleMesh(float width, float height, int capSegments)
    {
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();

        verts.Add(Vector3.zero);

        float r = height * 0.5f;
        float bodyHalf = (width - height) * 0.5f;
        int halfSegs = capSegments / 2;
        for (int i = 0; i <= halfSegs; i++)
        {
            float angle = -Mathf.PI / 2f + i * Mathf.PI / halfSegs;
            verts.Add(new Vector3(0f, Mathf.Sin(angle) * r, bodyHalf + Mathf.Cos(angle) * r));
        }

        for (int i = 0; i <= halfSegs; i++)
        {
            float angle = Mathf.PI / 2f + i * Mathf.PI / halfSegs;
            verts.Add(new Vector3(0f, Mathf.Sin(angle) * r, -bodyHalf + Mathf.Cos(angle) * r));
        }

        int count = verts.Count;
        for (int i = 1; i < count - 1; i++)
        {
            tris.Add(0); tris.Add(i); tris.Add(i + 1);
            tris.Add(0); tris.Add(i + 1); tris.Add(i);
        }
        tris.Add(0); tris.Add(count - 1); tris.Add(1);
        tris.Add(0); tris.Add(1); tris.Add(count - 1);

        Mesh mesh = new Mesh();
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();
        return mesh;
    }

    Color ToUnityColor(KeyColorType c)
    {
        switch (c)
        {
            case KeyColorType.Red: return Color.red;
            case KeyColorType.Blue: return Color.blue;
            case KeyColorType.Green: return Color.green;
            case KeyColorType.Yellow: return Color.yellow;
            case KeyColorType.Purple: return new Color(0.6f, 0.2f, 0.8f);
            case KeyColorType.Cyan: return Color.cyan;
            case KeyColorType.Orange: return new Color(1f, 0.5f, 0.1f);
            case KeyColorType.White: return Color.white;
            default: return Color.white;
        }
    }

    public void TryUnlock(KeyHeadShape shape, KeyColorType color)
    {
        Debug.Log("Solved scenes: " + string.Join(", ", GameManager.Instance.solvedScenes));
        if (GameManager.Instance != null && GameManager.Instance.solvedScenes.Contains(currentScene)) {
            Debug.Log("Scene already solved, loading next scene: " + sceneToLoad);
            SceneManager.LoadScene(sceneToLoad);
            return;
        }
        if (!KeyAnswer.hasValue) return;

        if (shape != KeyAnswer.shape || color != KeyAnswer.color)
        {
            wrongAttempts++;
            Debug.Log("Wrong key! Attempt " + wrongAttempts + " of " + maxWrongAttempts);
            if (wrongAttempts > maxWrongAttempts)
            {
                if (gameOverScreen != null)
                {
                    gameOverScreen.SetActive(true);
                    var player = GameObject.FindGameObjectWithTag("Player");
                    if (player != null)
                    {
                        player.GetComponent<PlayerMovement3D>().enabled = false;
                    }
                }
            }
            return;
        }

        if (GameManager.Instance != null)
            GameManager.Instance.targetSpawnID = spawnID;
        GameManager.Instance.solvedScenes.Add(currentScene);
        Debug.Log("Solved scenes: " + string.Join(", ", GameManager.Instance.solvedScenes));
        SceneManager.LoadScene(sceneToLoad);
    }
}
using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class KeyGenerator : MonoBehaviour
{
    public Transform[] spawnPoints; // key positions
    public float overallScale = 0.05f;
    public float slantX = 18f;
    public float slantZ = 10f;
    public float rotateSpeed = 60f;
    public KeyHeadShape correctShape;
    public KeyColorType correctColor;
    public string currentScene;

    public struct ShapeColorCombo
    {
        public KeyHeadShape shape;
        public KeyColorType color;
        public ShapeColorCombo(KeyHeadShape s, KeyColorType c)
        {
            shape = s;
            color = c;
        }
    }

    void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.solvedScenes.Contains(currentScene))
        {
            return; 
        }
        if (spawnPoints == null || spawnPoints.Length == 0) return;

        KeyHeadShape[] shapes = (KeyHeadShape[])System.Enum.GetValues(typeof(KeyHeadShape));
        KeyColorType[] colors = (KeyColorType[])System.Enum.GetValues(typeof(KeyColorType));
        if (KeyInventory.Instance != null)
            KeyInventory.Instance.requiredKeyCount = spawnPoints.Length;

        correctShape = shapes[Random.Range(0, shapes.Length)];
        correctColor = colors[Random.Range(0, colors.Length)];
        KeyAnswer.Set(correctShape, correctColor);
        ShapeColorCombo correct = new ShapeColorCombo(correctShape, correctColor);
        
        int count = Mathf.Min(spawnPoints.Length);

        ShapeColorCombo[] picked = PickCombosWithMax2(shapes, colors, count, correct);
        if (picked == null) return;

        Quaternion keyRot = Quaternion.Euler(slantX, 0f, slantZ);

        // getting count number of random unique numbers for teeth
        int[] teeth = MakeUniqueInts(count, 1, 7);
        for (int i = 0; i < count; i++)
        {
            Transform sp = spawnPoints[i];
            if (sp == null) continue;
            Vector3 pos = sp.position;
            KeyHeadShape shape = picked[i].shape;
            KeyColorType color = picked[i].color;
            CreateKeyObject($"Key_{i}_{color}_{shape}", pos, keyRot, shape, color, teeth[i]);
        }
    }

    int[] MakeUniqueInts(int count, int minInclusive, int maxExclusive)
    {
        int range = maxExclusive - minInclusive;
        int[] arr = new int[count];

        if (count <= range)
        {
            int[] pool = new int[range];
            for (int i = 0; i < range; i++) pool[i] = minInclusive + i;

            for (int i = pool.Length - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                int tmp = pool[i];
                pool[i] = pool[j];
                pool[j] = tmp;
            }

            for (int i = 0; i < count; i++) arr[i] = pool[i];
            return arr;
        }

        for (int i = 0; i < count; i++)
            arr[i] = Random.Range(minInclusive, maxExclusive);

        return arr;
    }

    ShapeColorCombo[] PickCombosWithMax2(KeyHeadShape[] shapes, KeyColorType[] colors, int count, ShapeColorCombo correct)
    {
        int shapeN = shapes.Length;
        int colorN = colors.Length;

        int[] shapeUsed = new int[shapeN];
        int[] colorUsed = new int[colorN];

        ShapeColorCombo[] all = new ShapeColorCombo[shapeN * colorN];
        int k = 0;
        for (int i = 0; i < shapeN; i++)
            for (int j = 0; j < colorN; j++)
                all[k++] = new ShapeColorCombo(shapes[i], colors[j]);

        for (int i = all.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            ShapeColorCombo tmp = all[i];
            all[i] = all[j];
            all[j] = tmp;
        }

        int correctShapeIndex = System.Array.IndexOf(shapes, correct.shape);
        int correctColorIndex = System.Array.IndexOf(colors, correct.color);
        if (correctShapeIndex < 0 || correctColorIndex < 0) return null;

        ShapeColorCombo[] picked = new ShapeColorCombo[count];
        int pickedCount = 0;

        picked[pickedCount++] = correct;
        shapeUsed[correctShapeIndex]++;
        colorUsed[correctColorIndex]++;

        for (int i = 0; i < all.Length && pickedCount < count; i++)
        {
            ShapeColorCombo c = all[i];

            if (c.shape.Equals(correct.shape) && c.color.Equals(correct.color))
                continue;

            int si = System.Array.IndexOf(shapes, c.shape);
            int ci = System.Array.IndexOf(colors, c.color);

            if (shapeUsed[si] >= 2) continue;
            if (colorUsed[ci] >= 2) continue;

            picked[pickedCount++] = c;
            shapeUsed[si]++;
            colorUsed[ci]++;
        }

        if (pickedCount < count) return null;

        for (int i = picked.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            ShapeColorCombo tmp = picked[i];
            picked[i] = picked[j];
            picked[j] = tmp;
        }

        return picked;
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

    void CreateKeyObject(string name, Vector3 worldPos, Quaternion rot, KeyHeadShape shape, KeyColorType colorType, int teeth)
    {
        GameObject root = new GameObject(name);
        root.transform.position = worldPos;
        root.transform.rotation = rot;
        root.transform.localScale = Vector3.one * overallScale;
        KeySpinY spin = root.AddComponent<KeySpinY>();
        spin.speed = rotateSpeed;
        KeyItem item = root.AddComponent<KeyItem>();
        item.shape = shape;
        item.color = colorType;
        SphereCollider pickup = root.AddComponent<SphereCollider>();
        pickup.isTrigger = true;
        pickup.radius = 0.8f;
        
        // shaft is added to the key
        GameObject shaft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        shaft.name = "Shaft";
        shaft.transform.SetParent(root.transform, false);
        shaft.transform.localRotation = Quaternion.identity;
        shaft.transform.localScale = new Vector3(0.12f, 0.95f, 0.12f);
        Destroy(shaft.GetComponent<Collider>());

        // teeth are added to the key
        float teethBaseY = -0.85f;
        // in future levels, we can see based on number of teeth, we can add some clue
        for (int t = 0; t < teeth; t++)
        {
            GameObject tooth = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tooth.name = $"Tooth_{t}";
            tooth.transform.SetParent(root.transform, false);
            tooth.transform.localScale = new Vector3(0.18f, 0.10f, 0.10f);
            tooth.transform.localPosition = new Vector3(0.12f, teethBaseY + t * 0.12f, 0f);
            Destroy(tooth.GetComponent<Collider>());
        }

        // head and color are added to the key
        float headY = 0.95f;
        CreateHeadShape(root.transform, shape, headY);
        Color c = ToUnityColor(colorType);
        foreach (var r in root.GetComponentsInChildren<Renderer>())
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.SetColor("_BaseColor", c);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", c * 0.6f);
            r.material = mat;
        }
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
        head.transform.localScale = new Vector3(0.42f, 0.05f, 0.42f);
        head.transform.localPosition = new Vector3(0, headY, 0);
        Destroy(head.GetComponent<Collider>());
    }

    void CreateSquareHead(Transform parent, float headY)
    {
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Cube);
        head.name = "Head_Square";
        head.transform.SetParent(parent, false);
        head.transform.localScale = new Vector3(0.42f, 0.07f, 0.42f);
        head.transform.localPosition = new Vector3(0, headY, 0);
        Destroy(head.GetComponent<Collider>());
    }

    void CreateCapsuleHead(Transform parent, float headY)
    {
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        head.name = "Head_Capsule";
        head.transform.SetParent(parent, false);
        head.transform.localRotation = Quaternion.Euler(0, 0, 90);
        head.transform.localScale = new Vector3(0.22f, 0.22f, 0.22f);
        head.transform.localPosition = new Vector3(0, headY, 0);
        Destroy(head.GetComponent<Collider>());
    }

    void CreateCrossHead(Transform parent, float headY)
    {
        GameObject a = GameObject.CreatePrimitive(PrimitiveType.Cube);
        a.name = "Cross_A";
        a.transform.SetParent(parent, false);
        a.transform.localScale = new Vector3(0.50f, 0.07f, 0.18f);
        a.transform.localPosition = new Vector3(0, headY, 0);
        Destroy(a.GetComponent<Collider>());

        GameObject b = GameObject.CreatePrimitive(PrimitiveType.Cube);
        b.name = "Cross_B";
        b.transform.SetParent(parent, false);
        b.transform.localScale = new Vector3(0.18f, 0.07f, 0.50f);
        b.transform.localPosition = new Vector3(0, headY, 0);
        Destroy(b.GetComponent<Collider>());
    }

}
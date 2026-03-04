using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProjectileController : MonoBehaviour
{
    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;

    [Header("ProjectileAttributes")]
    public float speed = 0f;
    public float lifetime = 5f;
    public float damage = 0f;

    private void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        CreateDelta();
        UpdateMesh();
        Destroy(gameObject, lifetime);
    }
    private void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
    void CreateDelta()
    {
        float w = 0.3f;
        vertices = new Vector3[]
        {
            new Vector3(0, 0.6f, 0.6f),
            new Vector3(1, 0.6f, 0.6f),
            new Vector3(0.5f, 1, -0.7f),

            new Vector3(0f, 0.6f - w, 0.6f),
            new Vector3(1, 0.6f - w, 0.6f),
            new Vector3(0.5f, 1-w, -0.7f)
        };

        triangles = new int[] {
            0, 1, 2,
            
            5, 4, 3,

            0, 2, 3,
            2, 5, 3,

            1, 4, 2,
            2, 4, 5,

            0, 3, 1,
            1, 3, 4
        };
    }

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        Vector2[] uvs = new Vector2[vertices.Length];
        for(int i = 0; i < uvs.Length; i++)
        {
            uvs[i] = new Vector2(vertices[i].x, vertices[i].z);
        }
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        //mesh.Optimize();
        UpdateCollider();
    }

    void UpdateCollider()
    {
        MeshCollider mc = GetComponent<MeshCollider>();
        if(mc == null)
        {
            mc = gameObject.AddComponent<MeshCollider>();
        }
        mc.sharedMesh = null;
        mc.sharedMesh = mesh;
    }
}

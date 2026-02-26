using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    public float speed = 40f;
    public float lifetime = 5f;
    public float damage = 10f;

    void Start()
    {
        Renderer renderer = gameObject.GetComponent<Renderer>();
        renderer.material.color = Color.red;
        Destroy(gameObject, lifetime);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Hit Player!");
        }

        Destroy(gameObject);
    }
}
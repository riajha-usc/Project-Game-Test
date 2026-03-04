using UnityEngine;

public class HpBoosterController : MonoBehaviour
{
    public float rotationSpeed = 45.0f;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Hit Player!");
        }

        Destroy(transform.gameObject);
    }
    void Update()
    {
        transform.Rotate(Vector3.right * rotationSpeed * Time.deltaTime);
    }
}

using UnityEngine;

public class PulsePerkController : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement3D pm = other.GetComponent<PlayerMovement3D>();
            if (pm != null)
            {
                pm.usePulse = true;
            }
        }

        Destroy(transform.gameObject);
    }
}

using UnityEngine;

public class SafeZoneController : MonoBehaviour
{
    public static bool InSafeZone = false;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            InSafeZone = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            InSafeZone = false;
        }
    }
}

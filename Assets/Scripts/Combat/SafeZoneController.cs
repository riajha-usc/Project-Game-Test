using UnityEngine;

public class SafeZoneController : MonoBehaviour
{
    public static bool InSafeZone = false;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            InSafeZone = true;
            Debug.Log("Player in SAfe Zone!");
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            InSafeZone = false;
            Debug.Log("Player Outside SafeZone");
        }
    }
}

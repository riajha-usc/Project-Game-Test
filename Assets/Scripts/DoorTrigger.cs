using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    public Door door;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (KeyInventoryUI.Instance != null)
            KeyInventoryUI.Instance.SetDoorNearby(door, true);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (KeyInventoryUI.Instance != null)
            KeyInventoryUI.Instance.SetDoorNearby(door, false);
    }
}
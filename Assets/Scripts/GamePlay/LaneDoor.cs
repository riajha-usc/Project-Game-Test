using UnityEngine;

public class LaneDoor : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // UIManager.Instance.ShowInteractPrompt();
            if (KeyInventoryUI.Instance != null)
                KeyInventoryUI.Instance.SetDoorInRange(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // UIManager.Instance.HideInteractPrompt();
            if (KeyInventoryUI.Instance != null)
                KeyInventoryUI.Instance.SetDoorInRange(false);
        }
    }
}
using UnityEngine;

public class UniversalTrapZoneController : MonoBehaviour
{
    public string trapType = "Spikes";
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (TutorialManager.Instance == null)
            return;

        if(trapType == "Spikes")
        {
            TutorialManager.Instance.ShowPopup("Beware of the Spikes!", 3.5f);
        }
        else if(trapType == "MovingDivider")
        {
            TutorialManager.Instance.ShowPopup("Watch out!", 3.5f);
        }
    }
}

using UnityEngine;

public class UniversalTrapZoneController : MonoBehaviour
{
    public string trapType = "Spikes";
    public float offTime = 5f;

    private static UniversalTrapZoneController currentZone;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        currentZone = this;

        if (TutorialManager.Instance == null)
            return;

        if (trapType == "Spikes")
        {
            bool isTrapTutorial = TutorialManager.Instance?.tutorialType == "traps";
            TutorialManager.Instance.ShowPopup(
                isTrapTutorial ? "Press F to deactivate the spikes!" : "Beware of the Spikes!",
                isTrapTutorial ? 0f : 3.5f);
            if (isTrapTutorial)
                TutorialManager.Instance.HideTutorialArrow();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (currentZone == this)
            currentZone = null;
    }

    private void Update()
    {
        if (currentZone != this) return;

        if (Input.GetKeyDown(KeyCode.F))
        {
            if (TrapCombatAgentManager.TryActivate("spikes", offTime))
            {
                TutorialManager.Instance?.OnTrapDeactivated("spikes");
                if (TutorialManager.Instance != null)
                {
                    TutorialManager.Instance.ShowPopup(
                        $"{trapType} deactivated for {offTime} seconds!", 3f);
                }
            }
            else
            {
                if (TutorialManager.Instance != null)
                {
                    TutorialManager.Instance.ShowPopup(
                        "You need a Deactivating Agent first.", 2.5f);
                }
            }
        }
    }
}
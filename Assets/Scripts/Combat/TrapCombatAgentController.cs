using System.Collections;
using UnityEngine;

public class TrapCombatAgentController : MonoBehaviour
{
    [Header("Operation Parameters")]
    public string mode = "default";
    public float offTime = 5f;

    private bool collected = false;

    private void OnTriggerEnter(Collider other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;

        collected = true;
        int charges = TrapAgentHUD.Instance != null ? TrapAgentHUD.Instance.usesPerPill : 2;
        TrapCombatAgentManager.AddCharge(charges);
        TutorialManager.Instance?.OnTrapAgentCollected();

        if (mode == "tutorial" && TutorialManager.Instance != null)
        {
            StartCoroutine(ShowTutorialMessages());
        }

        Destroy(gameObject);
    }

    private IEnumerator ShowTutorialMessages()
    {
        TutorialManager.Instance.ShowPopup(
            $"You collected a Deactivating Agent. Enter a trap zone and press 'F' to deactivate traps for {offTime} seconds.", 4f);

        yield return new WaitForSeconds(3f);

        TutorialManager.Instance.ShowPopup(
            $"Press 'F' only inside the trap zone!", 4f);
    }
}
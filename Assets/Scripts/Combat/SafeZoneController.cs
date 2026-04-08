using UnityEngine;
using System.Collections;

public class SafeZoneController : MonoBehaviour
{
    public string mode = "default";

    public static bool InSafeZone = false;

    private PlayerMovement3D player;
    private Coroutine healRoutine;

    public float healDuration = 3f;
    //public int maxUsage = 3;
    //private int usagesUsed = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        player = other.GetComponent<PlayerMovement3D>();
        if (player == null) return;

        if (InSafeZone)
            return;

        //if (usagesUsed >= maxUsage)
        //{
        //    TutorialManager.Instance.ShowPopup($"Can be Used ({maxUsage}) Times", 3.5f);
        //    return;
        //}

        InSafeZone = true;

        if (GameManager.Instance != null)
            GameManager.Instance.RecordSafeZoneEntry();

        if (mode == "tutorial" && TutorialManager.Instance != null)
        {
            TutorialManager.Instance.ShowPopup(
                "You are in the Safe Zone!\nRecovers 40% of your health.",
                4f
            );
        }

        if (healRoutine != null)
            StopCoroutine(healRoutine);

        healRoutine = StartCoroutine(HealPlayer(player));
        //usagesUsed++;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        InSafeZone = false;

        if (healRoutine != null)
        {
            StopCoroutine(healRoutine);
            healRoutine = null;
        }

        player = null;
    }

    private IEnumerator HealPlayer(PlayerMovement3D pc)
    {
        if (pc == null)
            yield break;

        float startHp = pc.hp;
        float healAmount = pc.maxHp * 0.4f;
        float targetHp = Mathf.Min(pc.maxHp, startHp + healAmount);

        float elapsed = 0f;

        while (elapsed < healDuration && InSafeZone && pc != null)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / healDuration;
            pc.hp = Mathf.Lerp(startHp, targetHp, t);
            yield return null;
        }

        if (pc != null && InSafeZone)
            pc.hp = targetHp;
    }
}
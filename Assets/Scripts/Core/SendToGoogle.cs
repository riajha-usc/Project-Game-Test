using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class SendToGoogle : MonoBehaviour
{
    [Header("Google Form URL")]
    [SerializeField] private string FormUrl;

    [Tooltip("Uncheck to disable (no HTTP calls, no errors during development)")]
    [SerializeField] private bool enableAnalytics = true;

    [Header("Form entry IDs")]
    [SerializeField] private string entrySessionId = "entry.821792080";
    [SerializeField] private string entryLevelTime = "entry.1195999058";
    [SerializeField] private string entryLevelName = "entry.47591005";
    [SerializeField] private string entrySafeZoneUsage = "entry.2138036155";
    [SerializeField] private string entryBeamsDeactivated = "entry.521773209";
    [SerializeField] private string entrySpikesDeactivated = "entry.309719805";
    [SerializeField] private string entryDividersDeactivated = "entry.776800071";
    [SerializeField] private string entryHealth = "entry.108452304";
    [SerializeField] private string entryResult = "entry.783075977";
    [SerializeField] private string entrySequenceVUsed = "entry.468448284";
    [SerializeField] private string entryNoOfAttempts = "entry.1018695553";
    [SerializeField] private string clueZoneEntered = "entry.2023973671";
    [SerializeField] private string beamHitsCount = "entry.469094003";
    [SerializeField] private string spikesHitsCount = "entry.640080099";
    [SerializeField] private string dividerHitsCount = "entry.612356627";

    public void Send(bool won)
    {
        if (!enableAnalytics || GameManager.Instance == null) return;

        string sessionId = GameManager.Instance.sessionId.ToString();
        string incorrectKey = GameManager.Instance.incorrectKeyCount.ToString();
        string incorrectCode = GameManager.Instance.incorrectCodeCount.ToString();
        string levelTime = Mathf.RoundToInt(GameManager.Instance.GetLevelTimeSeconds()).ToString();
        string levelName = SceneManager.GetActiveScene().name;
        string safeZoneUsage = GameManager.Instance.safeZoneEntryCount.ToString();
        string beams = TrapCombatAgentManager.BeamDeactivateCount.ToString();
        string spikes = TrapCombatAgentManager.SpikeDeactivateCount.ToString();
        string dividers = TrapCombatAgentManager.DividerDeactivateCount.ToString();

        float hp = 0f;
        var playerGo = GameObject.FindGameObjectWithTag("Player");
        if (playerGo != null)
        {
            var pm = playerGo.GetComponent<PlayerMovement3D>();
            if (pm != null) hp = pm.hp;
        }
        string health = Mathf.RoundToInt(hp).ToString();
        string result = won ? "Won" : "Lost";
        string sequenceV = KeyInventoryUI.Instance != null
            ? KeyInventoryUI.Instance.GetSequenceRevealUsedForAnalytics()
            : "N/A";

        // If player used V sequence disclosure, report attempts as 1; otherwise actual key attempts at door.
        string noOfAttempts = sequenceV == "Yes"
            ? "1"
            : GameManager.Instance.keyAttemptCount.ToString();
        string clueZoneEntries = GameManager.Instance.clueZoneEntryCount.ToString();
        string beamHits = GameManager.Instance.beamHitsCount.ToString();
        string spikeHits = GameManager.Instance.spikesHitsCount.ToString();
        string dividerHits = GameManager.Instance.dividerHitsCount.ToString();

        StartCoroutine(Post(
            sessionId, incorrectKey, incorrectCode, levelTime,
            levelName, safeZoneUsage, beams, spikes, dividers,
            health, result, sequenceV,
            noOfAttempts, clueZoneEntries, beamHits, spikeHits, dividerHits));
    }

    static void AddFieldIfId(WWWForm form, string entryId, string value)
    {
        if (string.IsNullOrEmpty(entryId) || value == null) return;
        form.AddField(entryId, value);
    }

    private IEnumerator Post(
        string sessionId,
        string incorrectKey,
        string incorrectCode,
        string levelTime,
        string levelName,
        string safeZoneUsage,
        string beams,
        string spikes,
        string dividers,
        string health,
        string result,
        string sequenceV,
        string noOfAttempts,
        string clueZoneEntries,
        string beamHits,
        string spikeHits,
        string dividerHits)
    {
        WWWForm form = new WWWForm();
        AddFieldIfId(form, entrySessionId, sessionId);
        AddFieldIfId(form, entryLevelTime, levelTime);
        AddFieldIfId(form, entryLevelName, levelName);
        AddFieldIfId(form, entrySafeZoneUsage, safeZoneUsage);
        AddFieldIfId(form, entryBeamsDeactivated, beams);
        AddFieldIfId(form, entrySpikesDeactivated, spikes);
        AddFieldIfId(form, entryDividersDeactivated, dividers);
        AddFieldIfId(form, entryHealth, health);
        AddFieldIfId(form, entryResult, result);
        AddFieldIfId(form, entrySequenceVUsed, sequenceV);
        AddFieldIfId(form, entryNoOfAttempts, noOfAttempts);
        AddFieldIfId(form, clueZoneEntered, clueZoneEntries);
        AddFieldIfId(form, beamHitsCount, beamHits);
        AddFieldIfId(form, spikesHitsCount, spikeHits);
        AddFieldIfId(form, dividerHitsCount, dividerHits);

        using (UnityWebRequest request = UnityWebRequest.Post(FormUrl, form))
        {
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Data sent successfully");
            }
            else
            {
                Debug.LogError("Failed to send data: " + request.error);
            }
        }
    }
}

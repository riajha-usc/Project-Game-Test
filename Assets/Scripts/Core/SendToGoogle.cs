using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class SendToGoogle : MonoBehaviour
{
    [Header("Google Form URL")]
    [SerializeField] private string FormUrl;

    [Tooltip("Uncheck to disable (no HTTP calls, no errors during development)")]
    [SerializeField] private bool enableAnalytics = true;

    public void Send()
    {
        if (!enableAnalytics || GameManager.Instance == null) return;

        string sessionId = GameManager.Instance.sessionId.ToString();
        string deathsToEnemy = GameManager.Instance.deathsToEnemy.ToString();
        string incorrectKey = GameManager.Instance.incorrectKeyCount.ToString();
        string incorrectCode = GameManager.Instance.incorrectCodeCount.ToString();
        string levelTime = Mathf.RoundToInt(GameManager.Instance.GetLevelTimeSeconds()).ToString();

        StartCoroutine(Post(sessionId, incorrectKey, incorrectCode, deathsToEnemy, levelTime));
    }

    private IEnumerator Post(
        string sessionId,
        string incorrectKey,
        string incorrectCode,
        string deathsToEnemy,
        string levelTime)
    {
        WWWForm form = new WWWForm();
        form.AddField("entry.821792080", sessionId);
        form.AddField("entry.469094003", deathsToEnemy);
        form.AddField("entry.640080099", incorrectKey);
        form.AddField("entry.612356627", incorrectCode);
        form.AddField("entry.47591005", levelTime);

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

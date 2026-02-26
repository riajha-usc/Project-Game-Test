using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class ClueItem : MonoBehaviour
{
    public string clueTitle = "Clue";
    [TextArea(2, 5)]
    public string clueText = "This is a clue.";
    public bool isFinalClue = false;
    public GameObject visualIndicator;
    public ParticleSystem collectEffect;
    public float collectRange = 5f;
    private bool collected = false;
    private bool playerInRange = false;
    private GameObject hintCanvas;
    private Transform playerTransform;
    private Camera mainCam;
    void Update()
    {
        if (collected) return;
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                playerTransform = playerObj.transform;
            else
                return;
        }
        if (mainCam == null)
            mainCam = Camera.main;
        float dist = Vector3.Distance(transform.position, playerTransform.position);
        if (dist <= collectRange)
        {
            if (!playerInRange)
            {
                playerInRange = true;
                ShowHint();
            }
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 50f))
                {
                    if (hit.transform == transform || hit.transform.IsChildOf(transform))
                    {
                        Collect();
                        return;
                    }
                }
                Vector3 screenPos = mainCam.WorldToScreenPoint(transform.position);
                if (screenPos.z > 0) // clue is in front of camera
                {
                    float screenDist = Vector2.Distance(
                        new Vector2(screenPos.x, screenPos.y),
                        new Vector2(Screen.width / 2f, Screen.height / 2f)
                    );
                    if (screenDist < Screen.height * 0.3f)
                    {
                        Collect();
                        return;
                    }
                }
            }
        }
        else
        {
            if (playerInRange)
            {
                playerInRange = false;
                HideHint();
            }
        }
    }
    private void ShowHint()
    {
        if (hintCanvas != null) return;
        GameObject canvasGO = new GameObject("ClueHintCanvas");
        hintCanvas = canvasGO;
        Canvas c = canvasGO.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 90;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        GameObject bgGO = new GameObject("HintBG", typeof(RectTransform));
        bgGO.transform.SetParent(canvasGO.transform, false);
        Image bg = bgGO.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.6f);
        RectTransform bgRect = bgGO.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0.5f, 0);
        bgRect.anchorMax = new Vector2(0.5f, 0);
        bgRect.pivot = new Vector2(0.5f, 0);
        bgRect.anchoredPosition = new Vector2(0, 50);
        bgRect.sizeDelta = new Vector2(400, 50);
        GameObject textGO = new GameObject("HintText", typeof(RectTransform));
        textGO.transform.SetParent(bgGO.transform, false);
        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = "Click to collect clue";
        tmp.fontSize = 24;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(1f, 0.9f, 0.3f);
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
    }
    private void HideHint()
    {
        if (hintCanvas != null)
        {
            Destroy(hintCanvas);
            hintCanvas = null;
        }
    }
    public void Collect()
    {
        if (collected) return;
        collected = true;
        playerInRange = false;
        HideHint();
        ClueManager.Instance.CollectClue(clueTitle, clueText, isFinalClue);
        if (collectEffect != null)
        {
            collectEffect.transform.SetParent(null);
            collectEffect.Play();
            Destroy(collectEffect.gameObject, collectEffect.main.duration + 1f);
        }
        if (visualIndicator != null)
            visualIndicator.SetActive(false);
        gameObject.SetActive(false);
    }
}
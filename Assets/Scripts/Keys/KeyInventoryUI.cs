using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KeyInventoryUI : MonoBehaviour
{
    public static KeyInventoryUI Instance { get; private set; }
    public Transform keyBarParent;
    public Button keyButtonTemplate;
    public GameObject keysText;
    public GameObject openPromptUI;
    List<Button> spawnedButtons = new List<Button>();
    bool doorInRange = false;
    Coroutine flashCoroutine;
    Coroutine vibrateCoroutine;
    Vector2 keyBarBasePos;

    void Awake()
    {
        Instance = this;
        if (openPromptUI != null) openPromptUI.SetActive(false);
    }

    void UpdateOpenPrompt()
    {
        if (openPromptUI == null) return;
        bool hasAllKeys = KeyInventory.Instance != null && KeyInventory.Instance.HasAllKeys();
        openPromptUI.SetActive(doorInRange && hasAllKeys);
    }

    public RectTransform GetFlyTarget()
    {
        if (spawnedButtons.Count > 0)
            return spawnedButtons[spawnedButtons.Count - 1].GetComponent<RectTransform>();
        return keyBarParent != null ? keyBarParent.GetComponent<RectTransform>() : null;
    }

    public void SetDoorInRange(bool inRange)
    {
        doorInRange = inRange;
        UpdateButtonsInteractable();
        UpdateOpenPrompt();
    }

    public void Refresh()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }
        if (keysText != null)
            keysText.transform.SetParent(keyBarParent, true);

        for (int i = 0; i < spawnedButtons.Count; i++)
            Destroy(spawnedButtons[i].gameObject);

        spawnedButtons.Clear();
        if (KeyInventory.Instance == null) return;

        var list = KeyInventory.Instance.keys;

        for (int i = 0; i < list.Count; i++)
        {
            int idx = i;
            var kd = list[i];
            Button b = Instantiate(keyButtonTemplate, keyBarParent);
            b.gameObject.SetActive(true);
            TMP_Text t = b.GetComponentInChildren<TMP_Text>(true);
            if (t != null) t.text = (idx + 1).ToString();
            b.onClick.RemoveAllListeners();
            b.onClick.AddListener(() => TryUnlockWithKey(kd));
            spawnedButtons.Add(b);

            if (idx == list.Count - 1)
                flashCoroutine = StartCoroutine(FlashButton(b));
        }

        if (keysText != null)
        {
            int collected = KeyInventory.Instance != null ? KeyInventory.Instance.keys.Count : 0;
            int total = KeyInventory.Instance != null ? KeyInventory.Instance.requiredKeyCount : 0;
            TMP_Text keysLabel = keysText.GetComponentInChildren<TMP_Text>(true);
            if (keysLabel != null)
                keysLabel.text = $"Collected Keys: <color=#4a90d9><b>{collected}/{total}</b></color>";

            if (spawnedButtons.Count > 0)
            {
                keysText.SetActive(true);
                RectTransform firstRect = spawnedButtons[0].GetComponent<RectTransform>();
                RectTransform keysRect = keysText.GetComponent<RectTransform>();
                if (keysRect != null && firstRect != null)
                {
                    keysRect.SetParent(firstRect, false);
                    keysRect.anchorMin = new Vector2(0, 1);
                    keysRect.anchorMax = new Vector2(0, 1);
                    keysRect.pivot = new Vector2(0, 0);
                    keysRect.anchoredPosition = new Vector2(0, 4f);
                    keysRect.sizeDelta = new Vector2(280f, 24f);
                }
            }
        }

        UpdateButtonsInteractable();
        UpdateOpenPrompt();
    }

    IEnumerator FlashButton(Button btn)
    {
        Image img = btn.GetComponent<Image>();
        Color originalColor = img != null ? img.color : Color.white;

        btn.transform.localScale = Vector3.one * 2.5f;
        float t = 0f;
        while (t < 0.4f)
        {
            t += Time.deltaTime;
            float p = t / 0.4f;
            float bounce = 1f + 1.5f * (1f - p) * Mathf.Abs(Mathf.Sin(p * Mathf.PI * 3f));
            btn.transform.localScale = Vector3.one * bounce;
            yield return null;
        }
        btn.transform.localScale = Vector3.one;

        if (img != null)
        {
            for (int i = 0; i < 4; i++)
            {
                img.color = Color.yellow;
                yield return new WaitForSeconds(0.1f);
                img.color = originalColor;
                yield return new WaitForSeconds(0.1f);
            }
            img.color = originalColor;
        }
        flashCoroutine = null;
    }

    void TryUnlockWithKey(KeyInventory.KeyData kd)
    {
        if (!doorInRange || GameManager.Instance == null) return;

        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        bool correct = false;

        Debug.Log("Key shape: " + kd.shape.ToString());
        Debug.Log("Key color: " + kd.color.ToString());
        Debug.Log("Key spinning: " + kd.spinning);

        Debug.Log("Scene name: " + sceneName);

        if (sceneName == "Level1-Lane1") {
            Debug.Log("Correct shape: " + GameManager.Instance.lane1CorrectShape);
            Debug.Log("Correct color: " + GameManager.Instance.lane1CorrectColor);
            correct = kd.shape.ToString() == GameManager.Instance.lane1CorrectShape
                   && kd.color.ToString() == GameManager.Instance.lane1CorrectColor;
            
        }
        else if (sceneName == "Level1-Lane2") {
            Debug.Log("Correct shape: " + GameManager.Instance.lane2CorrectShape);
            Debug.Log("Correct color: " + GameManager.Instance.lane2CorrectColor);
            Debug.Log("Correct spinning: " + GameManager.Instance.lane2CorrectKeySpinning);
            correct = kd.shape.ToString() == GameManager.Instance.lane2CorrectShape
                   && kd.color.ToString() == GameManager.Instance.lane2CorrectColor && kd.spinning == GameManager.Instance.lane2CorrectKeySpinning;
        }

        if (correct)
        {
            Debug.Log($"Correct key! {kd.color} {kd.shape}");
            if (openPromptUI != null)
                openPromptUI.SetActive(false);
            GameManager.Instance.LoadNextLane();
        }
        else
        {
            Debug.Log($"Wrong key: {kd.color} {kd.shape}");
            GameManager.Instance.RecordIncorrectKey();
            foreach (Button b in spawnedButtons)
                b.interactable = false;
            GameManager.Instance.GameOver();
        }
    }

    void UpdateButtonsInteractable()
    {
        bool all = (KeyInventory.Instance != null) && KeyInventory.Instance.HasAllKeys();
        bool canClick = doorInRange && all;
        for (int i = 0; i < spawnedButtons.Count; i++)
            spawnedButtons[i].interactable = canClick;

        if (canClick && keyBarParent != null)
        {
            if (vibrateCoroutine == null)
                vibrateCoroutine = StartCoroutine(VibrateKeyBar());
        }
        else if (vibrateCoroutine != null)
        {
            StopCoroutine(vibrateCoroutine);
            vibrateCoroutine = null;
            var rect = keyBarParent.GetComponent<RectTransform>();
            if (rect != null) rect.anchoredPosition = keyBarBasePos;
        }
    }

    IEnumerator VibrateKeyBar()
    {
        var rect = keyBarParent.GetComponent<RectTransform>();
        if (rect == null) yield break;

        keyBarBasePos = rect.anchoredPosition;
        float amplitude = 3f;
        float speed = 25f;

        while (true)
        {
            float offset = Mathf.Sin(Time.unscaledTime * speed) * amplitude;
            rect.anchoredPosition = keyBarBasePos + new Vector2(offset, 0f);
            yield return null;
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KeyInventoryUI : MonoBehaviour
{
    public static KeyInventoryUI Instance { get; private set; }
    public Transform keyBarParent;
    public Button keyButtonTemplate;
    List<Button> spawnedButtons = new List<Button>();
    bool doorNearby = false;
    Door doorInRange = null;
    void Awake()
    {
        Instance = this;
    }

    public void SetDoorNearby(Door door, bool nearby)
    {
        doorNearby = nearby;
        doorInRange = nearby ? door : null;
        if(doorInRange != null && doorNearby && GameManager.Instance != null && GameManager.Instance.solvedScenes.Contains(door.currentScene)) {
            doorInRange.TryUnlock(KeyAnswer.shape, KeyAnswer.color);
            return;
        }
        UpdateButtonsInteractable();
    }

    public void Refresh()
    {
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
            if (t != null) t.text = "K" + (idx + 1);
            b.onClick.RemoveAllListeners();
            b.onClick.AddListener(() =>
            {
                Debug.Log("Trying to unlock door with key: " + kd.shape + " " + kd.color);
                Debug.Log("Door nearby: " + doorNearby);
                Debug.Log("Door in range: " + doorInRange != null);
                if (doorNearby && doorInRange != null)
                    doorInRange.TryUnlock(kd.shape, kd.color);
            });
            spawnedButtons.Add(b);
        }
        UpdateButtonsInteractable();
    }

    void UpdateButtonsInteractable()
    {
        bool all = (KeyInventory.Instance != null) && KeyInventory.Instance.HasAllKeys();
        for (int i = 0; i < spawnedButtons.Count; i++)
            spawnedButtons[i].interactable = doorNearby && all;
    }
}
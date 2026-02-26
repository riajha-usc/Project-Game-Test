using System.Collections.Generic;
using UnityEngine;

public class KeyInventory : MonoBehaviour
{
    public static KeyInventory Instance { get; private set; }

    public struct KeyData
    {
        public KeyHeadShape shape;
        public KeyColorType color;
        public KeyData(KeyHeadShape s, KeyColorType c) { shape = s; color = c; }
    }

    public List<KeyData> keys = new List<KeyData>();
    public int requiredKeyCount = 0;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public bool HasAllKeys()
    {
        return requiredKeyCount > 0 && keys.Count >= requiredKeyCount;
    }

    public void AddKey(KeyHeadShape shape, KeyColorType color)
    {
        keys.Add(new KeyData(shape, color));

        if (KeyInventoryUI.Instance != null)
            KeyInventoryUI.Instance.Refresh();
    }
}
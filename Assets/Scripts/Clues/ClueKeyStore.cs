using System.Collections.Generic;
using UnityEngine;

public class ClueKeyStore : MonoBehaviour
{
    public static ClueKeyStore Instance { get; private set; }

    [System.Serializable]
    public class ClueKeyDisplayData
    {
        public string keyID;
        public string colorLabel;
        public Color  accentColor;
        public Sprite keyIcon;
    }

    private readonly List<ClueKeyDisplayData> _displayKeys 
        = new List<ClueKeyDisplayData>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void RegisterDisplayKey(ClueKeyDisplayData data)
    {
        if (data == null) return;
        _displayKeys.Add(data);
    }

    public IReadOnlyList<ClueKeyDisplayData> GetAllKeys() 
        => _displayKeys.AsReadOnly();

    public void Clear() => _displayKeys.Clear();
}
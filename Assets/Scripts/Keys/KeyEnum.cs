using UnityEngine;

public enum KeyHeadShape { Circle, Square, Capsule, Cross }
public enum KeyColorType { Green, Yellow, Blue, White }

public class KeyEnum : MonoBehaviour
{
    public KeyHeadShape headShape;
    public KeyColorType colorType;
}
using UnityEngine;

public enum KeyHeadShape { Circle, Square, Capsule, Cross }
public enum KeyColorType { Red, Blue, Green, Yellow, Purple, Cyan, Orange, White }

public class Key : MonoBehaviour
{
    public KeyHeadShape headShape;
    public KeyColorType colorType;
}
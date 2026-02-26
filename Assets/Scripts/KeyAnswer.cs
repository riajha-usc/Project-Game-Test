public static class KeyAnswer
{
    public static KeyHeadShape shape;
    public static KeyColorType color;
    public static bool hasValue;

    public static void Set(KeyHeadShape s, KeyColorType c)
    {
        shape = s;
        color = c;
        hasValue = true;
    }
}
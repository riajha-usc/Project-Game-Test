using UnityEngine;

public class KeySpinY : MonoBehaviour
{
    public float speed = 60f;
    void Update()
    {
        transform.Rotate(0f, speed * Time.deltaTime, 0f);
    }
}

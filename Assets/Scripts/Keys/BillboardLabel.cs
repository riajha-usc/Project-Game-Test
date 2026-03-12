using UnityEngine;


public class BillboardLabel : MonoBehaviour
{
    void LateUpdate()
    {
        if (Camera.main == null) return;
        transform.LookAt(Camera.main.transform);
        // Flip 180° so the text faces the camera rather than pointing away
        transform.Rotate(0f, 180f, 0f);
    }
}

using UnityEngine;

public class PlayerLegsAnimation : MonoBehaviour
{
    public Transform legLeft;
    public Transform legRight;
    public float swingSpeed = 20f;    // how fast legs swing
    public float swingAmount = 10f;  // how far legs rotate

    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        Vector3 horizontalVelocity = new Vector3(controller.velocity.x, 0, controller.velocity.z);

        if (horizontalVelocity.magnitude > 0.1f)
        {
            // Swing legs in opposite directions using sine wave
            float swing = Mathf.Sin(Time.time * swingSpeed) * swingAmount;

            legLeft.localRotation = Quaternion.Euler(swing, 0, 0);
            legRight.localRotation = Quaternion.Euler(-swing, 0, 0);
        }
        else
        {
            // Reset legs to straight when idle
            legLeft.localRotation = Quaternion.Lerp(legLeft.localRotation, Quaternion.identity, Time.deltaTime * 10f);
            legRight.localRotation = Quaternion.Lerp(legRight.localRotation, Quaternion.identity, Time.deltaTime * 10f);
        }
    }
}
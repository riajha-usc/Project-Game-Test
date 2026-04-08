using UnityEngine;


[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    public Transform player;

    public float distance = 2f;
    public float height = 1.8f;
    public float smoothSpeed = 12f;

    public LayerMask collisionMask;
    public float minDistance = 0.4f;

    [Header("Collision Tuning")]
    public float sphereRadius = 0.25f;
    public float wallBuffer = 0.15f;

    [Header("View Tuning")]
    public float lookAtHeight = 1.5f;
    public float extraBackOffset = 0.8f;

    private Vector3 currentVelocity;
    private Camera cam;
    private float currentDistance;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam != null)
            cam.nearClipPlane = 0.03f;

        currentDistance = distance + extraBackOffset;
    }

    void LateUpdate()
    {
        if (player == null) return;

        Vector3 pivot = player.position + Vector3.up * height;
        Vector3 lookTarget = player.position + Vector3.up * lookAtHeight;

        Vector3 desiredPosition = pivot - player.forward * (distance + extraBackOffset);
        Vector3 direction = (desiredPosition - pivot).normalized;

        float targetDistance = distance + extraBackOffset;

        RaycastHit hit;
        if (Physics.SphereCast(
                pivot,
                sphereRadius,
                direction,
                out hit,
                targetDistance,
                collisionMask,
                QueryTriggerInteraction.Ignore))
        {
            targetDistance = Mathf.Max(minDistance, hit.distance - wallBuffer);
        }

        currentDistance = Mathf.Lerp(
            currentDistance,
            targetDistance,
            smoothSpeed * Time.deltaTime
        );

        Vector3 finalPosition = pivot - player.forward * currentDistance;
        transform.position = finalPosition;

        transform.LookAt(lookTarget);
    }

    public void SetPlayer(Transform newPlayer)
    {
        player = newPlayer;
    }
}
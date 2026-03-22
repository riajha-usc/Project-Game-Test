using System.Collections;
using UnityEngine;

public class DividerSmashController : MonoBehaviour
{
    public Transform leftDivider;
    public Transform rightDivider;

    [Header("Movement")]
    public float moveSpeed = 6f;
    public float smashDistance = 0.5f;

    [Header("Timing")]
    public float smashPause = 0.3f;
    public float cycleDelay = 0.5f;

    [Header("Damage")]
    public float damagePerSecond = 40f;

    private Vector3 leftStart, rightStart;
    private Vector3 leftTarget, rightTarget;

    private bool playerInside = false;
    private Coroutine routine;

    void Start()
    {
        leftStart = leftDivider.position;
        rightStart = rightDivider.position;

        Vector3 center = (leftStart + rightStart) / 2f;

        leftTarget = new Vector3(center.x - smashDistance / 2f, leftStart.y, leftStart.z);
        rightTarget = new Vector3(center.x + smashDistance / 2f, rightStart.y, rightStart.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = true;

        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(SmashRoutine());
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = false;

        if (routine != null)
            StopCoroutine(routine);

        StartCoroutine(MoveDivider(leftDivider, leftDivider.position, leftStart));
        StartCoroutine(MoveDivider(rightDivider, rightDivider.position, rightStart));
    }

    IEnumerator SmashRoutine()
    {
        while (playerInside)
        {
            yield return StartCoroutine(MoveDivider(leftDivider, leftDivider.position, leftTarget));
            yield return StartCoroutine(MoveDivider(rightDivider, rightDivider.position, rightTarget));

            yield return new WaitForSeconds(smashPause);

            yield return StartCoroutine(MoveDivider(leftDivider, leftDivider.position, leftStart));
            yield return StartCoroutine(MoveDivider(rightDivider, rightDivider.position, rightStart));

            yield return new WaitForSeconds(cycleDelay);
        }
    }

    IEnumerator MoveDivider(Transform obj, Vector3 from, Vector3 to)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;
            obj.position = Vector3.Lerp(from, to, t);
            yield return null;
        }
        obj.position = to;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (SafeZoneController.InSafeZone) return;

        PlayerMovement3D player = other.GetComponent<PlayerMovement3D>();
        if (player == null) return;

        float leftX = leftDivider.position.x;
        float rightX = rightDivider.position.x;
        float playerX = other.transform.position.x;

        float minX = Mathf.Min(leftX, rightX);
        float maxX = Mathf.Max(leftX, rightX);

        bool isBetween = playerX > minX && playerX < maxX;

        float distance = Vector3.Distance(leftDivider.position, rightDivider.position);

        if (isBetween && distance < smashDistance + 0.2f)
        {
            player.hp = Mathf.Max(0f, player.hp - damagePerSecond * Time.deltaTime);
        }
    }
}
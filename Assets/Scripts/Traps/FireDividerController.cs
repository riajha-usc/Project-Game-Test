using System.Collections;
using UnityEngine;

public class FireDividerController : MonoBehaviour
{
    public enum MoveAxis { X, Z }

    [Header("Movement")]
    public MoveAxis moveAxis        = MoveAxis.X;
    public float    leftBound       = -2.5f;
    public float    rightBound      =  2.5f;
    public float    moveSpeed       =  1.8f;

    [Header("Damage")]
    public float    damagePerSecond = 30f;

    [Header("Optional Fire Particle")]
    [Tooltip("Assign a Particle System child for fire VFX — it moves with the divider automatically.")]
    public ParticleSystem fireParticle;

    [Header("Pause at Bounds (gives player time to cross)")]
    [Tooltip("Seconds the divider pauses at each end before reversing.")]
    public float pauseAtBound = 1.2f;

    private int   direction     = 1;   // 1 = moving right/+Z, -1 = moving left/-Z
    private bool  isPaused      = false;
    private float damageTimer   = 0f;
    private PlayerMovement3D playerInContact = null;

    void Start()
    {
        // Start at left bound
        SetAxisPosition(leftBound);
    }

    void Update()
    {
        if (isPaused) return;

        float currentPos = GetAxisPosition();
        float newPos = currentPos + direction * moveSpeed * Time.deltaTime;

        if (direction == 1 && newPos >= rightBound)
        {
            newPos = rightBound;
            StartCoroutine(PauseAndReverse());
        }
        else if (direction == -1 && newPos <= leftBound)
        {
            newPos = leftBound;
            StartCoroutine(PauseAndReverse());
        }

        SetAxisPosition(newPos);

        // Continuous burn damage while player is touching
        if (playerInContact != null && !SafeZoneController.InSafeZone)
        {
            damageTimer += Time.deltaTime;
            if (damageTimer >= 0.5f)
            {
                playerInContact.hp = Mathf.Max(0f, playerInContact.hp - (damagePerSecond * 0.5f));
                GameManager.Instance?.RecordDividerHit();
                damageTimer = 0f;
            }
        }
    }

    IEnumerator PauseAndReverse()
    {
        isPaused = true;
        yield return new WaitForSeconds(pauseAtBound);
        direction = -direction;
        isPaused = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInContact = other.GetComponent<PlayerMovement3D>();
        damageTimer = 0f;

        // Immediate hit on first contact
        if (playerInContact != null && !SafeZoneController.InSafeZone)
        {
            playerInContact.hp = Mathf.Max(0f, playerInContact.hp - (damagePerSecond * 0.5f));
            GameManager.Instance?.RecordDividerHit();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInContact = null;
        damageTimer = 0f;
    }

    // --- Axis helpers (so the wall only moves on one axis) ---

    float GetAxisPosition()
    {
        return moveAxis == MoveAxis.X
            ? transform.localPosition.x
            : transform.localPosition.z;
    }

    void SetAxisPosition(float val)
    {
        Vector3 p = transform.localPosition;
        if (moveAxis == MoveAxis.X) p.x = val;
        else                        p.z = val;
        transform.localPosition = p;
    }
}

using System.Collections;
using UnityEngine;

/// <summary>
/// Fire Divider Trap
///
/// Attach directly to any existing Divider object in Lane 1
/// (e.g. Dividers_Left_01, Dividers_Right_03, etc.)
///
/// SETUP STEPS:
/// 1. Select a Divider GameObject in the Lane1 scene hierarchy.
/// 2. On its BoxCollider → tick "Is Trigger" = TRUE
///    (so the player can overlap it and receive fire damage).
/// 3. On its MeshRenderer → assign your Fire Material (the one you will create).
/// 4. Attach this script (FireDividerController) to the same GameObject.
/// 5. Also attach FireUVScroll to the same GameObject so the fire texture animates.
/// 6. Set Inspector values:
///    - moveAxis    : X  (slides left-right across the corridor)
///    - leftBound   : set to the left limit in LOCAL x  (e.g. -2.5)
///    - rightBound  : set to the right limit in LOCAL x  (e.g.  2.5)
///      TIP: the gap between the divider edge and the corridor wall is where
///           the player passes through — size the bounds so the divider
///           never fully blocks the corridor on either side.
///    - moveSpeed   : 1.8  (slow enough to dodge)
///    - pauseAtBound: 1.2  (pause at each end so player has a window to cross)
///    - damagePerSecond: 30
///    - fireParticle: (optional) assign a Particle System child for extra fire FX
/// </summary>
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
            playerInContact.hp = Mathf.Max(0f, playerInContact.hp - (damagePerSecond * 0.5f));
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

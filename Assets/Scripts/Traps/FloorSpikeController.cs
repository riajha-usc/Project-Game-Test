using System.Collections;
using UnityEngine;

/// <summary>
/// Floor Spike Trap
/// 
/// HOW TO SET UP IN UNITY:
/// 1. Create a GameObject (e.g. a cylinder or spike mesh) — this is one spike.
/// 2. Add a BoxCollider (or CapsuleCollider) → set Is Trigger = TRUE.
/// 3. Attach this script.
/// 4. Set the spike's local Y position so it starts BELOW the floor (hidden).
///    e.g. if floor is at Y=0, place spike at Y=-1.5 (retracted).
/// 5. In Inspector set:
///    - retractedY  = -1.5  (below floor, hidden)
///    - extendedY   =  0.5  (above floor, dangerous)
///    - warningY    = -0.3  (just peeking — visual warning phase)
///    - damagePer   = 25    (HP removed on contact)
///    - downTime    = 2.0   (seconds spike stays retracted — player can cross)
///    - warningTime = 0.6   (seconds spike glows/peeks before extending)
///    - upTime      = 1.2   (seconds spike stays fully extended)
///    - riseSpeed   = 8     (how fast spike moves up/down)
/// 6. Optionally assign a warningLight (Point Light on the spike) and
///    a warningMaterial (bright red/orange emissive mat) + normalMaterial.
/// 7. Duplicate the spike GameObject for a whole row of spikes.
///    Stagger their initialDelay values so they don't all pop up at once!
///    e.g. spike1 delay=0, spike2 delay=0.3, spike3 delay=0.6 …
/// </summary>
public class FloorSpikeController : MonoBehaviour
{
    [Header("Positions (local Y)")]
    public float retractedY = -1.5f;
    public float extendedY  = 0.5f;
    public float warningY   = -0.3f;

    [Header("Timing (seconds)")]
    public float downTime    = 2.0f;
    public float warningTime = 0.6f;
    public float upTime      = 1.2f;

    [Header("Movement")]
    public float riseSpeed  = 8f;

    [Header("Damage")]
    public float damagePer  = 25f;

    [Header("Visual Feedback")]
    public Light   warningLight;
    public Material warningMaterial;
    public Material normalMaterial;

    [Header("Stagger")]
    [Tooltip("Delay before this spike starts its first cycle (use to stagger a row of spikes).")]
    public float initialDelay = 0f;

    private enum SpikeState { Retracted, Warning, Extending, Extended, Retracting }
    private SpikeState state = SpikeState.Retracted;

    private Vector3 targetPos;
    private Renderer spikeRenderer;
    private bool playerTouching = false;
    private PlayerMovement3D player;

    void Start()
    {
        spikeRenderer = GetComponentInChildren<Renderer>();
        SetLocalY(retractedY);
        targetPos = LocalY(retractedY);

        if (warningLight != null)
            warningLight.enabled = false;

        StartCoroutine(SpikeLoop());
    }

    void Update()
    {
        // Smooth movement toward target
        Vector3 current = transform.localPosition;
        if (Vector3.Distance(current, targetPos) > 0.005f)
            transform.localPosition = Vector3.MoveTowards(current, targetPos, riseSpeed * Time.deltaTime);
        else
            transform.localPosition = targetPos;
    }

    IEnumerator SpikeLoop()
    {
        yield return new WaitForSeconds(initialDelay);

        while (true)
        {
            // --- RETRACTED (safe phase) ---
            state = SpikeState.Retracted;
            SetVisual(false);
            targetPos = LocalY(retractedY);
            yield return new WaitForSeconds(downTime);

            // --- WARNING (spike peeks, glow starts) ---
            state = SpikeState.Warning;
            SetVisual(true);
            targetPos = LocalY(warningY);
            yield return new WaitForSeconds(warningTime);

            // --- EXTENDING ---
            state = SpikeState.Extending;
            targetPos = LocalY(extendedY);
            // Wait until fully extended
            yield return new WaitUntil(() =>
                Mathf.Abs(transform.localPosition.y - extendedY) < 0.05f);

            // --- EXTENDED (dangerous) ---
            state = SpikeState.Extended;
            yield return new WaitForSeconds(upTime);

            // --- RETRACTING ---
            state = SpikeState.Retracting;
            SetVisual(false);
            targetPos = LocalY(retractedY);
            yield return new WaitUntil(() =>
                Mathf.Abs(transform.localPosition.y - retractedY) < 0.05f);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerTouching = true;
        player = other.GetComponent<PlayerMovement3D>();
        if (player != null && (state == SpikeState.Extended || state == SpikeState.Extending))
            ApplyDamage();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerTouching = false;
        player = null;
    }

    // Called when spike extends while player is standing on it
    void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (state == SpikeState.Extending || state == SpikeState.Extended)
        {
            PlayerMovement3D pm = other.GetComponent<PlayerMovement3D>();
            if (pm != null && !SafeZoneController.InSafeZone)
                ApplyDamage();
        }
    }

    void ApplyDamage()
    {
        if (player == null || SafeZoneController.InSafeZone) return;
        player.hp = Mathf.Max(0f, player.hp - damagePer);
        // Prevent repeated damage in the same spike cycle
        playerTouching = false;
        player = null;
    }

    // --- Helpers ---

    Vector3 LocalY(float y)
    {
        Vector3 p = transform.localPosition;
        p.y = y;
        return p;
    }

    void SetLocalY(float y)
    {
        Vector3 p = transform.localPosition;
        p.y = y;
        transform.localPosition = p;
    }

    void SetVisual(bool warning)
    {
        if (warningLight != null)
            warningLight.enabled = warning;

        if (spikeRenderer != null)
        {
            if (warning && warningMaterial != null)
                spikeRenderer.material = warningMaterial;
            else if (!warning && normalMaterial != null)
                spikeRenderer.material = normalMaterial;
        }
    }
}

using System.Collections;
using UnityEngine;


public class FloorSpikeController : MonoBehaviour
{
    [Header("Positions (local Y)")]
    private float retractedY = -2.5f;
    public float extendedY  = 0.5f;
    private float warningY   = -0.3f;

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

    [Header("Analytics")]
    [Tooltip("Optional. All spikes in one trap should share the same group (defaults to parent).")]
    public Transform spikeGroupRoot;

    private enum SpikeState { Retracted, Warning, Extending, Extended, Retracting }
    private SpikeState state = SpikeState.Retracted;

    private Vector3 targetPos;
    private Renderer spikeRenderer;
    private PlayerMovement3D player;

    void Start()
    {
        spikeRenderer = GetComponentInChildren<Renderer>();
        //SetLocalY(retractedY);
        //targetPos = LocalY(retractedY);
        spikeRenderer.material = warningMaterial;
        //if (warningLight != null)
        //    warningLight.enabled = false;

        StartCoroutine(SpikeLoop());
    }

    //void Update()
    //{
    //    // Smooth movement toward target
    //    Vector3 current = transform.localPosition;
    //    if (Vector3.Distance(current, targetPos) > 0.005f)
    //        transform.localPosition = Vector3.MoveTowards(current, targetPos, riseSpeed * Time.deltaTime);
    //    else
    //        transform.localPosition = targetPos;
    //}

    IEnumerator SpikeLoop()
    {
        yield return new WaitForSeconds(initialDelay);

        while (true)
        {
            if (TrapCombatAgentManager.IsActiveFor("spikes"))
            {
                state = SpikeState.Retracted;
                SetLocalY(retractedY);
                SetVisual(false);
                yield return null;
                continue;
            }

            state = SpikeState.Extended;
            SetLocalY(extendedY);
            SetVisual(true);
            yield return new WaitForSeconds(upTime);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        player = other.GetComponent<PlayerMovement3D>();
        if (player != null && (state == SpikeState.Extended || state == SpikeState.Extending))
            ApplyDamage();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
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
        var group = spikeGroupRoot != null ? spikeGroupRoot : (transform.parent != null ? transform.parent : transform);
        GameManager.Instance?.RecordSpikeHitForGroup(group.gameObject.GetInstanceID());
        // Prevent repeated damage in the same spike cycle
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

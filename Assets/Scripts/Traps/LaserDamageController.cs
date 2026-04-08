using UnityEngine;

public class LaserDamageController : MonoBehaviour
{
    public float damagePerSecond = 20f;

    private float contactTimer = 0f;
    public float activationDelay = 0.15f;
    const float BeamHitAnalyticsInterval = 0.4f;
    float _timeSinceBeamHitLogged;

    private void OnTriggerStay(Collider other)
    {
        BeamController beam = GetComponentInParent<BeamController>();
        if (beam == null || !beam.isActive)
            return;

        if (other.CompareTag("Player") && !SafeZoneController.InSafeZone)
        {
            contactTimer += Time.deltaTime;

            if (contactTimer >= activationDelay)
            {
                PlayerMovement3D player = other.GetComponent<PlayerMovement3D>();
                if (player != null)
                {
                    player.hp = Mathf.Max(0f, player.hp - damagePerSecond * Time.deltaTime);
                    _timeSinceBeamHitLogged += Time.deltaTime;
                    while (_timeSinceBeamHitLogged >= BeamHitAnalyticsInterval)
                    {
                        _timeSinceBeamHitLogged -= BeamHitAnalyticsInterval;
                        GameManager.Instance?.RecordBeamHit();
                    }
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            contactTimer = 0f;
            _timeSinceBeamHitLogged = 0f;
        }
    }
}
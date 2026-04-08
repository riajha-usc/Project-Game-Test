using System;
using UnityEngine;

public class MovDividerController : MonoBehaviour
{
    private Vector3 startPosition;
    private Vector3 endPosition;
    public float movementSpeed = 4.0f;
    public float collisionDamage = 10.0f;

    private Renderer rend;
    private Color baseColor;
    public Color damageColor = new Color(1f, 0.3f, 0.3f);
    public float pulseSpeed = 2f;

    const float DividerHitAnalyticsCooldown = 0.35f;
    float _nextDividerHitAnalyticsUnscaledTime = -999f;

    void TryRecordDividerHitAnalytics()
    {
        if (GameManager.Instance == null) return;
        float now = Time.unscaledTime;
        if (now < _nextDividerHitAnalyticsUnscaledTime) return;
        _nextDividerHitAnalyticsUnscaledTime = now + DividerHitAnalyticsCooldown;
        GameManager.Instance.RecordDividerHit();
    }

    private void Start()
    {
        startPosition = transform.position;
        endPosition = startPosition + new Vector3(4f, 0f, 0f);

        rend = GetComponent<Renderer>();
        baseColor = rend.material.color;
    }
    private void Update()
    {
        float timefactor = Mathf.PingPong(Time.time * movementSpeed, 1f);
        transform.position = Vector3.Lerp(startPosition, endPosition, timefactor);
        float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
        rend.material.color = Color.Lerp(baseColor, damageColor, pulse * 0.5f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (SafeZoneController.InSafeZone) return;
        TryRecordDividerHitAnalytics();
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (SafeZoneController.InSafeZone) return;

        PlayerMovement3D player = other.GetComponent<PlayerMovement3D>();
        if (player == null) return;

        Vector3 pushDirection = (other.transform.position - transform.position).normalized;

        RaycastHit hit;
        float checkDistance = 0.6f;

        if (Physics.Raycast(other.transform.position, pushDirection, out hit, checkDistance))
        {
            if (!hit.collider.CompareTag("Divider") || !hit.collider.CompareTag("Wall"))
                // Akshith bro you have to add tags to walls and dividers
            {
                player.hp = Mathf.Max(0f, player.hp - collisionDamage * Time.deltaTime);
                TryRecordDividerHitAnalytics();
            }
        }
    }
}

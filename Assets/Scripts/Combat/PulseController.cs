using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PulseController : MonoBehaviour
{
    [Header("PulseParameters")]
    public float intensity = 1.0f;
    public float pulsespeed = 0.5f;
    public float fadeSpeed = 5.0f;
    public float maxScale = 7f;
    public float maxDamage = 50.0f;
    public float minDamage = 5.0f;
    public float damageRadius = 15.0f;
    public float CurrentPulse { get; private set; }
    private float chargePercent;

    private Renderer _renderer;

    void Start()
    {
        _renderer = GetComponent<Renderer>();
        if (_renderer == null)
        {
            Debug.LogError("PulseEffectController requires a Renderer component.");
            Destroy(gameObject);
            return;
        }
        StartCoroutine(PulseAndFade());
    }

    public IEnumerator PulseAndFade()
    {
        HashSet<EnemyController> damagedEnemies = new HashSet<EnemyController>();

        float timer = 0f;

        while (timer < 1f)
        {
            timer += Time.deltaTime * pulsespeed;

            float currentRadius = Mathf.Lerp(1f, maxScale, timer);
            transform.localScale = Vector3.one * currentRadius;

            Collider[] hits = Physics.OverlapSphere(transform.position, currentRadius);

            foreach (Collider hit in hits)
            {
                if (hit.CompareTag("Enemy"))
                {
                    EnemyController enemy = hit.GetComponent<EnemyController>();

                    if (enemy != null && !damagedEnemies.Contains(enemy))
                    {
                        float distance = Vector3.Distance(transform.position, enemy.transform.position);
                        float damage = CalculateDamage(distance) * chargePercent;

                        enemy.TakeDamage(damage);
                        damagedEnemies.Add(enemy);

                        Debug.Log("Pulse hit enemy for: " + damage);
                    }
                }
            }

            Color color = _renderer.material.color;
            color.a = Mathf.Lerp(1f, 0f, timer * fadeSpeed);
            _renderer.material.color = color;

            yield return null;
        }

        Destroy(gameObject);
    }
    public void Initialize(float percent)
    {
        chargePercent = Mathf.Clamp01(percent);
    }

    public float CalculateDamage(float distance)
    {
        // Takes distance from the player as input
        // Greater the distance lower the damage
        float damage = Mathf.Lerp(maxDamage, minDamage, distance/damageRadius);
        return damage;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("Enemy Taking DAMAGE!!");
            float distance = Vector3.Distance(transform.position, other.transform.position);

            float scaledDamage = CalculateDamage(distance) * chargePercent;

            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(scaledDamage);
            }
        }
    }

}

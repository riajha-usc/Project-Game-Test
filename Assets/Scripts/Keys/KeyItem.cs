using System.Collections;
using UnityEngine;

public class KeyItem : MonoBehaviour
{
    public KeyHeadShape shape;
    public KeyColorType color;
    public bool spinning;

    bool collected = false;

    void OnTriggerEnter(Collider other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;

        collected = true;

        Collider col = GetComponent<Collider>();
        if (col) col.enabled = false;

        KeySpinY spin = GetComponent<KeySpinY>();
        if (spin) spin.enabled = false;

        SpawnSparkle();
        StartCoroutine(FlyToButton());
    }

    IEnumerator FlyToButton()
    {
        if (KeyInventory.Instance != null)
            KeyInventory.Instance.AddKey(shape, color, spinning);

        RectTransform target = null;
        if (KeyInventoryUI.Instance != null)
            target = KeyInventoryUI.Instance.GetFlyTarget();

        if (target == null || Camera.main == null)
        {
            Destroy(gameObject);
            yield break;
        }

        Vector3 startPos = transform.position;
        Vector3 startScale = transform.localScale;
        float duration = 0.6f;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float p = t / duration;
            float ease = p * p * (3f - 2f * p);

            Vector3 screenTarget = RectTransformUtility.WorldToScreenPoint(null, target.position);
            Vector3 worldTarget = Camera.main.ScreenToWorldPoint(new Vector3(screenTarget.x, screenTarget.y, Camera.main.nearClipPlane + 2f));

            transform.position = Vector3.Lerp(startPos, worldTarget, ease);
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, ease);
            transform.Rotate(0f, 720f * Time.deltaTime, 0f);

            yield return null;
        }

        Destroy(gameObject);
    }

    void SpawnSparkle()
    {
        Color keyColor = ToUnityColor(color);

        GameObject fx = new GameObject("KeyCollectFX");
        fx.transform.position = transform.position;

        ParticleSystem ps = fx.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = ps.main;
        main.duration = 0.4f;
        main.startLifetime = 0.3f;
        main.startSpeed = 1.5f;
        main.startSize = 0.06f;
        main.startColor = keyColor;
        main.maxParticles = 15;
        main.loop = false;
        main.gravityModifier = 0.3f;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, 15)
        });

        var shapeModule = ps.shape;
        shapeModule.shapeType = ParticleSystemShapeType.Sphere;
        shapeModule.radius = 0.15f;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(keyColor, 0f),
                new GradientColorKey(Color.white, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = grad;

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, 0f);

        ParticleSystemRenderer psRenderer = fx.GetComponent<ParticleSystemRenderer>();
        Renderer keyRenderer = GetComponentInChildren<Renderer>();
        if (keyRenderer != null)
            psRenderer.material = new Material(keyRenderer.sharedMaterial);
        else
            psRenderer.material = new Material(Shader.Find("Particles/Standard Unlit"));

        ps.Play();
        Destroy(fx, 1f);
    }

    Color ToUnityColor(KeyColorType c)
    {
        switch (c)
        {
            case KeyColorType.Green:  return new Color(0.30f, 0.69f, 0.31f);
            case KeyColorType.Yellow: return new Color(1f, 0.84f, 0f);
            case KeyColorType.Blue:   return new Color(0.0f, 0.75f, 1.0f);
            case KeyColorType.White:  return new Color(0.93f, 0.93f, 0.96f);
            default:                  return Color.white;
        }
    }
}

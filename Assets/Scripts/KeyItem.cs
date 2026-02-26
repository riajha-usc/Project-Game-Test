using UnityEngine;

public class KeyItem : MonoBehaviour
{
    public KeyHeadShape shape;
    public KeyColorType color;

    bool collected = false;

    void OnTriggerEnter(Collider other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;

        collected = true;
        SpawnSparkle();
        if (KeyInventory.Instance != null)
            KeyInventory.Instance.AddKey(shape, color);
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
        main.duration = 0.6f;
        main.startLifetime = 0.5f;
        main.startSpeed = 3f;
        main.startSize = 0.15f;
        main.startColor = keyColor;
        main.maxParticles = 30;
        main.loop = false;
        main.gravityModifier = 0.5f;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, 30)
        });

        var shapeModule = ps.shape;
        shapeModule.shapeType = ParticleSystemShapeType.Sphere;
        shapeModule.radius = 0.3f;

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

        var renderer = fx.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));

        ps.Play();
        Destroy(fx, 1f);
    }

    Color ToUnityColor(KeyColorType c)
    {
        switch (c)
        {
            case KeyColorType.Red:    return Color.red;
            case KeyColorType.Blue:   return Color.blue;
            case KeyColorType.Green:  return Color.green;
            case KeyColorType.Yellow: return Color.yellow;
            case KeyColorType.Purple: return new Color(0.6f, 0.2f, 0.8f);
            case KeyColorType.Cyan:   return Color.cyan;
            case KeyColorType.Orange: return new Color(1f, 0.5f, 0.1f);
            case KeyColorType.White:  return Color.white;
            default:                  return Color.white;
        }
    }
}
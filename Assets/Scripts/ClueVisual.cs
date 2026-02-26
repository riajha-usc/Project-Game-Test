using UnityEngine;

public class ClueVisual : MonoBehaviour
{
    private ParticleSystem sparklePS;
    private Light glowLight;
    private Material clueMaterial;
    private float pulseSpeed = 2f;
    private float minIntensity = 1.5f;
    private float maxIntensity = 3f;
    private float rotateSpeed = 30f;

    void Start()
    {
        SetupMaterial();
        SetupGlow();
        SetupSparkles();
    }

    void Update()
    {
        float pulse = Mathf.Lerp(minIntensity, maxIntensity, (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f);

        if (glowLight != null)
            glowLight.intensity = pulse;

        if (clueMaterial != null)
        {
            Color emissionColor = new Color(1f, 0.9f, 0.2f) * pulse;
            clueMaterial.SetColor("_EmissionColor", emissionColor);
        }

        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime, Space.World);
    }

    void SetupMaterial()
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend == null) return;

        clueMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        clueMaterial.SetColor("_BaseColor", new Color(1f, 0.85f, 0.1f, 1f));
        clueMaterial.SetColor("_EmissionColor", new Color(1f, 0.9f, 0.2f) * 2f);
        clueMaterial.EnableKeyword("_EMISSION");
        clueMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        rend.material = clueMaterial;
    }

    void SetupGlow()
    {
        GameObject lightObj = new GameObject("ClueGlow");
        lightObj.transform.SetParent(transform, false);
        lightObj.transform.localPosition = Vector3.zero;

        glowLight = lightObj.AddComponent<Light>();
        glowLight.type = LightType.Point;
        glowLight.color = new Color(1f, 0.9f, 0.3f);
        glowLight.intensity = 2f;
        glowLight.range = 3f;
        glowLight.shadows = LightShadows.None;
    }

    void SetupSparkles()
    {
        GameObject psObj = new GameObject("Sparkles");
        psObj.transform.SetParent(transform, false);
        psObj.transform.localPosition = Vector3.zero;

        sparklePS = psObj.AddComponent<ParticleSystem>();

        var main = sparklePS.main;
        main.startLifetime = 0.8f;
        main.startSpeed = 0.5f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.06f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 1f, 0.5f, 1f),
            new Color(1f, 0.85f, 0.1f, 1f)
        );
        main.maxParticles = 30;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = -0.2f;

        var emission = sparklePS.emission;
        emission.rateOverTime = 15f;

        var shape = sparklePS.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.5f;

        var sizeOverLifetime = sparklePS.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1f);
        sizeCurve.AddKey(1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        var colorOverLifetime = sparklePS.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(1f, 1f, 0.5f), 0f),
                new GradientColorKey(new Color(1f, 0.85f, 0.1f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(grad);

        Renderer psRenderer = psObj.GetComponent<ParticleSystemRenderer>();
        psRenderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        psRenderer.material.SetColor("_Color", new Color(1f, 0.95f, 0.4f, 1f));
        psRenderer.material.SetFloat("_Mode", 1f);
    }
}
using UnityEngine;

/// <summary>
/// Scrolls a material's UV texture upward to simulate fire movement.
///
/// HOW TO USE:
/// 1. Attach this script to the wall mesh GameObject that has the fire material.
/// 2. Set scrollSpeedY to ~0.5–1.0 (controls how fast fire "rises").
/// 3. Set scrollSpeedX to a small value like 0.05 for subtle horizontal shimmer.
/// 4. materialIndex = 0 if the object has only one material.
///
/// NOTE: This modifies a unique material instance at runtime so it won't affect
///       other objects using the same material.
/// </summary>
[RequireComponent(typeof(Renderer))]
public class FireUVScroll : MonoBehaviour
{
    [Header("UV Scroll Speed")]
    public float scrollSpeedY = 0.8f;
    public float scrollSpeedX = 0.05f;

    [Header("Material Slot")]
    public int materialIndex = 0;

    private Material mat;

    void Start()
    {
        Renderer r = GetComponent<Renderer>();
        // Create a unique instance so we don't affect shared materials
        mat = r.materials[materialIndex];
    }

    void Update()
    {
        if (mat == null) return;
        float offsetX = (scrollSpeedX * Time.time) % 1f;
        float offsetY = (scrollSpeedY * Time.time) % 1f;
        mat.mainTextureOffset = new Vector2(offsetX, offsetY);
    }
}

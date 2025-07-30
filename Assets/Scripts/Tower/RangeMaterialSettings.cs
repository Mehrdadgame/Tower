using UnityEngine;

/// <summary>
/// Configuration for creating custom range indicator materials with optional transparency and emission effects.
/// </summary>
[System.Serializable]
public class RangeMaterialSettings
{
    [Header("Material Settings")]
    public Color baseColor = new Color(0f, 1f, 0f, 0.3f);       // Default color for normal range indicators
    public Color upgradeColor = new Color(0f, 0f, 1f, 0.3f);    // Color used for upgraded towers
    public Color selectedColor = new Color(1f, 1f, 0f, 0.4f);   // Color shown when tower is selected

    [Header("Visual Effects")]
    public bool useTransparency = true;                        // Enables alpha blending (semi-transparency)
    public bool useEmission = false;                           // Enables glow/emission effect
    public Color emissionColor = Color.green;                  // Color of the emission glow
    public float emissionIntensity = 0.5f;                     // Multiplier for how strong the glow appears

    /// <summary>
    /// Creates and returns a new material instance using the settings defined above.
    /// </summary>
    public Material CreateRangeMaterial()
    {
        // Create a new material using Unity's Standard Shader
        Material material = new Material(Shader.Find("Standard"));

        // Apply the base color to the material
        material.color = baseColor;

        // If transparency is enabled, set material render mode to Transparent
        if (useTransparency)
        {
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0); // Don't write to depth buffer
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000; // Set render queue after geometry
        }

        // If emission is enabled, make the material glow
        if (useEmission)
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", emissionColor * emissionIntensity);
        }

        return material;
    }
}

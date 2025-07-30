using UnityEngine;

/// <summary>
/// Controls the visual appearance of a range indicator, including pulsing, rotation, and color.
/// </summary>
public class RangeIndicatorController : MonoBehaviour
{
    [Header("Range Indicator Settings")]
    public Material rangeMaterial;                         // Base material for the range indicator
    public Color rangeColor = new Color(0f, 1f, 0f, 0.3f);  // Default color with transparency
    public float pulseSpeed = 2f;                          // Speed of the pulsing animation
    public bool enablePulseEffect = true;                 // Toggle for enabling/disabling the pulse effect
    public bool enableRotation = false;                   // Toggle for rotating the indicator
    public float rotationSpeed = 10f;                     // Speed of rotation (degrees per second)

    private Renderer rangeRenderer;                       // Renderer component of the indicator
    private Color originalColor;                          // Stores the base color for pulsing
    private float pulseTimer;                             // Timer used for calculating pulse effect

    /// <summary>
    /// Initializes the material and color at the start.
    /// </summary>
    private void Start()
    {
        rangeRenderer = GetComponent<Renderer>();
        if (rangeRenderer != null)
        {
            SetupRangeMaterial();
        }
    }

    /// <summary>
    /// Sets up a unique material instance and applies the initial color.
    /// </summary>
    private void SetupRangeMaterial()
    {
        if (rangeMaterial != null)
        {
            // Create a new material instance to prevent editing the shared material
            Material materialInstance = new Material(rangeMaterial);
            rangeRenderer.material = materialInstance;
        }

        originalColor = rangeColor;
        rangeRenderer.material.color = originalColor;
    }

    /// <summary>
    /// Updates the pulse effect and rotation each frame if enabled.
    /// </summary>
    private void Update()
    {
        if (enablePulseEffect)
        {
            UpdatePulseEffect();
        }

        if (enableRotation)
        {
            // Rotate around Y-axis
            transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        }
    }

    /// <summary>
    /// Handles the alpha pulsing effect by modifying the material's color alpha.
    /// </summary>
    private void UpdatePulseEffect()
    {
        pulseTimer += Time.deltaTime * pulseSpeed;

        // Sin wave alpha from 0.1 to 0.4
        float alpha = Mathf.Lerp(0.1f, 0.4f, (Mathf.Sin(pulseTimer) + 1f) / 2f);

        Color newColor = originalColor;
        newColor.a = alpha;

        rangeRenderer.material.color = newColor;
    }

    /// <summary>
    /// Sets the color of the range indicator (e.g., for different tower types).
    /// </summary>
    public void SetRangeColor(Color color)
    {
        rangeColor = color;
        originalColor = color;

        if (rangeRenderer != null)
        {
            rangeRenderer.material.color = color;
        }
    }

    /// <summary>
    /// Scales the range indicator based on the given radius (assuming a base size of 10 units).
    /// </summary>
    public void SetRangeSize(float range)
    {
        // Multiply by 2 for diameter, divide by 10 to match scale
        float scale = (range * 2f) / 10f;
        transform.localScale = new Vector3(scale, 1f, scale);
    }
}

using UnityEngine;

/// <summary>
/// Responsible for visualizing the tower's range using materials and indicator objects.
/// Handles customization for base, upgraded, and selected states.
/// </summary>
public class TowerRangeVisualizer : MonoBehaviour
{
    [Header("Range Visualization")]
    public RangeMaterialSettings materialSettings;       // Settings used to generate range materials
    public GameObject customRangeIndicatorPrefab;        // Optional custom prefab for visual range

    private Tower tower;                                 // Reference to the associated tower
    private GameObject rangeIndicator;                   // The actual visual object representing the range
    private RangeIndicatorController rangeController;    // Handles effects like pulsing, rotation, etc.

    private Material baseMaterial;                       // Material for normal tower
    private Material upgradeMaterial;                    // Material for upgraded tower
    private Material selectedMaterial;                   // Material for selected tower

    private void Awake()
    {
        tower = GetComponent<Tower>();
        CreateRangeMaterials();
    }

    /// <summary>
    /// Generates separate materials for base, upgraded, and selected visual states.
    /// </summary>
    private void CreateRangeMaterials()
    {
        baseMaterial = materialSettings.CreateRangeMaterial();

        upgradeMaterial = materialSettings.CreateRangeMaterial();
        upgradeMaterial.color = materialSettings.upgradeColor;

        selectedMaterial = materialSettings.CreateRangeMaterial();
        selectedMaterial.color = materialSettings.selectedColor;
    }

    /// <summary>
    /// Creates the range indicator object, using a custom prefab or a default plane.
    /// </summary>
    public void CreateRangeIndicator()
    {
        if (customRangeIndicatorPrefab != null)
        {
            rangeIndicator = Instantiate(customRangeIndicatorPrefab, transform.position, Quaternion.identity, transform);
        }
        else
        {
            CreateDefaultRangeIndicator();
        }

        rangeController = rangeIndicator.GetComponent<RangeIndicatorController>();
        if (rangeController == null)
        {
            rangeController = rangeIndicator.AddComponent<RangeIndicatorController>();
        }

        rangeController.rangeMaterial = baseMaterial;
        rangeController.SetRangeSize(tower.GetRange());

        rangeIndicator.SetActive(false);
    }

    /// <summary>
    /// Creates a default visual indicator using Unity's primitive plane.
    /// </summary>
    private void CreateDefaultRangeIndicator()
    {
        GameObject rangePlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        rangeIndicator = rangePlane;
        rangeIndicator.name = "RangeIndicator";
        rangeIndicator.transform.SetParent(transform);
        rangeIndicator.transform.localPosition = Vector3.zero;
        rangeIndicator.transform.localRotation = Quaternion.identity;

        // Remove collider so it's not interactive
        Collider planeCollider = rangeIndicator.GetComponent<Collider>();
        if (planeCollider != null)
        {
            Destroy(planeCollider);
        }

        // Set to ignore raycast to avoid blocking interactions
        rangeIndicator.layer = LayerMask.NameToLayer("Ignore Raycast");
    }

    /// <summary>
    /// Enables and shows the range indicator with appropriate material based on tower state.
    /// </summary>
    public void ShowRange(bool isSelected = false, bool isUpgraded = false)
    {
        if (rangeIndicator != null)
        {
            rangeIndicator.SetActive(true);

            Material materialToUse = baseMaterial;
            if (isSelected)
                materialToUse = selectedMaterial;
            else if (isUpgraded)
                materialToUse = upgradeMaterial;

            rangeController.rangeMaterial = materialToUse;
            rangeController.SetRangeSize(tower.GetRange());
        }
    }

    /// <summary>
    /// Hides the range indicator.
    /// </summary>
    public void HideRange()
    {
        if (rangeIndicator != null)
        {
            rangeIndicator.SetActive(false);
        }
    }

    /// <summary>
    /// Updates the range size if the tower's range value has changed.
    /// </summary>
    public void UpdateRangeSize()
    {
        if (rangeController != null)
        {
            rangeController.SetRangeSize(tower.GetRange());
        }
    }
}

using UnityEngine;
using System;

/* The `public class TowerManager : MonoBehaviour` is a C# class that inherits from the `MonoBehaviour`
class provided by Unity. This class is responsible for managing towers in a tower defense game. It
includes functionality for selecting, placing, upgrading, and deselecting towers within the game
environment. The class also handles tower placement previews, tower selection based on user input,
and tower placement validation. Additionally, it contains methods for creating actual towers in the
game world, handling tower upgrades, and managing tower selection events. */
public class TowerManager : MonoBehaviour
{
    [Header("Tower Settings")]
    public TowerData[] availableTowers;
    public LayerMask placementLayer = 1;
    public Material previewMaterial;

    private TowerData selectedTowerData;
    private Tower selectedTower;
    private Camera mainCamera;
    private GameObject previewTower;

    public System.Action<Tower> OnTowerSelected;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        HandleTowerPlacement();
        HandleTowerSelection();
    }

    /// <summary>
    /// The HandleTowerPlacement function checks for tower selection, updates preview tower, and allows
    /// for tower placement or cancellation based on mouse input.
    /// </summary>
    private void HandleTowerPlacement()
    {
        if (selectedTowerData != null)
        {
            UpdatePreviewTower();

            if (Input.GetMouseButtonDown(0))
            {
                PlaceTower();
            }

            if (Input.GetMouseButtonDown(1))
            {
                CancelPlacement();
            }
        }
    }

    /// <summary>
    /// The HandleTowerSelection function checks for tower selection based on mouse input and allows
    /// deselection with the Escape key.
    /// </summary>
    private void HandleTowerSelection()
    {
        if (selectedTowerData == null && Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Tower tower = hit.collider.GetComponent<Tower>();
                if (tower != null)
                {
                    SelectTower(tower);
                }
                else
                {
                    // Clicked on empty space, deselect tower
                    DeselectTower();
                }
            }
        }

        // Deselect with Escape key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            DeselectTower();
        }
    }

    /// <summary>
    /// The function UpdatePreviewTower updates the position of a preview tower based on the mouse cursor's
    /// position in the game world.
    /// </summary>
    private void UpdatePreviewTower()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, placementLayer))
        {
            Vector3 position = hit.point;

            if (previewTower == null)
            {
                previewTower = Instantiate(selectedTowerData.towerPrefab, position, Quaternion.identity);
                MakePreview(previewTower);
            }
            else
            {
                previewTower.transform.position = position;
            }
        }
    }

    /// <summary>
    /// The MakePreview function sets a preview material on all renderers, disables colliders, and
    /// disables a Tower component on a given GameObject.
    /// </summary>
    /// <param name="GameObject">A GameObject is a fundamental object in Unity that represents entities
    /// in the scene. It can hold components such as Renderers, Colliders, Scripts, etc., and is used to
    /// create interactive objects in the game world.</param>
    private void MakePreview(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            renderer.material = previewMaterial;
        }

        Collider[] colliders = obj.GetComponentsInChildren<Collider>();
        foreach (var collider in colliders)
        {
            collider.enabled = false;
        }

        Tower tower = obj.GetComponent<Tower>();
        if (tower != null)
        {
            tower.enabled = false;
        }
    }

    /// <summary>
    /// The PlaceTower function creates a tower object at a specified position if conditions are met.
    /// </summary>
    private void PlaceTower()
    {
        if (previewTower != null && CanPlaceTower(previewTower.transform.position))
        {
            if (GameManager.Instance.SpendMoney(selectedTowerData.cost))
            {
                // Create actual tower
                GameObject towerObj = Instantiate(selectedTowerData.towerPrefab, previewTower.transform.position, Quaternion.identity);
                Tower tower = towerObj.GetComponent<Tower>();
                if (tower != null)
                {
                    tower.Initialize(selectedTowerData);
                }

                DestroyPreview();
                selectedTowerData = null;
            }
        }
    }

    /// <summary>
    /// The function `CanPlaceTower` checks if a tower can be placed at a given position by ensuring it
    /// does not overlap with other towers or restricted areas and is on valid ground.
    /// </summary>
    /// <param name="Vector3">A Vector3 is a data structure in Unity that represents a point or direction
    /// in 3D space. It consists of three float values (x, y, z) that can be used to store positions,
    /// rotations, or scales in a 3D environment. In the context of the `Can</param>
    /// <returns>
    /// The CanPlaceTower method returns a boolean value. It returns true if the tower can be placed at
    /// the specified position without overlapping with other towers or being placed on restricted areas,
    /// and if the ground is valid for tower placement. It returns false if any of these conditions are
    /// not met.
    /// </returns>
    private bool CanPlaceTower(Vector3 position)
    {
        // Check for overlapping towers using a small radius
        Collider[] overlapping = Physics.OverlapSphere(position, 1.5f);
        foreach (var collider in overlapping)
        {
            // Check for other towers
            if (collider.CompareTag("Tower"))
            {
                return false;
            }

            // Check for path/restricted areas
            if (collider.CompareTag("Path") || collider.CompareTag("SpawnPoint") || collider.CompareTag("EndPoint"))
            {
                return false;
            }
        }

        // Additional check using raycast for ground
        if (Physics.Raycast(position + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f, placementLayer))
        {
            // Make sure we're placing on valid ground
            return true;
        }

        return false;
    }

    /// <summary>
    /// The CancelPlacement function destroys the preview of a tower and sets the selectedTowerData to
    /// null.
    /// </summary>
    private void CancelPlacement()
    {
        DestroyPreview();
        selectedTowerData = null;
    }

    /// <summary>
    /// The DestroyPreview function checks if a previewTower object exists and destroys it if it does.
    /// </summary>
    private void DestroyPreview()
    {
        if (previewTower != null)
        {
            Destroy(previewTower);
        }
    }

    /// <summary>
    /// The SelectTower function in C# selects a tower, hides the range indicator of the previously
    /// selected tower, shows the range indicator of the newly selected tower, and invokes an event with
    /// the selected tower as a parameter.
    /// </summary>
    /// <param name="Tower">The `SelectTower` method takes a `Tower` object as a parameter. This method
    /// is responsible for selecting a tower, hiding the range indicator of the previously selected
    /// tower (if any), showing the range indicator of the newly selected tower, and invoking the
    /// `OnTowerSelected` event with the selected</param>
    /// <summary>
    /// The SelectTower function in C# selects a tower, hides the range indicator of the previously
    /// selected tower, shows the range indicator of the newly selected tower, and invokes an event with
    /// the selected tower as a parameter.
    /// </summary>
    /// <param name="Tower">The `Tower` parameter in the `SelectTower` method represents a tower object
    /// that the player has selected in the game. The method first hides the range indicator of the
    /// previously selected tower (if any), then sets the `selectedTower` to the new tower that was
    /// passed as a parameter. Finally</param>
    private void SelectTower(Tower tower)
    {
        // Hide previous tower's range indicator
        if (selectedTower != null)
        {
            selectedTower.HideRangeIndicator();
        }

        selectedTower = tower;

        // Show new tower's range indicator
        if (selectedTower != null)
        {
            selectedTower.ShowRangeIndicator();
        }

        OnTowerSelected?.Invoke(tower);
    }

    /// <summary>
    /// The function DeselectTower in C# deselects a tower by hiding its range indicator and setting the
    /// selectedTower variable to null.
    /// </summary>
    public void DeselectTower()
    {
        if (selectedTower != null)
        {
            selectedTower.HideRangeIndicator();
            selectedTower = null;
        }

        OnTowerSelected?.Invoke(null);
    }

    /// <summary>
    /// The function `SetSelectedTowerData` sets the selected tower data and destroys any existing
    /// preview.
    /// </summary>
    /// <param name="TowerData">The TowerData parameter is a data structure that contains information
    /// about a tower in the game, such as its type, cost, damage, range, and other attributes.</param>
    public void SetSelectedTowerData(TowerData towerData)
    {
        selectedTowerData = towerData;
        DestroyPreview();
    }

    /// <summary>
    /// The function `UpgradeSelectedTower` checks if a tower is selected and can be upgraded, then
    /// deducts the upgrade cost from the player's money and upgrades the tower if possible.
    /// </summary>
    public void UpgradeSelectedTower()
    {
        if (selectedTower != null && selectedTower.CanUpgrade())
        {
            if (GameManager.Instance.SpendMoney(selectedTower.GetUpgradeCost()))
            {
                selectedTower.Upgrade();
            }
        }
    }

    /// <summary>
    /// The function GetSelectedTower returns the selected tower.
    /// </summary>
    public Tower GetSelectedTower() => selectedTower;
}

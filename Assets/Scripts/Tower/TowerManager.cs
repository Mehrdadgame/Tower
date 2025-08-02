using UnityEngine;
using System;
using UnityEngine.UI;

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
    public Image RightClickIcon;

    public Action<Tower> OnTowerSelected;

    // Constants
    private const float PLACEMENT_CHECK_RADIUS = 1.5f;
    private const float RAYCAST_DISTANCE = 20f;
    private const float PREVIEW_HEIGHT_OFFSET = 10f;

    private void Start()
    {
        mainCamera = Camera.main;
        if (RightClickIcon != null)
        {
            RightClickIcon.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        HandleTowerPlacement();
        HandleTowerSelection();
    }

    #region Tower Placement

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
    /// The UpdatePreviewTower function updates the position of a preview tower based on the mouse input and
    /// checks for valid placement using raycasting.
    /// </summary>
    private void UpdatePreviewTower()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, placementLayer))
        {
            Vector3 position = hit.point;

            if (previewTower == null)
            {
                CreatePreviewTower(position);
            }
            else
            {
                previewTower.transform.position = position;
            }

            // Update preview visual based on placement validity
            UpdatePreviewVisual(position);
        }
    }

    /// <summary>
    /// The function `CreatePreviewTower` instantiates a preview tower object at a specified position using
    /// the selected tower data.
    /// </summary>
    /// <param name="Vector3">A Vector3 is a data structure in Unity that represents a point or direction in
    /// 3D space. It consists of three float values (x, y, z) that can be used to store positions,
    /// rotations, or scales in a 3D environment. In this context, the Vector3</param>
    /// <returns>
    /// If the `selectedTowerData` is null or the `towerPrefab` property of `selectedTowerData` is null,
    /// then the method will return and not execute the rest of the code.
    /// </returns>
    private void CreatePreviewTower(Vector3 position)
    {
        if (selectedTowerData?.towerPrefab == null) return;

        previewTower = Instantiate(selectedTowerData.towerPrefab, position, Quaternion.identity);
        MakePreview(previewTower);
    }

    /// <summary>
    /// The MakePreview function sets a preview material, disables colliders, disables a tower script, and
    /// sets the layer to ignore raycast for a given GameObject.
    /// </summary>
    /// <param name="GameObject">A GameObject is a fundamental object in Unity that represents entities in
    /// the scene, such as characters, props, or scenery. It can hold components like renderers, colliders,
    /// scripts, and more to define its behavior and appearance within the game world. In the provided code
    /// snippet, the `MakePreview</param>
    private void MakePreview(GameObject obj)
    {
        // Set preview material
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            if (previewMaterial != null)
            {
                renderer.material = previewMaterial;
            }
        }

        // Disable colliders
        Collider[] colliders = obj.GetComponentsInChildren<Collider>();
        foreach (var collider in colliders)
        {
            collider.enabled = false;
        }

        // Disable tower script
        Tower tower = obj.GetComponent<Tower>();
        if (tower != null)
        {
            tower.enabled = false;
        }

        // Set layer to ignore raycast
        SetLayerRecursively(obj, LayerMask.NameToLayer("Ignore Raycast"));
    }
    /// <summary>
    /// The function SetLayerRecursively sets the layer of a GameObject and all of its children recursively.
    /// </summary>
    /// <param name="GameObject">A GameObject is an object in Unity that represents characters, props,
    /// scenery, cameras, waypoints, and more. It is the basic element in the Unity scene hierarchy.</param>
    /// <param name="layer">The `layer` parameter in the `SetLayerRecursively` method is an integer value
    /// that represents the layer to which the GameObject and its children will be set. Layers in Unity are
    /// used to selectively render objects in the scene based on the camera's culling mask settings. By
    /// setting the layer of</param>

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
    /// <summary>
    /// The UpdatePreviewVisual function changes the color of a preview tower based on its validity for
    /// placement.
    /// </summary>
    /// <param name="Vector3">A Vector3 is a data structure in Unity that represents a point or direction in
    /// 3D space. It consists of three float values (x, y, z) that can be used to store position, rotation,
    /// or scale information. In this context, the Vector3 position parameter likely represents the</param>
    /// <returns>
    /// If the `previewTower` is `null`, the method `UpdatePreviewVisual` will return early and exit the
    /// method without performing any further operations.
    /// </returns>

    private void UpdatePreviewVisual(Vector3 position)
    {
        if (previewTower == null) return;

        bool canPlace = CanPlaceTower(position);

        // Change preview color based on validity
        Renderer[] renderers = previewTower.GetComponentsInChildren<Renderer>();
        Color previewColor = canPlace ? Color.green : Color.red;
        previewColor.a = 0.7f;

        foreach (var renderer in renderers)
        {
            renderer.material.color = previewColor;
        }
    }
    /// <summary>
    /// The function `PlaceTower` checks if a tower can be placed at a given position, spends money to place
    /// the tower if possible, and logs appropriate messages.
    /// </summary>
    /// <returns>
    /// If the `previewTower` is `null`, the method `PlaceTower()` will return early and not execute the
    /// rest of the code within the method.
    /// </returns>

    private void PlaceTower()
    {
        if (previewTower == null) return;

        Vector3 position = previewTower.transform.position;

        if (CanPlaceTower(position))
        {
            if (GameManager.Instance.SpendMoney(selectedTowerData.cost))
            {
                // Create actual tower
                GameObject towerObj = Instantiate(selectedTowerData.towerPrefab, position, Quaternion.identity);
                Tower tower = towerObj.GetComponent<Tower>();
                if (tower != null)
                {
                    tower.Initialize(selectedTowerData);
                }

                DestroyPreview();
                selectedTowerData = null;

                Debug.Log($"Tower placed successfully at {position}");
            }
            else
            {
                Debug.Log("Not enough money to place tower!");
            }
        }
        else
        {
            Debug.Log("Cannot place tower at this location!");
        }
    }

    private bool CanPlaceTower(Vector3 position)
    {
        // Check for overlapping objects
        Collider[] overlapping = Physics.OverlapSphere(position, PLACEMENT_CHECK_RADIUS);

        foreach (var collider in overlapping)
        {
            // Check for other towers
            if (collider.CompareTag("Tower"))
            {
                return false;
            }

            // Check for restricted areas
            if (collider.CompareTag("Path") ||
                collider.CompareTag("SpawnPoint") ||
                collider.CompareTag("EndPoint") ||
                collider.CompareTag("Enemy"))
            {
                return false;
            }
        }

        // Check if on valid ground
        if (Physics.Raycast(position + Vector3.up * PREVIEW_HEIGHT_OFFSET, Vector3.down, out RaycastHit hit, RAYCAST_DISTANCE, placementLayer))
        {
            return true;
        }

        return false;
    }

    private void CancelPlacement()
    {
        DestroyPreview();
        selectedTowerData = null;

        if (RightClickIcon != null)
        {
            RightClickIcon.gameObject.SetActive(false);
        }
    }

    private void DestroyPreview()
    {
        if (previewTower != null)
        {
            Destroy(previewTower);
            previewTower = null;
        }

        if (RightClickIcon != null)
        {
            RightClickIcon.gameObject.SetActive(false);
        }
    }

    #endregion

    #region Tower Selection

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
                    DeselectTower();
                }
            }
        }

        // Deselect with Escape key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            DeselectTower();
            CancelPlacement();
        }
    }

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

    public void DeselectTower()
    {
        if (selectedTower != null)
        {
            selectedTower.HideRangeIndicator();
            selectedTower = null;
        }

        if (RightClickIcon != null)
        {
            RightClickIcon.gameObject.SetActive(false);
        }

        OnTowerSelected?.Invoke(null);
    }

    #endregion

    #region Public Interface

    public void SetSelectedTowerData(TowerData towerData)
    {
        if (RightClickIcon != null)
        {
            RightClickIcon.gameObject.SetActive(true);
        }

        selectedTowerData = towerData;
        DestroyPreview();
    }

    public void UpgradeSelectedTower()
    {
        if (selectedTower != null && selectedTower.CanUpgrade())
        {
            int upgradeCost = selectedTower.GetUpgradeCost();
            if (GameManager.Instance.SpendMoney(upgradeCost))
            {
                selectedTower.Upgrade();
                Debug.Log($"Tower upgraded for {upgradeCost} money");
            }
            else
            {
                Debug.Log("Not enough money to upgrade tower!");
            }
        }
    }

    public Tower GetSelectedTower() => selectedTower;

    #endregion
}
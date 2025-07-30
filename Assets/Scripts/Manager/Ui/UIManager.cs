using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the game's UI, including money, health, tower information, and game over panel.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI moneyText;             // UI text to show player's money
    public TextMeshProUGUI healthText;            // UI text to show player's health
    public Transform towerButtonsParent;          // Parent transform to hold dynamically created tower buttons
    public GameObject towerButtonPrefab;          // Prefab used to create tower selection buttons

    [Header("Tower Info Panel")]
    public GameObject towerInfoPanel;             // Panel that shows info about the selected tower
    public TextMeshProUGUI towerNameText;         // Text for displaying the tower's name or level
    public Slider damageSlider;                   // Slider that visually shows tower's damage
    public Slider rangeSlider;                    // Slider that visually shows tower's range
    public Slider fireRateSlider;                 // Slider that visually shows tower's fire rate
    public Button upgradeButton;                  // Button to upgrade the selected tower
    public TextMeshProUGUI upgradeCostText;       // Text to show the upgrade cost of the tower

    private TowerManager towerManager;            // Reference to the TowerManager
    [Header("Game Over Panel")]
    public GameObject gameOverPanel;              // Game over panel UI

    private void Start()
    {
        // Find TowerManager in the scene
        towerManager = FindFirstObjectByType<TowerManager>();

        // Subscribe to events from GameManager and TowerManager
        GameManager.Instance.OnGameOver += GameoverAction;
        GameManager.Instance.OnMoneyChanged += UpdateMoneyUI;
        GameManager.Instance.OnHealthChanged += UpdateHealthUI;
        towerManager.OnTowerSelected += ShowTowerInfo;

        // Create tower selection buttons
        CreateTowerButtons();

        // Initialize UI with current game data
        UpdateMoneyUI(GameManager.Instance.GetMoney());
        UpdateHealthUI(GameManager.Instance.GetHealth());

        // Hide tower info panel by default
        towerInfoPanel.SetActive(false);
    }

    /// <summary>
    /// Called when the game is over. Shows the game over panel and pauses the game.
    /// </summary>
    private void GameoverAction()
    {
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f; // Pause time
        ShowTowerInfo(null); // Hide tower info
    }

    /// <summary>
    /// Creates UI buttons for each available tower using the prefab.
    /// </summary>
    private void CreateTowerButtons()
    {
        foreach (TowerData towerData in towerManager.availableTowers)
        {
            GameObject buttonObj = Instantiate(towerButtonPrefab, towerButtonsParent);
            Button button = buttonObj.GetComponent<Button>();
            TextMeshProUGUI costText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            Image icon = buttonObj.transform.GetChild(0).GetComponent<Image>();

            if (costText != null)
                costText.text = towerData.cost.ToString();

            if (icon != null && towerData.uiIcon != null)
                icon.sprite = towerData.uiIcon;

            // Assign button click to select the tower
            button.onClick.AddListener(() => towerManager.SetSelectedTowerData(towerData));
        }
    }

    /// <summary>
    /// Updates the money display on the UI.
    /// </summary>
    private void UpdateMoneyUI(int money)
    {
        if (moneyText != null)
            moneyText.text = $"{money} Money";
    }

    /// <summary>
    /// Updates the health display on the UI.
    /// </summary>
    private void UpdateHealthUI(int health)
    {
        if (healthText != null)
            healthText.text = $"{health} Health";
    }

    /// <summary>
    /// Displays or hides the tower info panel and updates its content based on the selected tower.
    /// </summary>
    private void ShowTowerInfo(Tower tower)
    {
        if (tower == null)
        {
            towerInfoPanel.SetActive(false);
            return;
        }

        towerInfoPanel.SetActive(true);

        if (towerNameText != null)
            towerNameText.text = $"Tower Level {tower.GetUpgradeLevel() + 1}";

        // Normalize and update sliders
        if (damageSlider != null)
            damageSlider.value = Mathf.Clamp01(tower.GetDamage() / 100f);

        if (rangeSlider != null)
            rangeSlider.value = Mathf.Clamp01(tower.GetRange() / 100f);

        if (fireRateSlider != null)
            fireRateSlider.value = Mathf.Clamp01(tower.GetFireRate());

        // Check if the tower can be upgraded and if the player has enough money
        bool canUpgrade = tower.CanUpgrade() && GameManager.Instance.GetMoney() >= tower.GetUpgradeCost();
        upgradeButton.interactable = canUpgrade;

        if (upgradeCostText != null)
            upgradeCostText.text = $"{tower.GetUpgradeCost()} Upgrade Cost";

        // Clear previous listeners to avoid multiple calls
        upgradeButton.onClick.RemoveAllListeners();

        // Add upgrade logic
        upgradeButton.onClick.AddListener(() =>
        {
            towerManager.UpgradeSelectedTower();
            ShowTowerInfo(tower); // Refresh the UI after upgrade
        });
    }

    /// <summary>
    /// Unsubscribes from events to avoid memory leaks.
    /// </summary>
    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnMoneyChanged -= UpdateMoneyUI;
            GameManager.Instance.OnHealthChanged -= UpdateHealthUI;
            GameManager.Instance.OnGameOver -= GameoverAction;
        }
    }

    /// <summary>
    /// Called by a button to restart the game.
    /// </summary>
    public void RestartGame()
    {
        Time.timeScale = 1f; // Resume time
        gameOverPanel.SetActive(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Reload current scene
    }
}

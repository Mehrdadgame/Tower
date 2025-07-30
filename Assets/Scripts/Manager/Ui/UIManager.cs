using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI healthText;
    public Transform towerButtonsParent;
    public GameObject towerButtonPrefab;

    [Header("Tower Info Panel")]
    public GameObject towerInfoPanel;
    public TextMeshProUGUI towerNameText;
    public Slider damageSlider;
    public Slider rangeSlider;
    public Slider fireRateSlider;
    public Button upgradeButton;
    public TextMeshProUGUI upgradeCostText;

    private TowerManager towerManager;
    [Header("Gamover Panel")]
    public GameObject gameOverPanel;

    private void Start()
    {
        towerManager = FindFirstObjectByType<TowerManager>();

        // Subscribe to events
        GameManager.Instance.OnGameOver += GameoverAction;
        GameManager.Instance.OnMoneyChanged += UpdateMoneyUI;
        GameManager.Instance.OnHealthChanged += UpdateHealthUI;
        towerManager.OnTowerSelected += ShowTowerInfo;

        CreateTowerButtons();
        UpdateMoneyUI(GameManager.Instance.GetMoney());
        UpdateHealthUI(GameManager.Instance.GetHealth());

        towerInfoPanel.SetActive(false);
    }

    private void GameoverAction()
    {
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f; // Pause the game
        ShowTowerInfo(null); // Hide tower info panel
    }

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

            button.onClick.AddListener(() => towerManager.SetSelectedTowerData(towerData));
        }
    }

    private void UpdateMoneyUI(int money)
    {
        if (moneyText != null)
            moneyText.text = $"{money} Money";
    }

    private void UpdateHealthUI(int health)
    {
        if (healthText != null)
            healthText.text = $"{health} health";
    }

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

        // Update sliders (normalized values)
        if (damageSlider != null)
            damageSlider.value = Mathf.Clamp01(tower.GetDamage() / 100f);

        if (rangeSlider != null)
            rangeSlider.value = Mathf.Clamp01(tower.GetRange() / 100f);

        if (fireRateSlider != null)
            fireRateSlider.value = Mathf.Clamp01(tower.GetFireRate());

        // Update upgrade button
        bool canUpgrade = tower.CanUpgrade() && GameManager.Instance.GetMoney() >= tower.GetUpgradeCost();
        upgradeButton.interactable = canUpgrade;

        if (upgradeCostText != null)
            upgradeCostText.text = $"{tower.GetUpgradeCost()} Upgrade Cost"; ;

        upgradeButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.AddListener(() =>
        {
            towerManager.UpgradeSelectedTower();
            ShowTowerInfo(tower); // Refresh UI
        });
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnMoneyChanged -= UpdateMoneyUI;
            GameManager.Instance.OnHealthChanged -= UpdateHealthUI;
            GameManager.Instance.OnGameOver -= GameoverAction;
        }
    }
    public void RestartGame()
    {
        Time.timeScale = 1f; // Resume the game
        gameOverPanel.SetActive(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Reload the current scene
    }
}
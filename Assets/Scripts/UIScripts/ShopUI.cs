using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using TMPro;


public class ShopUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject shopPanel;

    // ==========================
    // UI DE MOEDAS
    // ==========================
    // Mostra o total de coins persistentes do jogador.
    [SerializeField] private TextMeshProUGUI totalCoinsText;

    // Mostra o total de gems persistentes do jogador.
    [SerializeField] private TextMeshProUGUI totalGemsText;

    // ==========================
    // MAGNET UPGRADE
    // ==========================
    [SerializeField] private TextMeshProUGUI magnetLevelText;
    [SerializeField] private TextMeshProUGUI magnetDurationText;
    [SerializeField] private TextMeshProUGUI magnetCostText;

    // ==========================
    // ENERGY UPGRADE
    // ==========================
    [SerializeField] private TextMeshProUGUI energyLevelText;
    [SerializeField] private TextMeshProUGUI energyDurationText;
    [SerializeField] private TextMeshProUGUI energyCostText;

    // ==========================
    // UNLOCKS
    // ==========================
    [Header("Enhanced Magnet Unlock UI")]
    [SerializeField] private TextMeshProUGUI enhancedMagnetTitleText;
    [SerializeField] private TextMeshProUGUI enhancedMagnetDescriptionText;
    [SerializeField] private TextMeshProUGUI enhancedMagnetCostText;

    [Header("Enhanced Energy Unlock UI")]
    [SerializeField] private TextMeshProUGUI enhancedEnergyTitleText;
    [SerializeField] private TextMeshProUGUI enhancedEnergyDescriptionText;
    [SerializeField] private TextMeshProUGUI enhancedEnergyCostText;

    [Header("Feedback")]
    // Texto curto para mensagens como "Not enough coins", "Unlocked", etc.
    [SerializeField] private TextMeshProUGUI feedbackText;

    [Header("Buttons")]
    [SerializeField] private Button magnetUpgradeButton;
    [SerializeField] private Button energyUpgradeButton;
    [SerializeField] private Button enhancedMagnetButton;
    [SerializeField] private Button enhancedEnergyButton;

    [Header("Button Visual States")]
    [SerializeField] private Color affordableColor = Color.white;
    [SerializeField] private Color unaffordableColor = new Color(0.4f, 0.4f, 0.4f, 1);

    [Header("Spend Feedback")]
    [SerializeField] private TextMeshProUGUI spendFeedbackText;
    [SerializeField] private float spendFeedbackDuration = 0.65f;
    [SerializeField] private float spendFeedbackMoveY = 35f;

    private Coroutine spendFeedbackRoutine;
    private Vector3 spendFeedbackStartPos;

    private void Start()
	{
        //A shop começa fechada
		if (shopPanel != null) {
            shopPanel.SetActive(false);
		}

        RefreshUI();

        if (spendFeedbackText != null) {
            spendFeedbackStartPos = spendFeedbackText.transform.localPosition;
            spendFeedbackText.gameObject.SetActive(false);
        }
    }

    // ==========================
    // OPEN/CLOSE
    // ==========================
    public void OpenShop()
	{
        // Esconde o StartPanel para a shop ocupar o foco.
        if (StartUI.instance != null) {
            StartUI.instance.HideStartPanel();
		}

		if (shopPanel != null) {
            shopPanel.SetActive(true);
		}

        if (PauseManager.instance != null) {
            PauseManager.instance.ResumeGame();
        }

        //if (SharedCurrencyBarUI.instance != null) {
        //    SharedCurrencyBarUI.instance.Show();
        //}
        if (SharedCurrencyBarUI.instance != null) {
            SharedCurrencyBarUI.instance.Hide();
        }

        ClearFeedback();

        RefreshUI();
	}

    public void CloseShop()
	{
		if (shopPanel != null) {
            shopPanel.SetActive(false);
		}

        // Ao fechar a shop, volta para o menu inicial.
        if (StartUI.instance != null) {
            StartUI.instance.ShowStartPanel();
		}

        if (SharedCurrencyBarUI.instance != null) {
            SharedCurrencyBarUI.instance.Show();
        }
    }

    public void ToggleShop()
    {
        if (shopPanel == null) {
            return;
        }

        bool newState = !shopPanel.activeSelf;
        shopPanel.SetActive(newState);

        if (newState) {
            if (StartUI.instance != null) {
                StartUI.instance.HideStartPanel();
            }

            ClearFeedback();
            RefreshUI();
        } else {
            if (StartUI.instance != null) {
                StartUI.instance.ShowStartPanel();
            }
        }
    }

    // ==========================
    // COMPRA MAGNET
    // ==========================
    public void BuyMagnetDurationUpgrade()
	{
        // Verifica se os managers necessários existem.
        if (UpgradeData.instance == null || PlayerCurrency.instance == null) {
            SetFeedback("Shop data not ready.");
            return;
		}

        // Impede comprar além do nível máximo.
        if (UpgradeData.instance.IsMagnetDurationMaxed()) {
            SetFeedback("Magnet is maxed.");
            return;
		}

        int cost = UpgradeData.instance.GetMagnetDurationUpgradeCost();

        // SpendCoins retorna false se o jogador não tiver moedas suficientes.
        if (!PlayerCurrency.instance.SpendCoins(cost)) {
            SetFeedback("Not enough coins.");
            return;
		}

        // Depois de pagar, tenta aplicar o upgrade.
        if (!UpgradeData.instance.TryUpgradeMagnetDuration()) {
            SetFeedback("Upgrade failed.");
            return;
        }

        SetFeedback("Magnet upgraded!");

        PlaySpendFeedback(cost, false);

        RefreshUI();
	}

    // ==========================
    // COMPRA ENERGY
    // ==========================
    public void BuyEnergyDurationUpgrade()
    {
        if (UpgradeData.instance == null || PlayerCurrency.instance == null) {
            SetFeedback("Shop data not ready.");
            return;
        }

        if (UpgradeData.instance.IsEnergyDurationMaxed()) {
            SetFeedback("Energy maxed.");
            return;
        }

        int cost = UpgradeData.instance.GetEnergyDurationUpgradeCost();

        if (!PlayerCurrency.instance.SpendCoins(cost)) {
            SetFeedback("Not enough coins.");
            return;
        }

        if (!UpgradeData.instance.TryUpgradeEnergyDuration()) {
            SetFeedback("Upgrade failed.");
            return;
        }

        SetFeedback("Energy upgraded!");

        PlaySpendFeedback(cost, false);

        RefreshUI();
    }

    // ==========================
    // GEM UNLOCKS
    // ==========================

    public void BuyEnhancedMagnetUnlock()
    {
        // Enhanced Magnet é comprado com gems.
        if (UnlockData.instance == null || PlayerCurrency.instance == null) {
            SetFeedback("Unlock data not ready.");
            return;
        }

        if (UnlockData.instance.EnhancedMagnetUnlocked) {
            SetFeedback("Enhanced Magnet already unlocked.");
            return;
        }

        int cost = UnlockData.instance.EnhancedMagnetCost;

        if (!PlayerCurrency.instance.SpendGems(cost)) {
            SetFeedback("Not enough gems.");
            return;
        }

        if (!UnlockData.instance.TryUnlockEnhancedMagnet()) {
            SetFeedback("Unlock failed.");
            return;
        }

        SetFeedback("Enhanced Magnet unlocked!");

        PlaySpendFeedback(cost, true);

        RefreshUI();
    }

    public void BuyEnhancedEnergyUnlock()
    {
        // Enhanced Energy também é comprado com gems.
        if (UnlockData.instance == null || PlayerCurrency.instance == null) {
            SetFeedback("Unlock data not ready.");
            return;
        }

        if (UnlockData.instance.EnhancedEnergyUnlocked) {
            SetFeedback("Enhanced Energy already unlocked.");
            return;
        }

        int cost = UnlockData.instance.EnhancedEnergyCost;

        if (!PlayerCurrency.instance.SpendGems(cost)) {
            SetFeedback("Not enough gems.");
            return;
        }

        if (!UnlockData.instance.TryUnlockEnhancedEnergy()) {
            SetFeedback("Unlock failed.");
            return;
        }

        SetFeedback("Enhanced Energy unlocked!");

        PlaySpendFeedback(cost, true);

        RefreshUI();
    }

    // ==========================
    // REFRESH UI
    // ==========================
    public void RefreshUI()
	{
        // Atualiza tudo que aparece na shop.
        RefreshCurrencies();
        RefreshMagnetUpgrade();
        RefreshEnergyUpgrade();
        RefreshEnhancedMagnetUnlock();
        RefreshEnhancedEnergyUnlock();
        RefreshButtonsState();
    }

    private void RefreshButtonsState()
	{
		if (PlayerCurrency.instance == null) {
            return;
		}

        UpdateMagnetButton();
        UpdateEnergyButton();
        UpdateEnhancedMagnetButton();
        UpdateEnhancedEnergyButton();
	}

	private void UpdateEnhancedEnergyButton()
	{
        if (enhancedEnergyButton == null || UnlockData.instance == null) {
            return;
        }

        bool unlocked = UnlockData.instance.EnhancedEnergyUnlocked;
        int cost = UnlockData.instance.EnhancedEnergyCost;
        bool canBuy = PlayerCurrency.instance.Gems >= cost && !unlocked;
        enhancedEnergyButton.interactable = canBuy;
        Image image = enhancedEnergyButton.GetComponent<Image>();
        if (image != null) {
            image.color = canBuy ? affordableColor : unaffordableColor;
        }
    }

    private void UpdateEnhancedMagnetButton()
    {
        if (enhancedMagnetButton == null || UnlockData.instance == null) {
            return;
        }

        bool unlocked = UnlockData.instance.EnhancedMagnetUnlocked;
        int cost = UnlockData.instance.EnhancedMagnetCost;
        bool canBuy = PlayerCurrency.instance.Gems >= cost && !unlocked;
        enhancedMagnetButton.interactable = canBuy;
        Image image = enhancedMagnetButton.GetComponent<Image>();
        if (image != null) {
            image.color = canBuy ? affordableColor : unaffordableColor;
        }
    }

	private void UpdateEnergyButton()
	{
        if (energyUpgradeButton == null || UpgradeData.instance == null) {
            return;
        }

        bool maxed = UpgradeData.instance.IsEnergyDurationMaxed();
        int cost = UpgradeData.instance.GetEnergyDurationUpgradeCost();
        bool canBuy = PlayerCurrency.instance.Coins >= cost && !maxed;
        energyUpgradeButton.interactable = canBuy;
        Image image = energyUpgradeButton.GetComponent<Image>();
        if (image != null) {
            image.color = canBuy ? affordableColor : unaffordableColor;
        }
    }

	private void UpdateMagnetButton()
	{
		if (magnetUpgradeButton == null || UpgradeData.instance == null) {
            return;
		}

        bool maxed = UpgradeData.instance.IsMagnetDurationMaxed();
        int cost = UpgradeData.instance.GetMagnetDurationUpgradeCost();
        bool canBuy = PlayerCurrency.instance.Coins >= cost && !maxed;
        magnetUpgradeButton.interactable = canBuy;
        Image image = magnetUpgradeButton.GetComponent<Image>();
		if (image != null) {
            image.color = canBuy ? affordableColor : unaffordableColor;
		}
	}

	private void RefreshCurrencies()
    {
        if (PlayerCurrency.instance == null) {
            if (totalCoinsText != null) {
                totalCoinsText.text = "Coins: 0";
            }

            if (totalGemsText != null) {
                totalGemsText.text = "Gems: 0";
            }

            return;
        }

        if (totalCoinsText != null) {
            totalCoinsText.text = "Coins: " + PlayerCurrency.instance.TotalCoins;
        }

        if (totalGemsText != null) {
            totalGemsText.text = "Gems: " + PlayerCurrency.instance.Gems;
        }
    }

    private void RefreshMagnetUpgrade()
    {
        if (UpgradeData.instance == null) {
            return;
        }

        int currentLevel = UpgradeData.instance.MagnetDurationLevel;
        int maxLevel = UpgradeData.instance.MaxDurationLevel;

        if (magnetLevelText != null) {
            magnetLevelText.text = "" + currentLevel + "/" + maxLevel;
        }

        if (magnetDurationText != null) {
            magnetDurationText.text = "" + UpgradeData.instance.CurrentMagnetDuration.ToString("0") + "s";
        }

        if (magnetCostText != null) {
            if (UpgradeData.instance.IsMagnetDurationMaxed()) {
                magnetCostText.text = "MAX";
            } else {
                magnetCostText.text = "" + UpgradeData.instance.GetMagnetDurationUpgradeCost();
            }
        }
    }

    private void RefreshEnergyUpgrade()
    {
        if (UpgradeData.instance == null) {
            return;
        }

        int currentLevel = UpgradeData.instance.EnergyDurationLevel;
        int maxLevel = UpgradeData.instance.MaxDurationLevel;

        if (energyLevelText != null) {
            energyLevelText.text = "" + currentLevel + "/" + maxLevel;
        }

        if (energyDurationText != null) {
            energyDurationText.text = "" + UpgradeData.instance.CurrentEnergyDuration.ToString("0") + "s";
        }

        if (energyCostText != null) {
            if (UpgradeData.instance.IsEnergyDurationMaxed()) {
                energyCostText.text = "MAX";
            } else {
                energyCostText.text = "" + UpgradeData.instance.GetEnergyDurationUpgradeCost();
            }
        }
    }

    private void RefreshEnhancedMagnetUnlock()
    {
        if (UnlockData.instance == null) {
            return;
        }

        if (enhancedMagnetTitleText != null) {
            enhancedMagnetTitleText.text = "ENHANCED SILVER COINS";
        }

        if (enhancedMagnetDescriptionText != null) {
            enhancedMagnetDescriptionText.text = "Silver coins value x2";
        }

        if (enhancedMagnetCostText != null) {
            if (UnlockData.instance.EnhancedMagnetUnlocked) {
                enhancedMagnetCostText.text = "MAX";
            } else {
                //enhancedMagnetCostText.text = "Cost: " + UnlockData.instance.EnhancedMagnetCost + " Gems";
                enhancedMagnetCostText.text = "" + UnlockData.instance.EnhancedMagnetCost;
            }
        }
    }

    private void RefreshEnhancedEnergyUnlock()
    {
        if (UnlockData.instance == null) {
            return;
        }

        if (enhancedEnergyTitleText != null) {
            enhancedEnergyTitleText.text = "ENHANCED ENERGY";
        }

        if (enhancedEnergyDescriptionText != null) {
            enhancedEnergyDescriptionText.text = "+30% Energy duration";
        }

        if (enhancedEnergyCostText != null) {
            if (UnlockData.instance.EnhancedEnergyUnlocked) {
                enhancedEnergyCostText.text = "MAX";
            } else {
                enhancedEnergyCostText.text = "" + UnlockData.instance.EnhancedEnergyCost;
            }
        }
    }

    // ==========================
    // FEEDBACK
    // ==========================
    private void SetFeedback(string message)
	{
		if (feedbackText != null) {
            feedbackText.text = message;
		}
	}

    private void ClearFeedback()
	{
		if (feedbackText != null) {
            feedbackText.text = "";
		}
	}

    // ==========================
    // DEBUG
    // ==========================
    public void ResetUpgradesForTest()
    {
        // Reseta apenas upgrades comprados com coins.
        if (UpgradeData.instance != null) {
            UpgradeData.instance.ResetUpgrades();
        }

        ClearFeedback();
        RefreshUI();
    }

    public void ResetUnlocksForTest()
    {
        // Reseta apenas unlocks comprados com gems.
        if (UnlockData.instance != null) {
            UnlockData.instance.ResetUnlocks();
        }

        ClearFeedback();
        RefreshUI();
    }

    private void PlaySpendFeedback(int amount, bool isGem)
    {
        if (spendFeedbackText == null || amount <= 0) {
            return;
        }

        if (spendFeedbackRoutine != null) {
            StopCoroutine(spendFeedbackRoutine);
        }

        spendFeedbackRoutine = StartCoroutine(SpendFeedbackRoutine(amount, isGem));
    }

    private IEnumerator SpendFeedbackRoutine(int amount, bool isGem)
    {
        spendFeedbackText.gameObject.SetActive(true);
        spendFeedbackText.text = isGem ? "-" + amount + " Gems" : "-" + amount + " Coins";
        spendFeedbackText.alpha = 1f;
        spendFeedbackText.transform.localPosition = spendFeedbackStartPos;

        float timer = 0f;

        while (timer < spendFeedbackDuration) {
            timer += Time.unscaledDeltaTime;

            float t = timer / spendFeedbackDuration;

            Vector3 pos = spendFeedbackStartPos;
            pos.y += Mathf.Lerp(0f, spendFeedbackMoveY, t);
            spendFeedbackText.transform.localPosition = pos;

            spendFeedbackText.alpha = Mathf.Lerp(1f, 0f, t);

            yield return null;
        }

        spendFeedbackText.gameObject.SetActive(false);
        spendFeedbackRoutine = null;
    }

}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
	[Header("Panel")]
	[SerializeField] private GameObject inventoryPanel;

	[Header("Currency Texts")]
	[SerializeField] private TextMeshProUGUI totalCoinsText;
	[SerializeField] private TextMeshProUGUI goldCoinsText;
	[SerializeField] private TextMeshProUGUI silverCoinsText;
	[SerializeField] private TextMeshProUGUI gemsText;

	[Header("Upgrade Texts")]
	[SerializeField] private TextMeshProUGUI magnetDurationText;
	[SerializeField] private TextMeshProUGUI energyDurationText;

	[Header("Upgrade Progress Bars")]
	[SerializeField] private Image magnetProgressFill;
	[SerializeField] private Image energyProgressFill;

	private void Start()
	{
		if (inventoryPanel != null) {
			inventoryPanel.SetActive(false);
		}

		RefreshUI();
	}

	public void OpenInventory()
	{
		if (StartUI.instance != null) {
			StartUI.instance.HideStartPanel();
		}

		if (inventoryPanel != null) {
			inventoryPanel.SetActive(true);
		}

		if (SharedCurrencyBarUI.instance != null) {
			SharedCurrencyBarUI.instance.Hide();
		}

		RefreshUI();
	}

	public void CloseInventory()
	{
		if (inventoryPanel != null) {
			inventoryPanel.SetActive(false);
		}

		if (StartUI.instance != null) {
			StartUI.instance.ShowStartPanel();
		}

		if (SharedCurrencyBarUI.instance != null) {
			SharedCurrencyBarUI.instance.Show();
		}
	}

	public void RefreshUI()
	{
		RefreshCurrencyTexts();
		RefreshUpgradeTexts();
		RefreshUpgradeBars();
	}

	private void RefreshCurrencyTexts()
	{
		PlayerCurrency currency = PlayerCurrency.instance;

		if (currency == null) {
			//Debug.LogWarning("PlayerCurrency.instance não encontrado no InventoryUI.");
			return;
		}

		if (totalCoinsText != null) {
			totalCoinsText.text = currency.TotalCoins.ToString();
		}

		if (goldCoinsText != null) {
			goldCoinsText.text = currency.GoldCoins.ToString();
		}

		if (silverCoinsText != null) {
			silverCoinsText.text = currency.SilverCoins.ToString();
		}

		if (gemsText != null) {
			gemsText.text = currency.Gems.ToString();
		}
	}

	private void RefreshUpgradeTexts()
	{
		UpgradeData upgrades = UpgradeData.instance;

		if (upgrades == null) {
			//Debug.LogWarning("UpgradeData.instance não encontrado no InventoryUI.");
			return;
		}

		if (magnetDurationText != null) {
			magnetDurationText.text = upgrades.CurrentMagnetDuration.ToString("0") + "s";
		}

		if (energyDurationText != null) {
			energyDurationText.text = upgrades.CurrentEnergyDuration.ToString("0") + "s";
		}
	}

	private void RefreshUpgradeBars()
	{
		UpgradeData upgrades = UpgradeData.instance;

		if (upgrades == null) {
			return;
		}

		float maxLevel = Mathf.Max(1, upgrades.MaxDurationLevel);

		if (magnetProgressFill != null) {
			magnetProgressFill.fillAmount = upgrades.MagnetDurationLevel / maxLevel;
		}

		if (energyProgressFill != null) {
			energyProgressFill.fillAmount = upgrades.EnergyDurationLevel / maxLevel;
		}
	}
}
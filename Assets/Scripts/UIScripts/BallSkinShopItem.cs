using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BallSkinShopItem : MonoBehaviour
{
	[Header("Skin")]
	[SerializeField] private BallSkinType skinType;

	[Header("Cost")]
	[SerializeField] private int coinCost = 5000;

	[Header("Images")]
	[SerializeField] private GameObject lockedVisual;
	[SerializeField] private GameObject equippedVisual;

	[Header("Button")]
	[SerializeField] private Button actionButton;
	[SerializeField] private TextMeshProUGUI buttonText;

	[Header("Selector")]
	[SerializeField] private BallSkinSelector skinSelector;

	[Header("Coin Icon")]
	[SerializeField] private GameObject coinIcon;

	private void OnEnable()
	{
		RefreshUI();
	}

	public void OnClick()
	{
		if (BallSkinSaveData.instance == null ||
			BallSkinData.instance == null ||
			PlayerCurrency.instance == null ||
			skinSelector == null) {
			return;
		}

		bool unlocked = BallSkinSaveData.instance.IsUnlocked(skinType);
		bool equipped = BallSkinData.instance.GetActiveSkin() == skinType;

		if (equipped) {
			return;
		}

		if (!unlocked) {
			if (PlayerCurrency.instance.Coins < coinCost) {
				RefreshUI();
				return;
			}

			PlayerCurrency.instance.SpendCoins(coinCost);
			BallSkinSaveData.instance.UnlockSkin(skinType);
		}

		skinSelector.SetSkin(skinType);

		if (BallSkinsPanelUI.instance != null) {
			BallSkinsPanelUI.instance.RefreshAll();
		} else {
			RefreshUI();
		}
	}

	public void RefreshUI()
	{
		if (BallSkinSaveData.instance == null ||
			BallSkinData.instance == null ||
			PlayerCurrency.instance == null) {
			return;
		}

		bool unlocked = BallSkinSaveData.instance.IsUnlocked(skinType);
		bool equipped = BallSkinData.instance.GetActiveSkin() == skinType;
		bool canBuy = PlayerCurrency.instance.Coins >= coinCost;

		if (lockedVisual != null) {
			lockedVisual.SetActive(!equipped);
		}

		if (equippedVisual != null) {
			equippedVisual.SetActive(equipped);
		}

		if (buttonText != null) {
			if (equipped) {
				buttonText.text = "EQUIPPED";
				if (coinIcon != null) {
					coinIcon.SetActive(false);
				}
			} else if (unlocked) {
				buttonText.text = "SELECT";
				if (coinIcon != null) {
					coinIcon.SetActive(false);
				}
			} else {
				buttonText.text = coinCost.ToString();
				if (coinIcon != null) {
					coinIcon.SetActive(true);
				}
			}
		}

		if (actionButton != null) {
			actionButton.interactable = !equipped && (unlocked || canBuy);
		}
	}
}
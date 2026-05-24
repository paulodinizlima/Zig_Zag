using UnityEngine;
using UnityEngine.UI;

public class BallSkinsPanelUI : MonoBehaviour
{
	public static BallSkinsPanelUI instance;

	[Header("Current Skin Preview")]
	[SerializeField] private Image currentSkinImage;

	[Header("Current Skin Sprites")]
	[SerializeField] private Sprite defaultSprite;
	[SerializeField] private Sprite goldSprite;
	[SerializeField] private Sprite purpleSprite;
	[SerializeField] private Sprite cyanSprite;
	[SerializeField] private Sprite lavaSprite;

	[Header("Skin Items")]
	[SerializeField] private BallSkinShopItem[] skinItems;

	private void Awake()
	{
		instance = this;
	}

	private void OnEnable()
	{
		RefreshAll();
	}

	public void RefreshAll()
	{
		RefreshCurrentSkinPreview();

		for (int i = 0; i < skinItems.Length; i++) {

			if (skinItems[i] != null) {
				skinItems[i].RefreshUI();
			}
		}
	}

	public void RefreshCurrentSkinPreview()
	{
		if (currentSkinImage == null ||
			BallSkinData.instance == null) {
			return;
		}

		switch (BallSkinData.instance.GetActiveSkin()) {

			case BallSkinType.Default:
				currentSkinImage.sprite = defaultSprite;
				break;

			case BallSkinType.Gold:
				currentSkinImage.sprite = goldSprite;
				break;

			case BallSkinType.Purple:
				currentSkinImage.sprite = purpleSprite;
				break;

			case BallSkinType.Cyan:
				currentSkinImage.sprite = cyanSprite;
				break;

			case BallSkinType.Lava:
				currentSkinImage.sprite = lavaSprite;
				break;
		}
	}
}
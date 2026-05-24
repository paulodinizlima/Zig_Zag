using UnityEngine;
using UnityEngine.UI;

public class MuteButtonUI : MonoBehaviour
{
	public enum AudioType
	{
		Music,
		SFX
	}

	[SerializeField] private AudioType audioType;

	[SerializeField] private Image iconImage;

	[SerializeField] private Sprite soundOnSprite;
	[SerializeField] private Sprite soundOffSprite;

	private void Start()
	{
		RefreshIcon();
	}

	public void OnClick()
	{
		if (AudioManager.instance == null) {
			return;
		}

		if (audioType == AudioType.Music) {
			AudioManager.instance.ToggleMusic();
		} else {
			AudioManager.instance.ToggleSFX();
		}

		RefreshIcon();
	}

	public void RefreshIcon()
	{
		if (AudioManager.instance == null ||
			iconImage == null) {
			return;
		}

		bool muted =
			audioType == AudioType.Music
			? AudioManager.instance.MusicMuted
			: AudioManager.instance.SfxMuted;

		iconImage.sprite =
			muted
			? soundOffSprite
			: soundOnSprite;
	}
}
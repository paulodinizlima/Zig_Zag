using UnityEngine;

public class AudioManager : MonoBehaviour
{
	public static AudioManager instance;

	private const string MusicMuteKey = "MusicMuted";
	private const string SfxMuteKey = "SfxMuted";

	private bool musicMuted;
	private bool sfxMuted;

	public bool MusicMuted => musicMuted;
	public bool SfxMuted => sfxMuted;

	private void Awake()
	{
		if (instance != null && instance != this) {
			Destroy(gameObject);
			return;
		}

		instance = this;

		DontDestroyOnLoad(gameObject);

		LoadState();
	}

	private void LoadState()
	{
		musicMuted =
			PlayerPrefs.GetInt(MusicMuteKey, 0) == 1;

		sfxMuted =
			PlayerPrefs.GetInt(SfxMuteKey, 0) == 1;
	}

	public void ToggleMusic()
	{
		musicMuted = !musicMuted;

		PlayerPrefs.SetInt(
			MusicMuteKey,
			musicMuted ? 1 : 0);

		PlayerPrefs.Save();

		ApplyMusicMute();
	}

	public void ToggleSFX()
	{
		sfxMuted = !sfxMuted;

		PlayerPrefs.SetInt(
			SfxMuteKey,
			sfxMuted ? 1 : 0);

		PlayerPrefs.Save();
	}

	public void ApplyMusicMute()
	{
		if (GameMusicManager.instance == null) {
			return;
		}

		GameMusicManager.instance.SetMuted(musicMuted);
	}
}
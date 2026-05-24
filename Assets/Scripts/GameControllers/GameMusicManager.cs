using UnityEngine;

public class GameMusicManager : MonoBehaviour
{
	public static GameMusicManager instance;

	[Header("Audio")]
	[SerializeField] private AudioSource musicSource;

	[Header("Clips")]
	[SerializeField] private AudioClip gameplayMusic;
	[SerializeField] private AudioClip menuMusic;

	private void Awake()
	{
		if (instance != null && instance != this) {
			Destroy(gameObject);
			return;
		}

		instance = this;

		DontDestroyOnLoad(gameObject);
	}

	private void Start()
	{
		PlayMenuMusic();

		if (AudioManager.instance != null) {
			AudioManager.instance.ApplyMusicMute();
		}
	}

	public void PlayGameplayMusic()
	{
		if (musicSource == null || gameplayMusic == null) {
			return;
		}

		if (musicSource.clip == gameplayMusic && musicSource.isPlaying) {
			return;
		}

		musicSource.clip = gameplayMusic;
		musicSource.loop = true;
		musicSource.Play();
	}

	public void PlayMenuMusic()
	{
		if (musicSource == null || menuMusic == null) {
			return;
		}

		if (musicSource.clip == menuMusic && musicSource.isPlaying) {
			return;
		}

		musicSource.clip = menuMusic;
		musicSource.loop = true;
		musicSource.Play();
	}

	public void StopMusic()
	{
		if (musicSource != null) {
			musicSource.Stop();
		}
	}

	public void SetMuted(bool muted)
	{
		if (musicSource == null) {
			return;
		}

		musicSource.mute = muted;
	}
}
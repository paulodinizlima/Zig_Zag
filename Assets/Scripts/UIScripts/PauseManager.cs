using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
	public static PauseManager instance;

	[Header("UI")]
	[SerializeField] private GameObject pausePanel;
	[SerializeField] private GameObject pauseButton;

	[Header("Keyboard")]
	[SerializeField] private KeyCode pauseKey = KeyCode.Escape;

	private bool isPaused = false;

	public bool IsPaused => isPaused;

	private void Awake()
	{
		if (instance != null && instance != this) {
			Destroy(gameObject);
			return;
		}

		instance = this;
	}

	private void Start()
	{
		if (pausePanel != null) {
			pausePanel.SetActive(false);
		}

		if (pauseButton != null) {
			pauseButton.SetActive(false);
		}

		isPaused = false;
	}

	private void Update()
	{
		// No PC/Editor, ESC alterna entre pause e resume.
		// No mobile, o botão de pause continua sendo o caminho principal.
		if (Input.GetKeyDown(pauseKey)) {
			TogglePause();
		}

		UpdatePauseButtonVisibility();
	}

	private void UpdatePauseButtonVisibility()
	{
		if (pauseButton == null)
			return;

		// durante pause o botão some
		if (isPaused) {
			pauseButton.SetActive(false);
			return;
		}

		bool shouldShow = false;

		// Só aparece se gameplay estiver rodando
		if (GameplayController.instance != null) {
			shouldShow =
				GameplayController.instance.gamePlaying;
		}

		pauseButton.SetActive(shouldShow);
	}

	public void PauseGame()
	{
		// Só pausa se o gameplay estiver ativo.
		if (GameplayController.instance == null || !GameplayController.instance.gamePlaying) {
			return;
		}

		if (isPaused) {
			return;
		}

		isPaused = true;

		if (pausePanel != null) {
			pausePanel.SetActive(true);
		}

		if (pauseButton != null) {
			pauseButton.SetActive(false);
		}

		Time.timeScale = 0f;
	}

	public void ResumeGame()
	{
		if (!isPaused) {
			return;
		}

		isPaused = false;

		if (pausePanel != null) {
			pausePanel.SetActive(false);
		}

		if (pauseButton != null) {
			pauseButton.SetActive(true);
		}

		Time.timeScale = 1f;
	}

	public void TogglePause()
	{
		if (isPaused) {
			ResumeGame();
		} else {
			PauseGame();
		}
	}

	public void GoHome()
	{
		Time.timeScale = 1f;
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

	public void RestartRun()
	{
		Time.timeScale = 1f;
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}
}
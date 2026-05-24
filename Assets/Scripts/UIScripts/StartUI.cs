using UnityEngine;
using TMPro;

public class StartUI : MonoBehaviour
{
	public static StartUI instance;

	[Header("Panel")]
	[SerializeField] private GameObject startPanel;

	[Header("Optional Texts")]
	[SerializeField] private TextMeshProUGUI tapToStartText;

	[Header("Tap Animation")]
	[SerializeField] private float pulseScale = 1.2f;
	[SerializeField] private float pulseSpeed = 3f;

	private Vector3 originalScale;
	private bool gameStarted = false;

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
		if (startPanel != null) {
			startPanel.SetActive(true);
		}

		if (tapToStartText != null) {
			originalScale = tapToStartText.transform.localScale;
		}

		gameStarted = false;
	}

	private void Update()
	{
		if (gameStarted) {
			return;
		}

		//Usado para deletar todo o save data - INICIO
		/*if (Input.GetKeyDown(KeyCode.Home)) {

			PlayerPrefs.DeleteAll();
			PlayerPrefs.Save();

			Debug.Log("SAVE RESETADO");
		}*/
		//Usado para deletar todo o save data - FIM

		AnimateTapText();

#if UNITY_EDITOR
		CheckEditorKeyboardStart();
#endif
	}

	private void CheckEditorKeyboardStart()
	{
		if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) {
			StartGame();
		}
	}

	public void StartGame()
	{
		if (gameStarted) {
			return;
		}

		gameStarted = true;

		if (tapToStartText != null) {
			tapToStartText.transform.localScale = originalScale;
		}

		if (startPanel != null) {
			startPanel.SetActive(false);
		}

		if (GameplayController.instance != null) {
			GameplayController.instance.StartGameplay();
		}

		if (SharedCurrencyBarUI.instance != null) {
			SharedCurrencyBarUI.instance.Show();
		}
	}

	public void HideStartPanel()
	{
		if (startPanel != null) {
			startPanel.SetActive(false);
		}
	}

	public void ShowStartPanel()
	{
		if (gameStarted) {
			return;
		}

		if (startPanel != null) {
			startPanel.SetActive(true);
		}

		if (SharedCurrencyBarUI.instance != null) {
			SharedCurrencyBarUI.instance.Show();
		}

		if (GameMusicManager.instance != null) {
			GameMusicManager.instance.PlayMenuMusic();
		}
	}

	private void AnimateTapText()
	{
		if (tapToStartText == null) {
			return;
		}

		float t = (Mathf.Sin(Time.unscaledTime * pulseSpeed) + 1f) * 0.5f;
		float scale = Mathf.Lerp(1f, pulseScale, t);

		tapToStartText.transform.localScale = originalScale * scale;
	}
}
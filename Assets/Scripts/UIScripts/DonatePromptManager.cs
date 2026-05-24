using UnityEngine;

public class DonatePromptManager : MonoBehaviour
{
    public static DonatePromptManager instance;

    [Header("Donate Prompt")]
    [SerializeField] private GameObject donatePromptPanel;

    [Header("Rules")]
    [SerializeField] private int gameOverBeforePrompt = 5;

    [Header("Links")]
    [SerializeField] private string donateUrl = "https://seu-link-de-donate.com";

    private const string GameOverCounterKey = "Donate_GameOverCounter";

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
		if (donatePromptPanel != null) {
            donatePromptPanel.SetActive(false);
		}        
	}

    public void RegisterGameOver()
    {
        int counter = PlayerPrefs.GetInt(GameOverCounterKey, 0);
        counter++;

        PlayerPrefs.SetInt(GameOverCounterKey, counter);
        PlayerPrefs.Save();

		if (counter >= gameOverBeforePrompt) {
            ShowDonatePrompt();
            ResetGameOverCounter();
		}
    }

    public void ShowDonatePrompt()
	{
		if (donatePromptPanel != null) {
            donatePromptPanel.SetActive(true);
		}
	}

    public void CloseDonatePrompt()
	{
		if (donatePromptPanel != null) {
            donatePromptPanel.SetActive(false);
		}
	}

    public void Donate()
	{
		if (!string.IsNullOrEmpty(donateUrl)) {
            Application.OpenURL(donateUrl);
		}

        CloseDonatePrompt();
	}

    public void ResetGameOverCounter()
	{
        PlayerPrefs.SetInt(GameOverCounterKey, 0);
        PlayerPrefs.Save();
	}
}

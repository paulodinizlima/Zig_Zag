using UnityEngine;
using TMPro;

public class SupportPromptManager : MonoBehaviour
{
    public static SupportPromptManager instance;

    [Header("Support Prompt UI")]
    [SerializeField] private GameObject supportPromptPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI messageText;
	[SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI feedbackText;

    [Header("GameOver Prompt Rules")]
    [SerializeField] private int gameOverBeforePrompt = 5;

    [Header("Text Content")]
    [SerializeField] private string title = "SUPPORT ZIGBALL";
    [SerializeField] private string message = "Ajude a apoiar o desenvolvimento de novas fases, melhorias e atualizações.\n\nEsta compra não oferece vantagens dentro do jogo.";

	[Header("Price (Fallback)")]
	[SerializeField] private string fallbackPrice = "R$ 4,99"; // usado antes do IAP

	private const string GameOverCounterKey = "Support_GameOverCounter";

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
		if (supportPromptPanel != null) {
            supportPromptPanel.SetActive(false);
		}

		RefreshUI();
	}

    private void RefreshUI()
	{
		if (titleText != null) {
            titleText.text = title;
		}

		if (messageText != null) {
            messageText.text = message;
		}

		UpdatePriceText();

		ClearFeedback();
	}

	private void UpdatePriceText()
	{
		if (priceText == null)
			return;

		// FUTURO: puxar preço do IAP
		if (SupportPurchaseManager.instance != null &&
			SupportPurchaseManager.instance.HasRealPrice) {
			priceText.text = SupportPurchaseManager.instance.LocalizedPrice;
		} else {
			priceText.text = fallbackPrice;
		}
	}

	// ================================
	// GAME OVER CONTROL
	// ================================
	public void RegisterGameOver()
	{
        int counter = PlayerPrefs.GetInt(GameOverCounterKey, 0);
        counter++;

        PlayerPrefs.SetInt(GameOverCounterKey, counter);
        PlayerPrefs.Save();

		if (counter >= gameOverBeforePrompt) {
            ResetGameOverCounter();
            ShowSupportPrompt();
		}
	}

	public void ResetGameOverCounter()
	{
		PlayerPrefs.SetInt(GameOverCounterKey, 0);
		PlayerPrefs.Save();
	}

	// ================================
	// PANEL CONTROL
	// ================================
	public void ShowSupportPrompt()
	{
		RefreshUI();

		if (supportPromptPanel != null) {
            supportPromptPanel.SetActive(true);
		}
	}

    public void CloseSupportPrompt()
	{
		if (supportPromptPanel != null) {
            supportPromptPanel.SetActive(false);
		}
	}

	// ================================
	// BUTTONS
	// ================================
	public void OnSupportButtonClicked()
	{
		if (SupportPurchaseManager.instance == null) {
            SetFeedback("Sistema de apoio não pronto ainda.");
            return;
		}

        SetFeedback("Abrindo compra...");

        SupportPurchaseManager.instance.BuySupportDevelopment();
	}

    public void OnPurchaseSuccess()
	{
        SetFeedback("Obrigado por Apoiar ZigBall!");

		Invoke(nameof(CloseSupportPrompt), 1.2f);
	}

    public void OnPurchaseFailed(string reason)
	{
		if (string.IsNullOrWhiteSpace(reason)) {
            SetFeedback("Compra falhou.");
		} else {
            SetFeedback(reason);
		}
	}

	// ================================
	// FEEDBACK
	// ================================
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
}

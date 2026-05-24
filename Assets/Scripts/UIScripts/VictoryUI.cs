using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class VictoryUI : MonoBehaviour
{
	public static VictoryUI instance;

	[Header("Panel")]
	[SerializeField] private GameObject victoryPanel;
	[SerializeField] private CanvasGroup canvasGroup;

	[Header("Content")]
	[SerializeField] private RectTransform contentTransform;
	[SerializeField] private float fadeDuration = 0.35f;
	[SerializeField] private float moveDuration = 0.35f;
	[SerializeField] private float startYOffset = -40f;

	[Header("Texts - Run Summary")]
	[SerializeField] private TextMeshProUGUI finalScoreText;
	[SerializeField] private TextMeshProUGUI bestScoreText;
	[SerializeField] private TextMeshProUGUI finalLevelText;
	[SerializeField] private TextMeshProUGUI finalGemsText;
	[SerializeField] private TextMeshProUGUI finalGoldCoinsText;
	[SerializeField] private TextMeshProUGUI finalSilverCoinsText;

	[SerializeField] private TextMeshProUGUI timePlayedText;

	[Header("Texts - Totals")]
	// Totais persistentes que serão atualizados no fechamento da vitória
	[SerializeField] private TextMeshProUGUI totalCoinsText;
	[SerializeField] private TextMeshProUGUI totalGemsText;

	[Header("Coin Transfer FX")]
	[SerializeField] private RectTransform flyingCoinPrefab;
	[SerializeField] private RectTransform flyingCoinsParent;
	[SerializeField] private RectTransform coinsFlyStartAnchor;
	[SerializeField] private RectTransform totalCoinsTargetAnchor;
	[SerializeField] private int maxFlyingCoinIcons = 10;
	[SerializeField] private float coinTransferDuration = 0.55f;
	[SerializeField] private float coinSpawnInterval = 0.035f;
	[SerializeField] private float coinFlyArcHeight = 75f;

	[Header("Gem Transfer FX")]
	[SerializeField] private RectTransform flyingGemPrefab;
	[SerializeField] private RectTransform flyingGemsParent;
	[SerializeField] private RectTransform gemsFlyStartAnchor;
	[SerializeField] private RectTransform totalGemsTargetAnchor;
	[SerializeField] private int maxFlyingGemIcons = 8;
	[SerializeField] private float gemTransferDuration = 0.6f;
	[SerializeField] private float gemSpawnInterval = 0.05f;
	[SerializeField] private float gemFlyArcHeight = 85f;

	[Header("Shared FX Variation")]
	[SerializeField] private float spawnOffsetX = 20f;
	[SerializeField] private float spawnOffsetY = 10f;
	[SerializeField] private float arcHeightVariation = 15f;
	[SerializeField] private float targetScalePunch = 1.08f;
	[SerializeField] private float targetScalePunchDuration = 0.1f;
	[SerializeField] private float transferStartDelay = 0.1f;

	[Header("Audio")]
	[SerializeField] private AudioSource uiAudioSource;
	[SerializeField] private AudioClip coinArriveSound;
	[SerializeField] [Range(0f, 1f)] private float coinArriveVolume = 0.75f;
	[SerializeField] private AudioClip gemArriveSound;
	[SerializeField] [Range(0f, 1f)] private float gemArriveVolume = 0.8f;

	private Vector2 contentStartPos;
	private Vector2 contentEndPos;

	// Escalas originais para o punch dos textos
	private Vector3 totalCoinsTextOriginalScale = Vector3.one;
	private Vector3 totalGemsTextOriginalScale = Vector3.one;

	private Coroutine totalCoinsPunchRoutine;
	private Coroutine totalGemsPunchRoutine;

	// Controle de commit para não duplicar o save
	private bool victoryRewardsCommitted = false;

	// Valores exibidos durante a animação
	private int displayedTotalCoins = 0;
	private int displayedTotalGems = 0;

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
		if (victoryPanel != null) {
			victoryPanel.SetActive(false);
		}

		if (contentTransform != null) {
			contentEndPos = contentTransform.anchoredPosition;
			contentStartPos = contentEndPos + new Vector2(0f, startYOffset);
		}

		if (canvasGroup != null) {
			canvasGroup.alpha = 0f;
			canvasGroup.interactable = false;
			canvasGroup.blocksRaycasts = false;
		}

		if (totalCoinsText != null) {
			totalCoinsTextOriginalScale = totalCoinsText.rectTransform.localScale;
		}

		if (totalGemsText != null) {
			totalGemsTextOriginalScale = totalGemsText.rectTransform.localScale;
		}

		if (uiAudioSource == null) {
			uiAudioSource = GetComponent<AudioSource>();
		}
	}

	public void ShowVictory(int finalScore, int bestScore, int finalLevel, int finalGems, int finalGoldCoins, int finalSilverCoins, float runTime)
	{
		if (victoryPanel != null) {
			victoryPanel.SetActive(true);
		}

		if (GameplayController.instance != null) {
			GameplayController.instance.HideGameplayHUD();
		}

		// Preenche os dados da run
		if (finalScoreText != null) {
			finalScoreText.text = "" + finalScore;
		}

		if (bestScoreText != null) {
			bestScoreText.text = "" + bestScore;
		}

		if (finalLevelText != null) {
			finalLevelText.text = "" + finalLevel;
		}

		if (finalGemsText != null) {
			finalGemsText.text = "" + finalGems;
		}

		if (finalGoldCoinsText != null) {
			finalGoldCoinsText.text = "" + finalGoldCoins;
		}

		if (finalSilverCoinsText != null) {
			finalSilverCoinsText.text = "" + finalSilverCoins;
		}

		if (SharedCurrencyBarUI.instance != null) {
			SharedCurrencyBarUI.instance.Hide();
		}

		if (timePlayedText != null) {
			timePlayedText.text = FormatTime(runTime);
		}

		if (GameMusicManager.instance != null) {
			GameMusicManager.instance.PlayMenuMusic();
		}

		StopAllCoroutines();
		StartCoroutine(ShowVictorySequence(finalGems, finalGoldCoins, finalSilverCoins));

		Time.timeScale = 0f;
	}

	private IEnumerator ShowVictorySequence(int finalGems, int finalGoldCoins, int finalSilverCoins)
	{
		yield return StartCoroutine(AnimateVictory());

		if (transferStartDelay > 0f) {
			float delay = 0f;
			while (delay < transferStartDelay) {
				delay += Time.unscaledDeltaTime;
				yield return null;
			}
		}

		yield return StartCoroutine(PlayVictoryTransfersAndCommit(finalGems, finalGoldCoins, finalSilverCoins));

		if (canvasGroup != null) {
			canvasGroup.interactable = true;
			canvasGroup.blocksRaycasts = true;
		}
	}

	private IEnumerator PlayVictoryTransfersAndCommit(int finalGems, int finalGoldCoins, int finalSilverCoins)
	{
		if (victoryRewardsCommitted) {
			yield break;
		}

		PlayerCurrency playerCurrency = PlayerCurrency.instance;
		if (playerCurrency == null) {
			//Debug.LogWarning("PlayerCurrency.instance não encontrado ao tentar consolidar recompensas da vitória.");
			yield break;
		}

		int totalCoinsFromRun = finalGoldCoins + finalSilverCoins;
		int initialTotalCoins = playerCurrency.TotalCoins;
		int finalTotalCoins = initialTotalCoins + totalCoinsFromRun;

		int initialTotalGems = playerCurrency.Gems;
		int finalTotalGems = initialTotalGems + finalGems;

		displayedTotalCoins = initialTotalCoins;
		displayedTotalGems = initialTotalGems;

		if (totalCoinsText != null) {
			totalCoinsText.text = "" + displayedTotalCoins;
			totalCoinsText.rectTransform.localScale = totalCoinsTextOriginalScale;
		}

		if (totalGemsText != null) {
			totalGemsText.text = "" + displayedTotalGems;
			totalGemsText.rectTransform.localScale = totalGemsTextOriginalScale;
		}

		// Primeiro transfere coins
		if (totalCoinsFromRun > 0) {
			bool hasCoinFxSetup =
				flyingCoinPrefab != null &&
				flyingCoinsParent != null &&
				coinsFlyStartAnchor != null &&
				totalCoinsTargetAnchor != null;

			if (hasCoinFxSetup) {
				yield return StartCoroutine(AnimateFlyingCoinsParallel(totalCoinsFromRun));
			} else {
				AddToDisplayedTotalCoins(totalCoinsFromRun);
			}
		}

		// Depois transfere gems
		if (finalGems > 0) {
			bool hasGemFxSetup =
				flyingGemPrefab != null &&
				flyingGemsParent != null &&
				gemsFlyStartAnchor != null &&
				totalGemsTargetAnchor != null;

			if (hasGemFxSetup) {
				yield return StartCoroutine(AnimateFlyingGemsParallel(finalGems));
			} else {
				AddToDisplayedTotalGems(finalGems);
			}
		}

		// Garante o valor final correto no texto
		displayedTotalCoins = finalTotalCoins;
		displayedTotalGems = finalTotalGems;

		if (totalCoinsText != null) {
			totalCoinsText.text = "" + displayedTotalCoins;
		}

		if (totalGemsText != null) {
			totalGemsText.text = "" + displayedTotalGems;
		}

		// Commit persistente real só depois do efeito visual
		if (finalGoldCoins > 0) {
			playerCurrency.AddGoldCoins(finalGoldCoins);
		}

		if (finalSilverCoins > 0) {
			playerCurrency.AddSilverCoins(finalSilverCoins);
		}

		if (finalGems > 0) {
			playerCurrency.AddGems(finalGems);
		}

		victoryRewardsCommitted = true;
	}

	// =========================
	// COIN FX
	// =========================

	private IEnumerator AnimateFlyingCoinsParallel(int totalCoinsFromRun)
	{
		int flyingCoinsCount = Mathf.Clamp(totalCoinsFromRun, 1, maxFlyingCoinIcons);
		List<int> incrementsPerCoin = BuildIncrementDistribution(totalCoinsFromRun, flyingCoinsCount);

		for (int i = 0; i < flyingCoinsCount; i++) {
			RectTransform coinFx = Instantiate(flyingCoinPrefab, flyingCoinsParent);
			coinFx.gameObject.SetActive(true);

			int incrementValue = incrementsPerCoin[i];
			StartCoroutine(AnimateSingleFlyingCoin(coinFx, incrementValue));

			if (coinSpawnInterval > 0f) {
				float spawnDelay = 0f;
				while (spawnDelay < coinSpawnInterval) {
					spawnDelay += Time.unscaledDeltaTime;
					yield return null;
				}
			}
		}

		float waitTime = coinTransferDuration + (coinSpawnInterval * flyingCoinsCount) + 0.08f;
		float timer = 0f;

		while (timer < waitTime) {
			timer += Time.unscaledDeltaTime;
			yield return null;
		}
	}

	private IEnumerator AnimateSingleFlyingCoin(RectTransform coinFx, int incrementValue)
	{
		Vector2 baseStartPos = coinsFlyStartAnchor.anchoredPosition;
		Vector2 endPos = totalCoinsTargetAnchor.anchoredPosition;

		Vector2 randomOffset = new Vector2(
			Random.Range(-spawnOffsetX, spawnOffsetX),
			Random.Range(-spawnOffsetY, spawnOffsetY)
		);

		Vector2 startPos = baseStartPos + randomOffset;
		float currentArcHeight = coinFlyArcHeight + Random.Range(-arcHeightVariation, arcHeightVariation);

		coinFx.anchoredPosition = startPos;
		coinFx.localScale = Vector3.one;

		float timer = 0f;

		while (timer < coinTransferDuration) {
			timer += Time.unscaledDeltaTime;
			float t = Mathf.Clamp01(timer / coinTransferDuration);

			Vector2 pos = Vector2.Lerp(startPos, endPos, t);
			float arc = Mathf.Sin(t * Mathf.PI) * currentArcHeight;
			pos.y += arc;

			coinFx.anchoredPosition = pos;

			float scale = Mathf.Lerp(1f, 0.72f, t);
			coinFx.localScale = new Vector3(scale, scale, scale);

			yield return null;
		}

		coinFx.anchoredPosition = endPos;

		AddToDisplayedTotalCoins(incrementValue);
		PlayCoinArriveSound();
		PunchTotalCoinsText();

		Destroy(coinFx.gameObject);
	}

	// =========================
	// GEM FX
	// =========================

	private IEnumerator AnimateFlyingGemsParallel(int totalGemsFromRun)
	{
		int flyingGemsCount = Mathf.Clamp(totalGemsFromRun, 1, maxFlyingGemIcons);
		List<int> incrementsPerGem = BuildIncrementDistribution(totalGemsFromRun, flyingGemsCount);

		for (int i = 0; i < flyingGemsCount; i++) {
			RectTransform gemFx = Instantiate(flyingGemPrefab, flyingGemsParent);
			gemFx.gameObject.SetActive(true);

			int incrementValue = incrementsPerGem[i];
			StartCoroutine(AnimateSingleFlyingGem(gemFx, incrementValue));

			if (gemSpawnInterval > 0f) {
				float spawnDelay = 0f;
				while (spawnDelay < gemSpawnInterval) {
					spawnDelay += Time.unscaledDeltaTime;
					yield return null;
				}
			}
		}

		float waitTime = gemTransferDuration + (gemSpawnInterval * flyingGemsCount) + 0.08f;
		float timer = 0f;

		while (timer < waitTime) {
			timer += Time.unscaledDeltaTime;
			yield return null;
		}
	}

	private IEnumerator AnimateSingleFlyingGem(RectTransform gemFx, int incrementValue)
	{
		Vector2 baseStartPos = gemsFlyStartAnchor.anchoredPosition;
		Vector2 endPos = totalGemsTargetAnchor.anchoredPosition;

		Vector2 randomOffset = new Vector2(
			Random.Range(-spawnOffsetX, spawnOffsetX),
			Random.Range(-spawnOffsetY, spawnOffsetY)
		);

		Vector2 startPos = baseStartPos + randomOffset;
		float currentArcHeight = gemFlyArcHeight + Random.Range(-arcHeightVariation, arcHeightVariation);

		gemFx.anchoredPosition = startPos;
		gemFx.localScale = Vector3.one;

		float timer = 0f;

		while (timer < gemTransferDuration) {
			timer += Time.unscaledDeltaTime;
			float t = Mathf.Clamp01(timer / gemTransferDuration);

			Vector2 pos = Vector2.Lerp(startPos, endPos, t);
			float arc = Mathf.Sin(t * Mathf.PI) * currentArcHeight;
			pos.y += arc;

			gemFx.anchoredPosition = pos;

			float scale = Mathf.Lerp(1f, 0.75f, t);
			gemFx.localScale = new Vector3(scale, scale, scale);

			yield return null;
		}

		gemFx.anchoredPosition = endPos;

		AddToDisplayedTotalGems(incrementValue);
		PlayGemArriveSound();
		PunchTotalGemsText();

		Destroy(gemFx.gameObject);
	}

	// =========================
	// HELPERS
	// =========================

	private List<int> BuildIncrementDistribution(int totalValue, int visualCount)
	{
		List<int> result = new List<int>(visualCount);

		int baseValue = totalValue / visualCount;
		int remainder = totalValue % visualCount;

		for (int i = 0; i < visualCount; i++) {
			int value = baseValue;

			if (i < remainder) {
				value += 1;
			}

			if (value <= 0) {
				value = 1;
			}

			result.Add(value);
		}

		return result;
	}

	private void AddToDisplayedTotalCoins(int amount)
	{
		if (amount <= 0) {
			return;
		}

		displayedTotalCoins += amount;

		if (totalCoinsText != null) {
			totalCoinsText.text = "Total Coins: " + displayedTotalCoins;
		}
	}

	private void AddToDisplayedTotalGems(int amount)
	{
		if (amount <= 0) {
			return;
		}

		displayedTotalGems += amount;

		if (totalGemsText != null) {
			totalGemsText.text = "Total Gems: " + displayedTotalGems;
		}
	}

	private void PlayCoinArriveSound()
	{
		if (uiAudioSource == null || coinArriveSound == null) {
			return;
		}

		uiAudioSource.PlayOneShot(coinArriveSound, coinArriveVolume);
	}

	private void PlayGemArriveSound()
	{
		if (uiAudioSource == null || gemArriveSound == null) {
			return;
		}

		uiAudioSource.PlayOneShot(gemArriveSound, gemArriveVolume);
	}

	private void PunchTotalCoinsText()
	{
		if (totalCoinsText == null) {
			return;
		}

		if (totalCoinsPunchRoutine != null) {
			StopCoroutine(totalCoinsPunchRoutine);
		}

		totalCoinsPunchRoutine = StartCoroutine(PunchTextRoutine(totalCoinsText.rectTransform, totalCoinsTextOriginalScale, () => totalCoinsPunchRoutine = null));
	}

	private void PunchTotalGemsText()
	{
		if (totalGemsText == null) {
			return;
		}

		if (totalGemsPunchRoutine != null) {
			StopCoroutine(totalGemsPunchRoutine);
		}

		totalGemsPunchRoutine = StartCoroutine(PunchTextRoutine(totalGemsText.rectTransform, totalGemsTextOriginalScale, () => totalGemsPunchRoutine = null));
	}

	private IEnumerator PunchTextRoutine(RectTransform target, Vector3 originalScale, System.Action onComplete)
	{
		Vector3 punchScale = originalScale * targetScalePunch;

		float halfDuration = targetScalePunchDuration * 0.5f;
		float timer = 0f;

		while (timer < halfDuration) {
			timer += Time.unscaledDeltaTime;
			float t = Mathf.Clamp01(timer / halfDuration);
			target.localScale = Vector3.Lerp(originalScale, punchScale, t);
			yield return null;
		}

		timer = 0f;

		while (timer < halfDuration) {
			timer += Time.unscaledDeltaTime;
			float t = Mathf.Clamp01(timer / halfDuration);
			target.localScale = Vector3.Lerp(punchScale, originalScale, t);
			yield return null;
		}

		target.localScale = originalScale;
		onComplete?.Invoke();
	}

	// =========================
	// PANEL ANIMATION
	// =========================

	private IEnumerator AnimateVictory()
	{
		float unscaledTime = 0f;

		if (canvasGroup != null) {
			canvasGroup.alpha = 0f;
			canvasGroup.interactable = false;
			canvasGroup.blocksRaycasts = false;
		}

		if (contentTransform != null) {
			contentTransform.anchoredPosition = contentStartPos;
		}

		while (unscaledTime < fadeDuration) {
			unscaledTime += Time.unscaledDeltaTime;
			float t = Mathf.Clamp01(unscaledTime / fadeDuration);

			if (canvasGroup != null) {
				canvasGroup.alpha = t;
			}

			if (contentTransform != null) {
				float moveT = Mathf.Clamp01(unscaledTime / moveDuration);
				contentTransform.anchoredPosition = Vector2.Lerp(contentStartPos, contentEndPos, moveT);
			}

			yield return null;
		}

		if (canvasGroup != null) {
			canvasGroup.alpha = 1f;
			canvasGroup.interactable = false;
			canvasGroup.blocksRaycasts = false;
		}

		if (contentTransform != null) {
			contentTransform.anchoredPosition = contentEndPos;
		}
	}

	public void RestartGame()
	{
		Time.timeScale = 1f;
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

	public void GoHome()
	{
		Time.timeScale = 1f;
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

	public string FormatTime(float time)
	{
		int minutes = Mathf.FloorToInt(time / 60f);
		int seconds = Mathf.FloorToInt(time % 60f);
		return $"{minutes:00}:{seconds:00}";
	}
}
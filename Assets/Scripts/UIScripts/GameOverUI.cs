using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverUI : MonoBehaviour
{
	public static GameOverUI instance;

	[Header("Panel")]
	[SerializeField] private GameObject gameOverPanel;
	[SerializeField] private CanvasGroup canvasGroup;

	[Header("Content")]
	[SerializeField] private RectTransform contentTransform;
	[SerializeField] private float fadeDuration = 0.35f;
	[SerializeField] private float moveDuration = 0.35f;
	[SerializeField] private float startYOffset = -40f;

	[Header("Texts")]
	[SerializeField] private TextMeshProUGUI finalScoreText;
	[SerializeField] private TextMeshProUGUI bestScoreText;
	[SerializeField] private TextMeshProUGUI finalLevelText;
	[SerializeField] private TextMeshProUGUI finalGemsText;

	[Header("Coin Result UI")]
	[SerializeField] private TextMeshProUGUI finalGoldCoinsText;
	[SerializeField] private TextMeshProUGUI finalSilverCoinsText;
	[SerializeField] private TextMeshProUGUI totalCoinsText;

	[Header("Coin Transfer FX")]
	[SerializeField] private RectTransform flyingCoinPrefab;
	[SerializeField] private RectTransform flyingCoinsParent;
	[SerializeField] private RectTransform coinsFlyStartAnchor;
	[SerializeField] private RectTransform totalCoinsTargetAnchor;
	[SerializeField] private int maxFlyingCoinIcons = 10;
	[SerializeField] private float coinTransferDuration = 0.55f;
	[SerializeField] private float coinSpawnInterval = 0.035f;
	[SerializeField] private float coinFlyArcHeight = 75f;
	[SerializeField] private float coinTransferStartDelay = 0.1f;

	[Header("Coin FX Variation")]
	[SerializeField] private float spawnOffsetX = 20f;
	[SerializeField] private float spawnOffsetY = 10f;
	[SerializeField] private float arcHeightVariation = 15f;
	[SerializeField] private float targetScalePunch = 1.08f;
	[SerializeField] private float targetScalePunchDuration = 0.1f;

	[Header("Coin FX Audio")]
	[SerializeField] private AudioSource uiAudioSource;
	[SerializeField] private AudioClip coinArriveSound;
	[SerializeField] [Range(0f, 1f)] private float coinArriveVolume = 0.5f;

	[Header("Final Target Spark FX")]
	//Prefab UI opcional para um brilho/spark no alvo final
	[SerializeField] private RectTransform finalTargetSparkPrefab;
	//Parent opcional do spark; se vazio, usa flyingCoinsParent
	[SerializeField] private RectTransform finalTargetSparkParent;
	//Duração do spark
	[SerializeField] private float finalTargetSparkLifetime = 0.35f;
	//Escala máxima do spark
	[SerializeField] private float finalTargetSparkScale = 1.35f;
	//Flash de cor no texto do total quando a última coin chega
	[SerializeField] private Color totalCoinsHighlightColor = new Color(1f, 0.92f, 0.35f);

	private Vector2 contentStartPos;
	private Vector2 contentEndPos;
	private bool runRewardsCommitted = false;

	private Vector3 totalCoinsTextOriginalScale;
	private Color totalCoinsTextOriginalColor;
	private Coroutine totalCoinsPunchRoutine;
	private Coroutine totalCoinsFlashRoutine;

	// Controle do valor visível durante a animação
	private int displayedTotalCoins = 0;

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
		if (gameOverPanel != null) {
			gameOverPanel.SetActive(false);
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
			totalCoinsTextOriginalColor = totalCoinsText.color;
		}

		if (uiAudioSource == null) {
			uiAudioSource = GetComponent<AudioSource>();
		}

		runRewardsCommitted = false;
	}

	public void ShowGameOver(int finalScore, int bestScore, int finalLevel, int finalGems)
	{
		if (gameOverPanel != null) {
			gameOverPanel.SetActive(true);
		}

		if (finalScoreText != null) {
			finalScoreText.text = "Score: " + finalScore;
		}

		if (bestScoreText != null) {
			bestScoreText.text = "Best: " + bestScore;
		}

		if (finalLevelText != null) {
			finalLevelText.text = "Level: " + finalLevel;
		}

		if (finalGemsText != null) {
			finalGemsText.text = "Gems: " + finalGems;
		}

		StopAllCoroutines();
		StartCoroutine(ShowGameOverSequence());

		Time.timeScale = 0f;
	}

	private IEnumerator ShowGameOverSequence()
	{
		yield return StartCoroutine(AnimateGameOver());

		if (coinTransferStartDelay > 0f) {
			float delay = 0f;
			while (delay < coinTransferStartDelay) {
				delay += Time.unscaledDeltaTime;
				yield return null;
			}
		}

		yield return StartCoroutine(PlayCoinTransferAndCommitOnLose());

		if (canvasGroup != null) {
			canvasGroup.interactable = true;
			canvasGroup.blocksRaycasts = true;
		}
	}

	private IEnumerator PlayCoinTransferAndCommitOnLose()
	{
		if (runRewardsCommitted) {
			yield break;
		}

		if (ScoreManager.instance == null) {
			Debug.LogWarning("ScoreManager.instance não encontrado ao tentar consolidar recompensas da run.");
			yield break;
		}

		int goldCoinsFromRun = ScoreManager.instance.GoldCoinsCollected;
		int silverCoinsFromRun = ScoreManager.instance.SilverCoinsCollected;
		int totalCoinsFromRun = goldCoinsFromRun + silverCoinsFromRun;

		if (finalGoldCoinsText != null) {
			finalGoldCoinsText.text = "Gold: " + goldCoinsFromRun;
		}

		if (finalSilverCoinsText != null) {
			finalSilverCoinsText.text = "Silver: " + silverCoinsFromRun;
		}

		PlayerCurrency playerCurrency = PlayerCurrency.instance;
		if (playerCurrency == null) {
			Debug.LogWarning("PlayerCurrency.instance não encontrado ao tentar consolidar recompensas da run.");
			yield break;
		}

		int initialTotalCoins = playerCurrency.TotalCoins;
		int finalTotalCoins = initialTotalCoins + totalCoinsFromRun;

		displayedTotalCoins = initialTotalCoins;

		if (totalCoinsText != null) {
			totalCoinsText.text = "Total Coins: " + displayedTotalCoins;
			totalCoinsText.rectTransform.localScale = totalCoinsTextOriginalScale;
			totalCoinsText.color = totalCoinsTextOriginalColor;
		}

		if (totalCoinsFromRun <= 0) {
			runRewardsCommitted = true;
			yield break;
		}

		bool hasCoinFxSetup =
			flyingCoinPrefab != null &&
			flyingCoinsParent != null &&
			coinsFlyStartAnchor != null &&
			totalCoinsTargetAnchor != null;

		if (hasCoinFxSetup) {
			yield return StartCoroutine(AnimateFlyingCoinsParallel(totalCoinsFromRun));
		} else {
			// Fallback: se não houver FX visual configurado, incrementa tudo de uma vez
			AddToDisplayedTotal(totalCoinsFromRun);
			TriggerFinalTargetSpark();
		}

		// Garante o valor final correto
		if (displayedTotalCoins != finalTotalCoins) {
			displayedTotalCoins = finalTotalCoins;

			if (totalCoinsText != null) {
				totalCoinsText.text = "Total Coins: " + displayedTotalCoins;
			}
		}

		if (goldCoinsFromRun > 0) {
			playerCurrency.AddGoldCoins(goldCoinsFromRun);
		}

		if (silverCoinsFromRun > 0) {
			playerCurrency.AddSilverCoins(silverCoinsFromRun);
		}

		runRewardsCommitted = true;
	}

	private IEnumerator AnimateFlyingCoinsParallel(int totalCoinsFromRun)
	{
		int flyingCoinsCount = Mathf.Clamp(totalCoinsFromRun, 1, maxFlyingCoinIcons);

		// Distribui o valor real entre as coins visuais
		List<int> incrementsPerCoin = BuildCoinIncrementDistribution(totalCoinsFromRun, flyingCoinsCount);

		for (int i = 0; i < flyingCoinsCount; i++) {
			RectTransform coinFx = Instantiate(flyingCoinPrefab, flyingCoinsParent);
			coinFx.gameObject.SetActive(true);

			int incrementValue = incrementsPerCoin[i];
			bool isLastCoin = (i == flyingCoinsCount - 1);

			StartCoroutine(AnimateSingleFlyingCoin(coinFx, incrementValue, isLastCoin));

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

	private List<int> BuildCoinIncrementDistribution(int totalCoinsFromRun, int flyingCoinsCount)
	{
		List<int> result = new List<int>(flyingCoinsCount);

		int baseValue = totalCoinsFromRun / flyingCoinsCount;
		int remainder = totalCoinsFromRun % flyingCoinsCount;

		for (int i = 0; i < flyingCoinsCount; i++) {
			int value = baseValue;

			// Distribui o resto nas primeiras coins
			if (i < remainder) {
				value += 1;
			}

			// Segurança para nunca gerar coin visual sem valor
			if (value <= 0) {
				value = 1;
			}

			result.Add(value);
		}

		return result;
	}

	private IEnumerator AnimateSingleFlyingCoin(RectTransform coinFx, int incrementValue, bool isLastCoin)
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

		// Quando a coin chega, incrementa o total visível junto com o efeito
		AddToDisplayedTotal(incrementValue);
		PlayCoinArriveSound();
		PunchTotalCoinsText();

		if (isLastCoin) {
			TriggerFinalTargetSpark();
		}

		Destroy(coinFx.gameObject);
	}

	private void AddToDisplayedTotal(int amount)
	{
		if (amount <= 0) {
			return;
		}

		displayedTotalCoins += amount;

		if (totalCoinsText != null) {
			totalCoinsText.text = "Total Coins: " + displayedTotalCoins;
		}
	}

	private void PlayCoinArriveSound()
	{
		if (uiAudioSource == null || coinArriveSound == null) {
			return;
		}

		uiAudioSource.PlayOneShot(coinArriveSound, coinArriveVolume);
	}

	private void PunchTotalCoinsText()
	{
		if (totalCoinsText == null) {
			return;
		}

		if (totalCoinsPunchRoutine != null) {
			StopCoroutine(totalCoinsPunchRoutine);
		}

		totalCoinsPunchRoutine = StartCoroutine(PunchTotalCoinsTextRoutine());
	}

	private IEnumerator PunchTotalCoinsTextRoutine()
	{
		RectTransform target = totalCoinsText.rectTransform;
		Vector3 startScale = totalCoinsTextOriginalScale;
		Vector3 punchScale = startScale * targetScalePunch;

		float halfDuration = targetScalePunchDuration * 0.5f;
		float timer = 0f;

		while (timer < halfDuration) {
			timer += Time.unscaledDeltaTime;
			float t = Mathf.Clamp01(timer / halfDuration);
			target.localScale = Vector3.Lerp(startScale, punchScale, t);
			yield return null;
		}

		timer = 0f;

		while (timer < halfDuration) {
			timer += Time.unscaledDeltaTime;
			float t = Mathf.Clamp01(timer / halfDuration);
			target.localScale = Vector3.Lerp(punchScale, startScale, t);
			yield return null;
		}

		target.localScale = startScale;
		totalCoinsPunchRoutine = null;
	}

	private void TriggerFinalTargetSpark()
	{
		//Sempre faz pelo menos o flash no texto
		if (totalCoinsFlashRoutine != null) {
			StopCoroutine(totalCoinsFlashRoutine);
		}

		totalCoinsFlashRoutine = StartCoroutine(FlashTotalCoinsTextRoutine());

		//Se houver prefab de spark configurado, instancia no alvo final
		if (finalTargetSparkPrefab == null || totalCoinsTargetAnchor == null) {
			return;
		}

		RectTransform sparkParent = finalTargetSparkParent != null ? finalTargetSparkParent : flyingCoinsParent;
		if (sparkParent == null) {
			return;
		}

		RectTransform sparkFx = Instantiate(finalTargetSparkPrefab, sparkParent);
		sparkFx.gameObject.SetActive(true);
		sparkFx.anchoredPosition = totalCoinsTargetAnchor.anchoredPosition;

		StartCoroutine(AnimateFinalTargetSpark(sparkFx));
	}

	private IEnumerator AnimateFinalTargetSpark(RectTransform sparkFx)
	{
		Vector3 startScale = Vector3.zero;
		Vector3 endScale = Vector3.one * finalTargetSparkScale;

		// Tenta usar CanvasGroup para fade; se não houver, adiciona
		CanvasGroup sparkCanvasGroup = sparkFx.GetComponent<CanvasGroup>();
		if (sparkCanvasGroup == null) {
			sparkCanvasGroup = sparkFx.gameObject.AddComponent<CanvasGroup>();
		}

		sparkFx.localScale = startScale;
		sparkCanvasGroup.alpha = 1f;

		float timer = 0f;

		while (timer < finalTargetSparkLifetime) {
			timer += Time.unscaledDeltaTime;
			float t = Mathf.Clamp01(timer / finalTargetSparkLifetime);

			// Cresce rápido e some suavemente
			float scaleT = Mathf.SmoothStep(0f, 1f, t);
			sparkFx.localScale = Vector3.Lerp(startScale, endScale, scaleT);

			sparkCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t);

			yield return null;
		}

		Destroy(sparkFx.gameObject);
	}

	private IEnumerator FlashTotalCoinsTextRoutine()
	{
		if (totalCoinsText == null) {
			yield break;
		}

		float duration = 0.18f;
		float timer = 0f;

		while (timer < duration) {
			timer += Time.unscaledDeltaTime;
			float t = Mathf.Clamp01(timer / duration);

			// Vai para a cor highlight e volta
			float pingPong = Mathf.Sin(t * Mathf.PI);
			totalCoinsText.color = Color.Lerp(totalCoinsTextOriginalColor, totalCoinsHighlightColor, pingPong);

			yield return null;
		}

		totalCoinsText.color = totalCoinsTextOriginalColor;
		totalCoinsFlashRoutine = null;
	}

	private IEnumerator AnimateGameOver()
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
}
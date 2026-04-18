using System.Collections;
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
	//Texto da pontuação final
	[SerializeField] private TextMeshProUGUI finalScoreText;
	//Texto do melhor score
	[SerializeField] private TextMeshProUGUI bestScoreText;
	//Texto do nível alcançado na run
	[SerializeField] private TextMeshProUGUI finalLevelText;
	//Texto da quantidade de gemas coletadas
	[SerializeField] private TextMeshProUGUI finalGemsText;

	private Vector2 contentStartPos;
	private Vector2 contentEndPos;

	private void Awake()
	{
		if(instance != null && instance != this) {
			Destroy(gameObject);
			return;
		}
		instance = this;
	}

	private void Start()
	{
		if(gameOverPanel != null) {
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
	}

	//Mostra da tela de Game Over com os dados finais da run
	public void ShowGameOver(int finalScore, int bestScore, int finalLevel, int finalGems)
	{
		//Ativa o painel de Game Over
		if(gameOverPanel != null) {
			gameOverPanel.SetActive(true);
		}
		//Atualiza o texto do score final
		if (finalScoreText != null) {
			finalScoreText.text = "Score: " + finalScore;
		}
		//Atualiza o texto do best score
		if (bestScoreText != null) {
			bestScoreText.text = "Best: " + bestScore;
		}
		//Atualiza o texto do nível final
		if (finalLevelText != null) {
			finalLevelText.text = "Level: " + finalLevel;
		}
		//Atualiza o texto da quantidade de gemas
		if (finalGemsText != null) {
			finalGemsText.text = "Gems: " + finalGems;
		}
		//Reinicia qualquer animação anterior e toca a animação de entrada
		StopAllCoroutines();
		StartCoroutine(AnimateGameOver());
		//Pausa o jogo
		Time.timeScale = 0f;
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
			unscaledTime += Time.unscaledTime;
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
			canvasGroup.interactable = true;
			canvasGroup.blocksRaycasts = true;
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

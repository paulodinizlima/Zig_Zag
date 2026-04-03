using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
	//Instância global para acesso fácil (singleton)
	public static ScoreManager instance;

	private BallScript ballScript;

	[Header("Score Values")]
	// Pontos ganhos ao passar por um tile
	[SerializeField] private int pointsPerTile = 1;

	//Pontos ganhos ao coletar uma gema
	[SerializeField] private int pointsPerGem = 10;

	//Valor máximo que o combo pode atingir
	[SerializeField] private int maxCombo = 5;

	[Header("UI")]
	// Referência para o texto de pontuação total
	[SerializeField] private TextMeshProUGUI scoreText;
	// Referência para o texto de gemas coletadas
	[SerializeField] private TextMeshProUGUI gemsText;
	// Referência para o texto do combo atual
	[SerializeField] private TextMeshProUGUI comboText;
	// Referência para o texto de melhor pontuação
	[SerializeField] private TextMeshProUGUI bestScoreText;
	[SerializeField] private TextMeshProUGUI speedText;

	[Header("Score Popup")]
	[SerializeField] private ScorePopup scorePopupPrefab;
	[SerializeField] private RectTransform scorePopupAnchor;

	//Variáveis internas de controle
	private int score;		// Pontuação atual
	private int gems;		// Quantidade de gemas coletadas
	private int combo = 1;	// Multiplicador atual (começa em 1)
	private int bestScore;  // Melhor pontuação salva
	private float speed;        // Velocidade da bola

	[Header("Combo Settings")]
	//Tempo máximo (em segundos) que o jogador pode ficar sem coletar uma gema
	[SerializeField] private float comboResetTime = 3f;

	//Timer interno que conta quanto tempo passou desde a última gema
	private float comboTimer = 0f;

	//Porpriedades públicas para leitura externa
	public int Score => score;
	public int Gems => gems;
	public int Combo => combo;
	public int BestScore => bestScore;
	public float Speed => speed;

	private void Awake()
	{
		//Garante que exista apenas uma instância do ScoreManager
		if (instance != null && instance != this) {
			Destroy(gameObject);
			return;
		}
		instance = this;
	}

	private void Start()
	{		
		// Carrega o melhor score salvo no PlayerPrefs
		bestScore = PlayerPrefs.GetInt("BestScore", 0);

		GameObject ball = GameObject.FindGameObjectWithTag("Ball");
		if (ball != null) {
			ballScript = ball.GetComponent<BallScript>();
		}
		// Atualiza a UI ao iniciar
		UpdateUI();
	}

	private void Update()
	{
		UpdateSpeedUI();
		UpdateComboTimer();
	}

	//Atualiza o tempo desde a última gema coletada
	private void UpdateComboTimer()
	{
		//Se o jogo não está rodando, não faz nada
		if (!GameplayController.instance.gamePlaying) {
			return;
		}
		//Se o combo já está no mínimo, não precisa contar tempo
		if (combo <= 1) {
			return;
		}

		//Incrementa o tempo
		comboTimer += Time.deltaTime;
		//Se passou do limite, reseta o combo
		if (comboTimer >= comboResetTime) {
			ResetCombo();
		}
	}

	//Atualiza o texto da velocidade na UI, mas mostrando como nível
	private void UpdateSpeedUI()
	{
		//Se não houver texto de UI ligado, não faz nada
		if (speedText == null) { 
			return;
		}
		//Se a bola não estiver disponível, mostra o nível inicial
		if (ballScript == null) {
			speedText.text = "Level: 1";
			return;
		}
		//Mostra o nível calculado com base na velocidade atual da bola
		speedText.text = "Level: " + ballScript.CurrentSpeedLevel;

	}

	public void AddTilePoint()
	{
		// Adiciona pontos ao passar por um tile
		score += pointsPerTile;
		UpdateUI();

		//SpawnScorePopup("+" + pointsPerTile, Color.white);
	}

	public void AddGem()
	{
		// Incrementa contador de gemas
		gems++;

		int earnedPoints = pointsPerGem * combo;
		score += earnedPoints;

		SpawnScorePopup("+" + earnedPoints, Color.yellow);

		//Reinicia o timer do combo ao coletar uma gema
		comboTimer = 0f;

		// Aumenta o combo até o limite máximo
		if (combo < maxCombo) { 
			combo++;
		}
		UpdateUI();
	}

	public void ResetCombo()
	{
		// Reseta o combo (ao morrer)
		combo = 1;
		UpdateUI();
	}

	//Finaliza a run, salva vest score e envia os dados para a tela de Game Over
	public void GameOver()
	{
		// Se o score atual for maior que o melhor registrado, salva
		if (score > bestScore) {
			bestScore = score;
			PlayerPrefs.SetInt("BestScore", bestScore);
			PlayerPrefs.Save();
		}
		
		//Atualiza os textos normais da HUD
		UpdateUI();
		
		//Garante que existe referência para a bola
		if(ballScript == null) {
			GameObject ball = GameObject.FindGameObjectWithTag("Ball");
			if(ball != null) {
				ballScript = ball.GetComponent<BallScript>();
			}
		}

		//Valor padrão do nível, caso não ache a bola
		int finalLevel = 1;

		//Se encontrou a bola, lê o nível dela
		if (ballScript != null) {
			finalLevel = ballScript.CurrentSpeedLevel;
		}

		if(GameOverUI.instance != null) {
			GameOverUI.instance.ShowGameOver(score, bestScore, finalLevel, gems);
		}
	}

	public void ResetRun()
	{
		// Reseta os valores para uma nova partida
		score = 0;
		gems = 0;
		combo = 1;
		speed = 0f;
		UpdateUI();
	}

	private void SpawnScorePopup(string text, Color color)
	{
		if (scorePopupPrefab == null || scorePopupAnchor == null) {
			return;
		}

		ScorePopup popup = Instantiate(scorePopupPrefab, scorePopupAnchor);
		popup.transform.localPosition = Vector3.zero;
		popup.Setup(text, color);
	}

	private void UpdateUI()
	{
		// Atualiza todos os textos da interface
		if(scoreText != null) {
			scoreText.text = "Score: " + score;
		}

		if(gemsText != null) {
			gemsText.text = "Gems: " + gems;
		} 

		if(comboText != null) {
			comboText.text = "Combo x" + combo;
		}

		if (bestScoreText != null) {
			bestScoreText.text = "Best: " + bestScore;
		}

		if(speedText != null) {
			speedText.text = "Speed: " + speed;
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class ScoreManager : MonoBehaviour
{
	//Instância global para acesso fácil por outros scripts
	public static ScoreManager instance;

	//Referências principais
	private BallScript ballScript;
	private PlayerCurrency playerCurrency;
	private PlayerMagnet playerMagnet;
	private PlayerTileEnergy playerTileEnergy;

	[Header("Score Values")]
	//Quantidade de pontos por tile percorrido
	[SerializeField] private int pointsPerTile = 1;

	//Quantidade de pontos por gem coletada
	[SerializeField] private int pointsPerGem = 10;

	//Limite máximo do multiplicador de combo
	[SerializeField] private int maxCombo = 5;

	[Header("UI")]
	//UI principal da run
	[SerializeField] private TextMeshProUGUI scoreText;
	[SerializeField] private TextMeshProUGUI gemsText;

	[SerializeField] private TextMeshProUGUI coinsText;       // saldo total persistente
	[SerializeField] private TextMeshProUGUI goldCoinsText;   // moedas normais douradas da run
	[SerializeField] private TextMeshProUGUI silverCoinsText; // moedas adjacentes prateadas da run

	[SerializeField] private TextMeshProUGUI comboText;
	[SerializeField] private TextMeshProUGUI bestScoreText;
	[SerializeField] private TextMeshProUGUI speedText;       // usado como Level

	[Header("Powerup HUD")]
	//Ícone de ímã que aparece enquanto o magnet estiver ativo
	[SerializeField] private GameObject magnetIconObject;
	//Ícone de energia dos tiles
	[SerializeField] private GameObject tileEnergyIconObject;

	[Header("Score Popup")]
	//Popup visual exibido quando a gem é coletada
	[SerializeField] private ScorePopup scorePopupPrefab;
	[SerializeField] private RectTransform scorePopupAnchor;

	//Dados da run atual
	private int score;
	private int gems;
	private int combo = 1;
	private int bestScore;
	private float speed;

	[Header("Coin Run Stats")]
	//"Normal" = moeda comum dourada do caminho
	//"AdjacentOnly" = moeda adjacente prateada, coletável com magnet
	private int normalCoinsCollected;
	private int adjacentCoinsCollected;

	[Header("Combo Settings")]
	//Tempo sem coletar gem até o combo resetar
	[SerializeField] private float comboResetTime = 3f;

	private float comboTimer = 0f;

	// =============================
	// PROPRIEDADES PÚBLICAS
	// =============================
	public int Score => score;
	public int Gems => gems;
	public int Combo => combo;
	public int BestScore => bestScore;
	public float Speed => speed;

	//Variáveis mantidas para compatibilidade com o restante do projeto
	public int NormalCoinsCollected => normalCoinsCollected;
	public int AdjacentCoinsCollected => adjacentCoinsCollected;


	//Variáveis auxiliares mais claras para a próxima etapa de commit no GameOverUI
	public int GoldCoinsCollected => normalCoinsCollected;
	public int SilverCoinsCollected => adjacentCoinsCollected;
	public int TotalRunCoins => normalCoinsCollected + adjacentCoinsCollected;

	private void OnEnable()
	{
		PlayerMagnet.MagnetStateChanged += HandleMagnetStateChanged;
		PlayerTileEnergy.TileEnergyStateChanged += HandleTileEnergyStateChanged;
	}

	private void OnDisable()
	{
		PlayerMagnet.MagnetStateChanged -= HandleMagnetStateChanged;
		PlayerTileEnergy.TileEnergyStateChanged -= HandleTileEnergyStateChanged;
	}

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
		//Carrega o best score salvo
		bestScore = PlayerPrefs.GetInt("BestScore", 0);

		//Atualiza referências ao iniciar
		RefreshBallReferences();

		//Sincroniza a UI com o estado inicial
		UpdateUI();

		UpdatePowerupHUD();
	}

	private void Update()
	{
		//Atualiza o "Level" na UI com base na velocidade da bola
		UpdateSpeedUI();

		//Controla o reset automático do combo
		UpdateComboTimer();
	}

	private void RefreshBallReferences()
	{
		//Rebusca a bola por tag caso a referência tenha se perdido
		GameObject ball = GameObject.FindGameObjectWithTag("Ball");

		if (ball != null) {
			ballScript = ball.GetComponent<BallScript>();
			playerCurrency = ball.GetComponent<PlayerCurrency>();
			playerMagnet = ball.GetComponent<PlayerMagnet>();
			playerTileEnergy = ball.GetComponent<PlayerTileEnergy>();
		} else {
			ballScript = null;
			playerCurrency = null;
			playerMagnet = null;
			playerTileEnergy = null;
		}
	}

	private void UpdateComboTimer()
	{
		//Só atualiza combo durante gameplay ativo
		if (!GameplayController.instance.gamePlaying) {
			return;
		}

		//Se não há combo acima de 1, não há motivo para contar tempo
		if (combo <= 1) {
			return;
		}

		comboTimer += Time.deltaTime;

		//Se passou muito tempo sem coletar gem, reseta o combo
		if (comboTimer >= comboResetTime) {
			ResetCombo();
		}
	}

	private void UpdateSpeedUI()
	{
		if (speedText == null) {
			return;
		}

		//Rebusca referência da bola caso necessário
		if (ballScript == null) {
			RefreshBallReferences();
		}

		if (ballScript == null) {
			speedText.text = "Level: 1";
			return;
		}

		//Usa o nível de velocidade atual vindo da BallScript
		speedText.text = "Level: " + ballScript.CurrentSpeedLevel;
	}

	public void AddTilePoint()
	{
		//Soma pontos ao percorrer um tile
		score += pointsPerTile;
		UpdateUI();
	}

	public void AddGem()
	{
		//Gems contam apenas para a run atual
		gems++;

		//A gem também dá score com multiplicador de combo
		int earnedPoints = pointsPerGem * combo;
		score += earnedPoints;

		//Popup visual de pontuação da gem
		SpawnScorePopup("+" + earnedPoints, Color.yellow);

		//Reinicia o timer do combo
		comboTimer = 0f;

		//Aumenta combo até o limite máximo
		if (combo < maxCombo) {
			combo++;
		}

		UpdateUI();
	}

	public void AddCoin(int amount, Coin.CoinType coinType)
	{
		if (amount <= 0) {
			return;
		}

		//Separa as moedas da run por tipo
		if (coinType == Coin.CoinType.Normal) {
			normalCoinsCollected += amount;
		} else if (coinType == Coin.CoinType.AdjacentOnly) {
			adjacentCoinsCollected += amount;
		}

		UpdateUI();
	}

	public void NotifyCoinsChanged()
	{
		//Chamado pelo PlayerCurrency para atualizar o HUD persistente
		UpdateUI();
	}

	public void ResetCombo()
	{
		combo = 1;
		UpdateUI();
	}

	public void GameOver()
	{
		//atualiza best score caso tenha superado o anterior
		if (score > bestScore) {
			bestScore = score;
			PlayerPrefs.SetInt("BestScore", bestScore);
			PlayerPrefs.Save();
		}

		UpdateUI();

		//Rebusca a bola para capturar o nível final corretamente
		if (ballScript == null) {
			RefreshBallReferences();
		}

		int finalLevel = 1;

		if (ballScript != null) {
			finalLevel = ballScript.CurrentSpeedLevel;
		}

		//Exibe a tela de GameOver com os dados da run
		if (GameOverUI.instance != null) {
			GameOverUI.instance.ShowGameOver(score, bestScore, finalLevel, gems);
		}
	}

	public void ResetRun()
	{
		//Reseta todos os dados temporários da run
		score = 0;
		gems = 0;
		combo = 1;
		speed = 0f;

		normalCoinsCollected = 0;
		adjacentCoinsCollected = 0;

		//Garante referência ao PlayerCurrency para a UI
		if (playerCurrency == null || playerMagnet == null) {
			RefreshBallReferences();
		}

		UpdateUI();
		UpdatePowerupHUD();
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
		//Score da run
		if (scoreText != null) {
			scoreText.text = "" + score;
		}

		//Gems da run
		if (gemsText != null) {
			gemsText.text = "" + gems;
		}

		//Total persistente de moedas
		if (coinsText != null) {
			if (playerCurrency == null) {
				RefreshBallReferences();
			}

			int currentCoins = 0;

			if (playerCurrency != null) {
				currentCoins = playerCurrency.Coins;
			}

			coinsText.text = "" + currentCoins;
		}

		//Gold coins da run
		if (goldCoinsText != null) {
			goldCoinsText.text = "" + normalCoinsCollected;
		}

		//Silver coins da run
		if (silverCoinsText != null) {
			silverCoinsText.text = "" + adjacentCoinsCollected;
		}

		//Combo atual
		if (comboText != null) {
			comboText.text = "Combo x" + combo;
		}

		//Melhor pontuação salva
		if (bestScoreText != null) {
			bestScoreText.text = "Best: " + bestScore;
		}

		UpdatePowerupHUD();
	}

	private void HandleMagnetStateChanged(bool isActive)
	{
		UpdateMagnetHUD(isActive);
	}

	private void HandleTileEnergyStateChanged(bool isActive)
	{
		UpdateTileEnergyHUD(isActive);
	}

	private void UpdatePowerupHUD()
	{
		if (playerMagnet == null || playerTileEnergy == null) {
			RefreshBallReferences();
		}

		bool isMagnetActive = playerMagnet != null && playerMagnet.IsMagnetActive;
		bool isTileEnergyActive = playerTileEnergy != null && playerTileEnergy.IsTileEnergyActive;

		UpdateMagnetHUD(isMagnetActive);
		UpdateTileEnergyHUD(isTileEnergyActive);
	}

	private void UpdateMagnetHUD(bool isMagnetActive)
	{
		if (magnetIconObject == null) {
			return;
		}

		if (magnetIconObject.activeSelf != isMagnetActive) {
			magnetIconObject.SetActive(isMagnetActive);
		}
	}

	private void UpdateTileEnergyHUD(bool isTileEnergyActive)
	{
		if (tileEnergyIconObject == null) {
			return;
		}

		if (tileEnergyIconObject.activeSelf != isTileEnergyActive) {
			tileEnergyIconObject.SetActive(isTileEnergyActive);
		}
	}
}
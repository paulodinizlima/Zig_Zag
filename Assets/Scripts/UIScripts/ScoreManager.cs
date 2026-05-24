using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
	public static ScoreManager instance;

	private BallScript ballScript;
	private PlayerCurrency playerCurrency;
	private PlayerMagnet playerMagnet;
	private PlayerTileEnergy playerTileEnergy;

	[Header("Score Values")]
	[SerializeField] private int pointsPerTile = 1;
	[SerializeField] private int pointsPerGem = 10;
	[SerializeField] private int maxCombo = 5;

	[Header("UI")]
	[SerializeField] private TextMeshProUGUI scoreText;
	[SerializeField] private TextMeshProUGUI gemsText;

	[SerializeField] private TextMeshProUGUI coinsText;
	[SerializeField] private TextMeshProUGUI goldCoinsText;
	[SerializeField] private TextMeshProUGUI silverCoinsText;

	[SerializeField] private TextMeshProUGUI comboText;
	[SerializeField] private TextMeshProUGUI bestScoreText;
	[SerializeField] private TextMeshProUGUI speedText;

	[Header("Powerup HUD")]
	[SerializeField] private GameObject magnetIconObject;
	[SerializeField] private GameObject tileEnergyIconObject;

	[Header("Score Popup")]
	[SerializeField] private ScorePopup scorePopupPrefab;
	[SerializeField] private RectTransform scorePopupAnchor;

	private int score;
	private int gems;
	private int combo = 1;
	private int bestScore;
	private float speed;

	[Header("Coin Run Stats")]
	private int normalCoinsCollected;
	private int adjacentCoinsCollected;

	[Header("Combo Settings")]
	[SerializeField] private float comboResetTime = 3f;

	private float comboTimer = 0f;

	public int Score => score;
	public int Gems => gems;
	public int Combo => combo;
	public int BestScore => bestScore;
	public float Speed => speed;

	public int NormalCoinsCollected => normalCoinsCollected;
	public int AdjacentCoinsCollected => adjacentCoinsCollected;

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
		if (instance != null && instance != this) {
			Destroy(gameObject);
			return;
		}

		instance = this;
	}

	private void Start()
	{
		bestScore = PlayerPrefs.GetInt("BestScore", 0);

		RefreshBallReferences();
		UpdateUI();
		UpdatePowerupHUD();
	}

	private void Update()
	{
		UpdateSpeedUI();
		UpdateComboTimer();
	}

	private void RefreshBallReferences()
	{
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
		if (GameplayController.instance == null || !GameplayController.instance.gamePlaying) {
			return;
		}

		if (combo <= 1) {
			return;
		}

		comboTimer += Time.deltaTime;

		if (comboTimer >= comboResetTime) {
			ResetCombo();
		}
	}

	private void UpdateSpeedUI()
	{
		if (speedText == null) {
			return;
		}

		if (ballScript == null) {
			RefreshBallReferences();
		}

		if (ballScript == null) {
			speedText.text = "Level: 1";
			return;
		}

		speedText.text = "Level: " + ballScript.CurrentSpeedLevel;
	}

	public void AddTilePoint()
	{
		score += pointsPerTile;
		UpdateUI();
	}

	public void AddGem()
	{
		gems++;

		int earnedPoints = pointsPerGem * combo;
		score += earnedPoints;

		SpawnScorePopup("+" + earnedPoints, Color.yellow);

		comboTimer = 0f;

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

		if (coinType == Coin.CoinType.Normal) {
			normalCoinsCollected += amount;
		} else if (coinType == Coin.CoinType.AdjacentOnly) {
			adjacentCoinsCollected += amount;
		}

		UpdateUI();
	}

	public void NotifyCoinsChanged()
	{
		UpdateUI();
	}

	public void ResetCombo()
	{
		combo = 1;
		UpdateUI();
	}

	public void GameOver()
	{
		if (score > bestScore) {
			bestScore = score;
			PlayerPrefs.SetInt("BestScore", bestScore);
			PlayerPrefs.Save();
		}

		UpdateUI();

		if (ballScript == null) {
			RefreshBallReferences();
		}

		int finalLevel = 1;

		if (ballScript != null) {
			finalLevel = ballScript.CurrentSpeedLevel;
		}

		if (GameOverUI.instance != null) {
			GameOverUI.instance.ShowGameOver(
				score,
				bestScore,
				finalLevel,
				gems
			);
		}
	}

	public void ResetRun()
	{
		score = 0;
		gems = 0;
		combo = 1;
		speed = 0f;
		comboTimer = 0f;

		normalCoinsCollected = 0;
		adjacentCoinsCollected = 0;

		if (playerCurrency == null || playerMagnet == null || playerTileEnergy == null) {
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
		if (scoreText != null) {
			scoreText.text = "" + score;
		}

		if (gemsText != null) {
			gemsText.text = "" + gems;
		}

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

		if (goldCoinsText != null) {
			goldCoinsText.text = "" + normalCoinsCollected;
		}

		if (silverCoinsText != null) {
			silverCoinsText.text = "" + adjacentCoinsCollected;
		}

		if (comboText != null) {
			comboText.text = "Combo x" + combo;
		}

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
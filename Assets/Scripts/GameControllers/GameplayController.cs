using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayController : MonoBehaviour
{
	public static GameplayController instance;

	[Header("Game State")]
	[HideInInspector] public bool gamePlaying = false;

	[Header("Tile Spawn")]
	[SerializeField] private GameObject tilePrefab;
	[SerializeField] private GameObject[] tileVariants;
	[SerializeField] private int initialTileCount = 20;
	[SerializeField] private float tileSpawnDelay = 0.32f;
	[SerializeField] private float minTileSpawnDelay = 0.25f;
	[SerializeField] private float maxBallSpeedForSpawnScaling = 3.2f;
	[SerializeField] private Vector3 startTilePosition = new Vector3(-2f, 0f, 2f);

	[Header("Risk Path")]
	[SerializeField] private GameObject riskTilePrefab;
	[SerializeField] private float riskPathChance = 0.08f;
	[SerializeField] private int minTilesBetweenRiskPaths = 25;
	[SerializeField] private int riskPathLength = 4;

	private int tilesSinceLastRiskPath = 0;

	[Header("Pooling")]
	[SerializeField] private TilePool tilePool;

	[Header("Phase End / Portal")]
	[SerializeField] private float distanceToTriggerEndPhase = 8f;
	[SerializeField] private int extraTilesAfterEndPhase = 8;
	[SerializeField] private GameObject portalPrefab;
	[SerializeField] private float portalYOffset = 1.2f;
	[SerializeField] private GameObject gameplayHUDRoot;

	[Header("References")]
	[SerializeField] private Material tileMat;
	[SerializeField] private Light dayLight;
	[SerializeField] private AudioSource audioSource;

	[Header("Visual Cycle")]
	[SerializeField] private float visualCycleInterval = 4f;
	[SerializeField] private float visualLerpDuration = 1f;

	private Camera mainCamera;
	private Vector3 currentTilePosition;
	private Coroutine tileSpawnRoutine;
	private BallScript ballScript;

	private Color originalCameraColor;
	private Color originalTileColor;

	private Color[] dayTileColors;
	private Color nightTileColor;

	private int tileColorIndex = 0;

	private bool isLerpingVisual = false;
	private float visualCycleTimer = 0f;
	private float visualLerpTimer = 0f;
	private int visualDirection = 1;

	private float runTime = 0f;
	public float RunTime => runTime;

	private bool tileSpawningActive = false;
	private bool endPhaseActive = false;
	private int spawnedTilesAfterEndPhase = 0;
	private bool phaseWon = false;
	private bool portalCanWinNow = false;

	private GameObject currentPortal;
	private PortalController currentPortalController;

	private readonly List<GameObject> recentSpawnedTiles = new List<GameObject>();

	private void Awake()
	{
		SetupSingleton();
		SetupReferences();
		SetupColors();
	}

	private void Start()
	{
		if (tilePool == null) {
			tilePool = TilePool.instance;
		}

		ResetPhaseState();
		currentTilePosition = startTilePosition;
		CreateInitialTiles();
		RefreshBallReference();
	}

	private void Update()
	{
		if (gamePlaying) {
			runTime += Time.deltaTime;
		}

		// Deixe comentado para performance no mobile.
		UpdateVisualCycle();

		if (ballScript == null) {
			RefreshBallReference();
		}

		CheckForEndPhase();
		UpdatePortalPosition();
	}

	private void OnDisable()
	{
		if (instance == this) {
			instance = null;
		}

		ResetVisuals();
	}

	public void HideGameplayHUD()
	{
		if (gameplayHUDRoot != null) {
			gameplayHUDRoot.SetActive(false);
		}
	}

	public void StartGameplay()
	{
		if (gamePlaying) {
			return;
		}

		Time.timeScale = 1f;
		runTime = 0f;

		if (GameMusicManager.instance != null) {
			GameMusicManager.instance.PlayGameplayMusic();
		}

		if (gameplayHUDRoot != null) {
			gameplayHUDRoot.SetActive(true);
		}

		ResetPhaseState();

		gamePlaying = true;
		tileSpawningActive = true;

		ActiveTileSpawner();

		if (ScoreManager.instance != null) {
			ScoreManager.instance.ResetRun();
		}
	}

	private void SetupSingleton()
	{
		if (instance != null && instance != this) {
			Destroy(gameObject);
			return;
		}

		instance = this;
	}

	private void SetupReferences()
	{
		if (audioSource == null) {
			audioSource = GetComponent<AudioSource>();
		}

		if (tilePool == null) {
			tilePool = TilePool.instance;
		}

		mainCamera = Camera.main;

		if (mainCamera != null) {
			originalCameraColor = mainCamera.backgroundColor;
		}

		if (tileMat != null) {
			originalTileColor = tileMat.color;
		}
	}

	private void SetupColors()
	{
		dayTileColors = new Color[3];
		dayTileColors[0] = new Color(10f / 256f, 139f / 256f, 203f / 256f);
		dayTileColors[1] = new Color(10f / 256f, 200f / 256f, 20f / 256f);
		dayTileColors[2] = new Color(220f / 256f, 170f / 256f, 45f / 256f);

		nightTileColor = new Color(1f, 20f / 256f, 11f / 256f);

		if (tileMat != null) {
			tileMat.color = dayTileColors[0];
		}
	}

	private void RefreshBallReference()
	{
		GameObject ball = GameObject.FindGameObjectWithTag("Ball");

		if (ball != null) {
			ballScript = ball.GetComponent<BallScript>();
		}
	}

	private void ResetPhaseState()
	{
		endPhaseActive = false;
		spawnedTilesAfterEndPhase = 0;
		tileSpawningActive = false;
		phaseWon = false;
		portalCanWinNow = false;

		recentSpawnedTiles.Clear();

		if (currentPortal != null) {
			Destroy(currentPortal);
			currentPortal = null;
		}

		currentPortalController = null;
	}

	private void CreateInitialTiles()
	{
		for (int i = 0; i < initialTileCount; i++) {
			CreateTile();
		}
	}

	private float GetCurrentTileSpawnDelay()
	{
		if (ballScript == null) {
			return tileSpawnDelay;
		}

		float speed = ballScript.CurrentSpeed;
		float t = Mathf.InverseLerp(3f, maxBallSpeedForSpawnScaling, speed);

		return Mathf.Lerp(tileSpawnDelay, minTileSpawnDelay, t);
	}

	private void CreateTile()
	{
		Vector3 newTilePosition = currentTilePosition;
		Vector3 tileDirection;

		int rand = Random.Range(0, 100);

		if (rand < 50) {
			newTilePosition.x -= 1f;
			tileDirection = Vector3.left;
		} else {
			newTilePosition.z += 1f;
			tileDirection = Vector3.forward;
		}

		currentTilePosition = newTilePosition;

		GameObject prefabToSpawn = GetRandomTilePrefab();

		if (prefabToSpawn == null) {
			//Debug.LogWarning("Nenhum prefab de tile configurado no GameplayController.");
			return;
		}

		GameObject newTile = null;

		if (tilePool != null) {
			newTile = tilePool.GetTile(prefabToSpawn, currentTilePosition, Quaternion.identity);
		}

		if (newTile == null) {
			//Debug.LogWarning("TilePool falhou ou não foi encontrado. Usando Instantiate como fallback.");
			newTile = Instantiate(prefabToSpawn, currentTilePosition, Quaternion.identity);
		}

		TileScript tileScript = newTile.GetComponent<TileScript>();

		if (tileScript != null) {
			tileScript.SetTilePathDirection(tileDirection);
		} else {
			//Debug.LogWarning("O tile criado não possui TileScript: " + newTile.name);
		}

		RegisterRecentTile(newTile);

		if (endPhaseActive && tileSpawningActive) {
			spawnedTilesAfterEndPhase++;

			if (spawnedTilesAfterEndPhase >= extraTilesAfterEndPhase) {
				tileSpawningActive = false;
				EnablePortalWin();
			}
		}

		// Deixe desativado por enquanto, até estabilizar a performance.
		
		if (DecorSpawner.instance != null) {
			DecorSpawner.instance.TrySpawnDecorNear(currentTilePosition);
		}

		tilesSinceLastRiskPath++;

		TryGenerateRiskPath(currentTilePosition, tileDirection);

	}

	private void TryGenerateRiskPath(Vector3 startPosition, Vector3 mainDirection)
	{
		if (riskTilePrefab == null)
			return;

		if (tilesSinceLastRiskPath < minTilesBetweenRiskPaths)
			return;

		if (Random.value > riskPathChance)
			return;

		GenerateRiskPath(startPosition, mainDirection);

		tilesSinceLastRiskPath = 0;
	}

	private void GenerateRiskPath(Vector3 startPosition, Vector3 mainDirection)
	{
		Vector3 sideDirection;

		// Decide direção lateral
		if (mainDirection == Vector3.forward) {
			sideDirection = Vector3.left;
		} else {
			sideDirection = Vector3.forward;
		}

		Vector3 currentRiskPosition = startPosition;

		int sideTiles = Mathf.Max(2, riskPathLength / 2);
		int reconnectTiles = riskPathLength - sideTiles;

		// PRIMEIRA PARTE:
		// afasta lateralmente
		for (int i = 0; i < sideTiles; i++) {

			currentRiskPosition += sideDirection;

			SpawnRiskTile(currentRiskPosition, sideDirection);
		}

		// SEGUNDA PARTE:
		// acompanha direção principal
		for (int i = 0; i < reconnectTiles; i++) {

			currentRiskPosition += mainDirection;

			SpawnRiskTile(currentRiskPosition, mainDirection);
		}
	}

	private void SpawnRiskTile(Vector3 position, Vector3 direction)
	{
		GameObject riskTile = null;

		if (tilePool != null) {
			riskTile = tilePool.GetTile(
				riskTilePrefab,
				position,
				Quaternion.identity
			);
		}

		if (riskTile == null) {
			riskTile = Instantiate(
				riskTilePrefab,
				position,
				Quaternion.identity
			);
		}

		TileScript tileScript = riskTile.GetComponent<TileScript>();

		if (tileScript != null) {
			tileScript.SetTilePathDirection(direction);
		}
	}

	private GameObject GetRandomTilePrefab()
	{
		if (tileVariants != null && tileVariants.Length > 0) {
			int safety = 0;

			while (safety < 10) {
				int index = Random.Range(0, tileVariants.Length);
				GameObject candidate = tileVariants[index];

				if (candidate != null) {
					return candidate;
				}

				safety++;
			}
		}

		return tilePrefab;
	}

	private void RegisterRecentTile(GameObject newTile)
	{
		if (newTile == null) {
			return;
		}

		recentSpawnedTiles.Add(newTile);

		if (recentSpawnedTiles.Count > 8) {
			recentSpawnedTiles.RemoveAt(0);
		}
	}

	private void CleanupRecentTiles()
	{
		for (int i = recentSpawnedTiles.Count - 1; i >= 0; i--) {
			if (recentSpawnedTiles[i] == null || !recentSpawnedTiles[i].activeInHierarchy) {
				recentSpawnedTiles.RemoveAt(i);
			}
		}
	}

	public void ActiveTileSpawner()
	{
		if (tileSpawnRoutine == null) {
			tileSpawnRoutine = StartCoroutine(SpawnTilesRoutine());
		}
	}

	private IEnumerator SpawnTilesRoutine()
	{
		while (gamePlaying && tileSpawningActive) {
			yield return new WaitForSeconds(GetCurrentTileSpawnDelay());
			CreateTile();
		}

		tileSpawnRoutine = null;
	}

	private void CheckForEndPhase()
	{
		if (!gamePlaying || endPhaseActive || phaseWon) {
			return;
		}

		if (ballScript == null) {
			return;
		}

		float distanceToEnd = Vector3.Distance(ballScript.transform.position, currentTilePosition);

		if (distanceToEnd <= distanceToTriggerEndPhase) {
			ActivateEndPhase();
		}
	}

	private void ActivateEndPhase()
	{
		if (endPhaseActive) {
			return;
		}

		endPhaseActive = true;
		spawnedTilesAfterEndPhase = 0;
		portalCanWinNow = false;

		SpawnPortalIfNeeded();
		UpdatePortalPosition();
	}

	private void SpawnPortalIfNeeded()
	{
		if (portalPrefab == null) {
			return;
		}

		if (currentPortal != null) {
			return;
		}

		Vector3 portalPos = GetPortalWorldPosition();
		currentPortal = Instantiate(portalPrefab, portalPos, Quaternion.identity);

		currentPortalController = currentPortal.GetComponent<PortalController>();

		if (currentPortalController != null) {
			currentPortalController.SetCanWin(false);
		}
	}

	private void EnablePortalWin()
	{
		portalCanWinNow = true;

		if (currentPortalController != null) {
			currentPortalController.SetCanWin(true);
		}
	}

	private void UpdatePortalPosition()
	{
		if (!endPhaseActive || currentPortal == null || phaseWon) {
			return;
		}

		currentPortal.transform.position = GetPortalWorldPosition();
	}

	private Vector3 GetPortalWorldPosition()
	{
		CleanupRecentTiles();

		Transform referenceTile = GetPortalReferenceTileTransform();
		Vector3 portalPos;

		if (referenceTile != null) {
			portalPos = referenceTile.position;
		} else {
			portalPos = currentTilePosition;
		}

		portalPos.y += portalYOffset;
		return portalPos;
	}

	private Transform GetPortalReferenceTileTransform()
	{
		CleanupRecentTiles();

		if (recentSpawnedTiles.Count == 0) {
			return null;
		}

		int targetIndex = Mathf.Max(0, recentSpawnedTiles.Count - 3);

		GameObject tileObj = recentSpawnedTiles[targetIndex];

		if (tileObj == null || !tileObj.activeInHierarchy) {
			return null;
		}

		return tileObj.transform;
	}

	public void TriggerWin()
	{
		if (phaseWon || !portalCanWinNow) {
			return;
		}

		phaseWon = true;
		gamePlaying = false;
		tileSpawningActive = false;

		if (gameplayHUDRoot != null) {
			gameplayHUDRoot.SetActive(false);
		}

		if (currentPortal != null) {
			Destroy(currentPortal);
			currentPortal = null;
		}

		int finalScore = 0;
		int bestScore = 0;
		int finalGems = 0;
		int finalGoldCoins = 0;
		int finalSilverCoins = 0;
		int finalLevel = 1;

		if (ScoreManager.instance != null) {
			finalScore = ScoreManager.instance.Score;
			bestScore = ScoreManager.instance.BestScore;
			finalGems = ScoreManager.instance.Gems;
			finalGoldCoins = ScoreManager.instance.NormalCoinsCollected;
			finalSilverCoins = ScoreManager.instance.AdjacentCoinsCollected;
		}

		if (ballScript != null) {
			finalLevel = ballScript.CurrentSpeedLevel;
		}

		if (finalScore > bestScore) {
			bestScore = finalScore;
			PlayerPrefs.SetInt("BestScore", bestScore);
			PlayerPrefs.Save();
		}

		if (VictoryUI.instance != null) {
			VictoryUI.instance.ShowVictory(
				finalScore,
				bestScore,
				finalLevel,
				finalGems,
				finalGoldCoins,
				finalSilverCoins,
				RunTime
			);
		} else {
			Time.timeScale = 0f;
		}
	}

	private void UpdateVisualCycle()
	{
		visualCycleTimer += Time.deltaTime;

		if (visualCycleTimer >= visualCycleInterval) {
			visualCycleTimer -= visualCycleInterval;
			isLerpingVisual = true;
			visualLerpTimer = 0f;
		}

		if (!isLerpingVisual) {
			return;
		}

		visualLerpTimer += Time.deltaTime;

		float percent = visualLerpTimer / visualLerpDuration;
		percent = Mathf.Clamp01(percent);

		ApplyVisualLerp(percent);

		if (percent >= 1f) {
			isLerpingVisual = false;
			visualDirection *= -1;

			if (visualDirection == -1) {
				tileColorIndex = Random.Range(0, dayTileColors.Length);
			}
		}
	}

	private void ApplyVisualLerp(float percent)
	{
		if (mainCamera == null || tileMat == null || dayLight == null) {
			return;
		}

		if (visualDirection == 1) {
			mainCamera.backgroundColor = Color.Lerp(originalCameraColor, Color.black, percent);
			tileMat.color = Color.Lerp(dayTileColors[tileColorIndex], nightTileColor * 2f, percent);
			dayLight.intensity = 1f - percent;
		} else {
			mainCamera.backgroundColor = Color.Lerp(Color.black, originalCameraColor, percent);
			tileMat.color = Color.Lerp(nightTileColor, dayTileColors[tileColorIndex], percent);
			dayLight.intensity = percent;
		}
	}

	private void ResetVisuals()
	{
		if (mainCamera != null) {
			mainCamera.backgroundColor = originalCameraColor;
		}

		if (tileMat != null) {
			tileMat.color = originalTileColor;
		}
	}

	public void PlayCollectableSound()
	{
		if (audioSource != null) {
			audioSource.Play();
		}
	}
}
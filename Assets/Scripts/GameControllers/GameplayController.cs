using System.Collections;
using UnityEngine;

public class GameplayController : MonoBehaviour
{
	//Instância global (singleton)
	public static GameplayController instance;

	[Header("Game State")]
	//Controla se o jogo está ativo (bola se movendo, tiles spawnando, etc.)
	[HideInInspector] public bool gamePlaying = false;

	[Header("Tile Spawn")]
	//Prefab do tile que será instanciado
	[SerializeField] private GameObject tilePrefab;

	//Quantidade inicial de tiles ao iniciar a cena
	[SerializeField] private int initialTileCount = 20;

	//Intervalo entre spawns de novos tiles
	[SerializeField] private float tileSpawnDelay = 0.3f;

	//Menor delay possível entre spawns quando a bola estiver muito rápida
	[SerializeField] private float minTileSpawnDelay = 0.05f;

	//Velocidade máxima considerada para escalar o spawn
	[SerializeField] private float maxBallSpeedForSpawnScaling = 5f;

	//Posição inicial do primeiro tile
	[SerializeField] private Vector3 startTilePosition = new Vector3(-2f, 0f, 2f);

	[Header("References")]
	//Material usado nos tiles (para trocar cor dinamicamente)
	[SerializeField] private Material tileMat;

	//Luz direcional (para simular dia/noite)
	[SerializeField] private Light dayLight;

	//Fonte de áudio (som de coleta)
	[SerializeField] private AudioSource audioSource;

	[Header("Visual Cycle")]
	//Tempo entre cada ciclo visual (dia -> noite -> dia)
	[SerializeField] private float visualCycleInterval = 4f;

	//Tempo que leva para fazer a transição (Lerp)
	[SerializeField] private float visualLerpDuration = 1f;

	//Referência da câmera principal
	private Camera mainCamera;

	//Posição atual usada para gerar o próximo tile
	private Vector3 currentTilePosition;

	//Referência da coroutine de spawn
	private Coroutine tileSpawnRoutine;

	//Referência para o script da bola, usada para ler a velocidade atual
	private BallScript ballScript;

	//Cores originis para restaurar depois
	private Color originalCameraColor;
	private Color originalTileColor;

	//Array de cores para o modo "dia"
	private Color[] dayTileColors;

	//Array de cores para o modo "noite"
	private Color nightTileColor;

	//Índice da cor atual no array de dia
	private int tileColorIndex = 0;

	//Controle do ciclo visual
	private bool isLerpingVisual = false;
	private float visualCycleTimer = 0f;
	private float visualLerpTimer = 0f;

	//Direção do ciclo:
	//1 - indo para noite
	//2 - voltando para o dia
	private int visualDirection = 1;

	private void Awake()
	{
		//Configura singleton
		SetupSingleton();

		//Busca referências (câmera, áudio, etc)
		SetupReferences();

		//Inicializa cores
		SetupColors();
	}

	private void Start()
	{
		//Define posição iniciao do primeiro tile
		currentTilePosition = startTilePosition;

		//Cria os tiles iniciais do mapa
		CreateInitialTiles();

		//Procura o objeto da bola pela tag "Ball"
		GameObject ball = GameObject.FindGameObjectWithTag("Ball");

		//Se encontrou a bola, pega o script BallScript dela
		if(ball != null) {
			ballScript = ball.GetComponent<BallScript>();
		}
	}

	//Inicia oficialmente o gameplay
	public void StartGameplay()
	{
		//Evita iniciar duas vezes
		if (gamePlaying) {
			return;
		}
		//Garante que o tempo está rodando(importante após GameOver)
		Time.timeScale = 1f;
		//Ativa o gameplay
		gamePlaying = true;

		//Ativa o spawn contínuo de tiles
		ActiveTileSpawner();

		//Se existir ScoreManager, reseta a run atual
		if (ScoreManager.instance != null) {
			ScoreManager.instance.ResetRun();
		}
	}

	private void Update()
	{
		//Atualiza o ciclo visual (dia/noite)
		UpdateVisualCycle();
	}

	private void OnDisable()
	{
		//Limpa a instância se esse objeto for destruído
		if (instance == this) {
			instance = null;
		}

		//Restaura cores originais
		ResetVisuals();
	}

	//=====================
	//SETUP
	//=====================

	private void SetupSingleton()
	{
		//Evita múltiplas instâncias
		if(instance != null && instance != this) {
			Destroy(gameObject);
			return;
		}
		instance = this;
	}

	private void SetupReferences()
	{
		//Se não tiver AudioSource, tenta pegar do próprio objeto
		if(audioSource == null) {
			audioSource = GetComponent<AudioSource>();
		}

		//Pega a câmera principal
		mainCamera = Camera.main;

		//Guarda a cor original da câmera
		if (mainCamera != null) {
			originalCameraColor = mainCamera.backgroundColor;
		}

		//Guarda a cor original do material do tile
		if (tileMat != null) {
			originalTileColor = tileMat.color;
		}
	}

	private void SetupColors()
	{
		//Define cores possíveis para o modo "dia"
		dayTileColors = new Color[3];

		dayTileColors[0] = new Color(10f / 256f, 139f / 256f, 203f / 256f);
		dayTileColors[1] = new Color(10f / 256f, 200f / 256f, 20f / 256f);
		dayTileColors[2] = new Color(220f / 256f, 170f / 256f, 45f / 256f);

		//Define cor do modo "noite"
		nightTileColor = new Color(1f, 20f / 256f, 11f / 256f);

		//Aplica cor inicial
		if (tileMat != null) {
			tileMat.color = dayTileColors[0];
		}
	}

	//========================
	// TILE SPAWN
	//========================

	private void CreateInitialTiles()
	{
		//Cria vários tiles no início do jogo
		for (int i = 0; i < initialTileCount; i++) {
			CreateTile();
		}
	}

	//Calcula dinamicamente o delay de spawn com base na velocidade da bola
	private float GetCurrentTileSpawnDelay()
	{
		//Se não encontrou a bola, usa o delay padrão
		if (ballScript == null) {
			return tileSpawnDelay;
		}
		//Lê a velocidade atual da bola
		float speed = ballScript.CurrentSpeed;
		//Converte a velocidade para uma escala de 0 até 1
		//Exemplo:
		//velocidade baixa	->	t perto de 0
		//velocidade alta	->	t perto de 1
		float t = Mathf.InverseLerp(3f, maxBallSpeedForSpawnScaling, speed);

		//Faz o delay diminuir conforme a velocidade aumenta
		return Mathf.Lerp(tileSpawnDelay, minTileSpawnDelay, t);
	}

	private void CreateTile()
	{
		//Copia a posição atual
		Vector3 newTilePosition = currentTilePosition;
		Vector3 tileDirection;

		//Decide aleatoriamente a direção
		int rand = Random.Range(0, 100);

		if (rand < 50) {
			//Move para a esquerda (eixo X)
			newTilePosition.x -= 1f;
			tileDirection = Vector3.left;
		} else {
			//Move para a frente (eixo Z)
			newTilePosition.z += 1f;
			tileDirection = Vector3.forward;
		}

		//Atualiza posição atual
		currentTilePosition = newTilePosition;

		//Instancia o tile
		GameObject newTile = Instantiate(tilePrefab, currentTilePosition, Quaternion.identity);

		TileScript tileScript = newTile.GetComponent<TileScript>();
		if (tileScript != null) {
			tileScript.SetTilePathDirection(tileDirection);
		}

		//Tenta criar uma decoração perto do tile recém-criado
		if (DecorSpawner.instance != null) {
			DecorSpawner.instance.TrySpawnDecorNear(currentTilePosition);
		}
	}

	public void ActiveTileSpawner()
	{
		//Inicia a coroutine de spawn apenas se não estiver rodando
		if (tileSpawnRoutine == null) {
			tileSpawnRoutine = StartCoroutine(SpawnTilesRoutine());
		}
	}

	private IEnumerator SpawnTilesRoutine()
	{
		//Enquanto o jogo estiver ativo, continua spawnando tiles
		while (gamePlaying) {
			//Espera um tempo que varia de acordo com a velocidade da bola
			yield return new WaitForSeconds(GetCurrentTileSpawnDelay());
			//Debug.Log(GetCurrentTileSpawnDelay());
			//Debug.Log(ballScript.CurrentSpeed);
			//Cria mais um tile
			CreateTile();
		}

		//Quando parar, limpa referência
		tileSpawnRoutine = null;
	}


	//================================
	// VISUAL (DIA / NOITE)
	//================================

	private void UpdateVisualCycle()
	{
		//Atualiza o timer principal
		visualCycleTimer += Time.deltaTime;

		//Quando atinge o intervalo, inicia transição
		if (visualCycleTimer >= visualCycleInterval) {
			visualCycleTimer -= visualCycleInterval;
			isLerpingVisual = true;
			visualLerpTimer = 0f;
		}

		//Se não está em transição, sai
		if (!isLerpingVisual) {
			return;
		}

		//Atualiza o tempo da transição
		visualLerpTimer += Time.deltaTime;

		float percent = visualLerpTimer / visualLerpDuration;
		percent = Mathf.Clamp01(percent);

		//Aplica o Lerp visual
		ApplyVisualLerp(percent);

		//Se terminou a transição
		if (percent >= 1f) {
			isLerpingVisual = false;

			//Inverte direção (dia / noite)
			visualDirection *= -1;

			//Se voltou para dia, escolhe nova cor
			if(visualDirection == -1) {
				tileColorIndex = Random.Range(0, dayTileColors.Length);
			}
		}
	}

	private void ApplyVisualLerp(float percent)
	{
		//Evita erro se referências estiverem faltando
		if (mainCamera == null || tileMat == null || dayLight == null) {
			return;
		}
		
		if (visualDirection == 1) {
			//Debug.Log("entrou noite");
			//Dia -> Noite
			mainCamera.backgroundColor = Color.Lerp(originalCameraColor, Color.black, percent);
			tileMat.color = Color.Lerp(dayTileColors[tileColorIndex], nightTileColor * 2f, percent);
			dayLight.intensity = 1f - percent;
		} else {
			//Debug.Log("entrou dia");
			//Noite -> Dia
			mainCamera.backgroundColor = Color.Lerp(Color.black, originalCameraColor, percent);
			tileMat.color = Color.Lerp(nightTileColor, dayTileColors[tileColorIndex], percent);
			dayLight.intensity = percent;
		}
	}

	private void ResetVisuals()
	{
		//Restaura cor da câmera
		if (mainCamera != null) {
			mainCamera.backgroundColor = originalCameraColor;
		}

		//Restaura cor dos tiles
		if (tileMat != null) {
			tileMat.color = originalTileColor;
		}
	}

	//==============================
	// AUDIO
	//==============================

	public void PlayCollectableSound()
	{
		//Toca som de coleta
		if (audioSource != null) {
			audioSource.Play();
		}
	}




} //class

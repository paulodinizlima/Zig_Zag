using UnityEngine;

public class BallScript : MonoBehaviour
{
	[Header("Movement")]
	[SerializeField] private float speed = 3.5f;

	[Header("Speed Progression")]
	// Velocidade inicial da bola
	[SerializeField] private float startSpeed = 3.0f;
	//Quanto a velocidade aumenta por segundo
	[SerializeField] private float speedIncreasePerSecond = 0.01f;
	// Velocidade máxima que a bola pode atingir
	[SerializeField] private float maxSpeed = 5f;
	// Quantidade de velocidade necessária para subir 1 nível visual
	[SerializeField] private float speedPerLevel = 0.5f;
	//Velocidade atual em tempo real
	private float currentSpeed;

	[Tooltip("Direção usada quando a bola está indo para a esquerda no mapa.")]
	[SerializeField] private Vector3 leftDirection = new Vector3(-1f, 0f, 0f);

	[Tooltip("Direção usada quando a bola está indo para frente no mapa.")]
	[SerializeField] private Vector3 forwardDirection = new Vector3(0f, 0f, 1f);

	[Header("Fall")]
	[SerializeField] private float destroyY = -3f;

	[Header("Optional Visual")]
	[SerializeField] private Transform visualToRotate;
	[SerializeField] private float visualRotationSpeed = 720f;

	[Header("Turn Impact")]
	//Objeto visual da bola que vai sofrer o squash/stretch
	[SerializeField] private Transform ballVisual;
	//Escala horizontal durante o impacto
	[SerializeField] private float turnSquashX = 1.15f;
	//Escala vertical durante o impacto
	[SerializeField] private float turnSquashY = 0.85f;
	//Escala no eixo Z durante o impacto
	[SerializeField] private float turnSquashZ = 1.15f;
	//Duração da animação de impacto
	[SerializeField] private float turnImpactDuration = 0.08f;
	//Guarda a escala original do visual
	private Vector3 originalVisualScale;
	//Controla a coroutine atual para não empilhar várias
	private Coroutine turnImpactRoutine;

	[Header("Magnet Visual Feedback")]
	//Multiplicador aplicado ao emissive original quando o magnet estiver ativo
	[SerializeField] private float magnetEmissionMultiplier = 2.0f;
	//Cor adicional de energia para o magnet
	[SerializeField] private Color magnetEmissionTint = new Color(0.25f, 0.85f, 1.0f);
	//Velocidade da transição visual do magnet
	[SerializeField] private float magnetVisualLerpSpeed = 10f;

	//Controle interno do material da bola
	private Renderer ballRenderer;
	private Material runtimeBallMaterial;
	private bool hasEmissionProperty = false;
	private Color originalEmissionColor = Color.black;
	private Color targetEmissionColor = Color.black;
	private Color currentEmissionColor = Color.black;
	private bool magnetVisualActive = false;

	//[Header("Grid Correction")]
	//Tamanho da grade usada para alinhar a bola nos eixos
	//[SerializeField] private float gridSize = 1f;

	//Retorna a velocidade atual da bola
	public float CurrentSpeed => currentSpeed;

	private bool isMovingLeft = true;
	private Vector3 currentDirection;

	private void OnEnable()
	{
		PlayerMagnet.MagnetStateChanged += HandleMagnetStateChanged;		
	}

	private void OnDisable()
	{
		PlayerMagnet.MagnetStateChanged -= HandleMagnetStateChanged;	
	}

	private void Awake()
	{
		//Começa a run com a velocidade inicial definida no Inspector
		currentSpeed = startSpeed;

		leftDirection = leftDirection.normalized;
		forwardDirection = forwardDirection.normalized;
		currentDirection = leftDirection;

		//Guarda a escala original do visual da bola
		if (ballVisual != null) {
			originalVisualScale = ballVisual.localScale;
		}

		SetupBallMaterial();
	}

	private void Start()
	{
		//Sincroniza o visual inicial com o estado atual do magnet, caso necessário
		PlayerMagnet playerMagnet = GetComponent<PlayerMagnet>();
		if (playerMagnet != null) {
			ApplyMagnetVisualState(playerMagnet.IsMagnetActive, true);
		}
	}

	private void Update()
	{
		HandleInput();
		CheckBallOutOfBounds();

		//Atualiza a progressão da velocidade da bola
		UpdateSpeed();

		//Atualiza a transição suave do emissive do magnet
		UpdateMagnetVisual();
	}

	private void FixedUpdate()
	{
		if (!GameplayController.instance.gamePlaying) {
			return;
		}

		MoveBall();
		RotateVisual();
	}
	
	//Converte a velocidade atual em um nível visual
	public int CurrentSpeedLevel
	{
		get {
			return Mathf.FloorToInt((currentSpeed - startSpeed) / speedPerLevel) + 1;
		}
	}

	//Atualiza a velocidade da bola ao longo do tempo
	private void UpdateSpeed()
	{
		//Só acelera se o jogo estiver em andamento
		if (!GameplayController.instance.gamePlaying) {
			return;
		}

		//Aumenta a velocidade continuamente
		currentSpeed += speedIncreasePerSecond * Time.deltaTime;

		//Limita a velocidade entre a inicial e a máxima
		currentSpeed = Mathf.Clamp(currentSpeed, startSpeed, maxSpeed);
	}

	private void HandleInput()
	{
		if (!TurnInputPressed()) {
			return;
		}

		//Só permite virar se o jogo já estiver em andamento
		if (!GameplayController.instance.gamePlaying) {
			return;
		}

		if (Input.GetMouseButtonDown(0) ||
			Input.GetKeyDown(KeyCode.Space) ||
			Input.GetKeyDown(KeyCode.A) ||
			Input.GetKeyDown(KeyCode.D) ||
			Input.GetKeyDown(KeyCode.LeftArrow) ||
			Input.GetKeyDown(KeyCode.RightArrow)) {

			ToggleDirection();
		}
	}

	private bool TurnInputPressed()
	{
		return Input.GetMouseButtonDown(0)
			|| Input.GetKeyDown(KeyCode.Space)
			|| Input.GetKeyDown(KeyCode.LeftArrow)
			|| Input.GetKeyDown(KeyCode.RightArrow)
			|| Input.GetKeyDown(KeyCode.A)
			|| Input.GetKeyDown(KeyCode.D);
	}

	private void ToggleDirection()
	{
		//Alterna a direção
		isMovingLeft = !isMovingLeft;
		currentDirection = isMovingLeft ? leftDirection : forwardDirection;

		//Corrige a posiçao da bola na grade para evitar drift diagonal
		//SnapToGridAfterTurn();

		//Toca o efeito visual de impacto ao virar
		PlayTurnImpact();
	}

	private void MoveBall()
	{
		transform.position += currentDirection * currentSpeed * Time.fixedDeltaTime;
	}

	private void RotateVisual()
	{
		if (visualToRotate == null)
			return;

		visualToRotate.Rotate(Vector3.right * visualRotationSpeed * Time.fixedDeltaTime, Space.Self);
	}

	//Dispara o efeito visual de impacto ao virar
	private void PlayTurnImpact()
	{
		//Se não houver visual definido, não faz nada
		if (ballVisual == null) {
			return;
		}
		//Se já existir uma animação rodando, interrompe para começar de novo
		if (turnImpactRoutine != null) {
			StopCoroutine(turnImpactRoutine);
		}
		//Inicia a animação
		turnImpactRoutine = StartCoroutine(TurnImpactCoroutine());
	}

	//Faz o squash/stretch da bola e retorna ao normal
	private System.Collections.IEnumerator TurnImpactCoroutine()
	{
		//Escala de impacto
		Vector3 impactScale = new Vector3(turnSquashX, turnSquashY, turnSquashZ);
		float halfDuration = turnImpactDuration * 0.5f;
		float timer = 0f;

		//Primeira metade: vai da escala normal até a escala de impacto
		while (timer < halfDuration) {
			timer += Time.deltaTime;
			float t = Mathf.Clamp01(timer / halfDuration);
			ballVisual.localScale = Vector3.Lerp(originalVisualScale, impactScale, t);
			yield return null;
		}

		//Garante que chegou exatamente na escala de impacto
		ballVisual.localScale = impactScale;
		timer = 0f;

		//Segunda metade: volta da escala de impacto para a escala original
		while (timer < halfDuration) {
			timer += Time.deltaTime;
			float t = Mathf.Clamp01(timer / halfDuration);
			ballVisual.localScale = Vector3.Lerp(impactScale, originalVisualScale, t);
			yield return null;
		}

		//Garante que terminou exatamente na escala original
		ballVisual.localScale = originalVisualScale;

		turnImpactRoutine = null;
	}

	private void SetupBallMaterial()
	{
		//Tenta pegar o Renderer do visual principal
		if (ballVisual != null) {
			ballRenderer = ballVisual.GetComponent<Renderer>();
			if (ballRenderer == null) {
				ballRenderer = ballVisual.GetComponentInChildren<Renderer>();
			}
		}

		//Fallback: tenta qualquer Renderer filho
		if (ballRenderer == null) {
			ballRenderer = GetComponentInChildren<Renderer>();
		}

		if (ballRenderer == null || ballRenderer.material == null) {
			return;
		}

		//Instancia uma cópia do material para não alterar o material compartilhado do projeto
		runtimeBallMaterial = ballRenderer.material;
		hasEmissionProperty = runtimeBallMaterial.HasProperty("_EmissionColor");

		if (!hasEmissionProperty) {
			return;
		}

		runtimeBallMaterial.EnableKeyword("_EMISSION");

		originalEmissionColor = runtimeBallMaterial.GetColor("_EmissionColor");
		currentEmissionColor = originalEmissionColor;
		targetEmissionColor = originalEmissionColor;

		ApplyEmissionColor(currentEmissionColor);
	}

	private void HandleMagnetStateChanged(bool isActive)
	{
		ApplyMagnetVisualState(isActive, false);
	}

	private void ApplyMagnetVisualState(bool isActive, bool instant)
	{
		magnetVisualActive = isActive;

		if (!hasEmissionProperty || runtimeBallMaterial == null) {
			return;
		}

		if (isActive) {
			//Combina a emissão original com um reforço visual em tom ciano
			targetEmissionColor = (originalEmissionColor + magnetEmissionTint) * magnetEmissionMultiplier;
		} else {
			targetEmissionColor = originalEmissionColor;
		}

		if (instant) {
			currentEmissionColor = targetEmissionColor;
			ApplyEmissionColor(currentEmissionColor);
		}
	}

	private void UpdateMagnetVisual()
	{
		if (!hasEmissionProperty || runtimeBallMaterial == null) {
			return;
		}

		currentEmissionColor = Color.Lerp(currentEmissionColor, targetEmissionColor, magnetVisualLerpSpeed * Time.deltaTime);

		ApplyEmissionColor(currentEmissionColor);
	}

	private void ApplyEmissionColor(Color emissionColor)
	{
		if (!hasEmissionProperty || runtimeBallMaterial == null) {
			return;
		}

		runtimeBallMaterial.SetColor("_EmissionColor", emissionColor);
	}

	//Alinha a bola na grade para evitar desvio diagonal
	/*private void SnapToGridAfterTurn()
	{
		Vector3 pos = transform.position;
		//Se estiver andando para a esquerda/direita, corrige o eixo Z
		if(currentDirection == Vector3.left || currentDirection == Vector3.right) {
			pos.z = Mathf.Round(pos.z / gridSize) * gridSize;
		}
		//Se estiver andando para frente/trás, corrige o eixo X
		else if (currentDirection == Vector3.forward || currentDirection == Vector3.back) {
			pos.x = Mathf.Round(pos.x / gridSize) * gridSize;
		}
		transform.position = pos;

	}*/

	private void CheckBallOutOfBounds()
	{
		if (!GameplayController.instance.gamePlaying)
			return;

		if(transform.position.y < destroyY) {
			GameplayController.instance.gamePlaying = false;

			if (ScoreManager.instance != null) {
				ScoreManager.instance.ResetCombo();
				ScoreManager.instance.GameOver();
			}

			//Destroy(gameObject);
		}
	}


} //class

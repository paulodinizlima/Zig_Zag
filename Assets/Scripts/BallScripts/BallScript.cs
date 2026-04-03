using UnityEngine;

public class BallScript : MonoBehaviour
{
	[Header("Movement")]
	//[SerializeField] private float speed = 3.5f;

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

	//Retorna a velocidade atual da bola
	public float CurrentSpeed => currentSpeed;

	private bool isMovingLeft = true;
	private Vector3 currentDirection;

	private void Awake()
	{
		//Começa a run com a velocidade inicial definida no Inspector
		currentSpeed = startSpeed;

		leftDirection = leftDirection.normalized;
		forwardDirection = forwardDirection.normalized;
		currentDirection = leftDirection;
	}

	private void Update()
	{
		HandleInput();
		CheckBallOutOfBounds();
		//Atualiza a progressão da velocidade da bola
		UpdateSpeed();
	}

	private void FixedUpdate()
	{
		if (!GameplayController.instance.gamePlaying)
			return;
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

		if (!GameplayController.instance.gamePlaying) {
			GameplayController.instance.gamePlaying = true;
			GameplayController.instance.ActiveTileSpawner();
			return;
		}

		ToggleDirection();
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
		isMovingLeft = !isMovingLeft;
		currentDirection = isMovingLeft ? leftDirection : forwardDirection;
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
			Destroy(gameObject);
		}
	}


} //class

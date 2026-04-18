using UnityEngine;
using TMPro;

public class StartUI : MonoBehaviour
{
	public static StartUI instance;
	[Header("Panel")]
	//Painel principal da tela inicial
	[SerializeField] private GameObject startPanel;

	[Header("Optional Texts")]
	//Texto "Tap do Start" para fazer animação simples depois
	[SerializeField] private TextMeshProUGUI tapToStartText;

	[Header("Tap Animation")]
	//Escala máxima do efeito de pulsar
	[SerializeField] private float pulseScale = 1.2f;
	//Velocidade do pulsar (quanto maior, mais rápido)
	[SerializeField] private float pulseSpeed = 3f;
	//Escala original do texto
	private Vector3 originalScale;

	private bool gameStarted = false;

	private void Awake()
	{
		//Garante que exista apenas uma instância
		if (instance != null && instance != this) {
			Destroy(gameObject);
			return;
		}
		instance = this;
	}
	private void Start()
	{
		//Garante que o painel inicial esteja visível ao abrir a cena
		if (startPanel != null) {
			startPanel.SetActive(true);
		}

		//Guarda a escala original do texto para usar no efeito de pulsar
		if (tapToStartText != null) {
			originalScale = tapToStartText.transform.localScale;
		}

		//O jogo começa pausado visualmente/lógica controlada pelos scripts
		gameStarted = false;
	}

	private void Update()
	{
		//Se já começou, não faz mais nada
		if (gameStarted) {
			return;
		}

		//Anima o texto "Tap to Start"
		AnimateTapText();

		//Detecta clique/toque/teclas para iniciar
		if (Input.GetMouseButtonDown(0) ||
			Input.GetKeyDown(KeyCode.Space) ||
			Input.GetKeyDown(KeyCode.Return)) {
			StartGame();
		}
	}

	//Inicia o jogo e esconde a tela inicial
	public void StartGame()
	{
		if (gameStarted) {
			return;
		}

		gameStarted = true;

		//Garante que o texto volte ao tamanho original antes de sumir
		if (tapToStartText != null) {
			tapToStartText.transform.localScale = originalScale;
		}

		//Esconde o painel inicial
		if (startPanel != null) {
			startPanel.SetActive(false);
		}

		//Inicia o gameplay
		if (GameplayController.instance != null) {
			GameplayController.instance.StartGameplay();
		}
	}

	//Faz o texto "respirar" (aumentar e diminuir suavemente)
	private void AnimateTapText()
	{
		//Se não existir referência, não faz nada
		if (tapToStartText == null) {
			return;
		}

		//Calcula um valor senoidal (vai de 0 a 1 suavemente)
		float t = (Mathf.Sin(Time.unscaledTime * pulseSpeed) + 1f) * 0.5f;
		//Interpola entre escala normal e escala aumentada
		float scale = Mathf.Lerp(1f, pulseScale, t);

		//Aplica a nova escala
		tapToStartText.transform.localScale = originalScale * scale;


	}
}

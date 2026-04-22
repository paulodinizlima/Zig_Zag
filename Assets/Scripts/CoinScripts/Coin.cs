using UnityEngine;

public class Coin : MonoBehaviour
{
	public enum CoinType
	{
		Normal,
		AdjacentOnly
	}

	[Header("Coin Type")]
	public CoinType coinType = CoinType.Normal;
	public int value = 1;

	[Header("Coin Visual")]
	[SerializeField] private Material normalMaterial;
	[SerializeField] private Material adjacentMaterial;

	[Header("Magnet Visual")]
	[SerializeField] private float magnetMoveSpeed = 8f;
	[SerializeField] private float collectDistance = 0.35f;

	[Header("Audio")]
	[SerializeField] private AudioClip collectSound;
	[SerializeField] private float soundVolume = 0.8f;

	[Header("Optional FX")]
	[SerializeField] private GameObject collectEffect;

	private bool collected = false;
	private bool isBeingPulled = false;

	private Transform ballTarget;
	private PlayerCurrency playerCurrency;
	private PlayerMagnet playerMagnet;

	private void Start()
	{
		//Busca referências da bola e componentes ligados à coleta/magnet
		GameObject ball = GameObject.FindGameObjectWithTag("Ball");
		if (ball != null) {
			ballTarget = ball.transform;
			playerCurrency = ball.GetComponent<PlayerCurrency>();
			playerMagnet = ball.GetComponent<PlayerMagnet>();
		}

		//Aplica o visual correto conforme o tipo da coin
		ApplyVisualByType();
	}

	private void Update()
	{
		//Evita processamento desnecessário após coleta
		if (collected) {
			return;
		}

		//Apenas coins adjacentes usam a lógica de atração pelo magnet
		if (coinType != CoinType.AdjacentOnly) {
			return;
		}

		//Se referências essenciais faltarem, não tenta puxar a coin
		if (ballTarget == null || playerCurrency == null || playerMagnet == null) {
			return;
		}

		//Sem magnet ativo, a coin não deve ser puxada
		if (!playerMagnet.IsMagnetActive) {
			isBeingPulled = false;
			return;
		}

		float distanceToBall = Vector3.Distance(transform.position, ballTarget.position);

		//Passa a ser puxada quando entra no raio de atração
		if (!isBeingPulled && distanceToBall <= playerMagnet.AttractionRadius) {
			isBeingPulled = true;
		}

		if (!isBeingPulled) {
			return;
		}

		//Vector3 targetPos = ballTarget.position + Vector3.up * 0.2f;

		//Move a coin em direção à bola
		transform.position = Vector3.MoveTowards(
			transform.position,
			ballTarget.position,
			magnetMoveSpeed * Time.deltaTime
		);

		//Se chegou perto o suficiente, coleta
		float updateDistance = Vector3.Distance(transform.position, ballTarget.position);

		if (updateDistance <= collectDistance) {
			Collect(playerCurrency);
		}
	}
	private void OnTriggerEnter(Collider other)
	{
		if (collected) {
			return;
		}

		if (!other.CompareTag("Ball")) {
			return;
		}

		//Busca o PlauyerCurrency a partir da bola
		PlayerCurrency currency = other.GetComponent<PlayerCurrency>();
		if (currency == null) {
			currency = other.GetComponentInParent<PlayerCurrency>();
		}
		if (currency == null) {
			return;
		}

		//Busca o magnet para validar coleta da coin adjacente
		PlayerMagnet magnet = other.GetComponent<PlayerMagnet>();
		if (magnet == null) {
			magnet = other.GetComponentInParent<PlayerMagnet>();
		}

		if (!CanCollectOnTouch(magnet)) {
			return;
		}

		Collect(currency);
	}

	private bool CanCollectOnTouch(PlayerMagnet magnet)
	{
		//Coin normal sempre pode ser coletada no toque
		if (coinType == CoinType.Normal) {
			return true;
		}

		//Coin adjacente só pode ser coletada com magnet ativo
		if(coinType == CoinType.AdjacentOnly) {
			return magnet != null && magnet.IsMagnetActive;
		}

		return false;
	}

	private void Collect(PlayerCurrency currency)
	{
		if (collected)
			return;

		collected = true;

		//IMPORTANTE
		//A partir de agora a coin NÂO entra mais no saldo persistente no momento da coleta.
		//O valor fica registrado apenas como estatística da run
		//A persistência real será feita no fechamento da run (Win/lose)
		//if (currency != null) {
		//	currency.AddCoins(value);
		//}

		// Estatística separada da run
		if (ScoreManager.instance != null) {
			ScoreManager.instance.AddCoin(value, coinType);
		}

		//Som da coleta
		if (collectSound != null) {
			AudioSource.PlayClipAtPoint(collectSound, transform.position, soundVolume);
		}

		//Efeito visual opcional
		if (collectEffect != null) {
			Instantiate(collectEffect, transform.position, Quaternion.identity);
		}

		Destroy(gameObject);
	}

	private void ApplyVisualByType()
	{
		Renderer renderer = GetComponentInChildren<Renderer>();

		if (renderer == null) {
			return;
		}

		//Define o material conforme o tipo da coin
		if (coinType == CoinType.AdjacentOnly) {
			if (adjacentMaterial != null) {
				renderer.material = adjacentMaterial;
			}
		} else {
			if (normalMaterial != null) {
				renderer.material = normalMaterial;
			}
		}
	}
}

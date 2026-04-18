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
		GameObject ball = GameObject.FindGameObjectWithTag("Ball");
		if (ball != null) {
			ballTarget = ball.transform;
			playerCurrency = ball.GetComponent<PlayerCurrency>();
			playerMagnet = ball.GetComponent<PlayerMagnet>();
		}

		ApplyVisualByType();
	}

	private void Update()
	{
		if (collected) {
			return;
		}
		if (coinType != CoinType.AdjacentOnly) {
			return;
		}

		if (ballTarget == null || playerCurrency == null || playerMagnet == null) {
			return;
		}

		if (!playerMagnet.IsMagnetActive) {
			isBeingPulled = false;
			return;
		}

		float distanceToBall = Vector3.Distance(transform.position, ballTarget.position);

		if (!isBeingPulled && distanceToBall <= playerMagnet.AttractionRadius) {
			isBeingPulled = true;
		}

		if (!isBeingPulled) {
			return;
		}

		//Vector3 targetPos = ballTarget.position + Vector3.up * 0.2f;

		transform.position = Vector3.MoveTowards(
			transform.position,
			ballTarget.position,
			magnetMoveSpeed * Time.deltaTime
		);

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

		PlayerCurrency currency = other.GetComponent<PlayerCurrency>();
		if (currency == null) {
			currency = other.GetComponentInParent<PlayerCurrency>();
		}
		if (currency == null) {
			Debug.LogWarning("PlayerCurrency não encontrado na bola.");
			return;
		}

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
		if (coinType == CoinType.Normal) {
			return true;
		}
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

		// saldo total persistente
		currency.AddCoins(value);

		// estatística separada da run
		if (ScoreManager.instance != null) {
			ScoreManager.instance.AddCoin(value, coinType);
		}

		if (collectSound != null) {
			AudioSource.PlayClipAtPoint(collectSound, transform.position, soundVolume);
		}

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

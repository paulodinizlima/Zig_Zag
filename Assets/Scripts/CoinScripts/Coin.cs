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

		// Apenas silver coins / AdjacentOnly são puxadas pelo magnet.
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

		transform.position = Vector3.MoveTowards(
			transform.position,
			ballTarget.position,
			magnetMoveSpeed * Time.deltaTime
		);

		float updatedDistance = Vector3.Distance(transform.position, ballTarget.position);

		if (updatedDistance <= collectDistance) {
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
			//Debug.LogWarning("PlayerCurrency não encontrado na bola.");
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

		if (coinType == CoinType.AdjacentOnly) {
			return magnet != null && magnet.IsMagnetActive;
		}

		return false;
	}

	private void Collect(PlayerCurrency currency)
	{
		if (collected) {
			return;
		}

		collected = true;

		int finalValue = GetFinalCoinValue();

		// Importante:
		// Não salva direto no PlayerCurrency aqui.
		// A coin entra na contagem da run pelo ScoreManager.
		if (ScoreManager.instance != null) {
			ScoreManager.instance.AddCoin(finalValue, coinType);
		}

		if (GameplayController.instance != null) {
			GameplayController.instance.PlayCollectableSound();
		}

		if (collectSound != null) {
			AudioSource.PlayClipAtPoint(collectSound, transform.position, soundVolume);
		}

		if (collectEffect != null) {
			Instantiate(collectEffect, transform.position, Quaternion.identity);
		}

		Destroy(gameObject);
	}

	private int GetFinalCoinValue()
	{
		// Gold/normal coin mantém o valor padrão.
		if (coinType == CoinType.Normal) {
			return value;
		}

		// Silver coin ganha valor especial se Enhanced Magnet foi desbloqueado.
		if (coinType == CoinType.AdjacentOnly) {
			if (UnlockData.instance != null && UnlockData.instance.EnhancedMagnetUnlocked) {
				return UnlockData.instance.EnhancedSilverCoinValue;
			}
		}

		return value;
	}

	private void ApplyVisualByType()
	{
		Renderer renderer = GetComponent<Renderer>();

		if (renderer == null) {
			return;
		}

		if (coinType == CoinType.Normal && normalMaterial != null) {
			renderer.material = normalMaterial;
		} else if (coinType == CoinType.AdjacentOnly && adjacentMaterial != null) {
			renderer.material = adjacentMaterial;
		}
	}
}
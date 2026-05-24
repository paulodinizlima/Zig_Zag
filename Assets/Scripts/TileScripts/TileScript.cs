using System.Collections;
using UnityEngine;

public class TileScript : MonoBehaviour
{

    private Rigidbody myBody;
    private AudioSource audioSource;

	[Header("Gem")]
	[SerializeField] private GameObject gem;
	[SerializeField] private float chanceForCollectable = 0.3f;

	[Header("Coins")]
	[SerializeField] private GameObject coinPrefab;
	[SerializeField] private float chanceForCoin = 0.35f;
	[SerializeField] private float chanceForAdjacentCoin = 0.15f;

	[Header("Coin Settings")]
	[SerializeField] private float coinHeight = 1f;
	[SerializeField] private float adjacentOffset = 1.2f;

	[Header("Rare Pickup - Magnet")]
	[SerializeField] private GameObject magnetPickupPrefab;
	[SerializeField] private float chanceForMagnetPickup = 0.03f;
	[SerializeField] private float magnetPickupHeight = 1.2f;

	[Header("Rare Pickup - Tile Light Energy")]
	[SerializeField] private GameObject lightEnergyPickupPrefab;
	[SerializeField] private float chanceForLightEnergyPickup = 0.03f;
	[SerializeField] private float lightEnergyPickupHeight = 1.2f;

	private Vector3 tilePathDirection = Vector3.zero;

	private void Awake()
	{
		myBody = GetComponent<Rigidbody>();
		audioSource = GetComponent<AudioSource>();
	}
	private void Start()
	{
		SpawnGem();
		SpawnCoins();
		SpawnRarePickup();
	}

	public void SetTilePathDirection(Vector3 direction)
	{
		tilePathDirection = direction.normalized;
	}

	private void SpawnGem()
	{
		if (gem != null && Random.value < chanceForCollectable) {
			Vector3 temp = transform.position;
			temp.y += 1.0f;
			Instantiate(gem, temp, Quaternion.identity);
		}
	}

	private void SpawnCoins()
	{
		if (coinPrefab == null)
			return;	

		//moeda normal (no caminho)
		if (Random.value < chanceForCoin) {
			Vector3 pos = transform.position;
			pos.y += coinHeight;
			SpawnCoin(pos, Coin.CoinType.Normal);
		}

		//moeda adjacente (fora do caminho)
		if (Random.value < chanceForAdjacentCoin) {
			Vector3 side = GetSafeAdjacentDirection();
			Vector3 pos = transform.position + side * adjacentOffset;
			pos.y += coinHeight;
			SpawnCoin(pos, Coin.CoinType.AdjacentOnly);
		}
	}

	private void SpawnCoin(Vector3 pos, Coin.CoinType type)
	{
		if (coinPrefab == null)
			return;

		GameObject coinObj = Instantiate(coinPrefab, pos, Quaternion.identity);
		Coin coin = coinObj.GetComponent<Coin>();

		if (coin != null) {
			coin.coinType = type;
		}
	}

	private void SpawnRarePickup()
	{
		//Se nenhum pickup raro estiver configurado, não faz nada
		bool hasMagnetPickup = magnetPickupPrefab != null;
		bool hasLightEnergyPickup = lightEnergyPickupPrefab != null;

		if (!hasMagnetPickup && !hasLightEnergyPickup) {
			return;
		}

		//Primeiro decide se o tile vai ter pickup raro algum.
		//Como agora temos dois pickups raros, usamos a maior das chances
		//como "porta de entrada" para não criar overlap de itens no mesmo tile
		float rarePickupGateChance = Mathf.Max(chanceForMagnetPickup, chanceForLightEnergyPickup);

		if (Random.value >= rarePickupGateChance) {
			return;
		}

		//Monta um sorteio ponderado entre os pickups raros disponíveis
		float magnetWeight = hasMagnetPickup ? chanceForMagnetPickup : 0f;
		float lightEnergyWeight = hasLightEnergyPickup ? chanceForLightEnergyPickup : 0f;
		float totalWeight = magnetWeight + lightEnergyWeight;

		if (totalWeight <= 0f) {
			return;
		}

		float roll = Random.value * totalWeight;

		//Magnet
		if (roll < magnetWeight) {
			Vector3 pos = transform.position;
			pos.y += magnetPickupHeight;
			Instantiate(magnetPickupPrefab, pos, Quaternion.identity);
			return;
		}

		// Light Energy
		Vector3 lightPos = transform.position;
		lightPos.y += lightEnergyPickupHeight;
		Instantiate(lightEnergyPickupPrefab, lightPos, Quaternion.identity);
	}

	private Vector3 GetSafeAdjacentDirection()
	{
		//Se o tile foi criado indo para a esquerda,
		//a moeda adjacente deve ir para trás no Z.
		if (tilePathDirection == Vector3.left) {
			return Vector3.back;
		}

		//Se o tile foi criado indo para frente,
		//a moeda adjacente deve ir para a direita no X.
		if (tilePathDirection == Vector3.forward) {
			return Vector3.right;
		}

		//fallback seguro
		return Vector3.right;
	}

	void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Ball")) {
			if (ScoreManager.instance != null) {
				ScoreManager.instance.AddTilePoint();
			}
			StartCoroutine(TriggerFallingDown());
		}
	}

	IEnumerator TriggerFallingDown()
	{
		yield return new WaitForSeconds(0.3f);
		myBody.isKinematic = false;
		//audioSource.Play();
		StartCoroutine(TurnOffGameObject());
	}
	IEnumerator TurnOffGameObject()
	{
		yield return new WaitForSeconds(2f);
		gameObject.SetActive(false);
	}

	


} //class

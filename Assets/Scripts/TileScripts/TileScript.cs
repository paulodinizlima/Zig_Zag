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

	private void Awake()
	{
		myBody = GetComponent<Rigidbody>();
		audioSource = GetComponent<AudioSource>();
	}
	private void Start()
	{
		SpawnGem();
		SpawnCoins();
	}

	private void SpawnGem()
	{
		if (gem != null && Random.value < chanceForCollectable) {
			Vector3 temp = transform.position;
			temp.y += 1.5f;
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
			Vector3 side = GetSideDirection();
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

	private Vector3 GetSideDirection()
	{
		//Zig-zag simples: lateral global
		return (Random.value < 0.5f) ? Vector3.right : Vector3.left;
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
		audioSource.Play();
		StartCoroutine(TurnOffGameObject());
	}
	IEnumerator TurnOffGameObject()
	{
		yield return new WaitForSeconds(2f);
		gameObject.SetActive(false);
	}

	


} //class

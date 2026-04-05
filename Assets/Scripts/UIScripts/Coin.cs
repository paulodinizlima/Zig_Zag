using UnityEngine;

public class Coin : MonoBehaviour
{
	public enum CoinType
	{
		Normal,
		AdjacentOnly
	}

	public CoinType coinType = CoinType.Normal;
	public int value = 1;

	private void OnTriggerEnter(Collider other)
	{
		if (!other.CompareTag("Ball")) {
			return;
		}

		PlayerCurrency currency = other.GetComponent<PlayerCurrency>();
		if (currency ==  null) {
			return;
		}

		PlayerMagnet magnet = other.GetComponent<PlayerMagnet>();

		if (!CanCollect(magnet)) {
			return;
		}

		currency.AddCoins(value);
		Destroy(gameObject);
	}

	private bool CanCollect(PlayerMagnet magnet)
	{
		if (coinType == CoinType.Normal) {
			return true;
		}
		if(coinType == CoinType.AdjacentOnly) {
			return magnet != null && magnet.IsMagnetActive;
		}

		return false;
	}
	
}

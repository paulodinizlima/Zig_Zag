using UnityEngine;

public class PlayerCurrency : MonoBehaviour
{
	public int coins;

	public void AddCoins(int amount)
	{
		coins += amount;
		Debug.Log("Coins: " + coins);
	}
}

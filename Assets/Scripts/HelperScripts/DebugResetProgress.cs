using UnityEngine;

public class DebugResetProgress : MonoBehaviour
{
	public void ResetAllProgress()
	{
		if (PlayerCurrency.instance != null) {
			PlayerCurrency.instance.ResetAllCurrency();
		}

		if (UpgradeData.instance != null) {
			UpgradeData.instance.ResetUpgrades();
		}

		if (InventoryData.instance != null) {
			InventoryData.instance.ResetInventory();
		}

		//Debug.Log("Todo o progresso foi resetado.");
	}

	// adiciona 200 gems para testes
	public void Add200Gems()
	{
		if (PlayerCurrency.instance == null) {
			return;
		}

		PlayerCurrency.instance.AddGems(200);

		//Debug.Log("200 Gems adicionadas.");
	}


	// opcional: coins para testar shop
	public void Add500Coins()
	{
		if (PlayerCurrency.instance == null) {
			return;
		}

		PlayerCurrency.instance.AddCoins(500);

		//Debug.Log("500 Coins adicionadas.");
	}
}

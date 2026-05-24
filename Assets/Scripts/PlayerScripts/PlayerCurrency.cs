using System.IO;
using UnityEngine;

public class PlayerCurrency : MonoBehaviour
{
	public static PlayerCurrency instance;

	private const string CoinsPlayerPrefsKey = "TotalCoins";
	private const string SaveFileName = "currency_save.json";

	//[SerializeField] private int coins = 0;
	//Novo formato persistente
	[SerializeField] private int goldCoins = 0;
	[SerializeField] private int silverCoins = 0;
	[SerializeField] private int gems = 0;
	//public int Coins => coins;
	//Compatibilidade com o sistema atual
	public int Coins => goldCoins + silverCoins;

	//Novos getters
	public int GoldCoins => goldCoins;
	public int SilverCoins => silverCoins;
	public int Gems => gems;
	public int TotalCoins => goldCoins + silverCoins;
	private string SaveFilePath => Path.Combine(Application.persistentDataPath, SaveFileName);

	private void Awake()
	{
		if (instance != null && instance != this) {
			Destroy(this);
			return;
		}
		instance = this;

		LoadCoins();
	}

	//Mantido para não quebrar o que já existe hoje no projeto.
	//Por enquanto, moedas adicionadas por esse método entram em silverCoins.
	public void AddCoins(int amount)
	{
		if (amount <= 0) {
			return;
		}
		//Debug.Log("Moeda Adicionada no Currency");
		silverCoins += amount;
		
		SaveCoins();
		NotifyUI();
	}

	//Mantido para compatibilidade com compras/gastos já existentes.
	//Consome primeiro silverCoins, depois goldCoins.
	public bool SpendCoins(int amount)
	{
		if(amount <= 0) {
			return false;
		}
		if (Coins < amount) {
			return false;
		}
		int remaining = amount;
		if (silverCoins >= remaining) {
			silverCoins -= remaining;
			remaining = 0;
		} else {
			remaining -= silverCoins;
			silverCoins = 0;
		}
		if (remaining > 0) {
			goldCoins = Mathf.Max(0, goldCoins - remaining);
		}
		//coins -= amount;
		SaveCoins();
		NotifyUI();
		return true;
	}

	// ============================
	// NOVOS MÉTODOS
	// ============================

	public void AddGoldCoins(int amount)
	{
		if (amount <= 0) {
			return;
		}

		goldCoins += amount;
		SaveCoins();
		NotifyUI();
	}

	public void AddSilverCoins(int amount)
	{
		if (amount <= 0) {
			return;
		}
		silverCoins += amount;
		SaveCoins();
		NotifyUI();
	}

	public void AddGems(int amount)
	{
		if (amount <= 0) {
			return;
		}
		gems += amount;
		SaveCoins();
		NotifyUI();
	}

	public bool SpendGoldCoins(int amount)
	{
		if (amount <= 0) {
			return false;
		}
		if (goldCoins < amount) {
			return false;
		}

		goldCoins -= amount;
		SaveCoins();
		NotifyUI();
		return true;
	}

	public bool SpendSilverCoins(int amount)
	{
		if (amount <= 0) {
			return false;
		}
		if (silverCoins < amount) {
			return false;
		}

		silverCoins -= amount;
		SaveCoins();
		NotifyUI();
		return true;
	}

	public bool SpendGems(int amount)
	{
		if (amount <= 0) {
			return false;
		}
		if (gems < amount) {
			return false;
		}

		gems -= amount;
		SaveCoins();
		NotifyUI();
		return true;
	}

	//##############################

	// ============================
	// LOAD / SAVE
	// ============================
	public void LoadCoins()
	{
		bool loadedFromJson = LoadFromJson();
		if (!loadedFromJson) {
			LoadFromPlayerPrefs();
			//Se carregou do PlayerPrefs, já recria o backup JSON
			SaveToJson();
		}
		NotifyUI();
	}

	public void SaveCoins()
	{
		SaveToPlayerPrefs();
		SaveToJson();
	}

	public void ResetAllCoins()
	{
		goldCoins = 0;
		silverCoins = 0;
		SaveCoins();
		NotifyUI();
	}

	public void ResetAllCurrency()
	{
		goldCoins = 0;
		silverCoins = 0;
		gems = 0;
		SaveCoins();
		NotifyUI();
	}

	public void SetCoinsForTest(int amount)
	{
		goldCoins = 0;
		silverCoins = Mathf.Max(0, amount);
		SaveCoins();
		NotifyUI();
	}

	private void NotifyUI()
	{
		if (ScoreManager.instance != null) {
			ScoreManager.instance.NotifyCoinsChanged();
		}
	}

	private void SaveToPlayerPrefs()
	{
		PlayerPrefs.SetInt(CoinsPlayerPrefsKey, Coins);
		PlayerPrefs.Save();
	}

	private void LoadFromPlayerPrefs()
	{
		// Migração do formato antigo:
		// tudo entra em silverCoins
		int totalCoins = PlayerPrefs.GetInt(CoinsPlayerPrefsKey, 0);

		goldCoins = 0;
		silverCoins = Mathf.Max(0, totalCoins);
		gems = 0;
	}

	private void SaveToJson()
	{
		try {
			CurrencySaveData data = new CurrencySaveData();

			// Compatibilidade
			data.coins = Coins;

			// Novo formato
			data.goldCoins = goldCoins;
			data.silverCoins = silverCoins;
			data.gems = gems;

			string json = JsonUtility.ToJson(data, true);
			File.WriteAllText(SaveFilePath, json);
		}
		catch (System.Exception ex) {
			//Debug.Log("Failed to save coins to JSON: " + ex.Message);
		}
	}

	private bool LoadFromJson()
	{
		try {
			if (!File.Exists(SaveFilePath)) {
				return false;
			}

			string json = File.ReadAllText(SaveFilePath);
			if (string.IsNullOrWhiteSpace(json)) {
				return false;
			}

			CurrencySaveData data = JsonUtility.FromJson<CurrencySaveData>(json);
			if (data == null) {
				return false;
			}

			// Caso novo formato já exista
			bool hasNewFormatData = data.goldCoins > 0 || data.silverCoins > 0 || data.gems > 0;

			if (hasNewFormatData) {
				goldCoins = Mathf.Max(0, data.goldCoins);
				silverCoins = Mathf.Max(0, data.silverCoins);
				gems = Mathf.Max(0, data.gems);
			} else {
				// Migração automática do formato antigo
				goldCoins = 0;
				silverCoins = Mathf.Max(0, data.coins);
				gems = 0;
			}

			return true;
		}
		catch (System.Exception ex) {
			//Debug.LogWarning("Failed to load coins from JSON: " + ex.Message);
			return false;
		}
	}
}

using System.IO;
using UnityEngine;

public class PlayerCurrency : MonoBehaviour
{
	private const string CoinsPlayerPrefsKey = "TotalCoins";
	private const string SaveFileName = "currency_save.json";

	[SerializeField] private int coins = 0;
	public int Coins => coins;

	private string SaveFilePath => Path.Combine(Application.persistentDataPath, SaveFileName);

	private void Awake()
	{
		LoadCoins();
	}

	public void AddCoins(int amount)
	{
		if (amount <= 0) {
			return;
		}
		Debug.Log("Moeda Adicionada no Currency");
		coins += amount;
		Debug.Log(amount);
		Debug.Log(coins);
		SaveCoins();
		NotifyUI();
	}

	public bool SpendCoins(int amount)
	{
		if(amount <= 0) {
			return false;
		}
		if (coins < amount) {
			return false;
		}
		coins -= amount;
		SaveCoins();
		NotifyUI();
		return true;
	}

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
		coins = 0;
		SaveCoins();
		NotifyUI();
	}

	public void SetCoinsForTest(int amount)
	{
		coins = Mathf.Max(0, amount);
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
		PlayerPrefs.SetInt(CoinsPlayerPrefsKey, coins);
		PlayerPrefs.Save();
	}

	private void LoadFromPlayerPrefs()
	{
		coins = PlayerPrefs.GetInt(CoinsPlayerPrefsKey, 0);
	}

	private void SaveToJson()
	{
		try {
			CurrencySaveData data = new CurrencySaveData();
			data.coins = coins;

			string json = JsonUtility.ToJson(data, true);
			File.WriteAllText(SaveFilePath, json);
		}
		catch (System.Exception ex) {
			Debug.Log("Failed to save coins to JSON: " + ex.Message);
		}
	}

	private bool LoadFromJson()
	{
		try {
			if (!File.Exists(SaveFilePath))
				return false;
			string json = File.ReadAllText(SaveFilePath);
			if (string.IsNullOrWhiteSpace(json)) {
				return false;
			}
			CurrencySaveData data = JsonUtility.FromJson<CurrencySaveData>(json);
			if (data == null) {
				return false;
			}
			coins = Mathf.Max(0, data.coins);
			return true;
		}
		catch (System.Exception ex) {
			Debug.LogWarning("Failed to load coins from JSON: " + ex.Message);
			return false;
		}
	}
}

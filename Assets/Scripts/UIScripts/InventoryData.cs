using System;
using System.IO;
using UnityEngine;

public class InventoryData : MonoBehaviour
{
    public static InventoryData instance;

    private const string SaveFileName = "inventory_save.json";

    [Header("Consumable Items")]
    [SerializeField] private int magnetCount = 0;
    [SerializeField] private int tileEnergyCount = 0;

    //Propriedades públicas de leitura
    //Outros scripts podem consultar os valores, mas não alterar diretamente.
    public int MagnetCount => magnetCount;
    public int TileEnergyCount => tileEnergyCount;

    private string SaveFilePath => Path.Combine(Application.persistentDataPath, SaveFileName);

	private void Awake()
	{
		//Singleton simples para facilitar acesso ao inventário em outros scripts
		if (instance != null && instance != this) {
            Destroy(gameObject);
            return;
		}

        instance = this;

        //Carrega o inventário salvo assim que o objeto nasce.
        LoadInventory();
	}

    // =========================
    // ADD ITEMS
    // =========================
    public void AddMagnet(int amount)
	{
		//Segurança: evita adicionar valor inválido.
		if (amount <= 0) {
            return;
		}

        magnetCount += amount;

        //Sempre que o inventário muda, salvamos.
        SaveInventory();
	}

    public void AddTileEnergy(int amount)
	{
		if (amount <= 0) {
            return;
		}

        tileEnergyCount += amount;
        SaveInventory();
	}

    // =========================
    // CONSUME ITEMS
    // =========================
    public bool TryConsumeMagnet()
	{
		//Retorna false quando o jogador não tem item suficiente
		if (magnetCount <= 0) {
            return false;
		}

        magnetCount--;
        SaveInventory();
        return true;
	}

    public bool TryConsumeTileEnergy()
	{
        if(tileEnergyCount <= 0) {
            return false;
		}

        tileEnergyCount--;
        SaveInventory();
        return true;
	}

    // =========================
    // TEST / DEBUG
    // =========================
    public void AddTestItems()
	{
        //Método temporário útil para testar a UI sem precisar da loja ainda.
        AddMagnet(1);
        AddTileEnergy(1);
	}

    public void ResetInventory()
	{
        //Zera apenas os itens do inventário.
        //Não mexe em coins/gems, porque esses ficam na PlayerCurrency.
        magnetCount = 0;
        tileEnergyCount = 0;
        SaveInventory();
	}

    // =========================
    // SAVE / LOAD
    // =========================
    public void SaveInventory()
	{
		try {
            InventorySaveData data = new InventorySaveData {
                magnetCount = magnetCount,
                tileEnergyCount = tileEnergyCount
            };

            //JsonUtility transforma a classe em texto JSON
            //O tre deixa o arquivo mais legível
            string json = JsonUtility.ToJson(data, true);

            //Salva em Application.persistentDataPath, que é o local correto
            //para dados persistentes do jogador.
            File.WriteAllText(SaveFilePath, json);
		}
		catch (Exception ex) {
            //Debug.LogWarning("Erro ao salvar inventário: " + ex.Message);
		}
	}

    public void LoadInventory()
	{
		try {
			//Se o arquivo ainda não existe, criamos um save inicial vazio
			if (!File.Exists(SaveFilePath)) {
                SaveInventory();
                return;
			}

            string json = File.ReadAllText(SaveFilePath);

			if (string.IsNullOrWhiteSpace(json)) {
                return;
			}

            InventorySaveData data = JsonUtility.FromJson<InventorySaveData>(json);

			if (data == null) {
                return;
			}

            //Mathf.Max evita carregar valores negativos por erro/corrupção do arquivo.
            magnetCount = Mathf.Max(0, data.magnetCount);
            tileEnergyCount = Mathf.Max(0, data.tileEnergyCount);
		}
		catch (Exception ex) {
            //Debug.LogWarning("Erro ao carregar inventário: " + ex.Message);
		}
	}
}
[Serializable]
public class InventorySaveData
{
    //Classe simples usada apenas para salvar/carregar o JSON.
    //Ela não deve ser colocada em GameObject
    public int magnetCount;
    public int tileEnergyCount;
}

using System;
using System.IO;
using UnityEngine;

public class UnlockData : MonoBehaviour
{
    public static UnlockData instance;

    private const string SaveFileName = "unlock_save.json";

    [Header("Unlock States")]
    //Define se o jogador já comprou o Enhanced Magnet
    [SerializeField] private bool enhancedMagnetUnlocked = false;

    //Define se o jogador já comprou o Enhanced Energy
    [SerializeField] private bool enhancedEnergyUnlocked = false;

    [Header("Unlock Costs")]
    //Custo em gems para liberar silver coins valendo 2
    [SerializeField] private int enhancedMagnetCost = 100;

    //Custo em gems para liberar +30% na duração do Energy.
    [SerializeField] private int enhancedEnergyCost = 100;

    [Header("Unlock Effects")]
    //Valor final das silver coins quando Enhanced Magnet estiver desbloqueado.
    [SerializeField] private int enhancedSilverCoinValue = 2;

    //Multiplicador da duração do Energy quando Enhanced Energy estiver desbloqueado.
    [SerializeField] private float enhancedEnergyMultiplier = 1.3f;

    //Propriedades públicas somente leitura
    //Outros scripts consultam esses valores, mas só UnlockData altera internamente.
    public bool EnhancedMagnetUnlocked => enhancedMagnetUnlocked;
    public bool EnhancedEnergyUnlocked => enhancedEnergyUnlocked;

    public int EnhancedMagnetCost => enhancedMagnetCost;
    public int EnhancedEnergyCost => enhancedEnergyCost;

    public int EnhancedSilverCoinValue => enhancedSilverCoinValue;
    public float EnhancedEnergyMultiplier => enhancedEnergyMultiplier;

    private string SaveFilePath => Path.Combine(Application.persistentDataPath, SaveFileName);


	private void Awake()
	{
		//Singleton simples para permitir acesso por UnlockData.instance.
		if (instance != null && instance != this) {
            Destroy(gameObject);
            return;
		}
        
        instance = this;

        //Carrega os unlocks salvos assim que o objeto nasce.
        LoadUnlocks();
	}

    public bool TryUnlockEnhancedMagnet()
	{
		//Se já foi comprado, não compra de novo.
		if (enhancedMagnetUnlocked) {
            return false;
		}

        enhancedMagnetUnlocked = true;
        SaveUnlocks();
        return true;
	}

    public bool TryUnlockEnhancedEnergy()
	{
		//Se já foi comprado, não compra de novo.
		if (enhancedEnergyUnlocked) {
            return false;
		}

        enhancedEnergyUnlocked = true;
        SaveUnlocks();
        return true;
	}

    public void ResetUnlocks()
	{
        //Método útil para teste/debug.
        //Zera somente os unlocks, não mexe em coins, gems ou upgrades.
        enhancedMagnetUnlocked = false;
        enhancedEnergyUnlocked = false;
        SaveUnlocks();
	}

    public void SaveUnlocks()
	{
		try {
            UnlockSaveData data = new UnlockSaveData {
                enhancedMagnetUnlocked = enhancedMagnetUnlocked, enhancedEnergyUnlocked = enhancedEnergyUnlocked
            };

            //Converte os dados para JSON e salva em persistentDataPath.
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SaveFilePath, json);
		}
		catch (Exception ex) {
            //Debug.LogWarning("Erro ao salvar unlocks: " + ex.Message);
		}
	}

    public void LoadUnlocks()
	{
		try {
			//Se ainda não existe save, cria um arquivo inicial
			if (!File.Exists(SaveFilePath)) {
                SaveUnlocks();
                return;
			}

            string json = File.ReadAllText(SaveFilePath);

			if (string.IsNullOrWhiteSpace(json)) {
                return;
			}

            UnlockSaveData data = JsonUtility.FromJson<UnlockSaveData>(json);

			if (data == null) {
                return;
			}

            enhancedMagnetUnlocked = data.enhancedMagnetUnlocked;
            enhancedEnergyUnlocked = data.enhancedEnergyUnlocked;
        }
		catch (Exception ex) {
            //Debug.LogWarning("Erro ao carregar unlocks: " + ex.Message);
		}
	}
}

[Serializable]
public class UnlockSaveData
{
    // Classe simples usada apenas para salvar/carregar JSON.
    // Não precisa ser anexada em nenhum GameObject.
    public bool enhancedMagnetUnlocked;
    public bool enhancedEnergyUnlocked;
}

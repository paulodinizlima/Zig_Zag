using System;
using System.IO;
using UnityEngine;

public class UpgradeData : MonoBehaviour
{
    /*
     Singleton simples:
    permite acessar UpgradeData.instance de qualquer script.
    Ex.:
    UpgradeData.instance.CurrentMagnetDuration
    */
    public static UpgradeData instance;

    private const string SaveFileName = "upgrade_save.json";

    // -----------------------------
    // LEVELS ATUAIS DOS UPGRADES
    // -----------------------------
    [Header("Upgrade Levels")]
    [SerializeField] private int magnetDurationLevel = 0;
    [SerializeField] private int energyDurationLevel = 0;

    /*
     Configuração dos upgrades
     maxDurationLevel:
     0-5 significa: Level 0 = sem upgrade, Level 5 = máximo
    */
    [Header("Upgrade Config")]
    [SerializeField] private int maxDurationLevel = 10;

    /*
		Duração base dos pickups.
		Esses valores são o "estado inicial"
		antes de qualquer melhoria.
	*/
    [SerializeField] private float baseMagnetDuration = 5f;
    [SerializeField] private float baseEnergyDuration = 5f;

    /*
		Cada nível comprado adiciona
		+1 segundo.
	*/
    [SerializeField] private float durationBonusPerLevel = 1f;

    /*
		Tabela fixa de custos definida
		no documento de economia.

		Índice:
		[0] = custo para ir do level 0 para 1
		[1] = custo do level 1 para 2
		etc.
	*/
    private readonly int[] durationUpgradeCosts = {
        50, 100, 175, 275, 400, 600, 900, 1100, 2000, 4000
    };

    // -----------------------------
    // PROPERTIES (READ ONLY)
    // -----------------------------
    public int MagnetDurationLevel => magnetDurationLevel;
    public int EnergyDurationLevel => energyDurationLevel;
    public int MaxDurationLevel => maxDurationLevel;

    /*
		Duração final aplicada em gameplay.
    	Ex:	base 10s
		level 2
    	10 + (2*1) = 12 segundos
	*/
    public float CurrentMagnetDuration => baseMagnetDuration + (magnetDurationLevel * durationBonusPerLevel);
    public float CurrentEnergyDuration => baseEnergyDuration + (energyDurationLevel * durationBonusPerLevel);

    private string SaveFilePath => Path.Combine(Application.persistentDataPath, SaveFileName);

	private void Awake()
	{
		//Singleton protection
		if (instance != null && instance != this) {
            Destroy(gameObject);
            return;
		}

        instance = this;

        LoadUpgrades();
	}

    // =================================
    // QUERIES / CONSULTAS
    // =================================
    public bool IsMagnetDurationMaxed()
	{
        return magnetDurationLevel >= maxDurationLevel;
	}

    public bool IsEnergyDurationMaxed()
	{
        return energyDurationLevel >= maxDurationLevel;
	}

    //Retorna o custo do próximo upgrade.
    public int GetMagnetDurationUpgradeCost()
	{
        return GetDurationUpgradeCost(magnetDurationLevel);
	}

    public int GetEnergyDurationUpgradeCost()
	{
        return GetDurationUpgradeCost(energyDurationLevel);
	}

    private int GetDurationUpgradeCost(int currentLevel)
	{
		//se já está no máximo
		if (currentLevel >= maxDurationLevel) {
            return 0;
		}

		if (currentLevel < 0 || currentLevel >= durationUpgradeCosts.Length) {
            return 0;
		}

        return durationUpgradeCosts[currentLevel];
	}

    // =================================
    // COMPRA DOS UPGRADES
    // =================================
    public bool TryUpgradeMagnetDuration()
	{
		if (IsMagnetDurationMaxed()) {
            return false;
		}

        magnetDurationLevel++;

        SaveUpgrades();

        return true;
	}

    public bool TryUpgradeEnergyDuration()
	{
		if (IsEnergyDurationMaxed()) {
            return false;
		}

        energyDurationLevel++;

        SaveUpgrades();

        return true;
	}

    // =================================
    // DEBUG
    // =================================
    public void ResetUpgrades()
    {
        magnetDurationLevel = 0;
        energyDurationLevel = 0;

        SaveUpgrades();
    }

    // =================================
    // SAVE / LOAD
    // =================================
    public void SaveUpgrades()
	{
		try {
            UpgradeSaveData data = new UpgradeSaveData {
                magnetDurationLevel = magnetDurationLevel,
                energyDurationLevel = energyDurationLevel
            };

            string json = JsonUtility.ToJson(data, true);

            File.WriteAllText(SaveFilePath, json);
		}
		catch (Exception ex) {
            //Debug.LogWarning("Erro ao salvar upgrades: " + ex.Message);
		}
	}

    public void LoadUpgrades()
	{
		try {
			if (!File.Exists(SaveFilePath)) {
                SaveUpgrades();
                return;
			}

            string json = File.ReadAllText(SaveFilePath);

			if (string.IsNullOrWhiteSpace(json)) {
                return;
			}

            UpgradeSaveData data = JsonUtility.FromJson<UpgradeSaveData>(json);

			if (data == null) {
                return;
			}

            //Clamp evita valores inválidos
            //caso save seja editado/corrompido
            magnetDurationLevel = Mathf.Clamp(data.magnetDurationLevel, 0, maxDurationLevel);

            energyDurationLevel = Mathf.Clamp(data.energyDurationLevel, 0, maxDurationLevel);
		}
        catch (Exception ex) {
            //Debug.LogWarning("Erro ao salvar upgrades: " + ex.Message);
        }
    }
}

[Serializable]
public class UpgradeSaveData
{
    //Classe simples só para persistência
    public int magnetDurationLevel;
    public int energyDurationLevel;
}

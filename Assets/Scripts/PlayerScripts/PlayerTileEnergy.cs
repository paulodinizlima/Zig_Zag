using System;
using UnityEngine;

public class PlayerTileEnergy : MonoBehaviour
{
    public static PlayerTileEnergy instance;

    //Evento global para avisar todos os tiles quando a energia ligar/desligar
    public static event Action<bool> TileEnergyStateChanged;

    [Header("Tile Energy")]
    [SerializeField] private bool isTileEnergyActive = false;
    [SerializeField] private float timer = 0f;

    public bool IsTileEnergyActive => isTileEnergyActive;

	private void Awake()
	{
		if (instance != null && instance != this) {
            Destroy(this);
            return;
		}

        instance = this;
	}

	private void Update()
	{
		if (!isTileEnergyActive) {
			return;
		}

		timer -= Time.deltaTime;

		if (timer <= 0f) {
			DeactivateTileEnergy();
		}
	}

	public void ActivateTileEnergy(float duration)
	{
		if (duration <= 0f) {
			return;
		}

		bool wasInactive = !isTileEnergyActive;

		isTileEnergyActive = true;
		timer = Mathf.Max(timer, duration);

		if (wasInactive) {
			TileEnergyStateChanged?.Invoke(true);
		}
	}

	private void DeactivateTileEnergy()
	{
		if (!isTileEnergyActive) {
			return;
		}

		isTileEnergyActive = false;
		timer = 0f;

		TileEnergyStateChanged?.Invoke(false);
	}
}

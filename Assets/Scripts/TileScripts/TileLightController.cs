using UnityEngine;

public class TileLightController : MonoBehaviour
{
    [SerializeField] private Light pointLight;

	private void OnEnable()
	{
		PlayerTileEnergy.TileEnergyStateChanged += HandleTileEnergyStateChanged;
	}

	private void OnDisable()
	{
		PlayerTileEnergy.TileEnergyStateChanged -= HandleTileEnergyStateChanged;
	}

	private void Awake()
	{
		if (pointLight == null) {
			pointLight = GetComponentInChildren<Light>(true);
		}

		if (pointLight != null) {
			pointLight.enabled = false;
		}
	}

	private void Start()
	{
		//Sincroniza o estado ao spawnar
		bool shouldBeActive = PlayerTileEnergy.instance != null && PlayerTileEnergy.instance.IsTileEnergyActive;
		SetLightActive(shouldBeActive);
	}

	private void HandleTileEnergyStateChanged(bool isActive)
	{
		SetLightActive(isActive);
	}

	private void SetLightActive(bool active)
	{
		if (pointLight == null) {
			return;
		}

		pointLight.enabled = active;
	}
}

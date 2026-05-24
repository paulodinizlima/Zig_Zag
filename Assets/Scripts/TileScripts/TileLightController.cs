using UnityEngine;

public class TileLightController : MonoBehaviour
{
	[Header("Emissive Sphere")]
	[SerializeField] private Renderer emissiveRenderer;

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
		if (emissiveRenderer == null) {
			emissiveRenderer = GetComponentInChildren<Renderer>(true);
		}

		SetLightActive(false);
	}

	private void Start()
	{
		// Sincroniza estado atual ao spawnar
		bool shouldBeActive =
			PlayerTileEnergy.instance != null &&
			PlayerTileEnergy.instance.IsTileEnergyActive;

		SetLightActive(shouldBeActive);
	}

	private void HandleTileEnergyStateChanged(bool isActive)
	{
		SetLightActive(isActive);
	}

	private void SetLightActive(bool active)
	{
		if (emissiveRenderer == null) {
			return;
		}

		emissiveRenderer.enabled = active;
	}
}
using UnityEngine;

public class LightEnergyPickup : MonoBehaviour
{
	[Header("Energy Settings")]
	// Fallback caso UpgradeData não exista na cena.
	[SerializeField] private float duration = 5f;

	[Header("Visual Motion")]
	[SerializeField] private float hoverAmplitude = 0.2f;
	[SerializeField] private float hoverSpeed = 2.2f;
	[SerializeField] private float rotateSpeed = 80f;

	[Header("Collect FX")]
	[SerializeField] private GameObject collectEffect;
	[SerializeField] private AudioClip collectSound;
	[SerializeField] [Range(0f, 1f)] private float collectVolume = 0.85f;

	private Vector3 startPosition;
	private bool collected = false;

	private void Start()
	{
		startPosition = transform.position;
	}

	private void Update()
	{
		if (collected) {
			return;
		}

		// Movimento vertical para o pickup parecer vivo.
		float yOffset = Mathf.Sin(Time.time * hoverSpeed) * hoverAmplitude;

		transform.position = new Vector3(
			startPosition.x,
			startPosition.y + yOffset,
			startPosition.z
		);

		// Rotação simples para melhorar leitura visual.
		transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (collected) {
			return;
		}

		if (!other.CompareTag("Ball")) {
			return;
		}

		PlayerTileEnergy tileEnergy = other.GetComponent<PlayerTileEnergy>();

		if (tileEnergy == null) {
			tileEnergy = other.GetComponentInParent<PlayerTileEnergy>();
		}

		if (tileEnergy == null) {
			//Debug.LogWarning("PlayerTileEnergy não encontrado na bola.");
			return;
		}

		collected = true;

		float finalDuration = GetFinalEnergyDuration();

		tileEnergy.ActivateTileEnergy(finalDuration);

		if (PickupFeedbackUI.instance != null) {
			PickupFeedbackUI.instance.ShowPickupMessage(
				"ENERGY +" + finalDuration.ToString("0") + "s"
			);
		}

		if (collectEffect != null) {
			Instantiate(collectEffect, transform.position, Quaternion.identity);
		}

		if (collectSound != null) {
			AudioSource.PlayClipAtPoint(collectSound, transform.position, collectVolume);
		}

		//Debug.Log("Energy coletada. Duração aplicada: " + finalDuration + "s");

		Destroy(gameObject);
	}

	private float GetFinalEnergyDuration()
	{
		float finalDuration = duration;

		if (UpgradeData.instance != null) {
			finalDuration = UpgradeData.instance.CurrentEnergyDuration;
		}

		// Enhanced Energy aplica bônus permanente de +30%.
		if (UnlockData.instance != null && UnlockData.instance.EnhancedEnergyUnlocked) {
			finalDuration *= UnlockData.instance.EnhancedEnergyMultiplier;
		}

		return finalDuration;
	}
}
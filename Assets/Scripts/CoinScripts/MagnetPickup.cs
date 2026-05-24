using UnityEngine;

public class MagnetPickup : MonoBehaviour
{
	[Header("Magnet Settings")]
	// Duração fallback usada apenas se UpgradeData não existir na cena.
	[SerializeField] private float magnetDuration = 10f;

	[Header("Optional FX")]
	[SerializeField] private GameObject collectEffect;

	//[Header("Optional Audio")]
	//[SerializeField] private AudioClip pickupSound;
	//[SerializeField] [Range(0f, 1f)] private float pickupVolume = 0.85f;

	private bool collected = false;

	private void OnTriggerEnter(Collider other)
	{
		if (collected) {
			return;
		}

		if (!other.CompareTag("Ball")) {
			return;
		}

		PlayerMagnet magnet = other.GetComponent<PlayerMagnet>();

		if (magnet == null) {
			magnet = other.GetComponentInParent<PlayerMagnet>();
		}

		if (magnet == null) {
			//Debug.LogWarning("PlayerMagnet não encontrado na bola.");
			return;
		}

		collected = true;

		//Integração com a shop
		//se UpgradeData existir, usamos a duração comprada.
		// Esta é a duração REAL que será aplicada.
		float finalDuration = GetFinalMagnetDuration();

		//magnet.ActivateMagnet(magnetDuration);

		// Importante: aqui precisa usar finalDuration, não magnetDuration.
		magnet.ActivateMagnet(finalDuration);

		if (PickupFeedbackUI.instance != null) {
			PickupFeedbackUI.instance.ShowPickupMessage(
				"MAGNET +" +
				finalDuration.ToString("0") +
				"s"
			);
		}

		if (collectEffect != null) {
			Instantiate(collectEffect, transform.position, Quaternion.identity);
		}

		//if (pickupSound != null) {
		//	AudioSource.PlayClipAtPoint(pickupSound, transform.position);
		//}

		//Debug.Log("Magnet coletado. Duração final: " + finalDuration + "s");

		Destroy(gameObject);
	}

	private float GetFinalMagnetDuration()
	{
		if (UpgradeData.instance != null) {
			return UpgradeData.instance.CurrentMagnetDuration;
		}

		return magnetDuration;
	}
}


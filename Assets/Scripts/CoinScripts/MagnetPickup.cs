using UnityEngine;

public class MagnetPickup : MonoBehaviour
{
	[Header("Magnet Settings")]
	[SerializeField] private float magnetDuration = 5f;

	[Header("Optional FX")]
	[SerializeField] private GameObject collectEffect;

	//[Header("Optional Audio")]
	//[SerializeField] private AudioClip pickupSound;

	private bool collected = false;

	private void OnTriggerEnter(Collider other)
	{
		Debug.Log("MagnetPickup tocou em: " + other.name);

		if (collected) {
			return;
		}

		if (!other.CompareTag("Ball")) {
			return;
		}

		PlayerMagnet magnet = other.GetComponent<PlayerMagnet>();

		if (magnet == null) {
			Debug.LogWarning("A bola não tem PlayerMagnet");
			return;
		}

		collected = true;

		magnet.ActivateMagnet(magnetDuration);

		if (collectEffect != null) {
			Instantiate(collectEffect, transform.position, Quaternion.identity);
		}

		//if (pickupSound != null) {
		//	AudioSource.PlayClipAtPoint(pickupSound, transform.position);
		//}

		Debug.Log("Pickup magnético coletado.");

		Destroy(gameObject);
	}
}


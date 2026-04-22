using UnityEngine;

public class LightEnergyPickup : MonoBehaviour
{
	[Header("Energy")]
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

	private void Start()
	{
		startPosition = transform.position;
	}

	private void Update()
	{
		// Bounce / hover vertical
		float yOffset = Mathf.Sin(Time.time * hoverSpeed) * hoverAmplitude;
		transform.position = new Vector3(startPosition.x, startPosition.y + yOffset, startPosition.z);

		// Rotação leve para dar vida ao pickup
		transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!other.CompareTag("Ball")) {
			return;
		}

		PlayerTileEnergy tileEnergy = other.GetComponent<PlayerTileEnergy>();

		if (tileEnergy == null) {
			tileEnergy = other.GetComponentInParent<PlayerTileEnergy>();
		}

		if (tileEnergy != null) {
			tileEnergy.ActivateTileEnergy(duration);
		}

		// Efeito visual de coleta
		if (collectEffect != null) {
			Instantiate(collectEffect, transform.position, Quaternion.identity);
		}

		// Som de coleta
		if (collectSound != null) {
			AudioSource.PlayClipAtPoint(collectSound, transform.position, collectVolume);
		}

		Destroy(gameObject);
	}
}

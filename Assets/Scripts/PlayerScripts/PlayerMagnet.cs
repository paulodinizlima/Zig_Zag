using UnityEngine;

public class PlayerMagnet : MonoBehaviour
{
	[SerializeField] private bool isMagnetActive = false;
	[SerializeField] private float timer = 0f;

	[Header("Magnet Range")]
	[SerializeField] private float attractionRadius = 2.25f;
	public bool IsMagnetActive => isMagnetActive;
	private float TimeRemaining => timer;
	public float AttractionRadius => attractionRadius;

	private void Update()
	{
		if (!isMagnetActive)
			return;

		timer -= Time.deltaTime;

		if (timer <= 0f) {
			DeactivateMagnet();
		}
	}
	public void ActivateMagnet(float duration)
	{
		if (duration <= 0f) {
			return;
		}

		isMagnetActive = true;

		//Se pegar outro magnet enquanto um já estiver ativo, 
		//mantém o maior tempo entre o atual e o novo
		timer = Mathf.Max(timer, duration);
		
		Debug.Log("Magnet ativado por " + duration + " segundos");
	}

	public void DeactivateMagnet()
	{
		isMagnetActive = false;
		timer = 0f;
		Debug.Log("Magnet desativado");
	}
}

using System;
using UnityEngine;

public class PlayerMagnet : MonoBehaviour
{
	//Evento global para HUD e feedback visual reagirem ao estado do magnet
	public static event Action<bool> MagnetStateChanged;

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

		bool wasInactive = !isMagnetActive;

		isMagnetActive = true;

		//Se pegar outro magnet enquanto um já estiver ativo, 
		//mantém o maior tempo entre o atual e o novo
		timer = Mathf.Max(timer, duration);

		//Só dispara o evento quando o estado realmente muda para ativo
		if (wasInactive) {
			MagnetStateChanged?.Invoke(true);
		}
	}

	public void DeactivateMagnet()
	{
		if (!isMagnetActive) {
			return;
		}

		isMagnetActive = false;
		timer = 0f;

		//Dispara o evneto para HUD e feedback visual
		MagnetStateChanged?.Invoke(false);
	}
}

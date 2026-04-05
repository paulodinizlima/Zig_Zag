using UnityEngine;

public class PlayerMagnet : MonoBehaviour
{
	public bool IsMagnetActive;
	private float timer;

	private void Update()
	{
		if (!IsMagnetActive)
			return;

		timer -= Time.deltaTime;

		if (timer <= 0) {
			IsMagnetActive = false;
		}
	}
	public void ActivateMagnet(float duration)
	{
		IsMagnetActive = true;
		timer = duration;
		Debug.Log("Magnet ativado por " + duration + " segundos");
	}
}

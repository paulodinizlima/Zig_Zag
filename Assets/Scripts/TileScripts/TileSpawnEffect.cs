using UnityEngine;

public class TileSpawnEffect : MonoBehaviour
{
	[SerializeField] private float duration = 0.12f;
	[SerializeField] private Vector3 startScale = new Vector3(0.85f, 0.85f, 0.85f);

	private Vector3 targetScale;
	private float timer = 0f;

	private void OnEnable()
	{
		targetScale = transform.localScale;
		transform.localScale = startScale;
		timer = 0f;
	}

	private void Update()
	{
		if (timer >= duration) {
			return;
		}

		timer += Time.deltaTime;
		float t = Mathf.Clamp01(timer / duration);

		transform.localScale = Vector3.Lerp(startScale, targetScale, t);
	}

}

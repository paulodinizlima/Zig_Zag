using System.Collections;
using UnityEngine;
using TMPro;

public class ScorePopup : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI popupText;
	[SerializeField] private float lifetime = 0.8f;
	[SerializeField] private float moveDistance = 40f;

	private RectTransform rectTransform;
	private CanvasGroup canvasGroup;
	private Vector2 startPos;
	private Vector2 endPos;

	private void Awake()
	{
		rectTransform = GetComponent<RectTransform>();
		canvasGroup = GetComponent<CanvasGroup>();

		if (canvasGroup == null) {
			canvasGroup = gameObject.AddComponent<CanvasGroup>();
		}
	}

	public void Setup(string text, Color color)
	{
		if (popupText != null) {
			popupText.text = text;
			popupText.color = color;
		}

		startPos = rectTransform.anchoredPosition;
		endPos = startPos + new Vector2(0f, moveDistance);

		StartCoroutine(AnimatePopup());
	}

	private IEnumerator AnimatePopup()
	{
		float timer = 0f;

		while (timer < lifetime) {
			timer += Time.deltaTime;
			float t = Mathf.Clamp01(timer / lifetime);

			rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);

			if (canvasGroup != null) {
				canvasGroup.alpha = 1f - t;
			}

			yield return null;
		}

		Destroy(gameObject);
	}
}

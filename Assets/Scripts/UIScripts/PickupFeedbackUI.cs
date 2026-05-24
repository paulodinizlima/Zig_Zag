using System.Collections;
using UnityEngine;
using TMPro;

public class PickupFeedbackUI : MonoBehaviour
{
    public static PickupFeedbackUI instance;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI feedbackText;

    [Header("Animation")]
    [SerializeField] private float riseDistance = 60f;

    [SerializeField] private float duration = 1f;

    [SerializeField] private float startScale = 0.8f;

    [SerializeField] private float endScale = 1.15f;

    private Vector2 originalPos;
    private Coroutine currentRoutine;

	private void Awake()
	{
		if (instance != null && instance != this) {
            Destroy(gameObject);
            return;
		}
        instance = this;
	}

	private void Start()
	{
		if (feedbackText != null) {
            originalPos = feedbackText.rectTransform.anchoredPosition;

            feedbackText.gameObject.SetActive(false);
		}
	}

    public void ShowPickupMessage(string message)
	{
		if (feedbackText == null) {
            return;
		}

		if (currentRoutine != null) {
            StopCoroutine(currentRoutine);
		}

        currentRoutine = StartCoroutine(AnimateFeedback(message));
	}

    private IEnumerator AnimateFeedback(string message)
	{
        RectTransform rect = feedbackText.rectTransform;
        feedbackText.text = message;
        feedbackText.gameObject.SetActive(true);
        Color c = feedbackText.color;
        c.a = 1f;
        feedbackText.color = c;
        rect.anchoredPosition = originalPos;
        rect.localScale = Vector3.one * startScale;
        float timer = 0f;
		while (timer < duration) {
            timer += Time.unscaledDeltaTime;
            float t = timer / duration;
            Vector2 pos = originalPos;
            pos.y += riseDistance * t;
            rect.anchoredPosition = pos;
            float scale = Mathf.Lerp(startScale, endScale, t);
            rect.localScale = Vector3.one * scale;
            Color fade = feedbackText.color;
            fade.a = 1f - t;
            feedbackText.color = fade;
            yield return null;
		}

        feedbackText.gameObject.SetActive(false);
        currentRoutine = null;
	}
}

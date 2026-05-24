using UnityEngine;
using TMPro;

public class TipRotatorUI : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI tipText;
	[SerializeField] private float changeInterval = 5f;

	[TextArea]
	[SerializeField] private string[] tips;

	private int currentIndex;
	private float timer;

	private void Start()
	{
		if(tips.Length > 0) {
			ShowTip(0);
		}
	}

	private void Update()
	{
		if (tips == null || tips.Length <= 1) {
			return;
		}

		timer += Time.unscaledDeltaTime;

		if (timer >= changeInterval) {
			timer = 0f;
			currentIndex++;

			if (currentIndex >= tips.Length) {
				currentIndex = 0;
			}

			ShowTip(currentIndex);
		}
	}

	private void ShowTip(int index)
	{
		if (tipText == null) {
			return;
		}

		tipText.text = tips[index];
	}
}

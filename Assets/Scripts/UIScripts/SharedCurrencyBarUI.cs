using UnityEngine;

public class SharedCurrencyBarUI : MonoBehaviour
{
	public static SharedCurrencyBarUI instance;

	[SerializeField] private GameObject sharedCurrencyBar;

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
		Show();
	}

	public void Show()
	{
		if (sharedCurrencyBar != null) {
			sharedCurrencyBar.SetActive(true);
		}
	}

	public void Hide()
	{
		if (sharedCurrencyBar != null) {
			sharedCurrencyBar.SetActive(false);
		}
	}
}
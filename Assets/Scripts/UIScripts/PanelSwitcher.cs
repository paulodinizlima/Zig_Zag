using UnityEngine;

public class PanelSwitcher : MonoBehaviour
{
	[SerializeField] private GameObject panelToClose;
	[SerializeField] private GameObject panelToOpen;

	public void SwitchPanel()
	{
		if (panelToClose != null) {
			panelToClose.SetActive(false);
		}

		if (panelToOpen != null) {
			panelToOpen.SetActive(true);
		}
	}
}
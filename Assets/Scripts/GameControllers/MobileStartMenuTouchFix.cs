using UnityEngine;

public class MobileStartMenuTouchFix : MonoBehaviour
{
	[Header("Buttons RectTransforms")]
	[SerializeField] private RectTransform shopButtonRect;
	[SerializeField] private RectTransform inventoryButtonRect;

	[Header("UI References")]
	[SerializeField] private ShopUI shopUI;
	[SerializeField] private InventoryUI inventoryUI;

	[SerializeField] private bool debugTouches = true;

	private void Update()
	{
		if (Input.touchCount <= 0)
			return;

		Touch touch = Input.GetTouch(0);

		if (touch.phase != TouchPhase.Ended)
			return;

		Vector2 touchPosition = touch.position;

		if (debugTouches) {
			Debug.Log("TOQUE DETECTADO: " + touchPosition);
		}

		if (shopButtonRect != null &&
			RectTransformUtility.RectangleContainsScreenPoint(shopButtonRect, touchPosition, null)) {

			Debug.Log("SHOP TOCADO VIA MOBILE FIX");

			if (shopUI != null) {
				shopUI.OpenShop();
			}

			return;
		}

		if (inventoryButtonRect != null &&
			RectTransformUtility.RectangleContainsScreenPoint(inventoryButtonRect, touchPosition, null)) {

			Debug.Log("INVENTORY TOCADO VIA MOBILE FIX");

			if (inventoryUI != null) {
				inventoryUI.OpenInventory();
			}

			return;
		}
	}
}
using UnityEngine;

public class PortalController : MonoBehaviour
{
	// Todos os colliders do portal (raiz + filhos)
	private Collider[] portalColliders;

	private bool triggered = false;
	private bool canWin = false;

	private void Awake()
	{
		// Pega todos os colliders do portal, inclusive em filhos inativos/ativos
		portalColliders = GetComponentsInChildren<Collider>(true);

		// O portal nasce sem poder vencer
		SetCanWin(false);
	}

	public void SetCanWin(bool value)
	{
		canWin = value;

		if (portalColliders == null || portalColliders.Length == 0) {
			return;
		}

		for (int i = 0; i < portalColliders.Length; i++) {
			if (portalColliders[i] != null) {
				portalColliders[i].enabled = value;
			}
		}

		//Debug.Log("Portal canWin = " + canWin);
	}

	private void OnTriggerEnter(Collider other)
	{
		//Debug.Log("Portal OnTriggerEnter chamado. canWin = " + canWin + " | objeto = " + other.name);

		if (triggered) {
			return;
		}

		if (!canWin) {
			return;
		}

		if (!other.CompareTag("Ball")) {
			return;
		}

		triggered = true;

		//Debug.Log("Portal ativou vitória.");

		if (GameplayController.instance != null) {
			GameplayController.instance.TriggerWin();
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GemScript : MonoBehaviour
{
    [SerializeField]
    private GameObject sparkleFX;

	private void OnTriggerEnter(Collider other)
	{
		if(other.CompareTag("Ball")) {
			Instantiate(sparkleFX, transform.position, Quaternion.identity);
			if (GameplayController.instance != null) {
				GameplayController.instance.PlayCollectableSound();
			}
			if (ScoreManager.instance != null) {
				ScoreManager.instance.AddGem();
			}
			gameObject.SetActive(false);
		}
	}

} //class

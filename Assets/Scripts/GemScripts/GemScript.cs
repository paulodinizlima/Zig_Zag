using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GemScript : MonoBehaviour
{
    [SerializeField]
    private GameObject sparkleFX;

	private void OnTriggerEnter(Collider other)
	{
		if(other.tag == "Ball") {
			Instantiate(sparkleFX, transform.position, Quaternion.identity);
			GameplayController.instance.PlayCollectableSound();
			gameObject.SetActive(false);
		}
	}

} //class

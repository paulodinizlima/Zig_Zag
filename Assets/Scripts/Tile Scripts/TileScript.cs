using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileScript : MonoBehaviour
{

    private Rigidbody myBody;
    private AudioSource audioSource;

	[SerializeField]
	private GameObject gem;

	[SerializeField]
	private float chanceForCollectable;

	private void Awake()
	{
		myBody = GetComponent<Rigidbody>();
		audioSource = GetComponent<AudioSource>();
	}
	private void Start()
	{
		if(Random.value < chanceForCollectable) {
			Vector3 temp = transform.position;
			temp.y += 1.5f;
			Instantiate(gem, temp, Quaternion.identity);
		}		
	}

	void OnTriggerExit(Collider other)
	{
		if (other.tag == "Ball") {
			StartCoroutine(TriggerFallingDown());
		}
	}
	IEnumerator TriggerFallingDown()
	{
		yield return new WaitForSeconds(0.3f);
		myBody.isKinematic = false;
		audioSource.Play();
		StartCoroutine(TurnOffGameObject());
	}
	IEnumerator TurnOffGameObject()
	{
		yield return new WaitForSeconds(2f);
		gameObject.SetActive(false);
	}

	


} //class

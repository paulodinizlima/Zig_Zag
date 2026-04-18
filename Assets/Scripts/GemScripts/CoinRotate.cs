using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CoinRotate : MonoBehaviour
{

	private float speed = 1f;
	private float angle;

	private void Update()
	{
		angle = (angle + speed) % 360f;
		transform.localRotation = Quaternion.Euler(new Vector3(90f, angle, 0f));
	}

} //class

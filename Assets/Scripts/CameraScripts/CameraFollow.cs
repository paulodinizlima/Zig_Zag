using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Transform ballTarget;
    private Vector3 oldPosition;

	private void Awake()
	{
		ballTarget = GameObject.FindGameObjectWithTag("Ball").transform;
		oldPosition = ballTarget.position;
	}

	private void FixedUpdate()
	{
		if (ballTarget) {
			Vector3 newPosition = ballTarget.position;
			Vector3 delta = oldPosition - newPosition;
			delta.y = 0f;
			transform.position = transform.position - delta;
			oldPosition = newPosition;
		}
	}


} //class

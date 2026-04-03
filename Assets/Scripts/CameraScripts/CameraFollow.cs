using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform ballTarget;

    [Header("Follow Settings")]
    [SerializeField] private Vector3 offset = new Vector3(6f, 8f, -6f);
    [SerializeField] private float followSpeed = 8f;

	private void Start()
	{
		if(ballTarget == null) {
			GameObject ball = GameObject.FindGameObjectWithTag("Ball");

			if(ball != null) {
				ballTarget = ball.transform;
			}
		}
	}

	private void LateUpdate()
	{
		if (ballTarget == null)
			return;

		Vector3 desiredPosition = ballTarget.position + offset;

		transform.position = Vector3.Lerp(
			transform.position,
			desiredPosition,
			followSpeed * Time.deltaTime
		);
	}


} //class

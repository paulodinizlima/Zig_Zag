using UnityEngine;

public class FollowXZ : MonoBehaviour
{
	[SerializeField] private Transform target;
	[SerializeField] private bool followX = true;
	[SerializeField] private bool followZ = true;
	[SerializeField] private Vector3 offset;

	private void Start()
	{
		if (target == null) {
			GameObject ball = GameObject.FindGameObjectWithTag("Ball");
			if (ball != null) {
				target = ball.transform;
			}
		}
	}

	private void LateUpdate()
	{
		if (target == null) {
			return;
		}

		Vector3 newPosition = transform.position;

		if (followX) {
			newPosition.x = target.position.x + offset.x;
		}

		if (followZ) {
			newPosition.z = target.position.z + offset.z;
		}

		newPosition.y = transform.position.y + offset.y;
		transform.position = newPosition;
	}
}

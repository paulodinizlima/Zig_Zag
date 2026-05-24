using UnityEngine;

public class AutoDestroyBehindBall : MonoBehaviour
{
	[Header("Return To Pool Settings")]
	[SerializeField] private float distanceBehindX = 18f;
	[SerializeField] private float distanceBehindZ = 18f;

	private Transform ballTarget;

	private void OnEnable()
	{
		FindBall();
	}

	private void FindBall()
	{
		GameObject ball = GameObject.FindGameObjectWithTag("Ball");

		if (ball != null) {
			ballTarget = ball.transform;
		}
	}

	private void Update()
	{
		if (ballTarget == null) {
			FindBall();
			return;
		}

		if (GameplayController.instance == null || !GameplayController.instance.gamePlaying) {
			return;
		}

		bool isBehindOnX = transform.position.x - ballTarget.position.x > distanceBehindX;
		bool isBehindOnZ = ballTarget.position.z - transform.position.z > distanceBehindZ;

		if (isBehindOnX || isBehindOnZ) {
			ReturnOrDestroy();
		}
	}

	private void ReturnOrDestroy()
	{
		if (TilePool.instance != null) {
			TilePool.instance.ReturnTile(gameObject);
		} else {
			Destroy(gameObject);
		}
	}
}
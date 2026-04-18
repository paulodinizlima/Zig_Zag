using UnityEngine;
public class AutoDestroyBehindBall : MonoBehaviour
{
	[Header("Destroy Settings")]
	//Distância atrás da bola para destruir o objeto
	[SerializeField] private float destroyDistance = 15f;

	private Transform ballTarget;

	private void Start()
	{
		//Tenta encontrar a bola pela tag
		GameObject ball = GameObject.FindGameObjectWithTag("Ball");

		if (ball != null) {
			ballTarget = ball.transform;
		}
	}

	private void Update()
	{
		//Se não encontrou a bola, não faz nada
		if (ballTarget == null) {
			return;
		}

		//Se o objeto ficou muito para trás da bola, destrói
		if (ballTarget.position.z - transform.position.z > destroyDistance) {
			Destroy(gameObject);
		}
	}
}

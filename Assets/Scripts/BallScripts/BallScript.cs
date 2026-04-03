using UnityEngine;

public class BallScript : MonoBehaviour
{
	[Header("Movement")]
	[SerializeField] private float speed = 4f;

	[Tooltip("Direção usada quando a bola está indo para a esquerda no mapa.")]
	[SerializeField] private Vector3 leftDirection = new Vector3(-1f, 0f, 0f);

	[Tooltip("Direção usada quando a bola está indo para frente no mapa.")]
	[SerializeField] private Vector3 forwardDirection = new Vector3(0f, 0f, 1f);

	[Header("Fall")]
	[SerializeField] private float destroyY = -3f;

	[Header("Optional Visual")]
	[SerializeField] private Transform visualToRotate;
	[SerializeField] private float visualRotationSpeed = 720f;

	


} //class

using UnityEngine;

public class GrassScroller : MonoBehaviour
{
	[Header("References")]
	//Bola que serve como centro da grade de água
	[SerializeField] private Transform ballTarget;

	[Header("Grass Planes")]
	//Lista dos 9 planes de água
	[SerializeField] private Transform[] grassPlanes;

	[Header("Grid Settings")]
	//Tamanho de cada célula da grade no mundo
	[SerializeField] private float cellSize = 100f;

	//Altura fixa da água
	[SerializeField] private float grassY = -3.5f;

	[Header("Parallax")]
	//Quanto a água acompanha a bola
	//1 = acompanha exatamente
	//menor que 1 = efeito parallax
	[SerializeField] private float parallaxFactor = 0.92f;

	private void Start()
	{
		//Se a bola não estiver ligada no Inspector, tenta encontrar pela tag
		if (ballTarget == null) {
			GameObject ball = GameObject.FindGameObjectWithTag("Ball");

			if (ball != null) {
				ballTarget = ball.transform;
			}
		}

		//Organiza os planes logo no início
		UpdateGrassGrid();
	}


	private void Update()
	{
		if (ballTarget == null || grassPlanes == null || grassPlanes.Length == 0) {
			return;
		}
		UpdateGrassGrid();
	}

	//Mantém os planes organizados em uma grade 3x3 com leve parallax
	private void UpdateGrassGrid()
	{
		//Aplica parallax na posição da bola
		float parallaxX = ballTarget.position.x * parallaxFactor;
		float parallaxZ = ballTarget.position.z * parallaxFactor;

		//Descobre a célula central da grade com base na posição "parallax"
		int centerCellX = Mathf.RoundToInt(parallaxX / cellSize);
		int centerCellZ = Mathf.RoundToInt(parallaxZ / cellSize);

		int index = 0;

		//Monta uma grade 3x3 ao redor da célula central da bola
		for (int z = -1; z <= 1; z++) {
			for (int x = -1; x <= 1; x++) {
				if (index >= grassPlanes.Length) {
					return;
				}

				float targetX = (centerCellX + x) * cellSize;
				float targetZ = (centerCellZ + z) * cellSize;

				grassPlanes[index].position = new Vector3(targetX, grassY, targetZ);

				index++;
			}
		}
	}
}

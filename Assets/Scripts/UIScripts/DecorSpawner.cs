using UnityEngine;

public class DecorSpawner : MonoBehaviour
{
	public static DecorSpawner instance;

	[Header("Decoration Prefabs")]
	//Lista de prefabs decorativos possíveis
	[SerializeField] private GameObject[] decorPrefabs;

	[Header("Spawn Chance")]
	//Chance de um tile gerar decoração por perto
	[SerializeField] private float spawnChance = 0.25f;

	[Header("Position Offsets")]
	//Distância mínima lateral em relação ao tile
	[SerializeField] private float minSideOffset = 3f;

	//Distância máxima lateral em relação ao tile
	[SerializeField] private float maxSideOffset = 6f;

	//Distância mínima para frente/atrás em relação ao tile
	[SerializeField] private float minForwardOffset = 1f;

	//Sistância máxima para frente/atrás em relação ao tile
	[SerializeField] private float maxForwardOffset = 4f;

	[Header("Height")]
	//Altura em que os props devem nascer
	[SerializeField] private float decorY = -3f;

	[Header("Random Rotation")]
	//Permite girar o prop aleatoriamente no eixo Y
	[SerializeField] private bool randomYRotation = true;

	[Header("Random Scale")]
	//Permite variar um pouco a escala dos props
	[SerializeField] private bool randomScale = true;

	[SerializeField] private float minScaleMultiplier = 0.85f;
	[SerializeField] private float maxScaleMultiplier = 1.2f;

	private void Awake()
	{
		//Garante uma única instância
		if (instance != null && instance != this) {
			Destroy(gameObject);
			return;
		}
		instance = this;
	}
	//Tenta gerar um prop decorativo perto da posição de um tile
	public void TrySpawnDecorNear(Vector3 tilePosition)
	{
		//Se não houver prefabs, não faz nada
		if (decorPrefabs == null || decorPrefabs.Length == 0) {
			return;
		}

		//Sorteia se vai gerar ou não
		if (Random.value > spawnChance) {
			return;
		}

		//Escolhe um lado aleatório: esquerda ou direita
		float sideSign = Random.value < 0.5f ? -1f : 1f;

		//Escolhe um deslocamento lateral
		float sideOffset = Random.Range(minSideOffset, maxSideOffset) * sideSign;

		//Escolhe um deslocamento adicional em profundidade
		float forwardOffset = Random.Range(minForwardOffset, maxForwardOffset);

		//Monta a posição final do prop
		Vector3 spawnPosition = new Vector3(tilePosition.x + sideOffset, decorY, tilePosition.z + forwardOffset);

		//Escolhe um prefab aleatório
		GameObject chosenPrefab = decorPrefabs[Random.Range(0, decorPrefabs.Length)];

		//Define rotação inicial
		Quaternion rotation = Quaternion.identity;

		if (randomYRotation) {
			rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
		}

		//Instancia o prop
		GameObject spawnedDecor = Instantiate(chosenPrefab, spawnPosition, rotation);

		//Aplica variação leve de escala, se ativado
		if (randomScale) {
			float scaleMultiplier = Random.Range(minScaleMultiplier, maxScaleMultiplier);
			spawnedDecor.transform.localScale *= scaleMultiplier;
		}
	}
}

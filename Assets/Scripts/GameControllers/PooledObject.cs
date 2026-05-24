using UnityEngine;

public class PooledObject : MonoBehaviour
{
	public GameObject SourcePrefab { get; private set; }

	public void SetSourcePrefab(GameObject prefab)
	{
		SourcePrefab = prefab;
	}
}
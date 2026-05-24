using System.Collections.Generic;
using UnityEngine;

public class TilePool : MonoBehaviour
{
	public static TilePool instance;

	[Header("Pool Settings")]
	[SerializeField] private GameObject[] tilePrefabs;
	[SerializeField] private int initialAmountPerPrefab = 20;

	private readonly Dictionary<GameObject, Queue<GameObject>> pools = new Dictionary<GameObject, Queue<GameObject>>();

	private void Awake()
	{
		if (instance != null && instance != this) {
			Destroy(gameObject);
			return;
		}

		instance = this;

		PrewarmPools();
	}

	private void PrewarmPools()
	{
		foreach (GameObject prefab in tilePrefabs) {
			if (prefab == null) {
				continue;
			}

			if (!pools.ContainsKey(prefab)) {
				pools.Add(prefab, new Queue<GameObject>());
			}

			for (int i = 0; i < initialAmountPerPrefab; i++) {
				GameObject obj = Instantiate(prefab, transform);
				obj.SetActive(false);

				PooledObject pooledObject = obj.GetComponent<PooledObject>();

				if (pooledObject == null) {
					pooledObject = obj.AddComponent<PooledObject>();
				}

				pooledObject.SetSourcePrefab(prefab);

				pools[prefab].Enqueue(obj);
			}
		}
	}

	public GameObject GetTile(GameObject prefab, Vector3 position, Quaternion rotation)
	{
		if (prefab == null) {
			return null;
		}

		if (!pools.ContainsKey(prefab)) {
			pools.Add(prefab, new Queue<GameObject>());
		}

		GameObject obj;

		if (pools[prefab].Count > 0) {
			obj = pools[prefab].Dequeue();
		} else {
			obj = Instantiate(prefab, transform);

			PooledObject pooledObject = obj.GetComponent<PooledObject>();

			if (pooledObject == null) {
				pooledObject = obj.AddComponent<PooledObject>();
			}

			pooledObject.SetSourcePrefab(prefab);
		}

		obj.transform.SetPositionAndRotation(position, rotation);
		obj.SetActive(true);

		return obj;
	}

	public void ReturnTile(GameObject obj)
	{
		if (obj == null) {
			return;
		}

		PooledObject pooledObject = obj.GetComponent<PooledObject>();

		if (pooledObject == null || pooledObject.SourcePrefab == null) {
			obj.SetActive(false);
			return;
		}

		GameObject prefab = pooledObject.SourcePrefab;

		if (!pools.ContainsKey(prefab)) {
			pools.Add(prefab, new Queue<GameObject>());
		}

		obj.SetActive(false);
		obj.transform.SetParent(transform);

		pools[prefab].Enqueue(obj);
	}
}

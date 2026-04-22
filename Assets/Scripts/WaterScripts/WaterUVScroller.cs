using UnityEngine;

public class WaterUVScroller : MonoBehaviour
{
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Vector2 scrollSpeed = new Vector2(0.015f, 0.01f);

    private Material runtimeMaterial;
    private Vector2 currentOffset;

	private void Awake()
	{
		if (targetRenderer == null) {
			targetRenderer = GetComponent<Renderer>();
		}

		if (targetRenderer != null) {
			runtimeMaterial = targetRenderer.material;
		}
	}

	private void Update()
	{
		if (runtimeMaterial == null) {
			return;
		}

		currentOffset += scrollSpeed * Time.deltaTime;
		runtimeMaterial.mainTextureOffset = currentOffset;
	}
}

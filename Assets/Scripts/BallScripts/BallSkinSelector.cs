using UnityEngine;

public class BallSkinSelector : MonoBehaviour
{
	[System.Serializable]
	public class BallSkinEntry
	{
		public BallSkinType skinType;
		public Material material;
	}

	[SerializeField] private MeshRenderer targetRenderer;

	[SerializeField] private BallSkinEntry[] skins;

	private void Start()
	{
		ApplyCurrentSkin();
	}

	public void ApplyCurrentSkin()
	{
		if (BallSkinData.instance == null) {
			return;
		}

		BallSkinType activeSkin = BallSkinData.instance.GetActiveSkin();

		for (int i = 0; i < skins.Length; i++) {

			if (skins[i].skinType == activeSkin) {

				if (skins[i].material != null) {
					targetRenderer.material = skins[i].material;

					BallScript ballScript = GetComponent<BallScript>();

					if (ballScript == null) {
						ballScript = GetComponentInParent<BallScript>();
					}

					if (ballScript != null) {
						ballScript.RefreshBallMaterialAfterSkinChange();
					}
				}

				return;
			}
		}
	}

	public void SetSkin(BallSkinType skin)
	{
		if (BallSkinData.instance == null) {
			return;
		}

		BallSkinData.instance.SetActiveSkin(skin);

		ApplyCurrentSkin();
	}
}
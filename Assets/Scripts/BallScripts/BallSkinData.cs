using UnityEngine;

public class BallSkinData : MonoBehaviour
{
	public static BallSkinData instance;

	private const string ActiveSkinKey = "ACTIVE_BALL_SKIN";

	private void Awake()
	{
		if (instance != null && instance != this) {
			Destroy(gameObject);
			return;
		}

		instance = this;
	}

	public BallSkinType GetActiveSkin()
	{
		return (BallSkinType)PlayerPrefs.GetInt(ActiveSkinKey, 0);
	}

	public void SetActiveSkin(BallSkinType skin)
	{
		PlayerPrefs.SetInt(ActiveSkinKey, (int)skin);
		PlayerPrefs.Save();
	}
}
using UnityEngine;

public class BallSkinSaveData : MonoBehaviour
{
	public static BallSkinSaveData instance;

	private void Awake()
	{
		if (instance != null && instance != this) {
			Destroy(gameObject);
			return;
		}

		instance = this;
	}

	private string GetSkinKey(BallSkinType skin)
	{
		return "BALL_SKIN_UNLOCKED_" + skin;
	}

	public bool IsUnlocked(BallSkinType skin)
	{
		// Default sempre desbloqueada
		if (skin == BallSkinType.Default) {
			return true;
		}

		return PlayerPrefs.GetInt(GetSkinKey(skin), 0) == 1;
	}

	public void UnlockSkin(BallSkinType skin)
	{
		PlayerPrefs.SetInt(GetSkinKey(skin), 1);
		PlayerPrefs.Save();
	}
}

using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour {
	
	void LateUpdate () {
		if (ClientPlayer.mine == null || ClientPlayer.mine.avatar == null) return;
		MapData map = Data.maps[Map.current];
		Transform avatar = ClientPlayer.mine.avatar.transform;
		Vector3 pos = new Vector3(0f, 0f, -1f);
		if (Data.maps[Map.current].width * 32f <= 512)
			pos.x = 512 / 2f;
		else
			pos.x = Mathf.FloorToInt(Mathf.Clamp(avatar.position.x, 512 / 2f, map.width * 32f - 512 / 2f)) + 0.5f;
		if (Data.maps[Map.current].height * 32f <= Screen.height)
			pos.y = -Screen.height / 2f;
		else
			pos.y = Mathf.FloorToInt(Mathf.Clamp(avatar.position.y, -map.height * 32f + Screen.height / 2f, -Screen.height / 2f)) + 0.5f;
		transform.position = pos;
	}
}

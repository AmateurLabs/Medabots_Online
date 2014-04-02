using UnityEngine;
using System.Collections;

public class CameraSwitch : MonoBehaviour {
	
	public GameMode[] modes;
	
	void LateUpdate () {
		camera.enabled = false;
		foreach (GameMode mode in modes)
			if (mode == Game.mode) camera.enabled = true;
	}
}

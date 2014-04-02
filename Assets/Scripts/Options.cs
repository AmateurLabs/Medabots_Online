using UnityEngine;
using System.Collections;

public class Options : MonoBehaviour {
	
	public static bool autoPath;
	
	public static bool visible;
	public static Rect rect = new Rect(192f, 192f, 128f, 128f);
	
	void Awake() {
		autoPath = (PlayerPrefs.GetInt("autoPath", 1) == 1);
	}
	
	void Update() {
		if (Game.netState == NetState.LoggedIn && Input.GetKeyDown(KeyCode.O)) {
			visible = !visible;
		}
		if (Input.GetKeyDown(KeyCode.Escape)) {
			visible = false;
		}
	}
	
	void OnGUI() {
		if (!visible) {
			return;
		}
		rect = GUILayout.Window(67, rect, Window, "Options");
		rect.x = Mathf.Clamp(rect.x, 0f, Screen.width - rect.width);
		rect.y = Mathf.Clamp(rect.y, 0f, Screen.height - rect.height);
	}
	
	void Window(int id){
		GUILayout.BeginVertical();
		autoPath = GUILayout.Toggle(autoPath, "Auto-Pathing Enabled");
		GUILayout.EndVertical();
		if (GUI.Button(new Rect(rect.width - 24f, 0f, 24f, 24f), " X", "label")) visible = false;
		GUI.DragWindow();
	}
	
	void OnApplicationQuit() {
		PlayerPrefs.SetInt("autoPath", (autoPath) ? 1 : 0);
	}
}

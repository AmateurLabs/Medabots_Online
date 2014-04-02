using UnityEngine;
using System.Collections;

public class XGUI : MonoBehaviour {
	public static XGUI use;
	
	public GUISkin skin;
	
	void Awake() {
		use = this;
	}
	
	public static void Init() {
		GUI.skin = use.skin;
	}
	
	void OnGUI() {
		Init();
	}
	
	public static void ValueBar (Rect rect, float val, string label, Color color, bool flip) {
		GUI.Box(rect, "", "ValueBar");
		Color oldColor = GUI.color;
		GUI.color = new Color(0.875f, 0.875f, 0.875f, 1f);
		GUI.skin.label.alignment = flip ? TextAnchor.UpperRight : TextAnchor.UpperLeft;
		GUI.Label(new Rect(rect.x + (flip ? 0 : 8), rect.y + 4, rect.width - 8, rect.height - 4), label);
		GUI.color = color;
		GUI.BeginGroup(new Rect(rect.x + (flip ? rect.width - rect.width * val : 0), rect.y, rect.width * val, rect.height));
		GUI.Box(new Rect((flip ? -(rect.width - rect.width * val) : 0), 0, rect.width, rect.height), "", "ValueBarFill");
		GUI.color = new Color(color.r + 0.875f, color.g + 0.875f, color.b + 0.875f, 1f);
		GUI.Label(new Rect((flip ? -(rect.width - rect.width * val) : 8), 4, rect.width - 8, rect.height - 4), label);
		GUI.EndGroup();
		GUI.skin.label.alignment = TextAnchor.UpperLeft;
		GUI.color = oldColor;
	}
}

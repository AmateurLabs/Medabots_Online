using UnityEngine;
using System.Collections;

public class Fade : MonoBehaviour {
	public static Fade use;
	
	void Awake() {
		use = this;
	}
	
	public static void In(float duration) {
		use.StartCoroutine(use.DoFade(duration, false));
	}
	
	public static void Out(float duration) {
		use.StartCoroutine(use.DoFade(duration, true));
	}
	
	private Color inColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
	private Color outColor = new Color(0f, 0f, 0f, 0f);
	
	public IEnumerator DoFade(float dur, bool dir) {
		if (dir)
			guiTexture.color = outColor;
		else
			guiTexture.color = inColor;
		float t = 0;
		while (t < 1f) {
			if (dir)
				guiTexture.color = Color.Lerp(outColor, inColor, t);
			else
				guiTexture.color = Color.Lerp(inColor, outColor, t);
			t += Time.deltaTime / dur;
			yield return null;
		}
		if (dir)
			guiTexture.color = inColor;
		else
			guiTexture.color = outColor;
	}
}

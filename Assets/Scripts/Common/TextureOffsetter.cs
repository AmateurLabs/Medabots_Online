using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class TextureOffsetter : MonoBehaviour {

	public Vector2 offset;
	public Vector2 scale;
	
	void Update () {
		int width = renderer.sharedMaterial.mainTexture.width;
		int height = renderer.sharedMaterial.mainTexture.height;
		renderer.sharedMaterial.mainTextureOffset = new Vector2(offset.x / width, (height - offset.y - scale.y) / height);
		renderer.sharedMaterial.mainTextureScale = new Vector2(scale.x / width, scale.y / height);
	}
}

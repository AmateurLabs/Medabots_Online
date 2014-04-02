using UnityEngine;
using System.Collections;

public class UVScroll : MonoBehaviour {

	public Vector2 speed;
	
	void Update () {
		renderer.material.mainTextureOffset += speed * Time.deltaTime;
	}
}

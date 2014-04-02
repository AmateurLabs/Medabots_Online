using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Avatar : MonoBehaviour {
	public ClientPlayer player;
	public string[] layers;
	public Texture2D blankTex;
	public bool moving;
	public bool pathing;
	public bool waiting;
	public Dir dir;
	public int step;
	
	void Awake() {
		Texture2D tex = new Texture2D(blankTex.width, blankTex.height, TextureFormat.RGBA32, false);
		tex.filterMode = FilterMode.Point;
		renderer.material.mainTexture = tex;
		SetTexture();
	}
	
	void Update() {		
		if (!moving && !waiting && pathing)
			AutoPath();
	}

	public void OnPathComplete(List<Point> p) {
		path = p;
		if (path.Count > 0) pathing = true;
	}

	List<Point> path;

	void AutoPath() {
		Dir direction = (path[0] - player.loc).dir;
		if (direction == Dir.None) Debug.LogWarning("Bad direction for " + (path[0] - player.loc));
		NetClient.use.AskMove(direction, true);
		path.RemoveAt(0);
		if (path.Count == 0) pathing = false;
	}
	
	public void SetTexture() {
		if (player != null) {
			for (Layers i = Layers.Base; i < Layers.Length; i ++) {
				layers[(int)i] = Data.itemNode[""+i][player.outfit[i]].Value;
			}
		}
		Texture2D tex = (Texture2D)renderer.material.mainTexture;
		Color[] colors = blankTex.GetPixels();
		for (Layers i = Layers.Base; i < Layers.Length; i ++) {
			Texture2D layer = (Texture2D)Resources.Load("Players/" + i + "/" +layers[(int)i]);
			if (layer == null) continue;
			Color[] layerColors = layer.GetPixels();
			for (int t = 0; t < layerColors.Length; t ++) {
				if (layerColors[t].a > 0f)
					colors[t] = layerColors[t];
			}
		}
		tex.SetPixels(colors);
		tex.Apply();
	}
	
	public void SetOffset() {
		renderer.material.mainTextureOffset = new Vector2(0.25f * ((int)dir-1) + (1f/12f) * step, 0f);
	}
	
	public void Move(Dir direction, bool auto) {
		if (!auto && pathing) pathing = false;
		dir = direction;
		SetOffset();
		Point targetLoc = player.loc + Point.FromDir(dir);
		player.loc = targetLoc;
		moving = true;
		StartCoroutine(MoveRoutine());
	}
	
	public void Warp(int map, int x, int y) {
		StartCoroutine(WarpRoutine(map, x, y));
	}
	
	IEnumerator WarpRoutine(int map, int x, int y) {
		moving = true;
		Fade.Out(1f/3f);
		yield return new WaitForSeconds(1f/3f);
		Map.Load(map);
		player.loc = new Point(x, y);
		Vector3 newPos = (Vector3)player.loc;
		newPos.z = 0.001f * newPos.y;
		transform.localPosition = newPos - (Vector3)Point.FromDir(dir);
		StartCoroutine(MoveRoutine());
		Fade.In(1f);
		waiting = false;
	}
	
	private IEnumerator MoveRoutine() {
		Vector3 pos = (Vector3)(player.loc - Point.FromDir(dir));
		Vector3 targ = (Vector3)player.loc;
		targ.z = 0f;
		float t = 0f;
		while (t < 1f) {
			transform.localPosition = Vector3.Lerp(pos, targ, t);
			Vector3 wPos = transform.position;
			wPos.x = Mathf.Round(wPos.x);
			wPos.y = Mathf.Round(wPos.y);
			wPos.z = (transform.localPosition.y + 0.1f * (int)player.userlevel) * transform.parent.localScale.z;
			transform.position = wPos;
			t += Time.deltaTime * 3f;
			step = Mathf.RoundToInt(t * 3f) % 3;
			SetOffset();
			yield return null;
		}
		targ.z = transform.localPosition.y + 0.1f * (int)player.userlevel;
		transform.localPosition = targ;
		moving = false;
	}
	
	public enum Layers {
		Base,
		Top,
		Bottom,
		Shoes,
		Coat,
		Eyes,
		Mask,
		Hair,
		Length
	}
}

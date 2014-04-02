using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class Paperdoll : MonoBehaviour {
	public static ALDNode offsetData;
	
	void Awake () {
#if !UNITY_WEBPLAYER
		Screen.SetResolution(800, 512, false);
		Files = new List<ImageFile>();
		filePath = Application.dataPath + ((Application.isEditor) ? "/Resources/Bots/" : "/../");
		if (File.Exists(filePath+"Offsets.ald")) {
			offsetData = ALDNode.ParseFile(filePath+"Offsets.ald");
		}else{
			offsetData = new ALDNode("", "");
		}
		BuildFileList();
		Application.RegisterLogCallback((log, stack, type) => {
			debug.text += log + '\n';
		});
#else
		offsetData = ALDNode.ParseString(Resources.Load<TextAsset>("Bots/Offsets").text);
#endif
	}

#if !UNITY_WEBPLAYER

	public List<ImageFile> Files;
	public string filePath;
	public static Dictionary<string, string> offsetStrings = new Dictionary<string, string>();

	void OnApplicationQuit() {
		File.WriteAllText(filePath+"Offsets.ald", offsetData.Serialize());
	}
	
	void BuildFileList() {
		foreach (ImageFile file in Files) {
			Destroy(file.Image);
		}
		Files.Clear();
		foreach (string path in Directory.GetFiles(filePath, "*.png")) {
			Files.Add(new ImageFile(Path.GetFileNameWithoutExtension(path), path));
		}
	}

	public GUIText debug;
	
	Vector2 scrollPos;

	void OnGUI () {
		GUILayout.BeginArea(new Rect(0f, 0f, 512f, 512f));
		GUILayout.BeginVertical();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Animation Type:");
		AnimBot.use.animType = (AnimType)GUILayout.SelectionGrid((int)AnimBot.use.animType, System.Enum.GetNames(typeof(AnimType)), 3);
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		AnimBot.use.flipped = GUILayout.Toggle(AnimBot.use.flipped, "Flip", "button");
		AnimBot.use.female = GUILayout.Toggle(AnimBot.use.female, "Female", "button");
		if (GUILayout.Button(((AnimBot.use.animation.isPlaying) ? "Pause" : "Resume") + " Animation")) {
			if (AnimBot.use.animation.isPlaying) {
				AnimBot.use.animation.Stop();
			}else{
				AnimBot.use.animation.Play();
			}
		}
		GUILayout.EndHorizontal();
		scrollPos = GUILayout.BeginScrollView(scrollPos, false, true);

		foreach (ImageFile file in Files) {
			GUILayout.BeginHorizontal();
			GUILayout.Label(file.Name, GUILayout.Width(96f));
			if (GUILayout.Button("Head")) {
				AnimBot.use.headTex = file.Image;
			}else if (GUILayout.Button("Left Arm")) {
				AnimBot.use.lArmTex = file.Image;
			}else if (GUILayout.Button("Right Arm")) {
				AnimBot.use.rArmTex = file.Image;
			}else if (GUILayout.Button("Legs")) {
				AnimBot.use.legsTex = file.Image;
			}else if (GUILayout.Button("Refresh")) {
				file.Refresh();
			}
			GUILayout.EndHorizontal();
		}
		GUILayout.EndScrollView();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Clear", GUILayout.Width(96f));
		if (GUILayout.Button("Head")) {
			AnimBot.use.headTex = null;
		}else if (GUILayout.Button("Left Arm")) {
			AnimBot.use.lArmTex = null;
		}else if (GUILayout.Button("Right Arm")) {
			AnimBot.use.rArmTex = null;
		}else if (GUILayout.Button("Legs")) {
			AnimBot.use.legsTex = null;
		}else if (GUILayout.Button("All")) {
			AnimBot.use.headTex = null;
			AnimBot.use.lArmTex = null;
			AnimBot.use.rArmTex = null;
			AnimBot.use.legsTex = null;
		}
		GUILayout.EndHorizontal();
		if (GUILayout.Button("Refresh File List")) {
			BuildFileList();
		}
		int i = 1;
		GUILayout.BeginHorizontal();
		foreach (Transform child in AnimBot.use.GetComponentsInChildren<Transform>()) {
			if (child.name != "Graphics") continue;
			if (!offsetData.Contains(child.renderer.material.mainTexture.name)) offsetData.AddNode(new ALDNode(child.renderer.material.mainTexture.name, ""));
			ALDNode data = offsetData[child.renderer.material.mainTexture.name];
			Vector3 pos = child.localPosition;
			string name = child.parent.name;
			if (!data.Contains(name)) data.AddNode(new ALDNode(name, "" + pos.x + " " + pos.y + " " + pos.z));
			string[] strings = data[name].Value.ToString().Split(' ');
			GUILayout.Label(name, GUILayout.MinWidth(64f));
			strings[0] = GUILayout.TextField(strings[0], GUILayout.MinWidth(24f));
			strings[1] = GUILayout.TextField(strings[1], GUILayout.MinWidth(24f));
			strings[2] = GUILayout.TextField(strings[2], GUILayout.MinWidth(24f));
			float.TryParse(strings[0], out pos.x);
			float.TryParse(strings[1], out pos.y);
			float.TryParse(strings[2], out pos.z);
			child.localPosition = pos;
			data[name].Value = strings[0] + " " + strings[1] + " " + strings[2];
			if (i % 3 == 0) {
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
			}
			i ++;
		}
		if (GUILayout.Button("Save All")) {
			File.WriteAllText(filePath+"Offsets.ald", offsetData.Serialize());
		}
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();
		GUILayout.EndArea();
	}

	public void LateUpdate() {
		AnimBot.use.Refresh();
	}
	
	public class ImageFile {
		public string Name;
		public string Path;
		private Texture2D img;
		
		public Texture2D Image {
			get {
				if (img == null) {
					img = new Texture2D(128, 192, TextureFormat.ARGB32, false, true);
					img.LoadImage(System.IO.File.ReadAllBytes(Path));
					img.filterMode = FilterMode.Point;
					img.name = Name;
				}
				return img;
			}
		}
		
		public void Refresh() {
			if (img == null) {
				img = Image;
			}else{
				img.LoadImage(System.IO.File.ReadAllBytes(Path));
			}
		}
		
		public ImageFile(string name, string path) {
			Name = name;
			Path = path;
		}
	}
#endif
}

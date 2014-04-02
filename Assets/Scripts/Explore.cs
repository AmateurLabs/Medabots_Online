using UnityEngine;
using System;
using System.Collections.Generic;

public class Explore : MonoBehaviour {
	
	void Update() {
		if (Game.mode != GameMode.Explore || ClientPlayer.mine == null || ClientPlayer.mine.avatar == null) return;
		Avatar avatar = ClientPlayer.mine.avatar;
		if (!avatar.moving && !avatar.waiting) {
			if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
				NetClient.use.AskMove(Dir.Right, false);
			else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
				NetClient.use.AskMove(Dir.Left, false);
			else if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
				NetClient.use.AskMove(Dir.Up, false);
			else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
				NetClient.use.AskMove(Dir.Down, false);
		}

		bool clickedOnGui = false;

		if (ctxMenu.Count > 0) {
			if (Input.mousePosition.x < ctxMenuPos.x
			    || Input.mousePosition.x > ctxMenuPos.x + ctxMenuWidth
			    || (Screen.height - Input.mousePosition.y) < ctxMenuPos.y
			    || (Screen.height - Input.mousePosition.y) > ctxMenuPos.y + ctxMenu.Count * 20f) ctxMenu.Clear();
			else clickedOnGui = true;
		}
		
		if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) {
			if (Input.mousePosition.x > Chat.use.window.x) return;
			if (Input.mousePosition.y < 24) return;
			if (clickedOnGui) return;
			Plane plane = new Plane(Vector3.back, Vector3.zero);
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			float dist = 0f;
			plane.Raycast(ray, out dist);
			Point loc = Map.PointFromWorld(ray.GetPoint(dist));

			ctxMenuPos = new Vector2(Input.mousePosition.x - 10f, Screen.height - Input.mousePosition.y - 10f);
			ctxMenu.Clear();
			ctxMenuWidth = 50f;
			foreach (ClientPlayer p in NetClient.use.players) {
				if (p.loc == loc && p != ClientPlayer.mine) {
					if (NetClient.use.challengers.Contains(p.dbId) || p.userlevel == UserLevel.NPC) {
						ctxMenu.Add(new ContextMenuOption<ClientPlayer>("Battle " + p.name, p, (auto, arg) => {
							if (Point.Distance(loc, ClientPlayer.mine.loc) <= 1) {
								NetClient.use.AskBattle(arg.dbId);
							} else {
								NetClient.use.Log("<color=orange>You're too far away to challenge " + arg.name + "!</color>");
							}
						}));
					}else{
						ctxMenu.Add(new ContextMenuOption<ClientPlayer>("Challenge " + p.name, p, (auto, arg) => { NetClient.use.AskChallenge(arg.dbId); }));
					}
					ctxMenu.Add(new ContextMenuOption<ClientPlayer>("Message " + p.name, p, (auto, arg) => {
						GUI.FocusControl("ChatField");
						Chat.use.chatMsg = "/tell " + arg.name + " ";
					}));
				}
			}
			foreach (ShopPoint sp in Data.maps[ClientPlayer.mine.map].shops) {
				if (sp.loc == loc) {
					ctxMenu.Add(new ContextMenuOption<int>("Browse " + sp.name, sp.id, (auto, arg) => {
						if (Point.Distance(loc, ClientPlayer.mine.loc) <= 1) {
							NetClient.use.AskShop(arg);
							Game.mode = GameMode.Shop;
						}else{
							NetClient.use.Log("<color=orange>You're too far away to browse that shop!</color>");
							//ClientPlayer.mine.avatar.OnPathComplete(Map.GetPath(ClientPlayer.mine.loc, loc));
						}
					}));
				}
			}
			if (Data.maps[Map.current].CanMove(loc)) {
				ctxMenu.Add(new ContextMenuOption<Point>("Walk Here", loc, (auto, arg) => {
					if ((!auto || Options.autoPath) && !avatar.waiting) ClientPlayer.mine.avatar.OnPathComplete(Map.GetPath(ClientPlayer.mine.loc, arg));
				}));
			}
			ctxMenu.Add(new ContextMenuOption<bool>("Cancel", false, null));

			if (Input.GetMouseButtonDown(0) && ctxMenu.Count > 0) {
				ctxMenu[0].Select(true);
			}
		}
	}

	public static List<ContextMenuOption> ctxMenu = new List<ContextMenuOption>();
	public static Vector2 ctxMenuPos = Vector2.zero;
	public static float ctxMenuWidth = 0f;

	void OnGUI() {
		if (Game.mode != GameMode.Explore) return;
		if (ClientPlayer.mine == null) return;
		GUI.depth = 100;
		foreach (ClientPlayer p in NetClient.use.players) {
			Vector3 screenPoint = Camera.main.WorldToScreenPoint(p.avatar.transform.position);
			screenPoint.x = Mathf.Round(screenPoint.x);
			screenPoint.y = Mathf.Round(screenPoint.y);
			GUIContent content = new GUIContent("<size=10><b>"+p.name+"</b></size>");
			Vector2 size = GUI.skin.label.CalcSize(content);
			GUI.color = Color.black;
			GUI.Label(new Rect(screenPoint.x - size.x / 2f, Screen.height - screenPoint.y - 41f, 200f, 24f), content);
			GUI.Label(new Rect(screenPoint.x - size.x / 2f, Screen.height - screenPoint.y - 39f, 200f, 24f), content);
			GUI.Label(new Rect(screenPoint.x+1 - size.x / 2f, Screen.height - screenPoint.y - 40f, 200f, 24f), content);
			GUI.Label(new Rect(screenPoint.x-1 - size.x / 2f, Screen.height - screenPoint.y - 40f, 200f, 24f), content);
			GUI.color = Color.white;
			GUI.Label(new Rect(screenPoint.x - size.x / 2f, Screen.height - screenPoint.y - 40f, 200f, 24f), "<color="+Player.userColors[(int)p.userlevel]+">"+content.text+"</color>");
		}
		if (ctxMenu.Count > 0) {
			foreach (ContextMenuOption option in ctxMenu) {
				ctxMenuWidth = Mathf.Max(ctxMenuWidth, GUI.skin.button.CalcSize(new GUIContent(option.text)).x);
			}
			GUI.Box(new Rect(ctxMenuPos.x, ctxMenuPos.y, ctxMenuWidth, ctxMenu.Count * 20f), "");
			for (int i = 0; i < ctxMenu.Count; i ++) {
				if (GUI.Button(new Rect(ctxMenuPos.x, ctxMenuPos.y + 20f * i, ctxMenuWidth, 20f), ctxMenu[i].text)) {
					ctxMenu[i].Select(false);
				}
			}
		}
	}

	public abstract class ContextMenuOption {
		public string text;
		public ContextMenuOption[] suboptions;
		public abstract void Select(bool auto);
	}

	public class ContextMenuOption<T> : ContextMenuOption {
		public T arg;
		public Action<bool, T> func;

		public ContextMenuOption(string text, T arg, Action<bool, T> func) {
			this.text = text;
			this.arg = arg;
			this.func = func;
		}

		public override void Select (bool auto)
		{
			if (func != null) func(auto, arg);
			Explore.ctxMenu.Clear();
			if (suboptions != null) Explore.ctxMenu.AddRange(suboptions);
		}
	}
}

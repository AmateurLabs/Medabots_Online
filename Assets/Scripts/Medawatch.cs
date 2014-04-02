using UnityEngine;
using System.Collections.Generic;

public class Medawatch : MonoBehaviour {
	public static Medawatch use;
	
	public List<Medabot> bots = new List<Medabot>();
	public List<Medal> medals = new List<Medal>();
	
	void Awake() {
		use = this;
	}
	
	public Medabot previewBot;
	public Item.Type tab;
	private Vector2 scroll;
	
	void OnGUI() {
		if (Game.mode != GameMode.Medawatch) return;
		XGUI.Init();
		GUILayout.BeginArea(new Rect(0f, 0f, Screen.width - 256, Screen.height), "Medabots", "window");
		GUILayout.BeginHorizontal();
		GUILayout.BeginVertical("Medabots", "window", GUILayout.Width(128f));
		foreach (Medabot bot in bots) {
			if (GUILayout.Button((bot.medal != null) ? ""+bot.medal : "-----")) {
				previewBot = bot;
				previewBot.TryLoadItems();
				if (bot.head != null && bot.lArm != null && bot.rArm != null && bot.legs != null && bot.tinpet != null) {
					AnimBot.use.bot = new Bot(previewBot);
					AnimBot.use.Refresh();
				}
			}
		}
		GUILayout.EndVertical();
		if (previewBot != null) {
			GUILayout.BeginVertical(GUILayout.Width(80f));
			for (Item.Type type = Item.Type.Head; type <= Item.Type.Medal; type ++){
				if (GUILayout.Button((tab == type) ? "<b>"+Item.typeNames[(int)type]+"</b>" : ""+Item.typeNames[(int)type])){
					tab = type;
				}
			}
			GUILayout.EndVertical();
			GUILayout.BeginVertical("Items", "window");
				scroll = GUILayout.BeginScrollView(scroll);
					if (tab != Item.Type.Invalid){
						if (previewBot != null && previewBot[tab] != null && GUILayout.Button("Remove Part")) {
							NetClient.use.AskEquipItem(previewBot.dbId, 0 - (int)tab);
						}
						foreach (Item item in Inventory.use.items) {
							if (item.type == tab){
								int amount = item.amount;
								foreach (Medabot bot in bots){
									if (bot[item.type] != null && bot[item.type].id == item.id) {
										amount --;
									}
								}
								if (GUILayout.Button((previewBot[tab] != null && previewBot[tab].id == item.id) ? "<b>"+item+" x"+amount+"</b>" : ""+item+" x"+amount, "label") && previewBot[tab] != item){
									NetClient.use.AskEquipItem(previewBot.dbId, item.id);
								}
							}
						}
					}
				GUILayout.EndScrollView();
			GUILayout.EndVertical();
			GUILayout.BeginVertical("Preview", "window", GUILayout.Width(128f));
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label(previewBot.GetImg());
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			string desc = "";
			if (previewBot.tinpet != null) desc += previewBot.tinpet + "\n";
			if (previewBot.medal != null) desc += previewBot.medal + "\n";
			if (previewBot.head != null) desc += previewBot.head + "\n";
			if (previewBot.lArm != null) desc += previewBot.lArm + "\n";
			if (previewBot.rArm != null) desc += previewBot.rArm + "\n";
			if (previewBot.legs != null) desc += previewBot.legs;
			GUILayout.Label(desc);
			if (previewBot[tab] != null) {
				GUILayout.Label(previewBot[tab].GetDescription());
			}
			GUILayout.EndVertical();
		}
		GUILayout.EndHorizontal();
		GUILayout.EndArea();
		if (GUI.Button(new Rect(Screen.width - 256f - 24f, 0f, 24f, 24f), " X", "label")) Game.mode = GameMode.Explore;
	}
	
	public void Update() {
		if (Game.mode == GameMode.Medawatch && Input.GetKeyDown(KeyCode.Escape)){
			previewBot = null;
			Game.mode = GameMode.Explore;
		}
		if (Input.GetKeyDown(KeyCode.Backslash)) {
			AnimBot.use.flipped = !AnimBot.use.flipped;
			AnimBot.use.Refresh();
		}
	}
}

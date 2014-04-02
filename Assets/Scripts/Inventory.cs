using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Inventory : MonoBehaviour {
	public static Inventory use;
	public List<Item> items = new List<Item>();
	public Item.Type tab;
	
	public void Awake() {
		use = this;
	}
	
	public Item previewItem;
	private bool showWindow;
	private Rect windowRect;
	private string windowName = "";
	private float windowTime = 0f;
	private int quantity = 1;
	private Vector2 scroll;
	private int clickCount;
	private bool selling;
	
	public void OnGUI() {
		Event e = Event.current;
		if (e.type == EventType.MouseDown) {
			clickCount = e.clickCount;
		}
		if (Game.mode == GameMode.Shop || Game.mode == GameMode.Inventory) {
			XGUI.Init();
			bool isShop = (Game.mode == GameMode.Shop);
			GUILayout.BeginArea(new Rect(0f, 0f, Screen.width - 256, Screen.height), (isShop) ? Shop.use.name : "Inventory", "window");
				GUILayout.BeginHorizontal();
					GUILayout.BeginVertical(GUILayout.Width(80f));
						if (isShop && GUILayout.Button((selling) ? "Sell" : "Buy")) selling = !selling;
						GUILayout.Label(Util.FormatMoney(ClientPlayer.mine.money), "box");
						for (Item.Type type = Item.Type.Head; type < Item.Type.Length; type ++){
							bool hasType = false;
							foreach (Item item in ((isShop) ? Shop.use.items : Inventory.use.items))
								if (item.type == type)
									hasType = true;
							if (hasType) {
								if (GUILayout.Button((tab == type) ? "<b>"+Item.typeNames[(int)type]+"</b>" : ""+Item.typeNames[(int)type])){
									tab = type;
									previewItem = null;
								}
							}else{
								//GUILayout.Box("<color=black><i>"+Item.typeNames[(int)type]+"</i></color>");
							}
						}
					GUILayout.EndVertical();
					GUILayout.BeginVertical("Items", "window");
						scroll = GUILayout.BeginScrollView(scroll);
							if (tab != Item.Type.Invalid){
								foreach (Item item in ((isShop && !selling) ? Shop.use.items : Inventory.use.items)) {
									if (item.type == tab){
										GUILayout.BeginHorizontal();
										if (GUILayout.Button((previewItem == item) ? "<b>"+item+"</b>" : ""+item, "label")) {
											if (clickCount == 1) {
												previewItem = item;
											}else if (clickCount == 2 && item.IsClothing && !isShop) {
												NetClient.use.AskWearItem((int)Item.LayerFromType(item.type), item.item);
											}
										}
										if (isShop) {
											GUILayout.FlexibleSpace();
											if (item.amount <= 0) GUILayout.Label("<color=red>(Out of stock!)</color>");
											else if (item.amount < 65535) GUILayout.Label("(" + ((selling) ? "Owned: " : "In Stock: ") + item.amount + ")");
											if (selling) {
												foreach (Item shopItem in Shop.use.items){
													if (shopItem.type == item.type && shopItem.item == item.item && shopItem.meta == item.meta && shopItem.amount < 65535)
														GUILayout.Label("(In Stock: " + shopItem.amount + ")");
												}
											}else{
												foreach (Item inventoryItem in Inventory.use.items){
													if (inventoryItem.type == item.type && inventoryItem.item == item.item && inventoryItem.amount < 65535)
														GUILayout.Label("(Owned: " + inventoryItem.amount + ")");
												}
											}
										}
										GUILayout.EndHorizontal();
									}
								}
							}
						GUILayout.EndScrollView();
					GUILayout.EndVertical();
					if (previewItem != null) {
						GUILayout.BeginVertical("Preview", "window", GUILayout.Width(128f));
							GUILayout.BeginHorizontal();
								GUILayout.FlexibleSpace();
								GUILayout.Label(previewItem.GetPreviewImg());
								GUILayout.FlexibleSpace();
							GUILayout.EndHorizontal();
							GUILayout.Label(previewItem.GetDescription());
							if (isShop) {
								GUILayout.Label("Amount: ");
								GUILayout.BeginHorizontal();
									int.TryParse(GUILayout.TextField(""+quantity), out quantity);
									GUILayout.FlexibleSpace();
									if (!selling && GUILayout.Button("Buy"))
										NetClient.use.AskBuyItem(previewItem.id, quantity);
									if (selling && GUILayout.Button("Sell"))
										NetClient.use.AskSellItem(Shop.id, previewItem.id, quantity);
								GUILayout.EndHorizontal();
							}
						GUILayout.EndVertical();
					}
				GUILayout.EndHorizontal();
			GUILayout.EndArea();
			if (showWindow)
				windowRect = GUILayout.Window(5, windowRect, Window, windowName);
			if (GUI.Button(new Rect(Screen.width - 24f - 256f, 0f, 24f, 24f), " X", "label")) Game.mode = GameMode.Explore;
		}
	}
		
	private GUIContent windowContent;
	
	void Window(int windowId) {
		if (GUI.Button(new Rect(windowRect.width - 24f, 0f, 24f, 24f), " X", "label")) showWindow = false;
		GUILayout.Label(windowContent);
		GUI.DragWindow();
	}
	
	public void ShowWindow(string winName, string content) {
		showWindow = true;
		windowName = winName;
		windowContent = new GUIContent(content);
		windowRect = new Rect(Screen.width/2f - 96f, Screen.height/2f - 64f, 192f, 128f);
		windowTime = Time.time;
	}
	
	public void Update() {
		if (showWindow && Time.time > windowTime + 3f) showWindow = false;
		if ((Game.mode == GameMode.Inventory || Game.mode == GameMode.Shop) && Input.GetKeyDown(KeyCode.Escape)){
			previewItem = null;
			Game.mode = GameMode.Explore;
		}
	}
}

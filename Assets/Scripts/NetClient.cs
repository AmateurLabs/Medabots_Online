using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class NetClient : MonoBehaviour {
	public static NetClient use;
	
	public GameObject avatarPrefab;
	
	public List<ClientPlayer> players;
	public HashSet<int> challengers = new HashSet<int>();
	
	void Awake() {
		use = this;
	}
	
	void Start() {
		Application.RegisterLogCallback(OnLog);
		players = new List<ClientPlayer>();
		Fade.Out(0f);
		Network.Connect("www.amateurlabs.com", 2101);
	}
	
	void OnConnectedToServer() {
		Game.netState = NetState.Connected;
		Application.LoadLevelAdditive(3);
	}
	
	void OnDisconnectedFromServer(NetworkDisconnection info) {
		Game.netState = NetState.Disconnected;
		Application.LoadLevel("Boot");
	}
	
	void OnFailedToConnect(NetworkConnectionError error) {
		Application.LoadLevel("Boot");
	}
	
	//Client-side RPC-sending functions, prefixed by Ask
	
	public void AskRegister(string username, string password) {
		joinMsg = "Registering...";
		networkView.RPC("ReqRegister", RPCMode.Server, username, Md5("vectroid" + password));
	}
	
	public void AskJoin(string username, string password) {
		joinMsg = "Logging in...";
		networkView.RPC("ReqJoin", RPCMode.Server, username, Md5("vectroid" + password));
	}
	
	public void AskMove(Dir direction, bool auto) {
		ClientPlayer.mine.avatar.waiting = true;
		networkView.RPC("ReqMove", RPCMode.Server, (int)direction, auto);
	}
	
	public void AskChat(string msg) {
		networkView.RPC("ReqChat", RPCMode.Server, msg);
	}
	
	public void AskItems() {
		networkView.RPC("ReqItems", RPCMode.Server);
	}
	
	public void AskShop(int id) {
		networkView.RPC("ReqShop", RPCMode.Server, id);
	}
	
	public void AskBuyItem(int id, int num) {
		networkView.RPC("ReqBuyItem", RPCMode.Server, id, num);
	}

	public void AskSellItem(int shopId, int id, int num) {
		networkView.RPC("ReqSellItem", RPCMode.Server, shopId, id, num);
	}
	
	public void AskMedals() {
		networkView.RPC("ReqMedals", RPCMode.Server);
	}
	
	public void AskMedal(int id) {
		networkView.RPC("ReqMedal", RPCMode.Server, id);
	}
	
	public void AskBots() {
		networkView.RPC("ReqBots", RPCMode.Server);
	}
	
	public void AskEquipItem(int botId, int itemId) {
		networkView.RPC("ReqEquipItem", RPCMode.Server, botId, itemId);
	}
	
	public void AskWearItem(int layer, int item) {
		networkView.RPC("ReqWearItem", RPCMode.Server, layer, item);
	}

	public void AskChallenge(int playerId) {
		networkView.RPC("ReqChallenge", RPCMode.Server, playerId);
	}

	public void AskBattle(int playerId) {
		networkView.RPC("ReqBattle", RPCMode.Server, playerId);
	}

	public void AskSetBattleFlags(BattleFlags flags) {
		networkView.RPC("ReqBattleFlags", RPCMode.Server, (int)flags);
	}

	public void AskBattleReady(int id0, int id1, int id2) {
		networkView.RPC("ReqBattleReady", RPCMode.Server, id0, id1, id2);
	}

	public void AskQuitBattle() {
		networkView.RPC("ReqQuitBattle", RPCMode.Server);
	}

	public void AskSetAbility(int bot, PartIndex part) {
		networkView.RPC("ReqSetAbility", RPCMode.Server, bot, (int)part);
	}

	//'Action' RPCs, recieved client-side
	
	[RPC]
	public void Join() {
		Game.netState = NetState.LoggedIn;
		Game.mode = GameMode.Explore;
	}
	
	[RPC]
	public void JoinError(string msg) {
		joinMsg = msg;
	}
	
	[RPC]
	public void Move(int playerId, int direction, bool auto) {
		if (direction == 0) Debug.LogWarning("Tried to move in Direction.None");
		ClientPlayer p = PlayerById(playerId);
		if (p == null) {
			Log("<color=red>Recieved Move request for invalid player: " + playerId + "</color>");
			return;
		}
		p.avatar.Move((Dir)direction, auto);			
	}
	
	[RPC]
	public void EndMove() {
		ClientPlayer.mine.avatar.waiting = false;
	}
	
	[RPC]
	public void Warp(int map, int x, int y, int dir) {
		if (dir >= 0) ClientPlayer.mine.avatar.dir = (Dir)dir;
		ClientPlayer.mine.avatar.Warp(map, x, y);
	}
	
	[RPC]
	public void CreateMyPlayer(string name, int x, int y, int map, int userlevel, int money, int dbId) {
		ClientPlayer.mine = new ClientPlayer(name, new Point(x, y), map, (UserLevel)userlevel, money, dbId);
		players.Add(ClientPlayer.mine);
		players.Sort((a, b) => {
			if (a.userlevel != b.userlevel) return a.userlevel.CompareTo(b.userlevel);
			return a.loc.y.CompareTo(b.loc.y);
		});
		Avatar avatar = ((GameObject)Instantiate(avatarPrefab)).GetComponent<Avatar>();
		avatar.transform.parent = transform.Find("/Explore/Avatar Grid");
		avatar.transform.localPosition = new Vector3(ClientPlayer.mine.loc.x, -ClientPlayer.mine.loc.y, 0f);
		avatar.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
		avatar.transform.localScale = new Vector3(1f, 1.5f, 1f);
		avatar.player = ClientPlayer.mine;
		ClientPlayer.mine.avatar = avatar;
		
		Map.Load(map);
		Fade.In(1f);
	}
	
	[RPC]
	public void CreatePlayer(string name, int x, int y, int userlevel, int dbId) {
		ClientPlayer p = new ClientPlayer(name, new Point(x, y), ClientPlayer.mine.map, (UserLevel)userlevel, -1, dbId);
		if (PlayerById(dbId) != null) {
			Log("<color=red>Received CreatePlayer RPC for existing player: " + name + "</color>");
		}
		players.Add(p);
		players.Sort((a, b) => {
			if (a.userlevel != b.userlevel) return -a.userlevel.CompareTo(b.userlevel);
			return -a.loc.y.CompareTo(b.loc.y);
		});
		Avatar avatar = ((GameObject)Instantiate(avatarPrefab)).GetComponent<Avatar>();
		avatar.transform.parent = transform.Find("/Explore/Avatar Grid");
		avatar.transform.localPosition = new Vector3(p.loc.x, -p.loc.y, -p.loc.y + 0.1f * (int)p.userlevel);
		avatar.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
		avatar.transform.localScale = new Vector3(1f, 1.5f, 1f);
		avatar.player = p;
		p.avatar = avatar;
	}
	
	[RPC]
	public void RemovePlayer(int playerId) {
		ClientPlayer p = PlayerById(playerId);
		foreach (Avatar avatar in FindObjectsOfType(typeof(Avatar))) {
			if (avatar.player == p)
				Destroy(avatar.gameObject);
		}
		if (p != null) {
			players.Remove(p);
		}
	}
	
	[RPC]
	public void ChatMsg(string sender, int channels, string msg) {
		Chat.use.AddMsg(new ChatMsg(sender, (Channels)channels, msg));
	}
	
	[RPC]
	public void GetItems(byte[] bytes) {
		Inventory.use.items.Clear();
		for (int i = 0; i < bytes.Length; i += 12) {
			byte[] itemBytes = new byte[12];
			for (int j = 0; j < 12; j ++)
				itemBytes[j] = bytes[i+j];
			Item item = Item.FromBytes(itemBytes);
			Data.items[item.id] = item;
			Inventory.use.items.Add(item);
		}
		foreach (Medabot bot in Medawatch.use.bots) {
			bot.TryLoadItems();
		}
	}
	
	[RPC]
	public void GetShop(int id, byte[] bytes) {
		foreach (ShopPoint sp in Data.maps[ClientPlayer.mine.map].shops)
			if (sp.id == id)
				Shop.use.name = sp.name;
		Shop.id = id;
		Shop.use.items.Clear();
		for (int i = 0; i < bytes.Length; i += 12) {
			byte[] itemBytes = new byte[12];
			for (int j = 0; j < 12; j ++)
				itemBytes[j] = bytes[i+j];
			Shop.use.items.Add(Item.FromBytes(itemBytes));
		}
		AskItems();
	}
	
	[RPC]
	public void BuyItem(byte[] bytes) {
		Item item = Item.FromBytes(bytes);
		if (item.type == Item.Type.Invalid) {
			if (item.item == (byte)Item.Error.NotEnoughCash) Inventory.use.ShowWindow("Error", "Insufficient funds");
			else if (item.item == (byte)Item.Error.BadShop) Inventory.use.ShowWindow("Error", "Shop not found. Wrong map?");
			else if (item.item == (byte)Item.Error.Unknown) Inventory.use.ShowWindow("Error", "Transaction failed");
		}else{
			ClientPlayer.mine.money -= item.GetCost() * item.amount;
			Game.mode = GameMode.Inventory;
			Inventory.use.ShowWindow("Transaction Complete", "Purchased " + item + "!");
			Game.mode = GameMode.Shop;
			AskShop(Shop.id);
		}
	}

	[RPC]
	public void SellItem(byte[] bytes) {
		Item item = Item.FromBytes(bytes);
		if (item.type == Item.Type.Invalid) {
			if (item.item == (byte)Item.Error.BadShop) Inventory.use.ShowWindow("Error", "Shop not found. Wrong map?");
			else if (item.item == (byte)Item.Error.Unknown) Inventory.use.ShowWindow("Error", "Transaction failed");
		}else{
			ClientPlayer.mine.money += item.GetCost() * item.amount;
			Game.mode = GameMode.Inventory;
			Inventory.use.ShowWindow("Transaction Complete", "Sold " + item + "!");
			Game.mode = GameMode.Shop;
			AskShop(Shop.id);
		}
	}
	
	[RPC]
	public void GetMedals(byte[] bytes) {
		Medawatch.use.medals.Clear();
		int i = 0;
		while (i < bytes.Length) {
			byte length = bytes[i];
			i ++;
			byte[] medalBytes = new byte[length];
			for (int j = 0; j < length; j ++)
				medalBytes[j] = bytes[j+i];
			Medal medal = Medal.FromBytes(medalBytes);
			Data.medals[medal.dbId] = medal;
			Medawatch.use.medals.Add(medal);
			i += length;
		}
		foreach (Medabot bot in Medawatch.use.bots) {
			bot.TryLoadItems();
		}
	}
	
	[RPC]
	public void GetMedal(byte[] bytes) {
		Medal medal = Medal.FromBytes(bytes);
		Data.medals[medal.dbId] = medal;
	}
	
	[RPC]
	public void GetBots(byte[] bytes) {
		Medawatch.use.bots.Clear();
		for (int i = 0; i < bytes.Length; i += 32) {
			byte[] botBytes = new byte[32];
			for (int j = 0; j < 32; j ++) {
				botBytes[j] = bytes[i+j];
			}
			Medawatch.use.bots.Add(Medabot.FromBytes(botBytes));
		}
		AskItems();
		AskMedals();
	}
	
	[RPC]
	public void EquipItem(int botId, int itemId) {
		Item item = null;
		if (itemId > 0) {
			if (!Data.items.TryGetValue(itemId, out item)) {
				return;
			}
		}else{
			item = new Item(itemId, Item.Type.Invalid, 0, ushort.MaxValue, 0);
		}
		foreach (Medabot bot in Medawatch.use.bots) {
			if (bot.dbId == botId) {
				bot.EquipItem(item);
				break;
			}
		}
	}
	
	[RPC]
	public void SetOutfit(int playerId, byte[] bytes) {
		ClientPlayer p = PlayerById(playerId);
		if (p == null) {
			Log("<color=red>Recieved Outfit data for invalid player: " + playerId + "</color>");
			return;
		}
		p.outfit = Outfit.FromBytes(bytes);
		p.avatar.SetTexture();
	}

	[RPC]
	public void SetChallenge(int playerId, bool on) {
		if (on && !challengers.Contains(playerId)) challengers.Add(playerId);
		if (!on && challengers.Contains(playerId)) challengers.Remove(playerId);
	}

	[RPC]
	public void SetBattle(int playerId, bool attacker) {
		ClientPlayer other = NetClient.use.PlayerById(playerId);
		if (attacker) Battler.use.battle = new Battle(ClientPlayer.mine, other);
		else Battler.use.battle = new Battle(other, ClientPlayer.mine);
		Game.mode = GameMode.Battle;
		AskBots();
	}

	[RPC]
	public void SetBattleFlags(int flags) {
		if (Battler.use.battle != null) Battler.use.battle.flags = (BattleFlags)flags;
	}

	[RPC]
	public void QuitBattle() {
		Battler.use.ClearBattle();
	}

	[RPC]
	public void GetBattleBots(byte[] bytes) {
		int i = 0;
		for (int j = 0; j < 6; j ++) {
			int length = BitConverter.ToInt32(bytes, i); i += 4;
			if (length > 0) {
				Battler.use.battle.bots[j] = Bot.FromBytes(Util.CopyBytes(bytes, i, length));
				Battler.use.animBots[j].gameObject.SetActive(true);
				Battler.use.animBots[j].bot = Battler.use.battle.bots[j];
				Battler.use.animBots[j].Refresh();
				i += length;
			} else {
				Battler.use.animBots[j].gameObject.SetActive(false);
			}
		}
	}

	[RPC]
	public void UseAbility(int bot, int targ, int part, int r0, int r1) {
		Battle b = Battler.use.battle;
		if (b == null) return;
		b.UseAbility(b.bots[bot], (targ != -1) ? b.bots[targ] : null, (PartIndex)part, (byte)r0, (byte)r1);
	}

	[RPC]
	public void SetAbility(int bot, int part) {
		Battle b = Battler.use.battle;
		if (b == null) return;
		b.SetAbility(b.bots[bot], (PartIndex)part);
	}
	
	[RPC]
	public void Message(string msg) {
		Log(msg);
	}

	public ClientPlayer PlayerById(int dbId) {
		foreach (ClientPlayer p in players) {
			if (p.dbId == dbId) return p;
		}
		return null;
	}
	
	public bool debug = false;
	
	void Update() {
		if (Game.netState == NetState.LoggedIn && Game.mode == GameMode.Explore && Input.GetKeyDown(KeyCode.I)) {
			AskItems();
			Game.mode = GameMode.Inventory;
		}
		if (Game.netState == NetState.LoggedIn && Game.mode == GameMode.Explore && Input.GetKeyDown(KeyCode.T)) {
			AskItems();
			AskMedals();
			AskBots();
			Game.mode = GameMode.Medawatch;
		}
		if (Input.GetKeyDown(KeyCode.BackQuote)) debug = !debug;
	}
	
	void OnLog(string log, string stack, LogType type) {
		switch (type) {
			case LogType.Log:
				Log("<color=white>"+log+"</color>");
				break;
			case LogType.Warning:
				Log("<color=orange>"+log+"</color>");
				break;
			case LogType.Error:
			case LogType.Exception:
			case LogType.Assert:
				Log("<color=red>"+log+"<size=12>" + stack + "</size></color>");
				Application.ExternalEval("alert(\""+log + " (" + stack + ")\");");
				break;
		}
	}
	
	public void Log(string message) {
		Chat.use.AddMsg(new ChatMsg("System", Channels.System, message));
		if (Application.isWebPlayer) Application.ExternalEval("console.log(\"" + message + "\")");
	}
	
	private string _username = "";
	private string _password = "";
	private string joinMsg = "";
	
	void OnGUI() {
		if (Game.netState == NetState.Connected) {
			bool submit = false;
			if (Event.current.type == EventType.keyDown && Event.current.keyCode == KeyCode.Return) {
				submit = true;
			}
			GUILayout.BeginArea(new Rect(0f, 0f, Screen.width, Screen.height));
			GUILayout.BeginVertical();
			GUILayout.FlexibleSpace();
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.BeginVertical("Login", "window", GUILayout.MinWidth(256f));
			GUILayout.Label("Username");
			_username = GUILayout.TextField(_username);
			GUILayout.Label("Password");
			_password = GUILayout.PasswordField(_password, '*');
			GUILayout.BeginHorizontal();
			if ((GUILayout.Button("Login") || submit) && _username != "" && _password != "") {
				AskJoin(_username, _password);
			}
			if (GUILayout.Button("Register") && _username != "" && _password != "") {
				AskRegister(_username, _password);
			}
			GUILayout.EndHorizontal();
			GUILayout.Label(joinMsg);
			GUILayout.EndVertical();
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.EndVertical();
			GUILayout.EndArea();
		}
		if (Game.netState == NetState.LoggedIn && Game.mode == GameMode.Explore) {
			if (debug) {
				GUI.color = Color.magenta;
				GUI.Label(new Rect(0f, 0f, 100f, 24f), "<b>" + ClientPlayer.mine.loc + "</b>");
				GUI.Label(new Rect(0f, 24f, 100f, 24f), "<b>" + ClientPlayer.mine.avatar.transform.position + "</b>");
				GUI.color = Color.white;
			}
			GUILayout.BeginArea(new Rect(0f, Screen.height - 24f, 512f, 24f));
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Inventory <size=10>(I)</size>")) {
				AskItems();
				Game.mode = GameMode.Inventory;
			}
			if (GUILayout.Button("Medawatch <size=10>(T)</size>")) {
				AskItems();
				AskMedals();
				AskBots();
				Game.mode = GameMode.Medawatch;
			}
			if (GUILayout.Button("Options <size=10>(O)</size>")) {
				Options.visible = !Options.visible;
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.EndArea();
		}
	}
	
	public string Md5(string strToEncrypt) {
		System.Text.UTF8Encoding ue = new System.Text.UTF8Encoding();
		byte[] bytes = ue.GetBytes(strToEncrypt);
	 	
		System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
		byte[] hashBytes = md5.ComputeHash(bytes);
	 	
		string hashString = "";
	 
		for (int i = 0; i < hashBytes.Length; i++) {
			hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
		}
	 
		return hashString.PadLeft(32, '0');
	}
	
	//RPC stubs
	[RPC] public void ReqRegister(string username, string password) { }
	[RPC] public void ReqJoin(string username, string password) { }
	[RPC] public void ReqMove(int direction, bool auto) { }
	[RPC] public void ReqChat(string msg) { }
	[RPC] public void ReqItems() { }
	[RPC] public void ReqShop(int id) { }
	[RPC] public void ReqBuyItem(int id, int num) { }
	[RPC] public void ReqMedals() { }
	[RPC] public void ReqMedal(int id) { }
	[RPC] public void ReqBots() { }
	[RPC] public void ReqEquipItem(int botId, int itemId) { }
	[RPC] public void ReqWearItem(int layer, int item) { }
	[RPC] public void ReqSellItem(int shopId, int itemId, int num) { }
	[RPC] public void ReqChallenge(int playerId) { }
	[RPC] public void ReqBattle(int playerId) { }
	[RPC] public void ReqBattleFlags(int flags) { }
	[RPC] public void ReqBattleReady(int bot0, int bot1, int bot2) { }
	[RPC] public void ReqQuitBattle() { }
	[RPC] public void ReqSetAbility(int bot, int part) { }
}

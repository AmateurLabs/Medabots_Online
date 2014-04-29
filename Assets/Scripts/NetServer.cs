using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class NetServer : MonoBehaviour {
	public static NetServer use;
	
	public Dictionary<NetworkPlayer, ServerPlayer> netPlayers; //Server-side dictionary of players by NetworkPlayer
	public Dictionary<int, List<ServerPlayer>> players; //Dictionary of players by map
	public Dictionary<int, ServerPlayer> dbPlayers; //Dictionary of players by database id

	void Awake() {
		use = this;
	}
	
	void Start() {
		Application.RegisterLogCallback(OnLog);
		netPlayers = new Dictionary<NetworkPlayer, ServerPlayer>();
		players = new Dictionary<int, List<ServerPlayer>>();
		dbPlayers = new Dictionary<int, ServerPlayer>();
		for (int i = 0; i < Data.maps.Count; i ++)
			players[i] = new List<ServerPlayer>();
		Network.InitializeServer(64, 2101, false);
	}
	
	void OnPlayerConnected(NetworkPlayer player) {
		Log("" + player.ipAddress + " connected");
	}
	
	void OnPlayerDisconnected(NetworkPlayer player) {
		Log("" + player.ipAddress + " disconnected");
		if (!netPlayers.ContainsKey(player)) return;
		ServerPlayer p = netPlayers[player];
		players[p.map].Remove(p);
		netPlayers.Remove(player);
		if (dbPlayers.ContainsKey(p.dbId)) dbPlayers.Remove(p.dbId);
		Log("" + p.name + " logged out");
		SavePlayer(p);
		if (p.battle != null) HandleQuitBattle(p.battle, p);
		if (!players[p.map].Contains(p)) return;
		foreach (ServerPlayer otherPlayer in players[p.map]) {
			if (otherPlayer != p) {
				SendRPC("RemovePlayer", otherPlayer.netPlayer, p.dbId);
			}
		}
	}
	
	void OnServerInitialized() {
		Log("Server Initialized");
		Game.netState = NetState.Server;
		int npcId = -1;
		foreach (MapData data in Data.maps) {
			if (!data.raw.Contains("npcs")) continue;
			foreach (ALDNode node in data.raw["npcs"]) {
				NPC npc = new NPC(node["name"].Value, new Point((int)node["x"].Value, (int)node["y"].Value), Data.maps.IndexOf(data));
				npc.rawData = node;
				if (node.Contains("bots")) {
					npc.bots = new Medabot[node["bots"].ChildCount];
					for (int i = 0; i < node["bots"].ChildCount; i ++) npc.bots[i] = Medabot.Generate(npc.dbId, node["bots"][i]);
				}
				npc.outfit = new Outfit(node["outfit"]);
				players[npc.map].Add(npc);
				dbPlayers[npc.dbId] = npc;
				npcId --;
			}
		}
	}
	
	//Server-side functions
	
	public void SavePlayer(ServerPlayer p) {
		Web.Call("http://localhost/local/mbo/saveplayer.php?playerId="+p.dbId+"&x="+p.loc.x+"&y="+p.loc.y+"&map="+p.map+"&money="+p.money+
		"&base="+p.outfit.baseId+"&top="+p.outfit.topId+"&bottom="+p.outfit.bottomId+"&shoes="+p.outfit.shoesId+"&coat="+p.outfit.coatId+"&eyes="+p.outfit.eyesId+"&mask="+p.outfit.maskId+"&hair="+p.outfit.hairId,
		(result) => {
			if ((bool)result["success"].Value == true) {
				Log("Saved data for " + p.name);
			}else if (result.Contains("error")) Debug.LogWarning("SavePlayer error: " + result["error"].Value);
		});
	}
	
	public void MsgPlayer(ServerPlayer p, string msg) {
		if (p != null && msg != "") {
			SendRPC("Message", p.netPlayer, msg);
		}
	}
	
	public void WarpPlayer(ServerPlayer p, int mapId, int x, int y, int dir) {
		foreach (ServerPlayer otherPlayer in players[p.map]) {
			if (otherPlayer != p) {
				SendRPC("RemovePlayer", otherPlayer.netPlayer, p.dbId);
				SendRPC("RemovePlayer", p.netPlayer, otherPlayer.dbId);
			}
		}
		players[p.map].Remove(p);
		p.map = mapId;
		p.loc = new Point(x, y);
		players[p.map].Add(p);
		SendRPC("Warp", p.netPlayer, mapId, x, y, dir);
		foreach (ServerPlayer otherPlayer in players[p.map]) {
			if (otherPlayer != p) {
				SendRPC("CreatePlayer", otherPlayer.netPlayer, p.name, p.loc.x, p.loc.y, (int)p.userlevel, p.dbId);
				SendRPC("CreatePlayer", p.netPlayer, otherPlayer.name, otherPlayer.loc.x, otherPlayer.loc.y, (int)otherPlayer.userlevel, otherPlayer.dbId);
				SendRPC("SetOutfit", otherPlayer.netPlayer, p.dbId, p.outfit.ToBytes());
				SendRPC("SetOutfit", p.netPlayer, otherPlayer.dbId, otherPlayer.outfit.ToBytes());
			}
		}
		Log(p.name + " warped to " + p.map + " " + p.loc);
	}

	public void SendBattleBots(ServerPlayer[] players, Battle battle) {
		List<byte> bytes = new List<byte>();
		for (int i = 0; i < 6; i++) {
			if (battle.bots[i] == null) {
				bytes.AddRange(BitConverter.GetBytes((int)0));
			}else{
				byte[] botBytes = battle.bots[i].ToBytes();
				bytes.AddRange(BitConverter.GetBytes(botBytes.Length));
				bytes.AddRange(botBytes);
			}
		}
		byte[] byteArr = bytes.ToArray();
		foreach (ServerPlayer p in players) {
			SendRPC("GetBattleBots", p.netPlayer, byteArr);
		}
	}

	public void HandleQuitBattle(Battle battle, ServerPlayer player) {
		if (player.battle.attacker == player) {
			player.battle.attacker = null;
			if (player.battle.defender != null) {
				ServerPlayer defender = (ServerPlayer)player.battle.defender;
				SendRPC("Message", defender.netPlayer, player.name + " abandoned the Robattle! You win by default!");
				SendRPC("QuitBattle", defender.netPlayer);
				player.battle.defender.battle = null;
			}
		} else if (player.battle.defender == player) {
			player.battle.defender = null;
			if (player.battle.attacker != null) {
				ServerPlayer attacker = (ServerPlayer)player.battle.attacker;
				if (netPlayers.ContainsKey(attacker.netPlayer)) {
					SendRPC("Message", attacker.netPlayer, player.name + " abandoned the Robattle! You win by default!");
					SendRPC("QuitBattle", attacker.netPlayer);
				}
				player.battle.attacker.battle = null;
			}
		}
		Battle.battles.Remove(player.battle.id);
		player.battle = null;
		if (netPlayers.ContainsKey(player.netPlayer)) {
			SendRPC("Message", player.netPlayer, "You abandoned the Robattle!");
			SendRPC("QuitBattle", player.netPlayer);
		}
	}

	//'Request' RPCs, prefixed be Req and recieved server-side
	
	[RPC]
	public void ReqRegister(string username, string password, NetworkMessageInfo info) {
		username = WWW.EscapeURL(username);
		password = WWW.EscapeURL(password);
		if (!System.Text.RegularExpressions.Regex.Match(username, @"^[\p{L} \p{Nd}_]+$").Success) {
			SendRPC("JoinError", info.sender, "<color=red>Usernames can only contain letters,\ndigits, and underscores</color>");
			return;
		}
		Web.Call("http://localhost/local/mbo/register.php?username=" + username + "&password=" + password, (result) => {
			List<NetworkPlayer> connections = new List<NetworkPlayer>(Network.connections);
			if (!connections.Contains(info.sender)) return;
			if ((bool)result["success"].Value == true) {
				Log("New user registered: " + username);
				SendRPC("JoinError", info.sender, "Registration Complete");
			}else if (result.Contains("error")){
				SendRPC("JoinError", info.sender, "<color=red>" + result["error"].Value + "</color>");
				Debug.LogWarning("ReqRegister error: " + result["error"].Value);
			}
		});
	}
	
	[RPC]
	public void ReqJoin(string username, string password, NetworkMessageInfo info) {
		username = WWW.EscapeURL(username);
		password = WWW.EscapeURL(password);
		Web.Call("http://localhost/local/mbo/login.php?username=" + username + "&password=" + password, (result) => {
			List<NetworkPlayer> connections = new List<NetworkPlayer>(Network.connections);
			if (!connections.Contains(info.sender)) return;
            if (netPlayers.ContainsKey(info.sender)) return;
			if ((bool)result["success"].Value == true) {
				foreach (ServerPlayer otherPlayer in netPlayers.Values) {
					if (otherPlayer.dbId == (int)result["id"].Value) {
						SendRPC("JoinError", info.sender, "<color=red>" + otherPlayer.name + " is already logged in!" + "</color>");
						Debug.LogWarning("ReqJoin error: " + "Attempted simultaneous login by " + otherPlayer.name);
						return;
					}
				}
				Log("" + username + " logged in");
				int map = (int)result["map"].Value;
				ServerPlayer p = new ServerPlayer(""+result["username"].Value, new Point((int)result["x"].Value, (int)result["y"].Value), map, (UserLevel)(int)result["userlevel"].Value, (int)result["money"].Value, (int)result["id"].Value);
				p.name = p.name.Replace('_', ' ');
				p.outfit = new Outfit(result["outfit"]);
				p.netPlayer = info.sender;
				netPlayers.Add(p.netPlayer, p);
				dbPlayers.Add(p.dbId, p);
				SendRPC("Join", p.netPlayer);
				SendRPC("CreateMyPlayer", p.netPlayer, p.name, p.loc.x, p.loc.y, p.map, (int)p.userlevel, p.money, p.dbId);
				Commands.Handle(p, "playerlist");
				players[map].Add(p);
				foreach (ServerPlayer player in players[map]){
					if (player == p) continue;
					SendRPC("CreatePlayer", player.netPlayer, p.name, p.loc.x, p.loc.y, (int)p.userlevel, p.dbId);
					SendRPC("CreatePlayer", p.netPlayer, player.name, player.loc.x, player.loc.y, (int)player.userlevel, player.dbId);
				}
				foreach (ServerPlayer player in players[map]) {
					SendRPC("SetOutfit", player.netPlayer, p.dbId, p.outfit.ToBytes());
					SendRPC("SetOutfit", p.netPlayer, player.dbId, player.outfit.ToBytes());
				}
			}else{
				if (result.Contains("error")){
					SendRPC("JoinError", info.sender, "<color=red>" + result["error"].Value + "</color>");
					Debug.LogWarning("ReqJoin error: " + result["error"].Value);
				}
			}
		});
	}
	
	[RPC]
	public void ReqMove(int direction, bool auto, NetworkMessageInfo info) {
		if (!netPlayers.ContainsKey(info.sender)) return;
		ServerPlayer player = netPlayers[info.sender];
		bool valid = true;
		if (Time.time < player.moveTime + 0.3f) valid = false;
		Point targetLoc = player.loc + Point.FromDir((Dir)direction);
		if (!Data.maps[player.map].CanMove(targetLoc) && !player.noCollide) valid = false;
		if (valid) {
			player.loc = targetLoc;
			foreach (Warp warp in Data.maps[player.map].warps) {
				if (warp.loc == player.loc){
					WarpPlayer(player, warp.map, warp.tLoc.x, warp.tLoc.y, direction);
					return;
				}
			}
			player.moveTime = Time.time;
			foreach (ServerPlayer p in players[player.map]){
				SendRPC("Move", p.netPlayer, player.dbId, direction, auto);
			}
		}
		SendRPC("EndMove", player.netPlayer);
	}
	
	[RPC]
	public void ReqChat(string msg, NetworkMessageInfo info) {
		if (!netPlayers.ContainsKey(info.sender)) return;
		ServerPlayer player = netPlayers[info.sender];
		if (Time.time < player.chatTime + 1f) return;
		player.chatTime = Time.time;
		if (msg.StartsWith("/")) {
			Commands.Handle(player, msg.Substring(1));
		}else{
			if (player.battle != null) {
				Commands.Handle(player, "bsay " + msg);
			}else{
				Commands.Handle(player, "say " + msg);
			}
		}
		Log("["+Data.mapList[player.map]+"]<color="+Player.userColors[(int)player.userlevel]+">["+player.name+"] </color>" + msg);
	}
	
	[RPC]
	public void ReqItems(NetworkMessageInfo info) {
		if (!netPlayers.ContainsKey(info.sender)) return;
		ServerPlayer player = netPlayers[info.sender];
		Web.Call("http://localhost/local/mbo/items.php?playerId=" + player.dbId, (result) => {
			if ((bool)result["success"].Value == true) {
				List<byte> bytes = new List<byte>();
				foreach (ALDNode itemNode in result["items"]) {
					Item item = new Item(itemNode);
					bytes.AddRange(item.ToBytes());
				}
				SendRPC("GetItems", player.netPlayer, bytes.ToArray());
			}else{
				if (result.Contains("error")){
					Debug.LogWarning("ReqItems error: " + result["error"].Value);
				}
			}
		});
	}
	
	[RPC]
	public void ReqShop(int id, NetworkMessageInfo info) {
		if (!netPlayers.ContainsKey(info.sender)) return;
		ServerPlayer player = netPlayers[info.sender];
		Web.Call("http://localhost/local/mbo/shop.php?shopId=" + id, (result) => {
			if ((bool)result["success"].Value == true) {
				List<byte> bytes = new List<byte>();
				foreach (ALDNode itemNode in result["items"]) {
					Item item = new Item(itemNode);
					bytes.AddRange(item.ToBytes());
				}
				SendRPC("GetShop", player.netPlayer, id, bytes.ToArray());
			}else{
				if (result.Contains("error")){
					Debug.LogWarning("ReqShop error: " + result["error"].Value);
				}
			}
		});
	}
	
	[RPC]
	public void ReqBuyItem(int id, int num, NetworkMessageInfo info) {
		if (!netPlayers.ContainsKey(info.sender)) return;
		ServerPlayer player = netPlayers[info.sender];
		Web.Call("http://localhost/local/mbo/buyitem.php?itemId="+id+"&playerId="+player.dbId+"&num="+num, (result) => {
			if ((bool)result["success"].Value == true) {
				Item item = new Item(result["item"]);
				SendRPC("BuyItem", player.netPlayer, item.ToBytes());
				Log(player.name + " bought " + num + " of " + item);
				player.money -= (int)(Data.itemNode[""+item.type][item.item]["price"].Value) * num;
			}else if (result.Contains("error")) {
				Debug.LogWarning("ReqBuyItem error: " + result["error"].Value);
				if(result.Contains("clientMsg")) {
					SendRPC("Message", player.netPlayer, ""+result["clientMsg"].Value);
				}
			}
		});
	}

	[RPC]
	public void ReqSellItem(int shopId, int itemId, int num, NetworkMessageInfo info) {
		if (!netPlayers.ContainsKey(info.sender)) return;
		ServerPlayer player = netPlayers[info.sender];
		
		Web.Call("http://localhost/local/mbo/sellitem.php?playerId="+player.dbId + "&shopId=" + shopId + "&itemId=" + itemId + "&num=" + num, (result) => {
			if ((bool)result["success"].Value == true) {
				Item item = new Item(result["item"]);
				SendRPC("SellItem", player.netPlayer, item.ToBytes());
				Log(player.name + " sold " + num + " of " + item);
				player.money += (int)(Data.itemNode[""+item.type][item.item]["price"].Value) * num;
			}else if (result.Contains("error")) {
				Debug.LogWarning("ReqSellItem error: " + result["error"].Value);
				if(result.Contains("clientMsg")) {
					SendRPC("Message", player.netPlayer, ""+result["clientMsg"].Value);
				}
			}
		});
	}
	
	[RPC]
	public void ReqMedals(NetworkMessageInfo info) {
		if (!netPlayers.ContainsKey(info.sender)) return;
		ServerPlayer player = netPlayers[info.sender];
		Web.Call("http://localhost/local/mbo/medals.php?playerId=" + player.dbId, (result) => {
			if ((bool)result["success"].Value == true) {
				List<byte> bytes = new List<byte>();
				foreach (ALDNode medalNode in result["medals"]) {
					Medal medal = new Medal(medalNode);
					byte[] medalBytes = medal.ToBytes();
					bytes.Add((byte)medalBytes.Length);
					bytes.AddRange(medalBytes);
				}
				SendRPC("GetMedals", player.netPlayer, bytes.ToArray());
			}else{
				if (result.Contains("error")){
					Debug.LogWarning("ReqItems error: " + result["error"].Value);
				}
			}
		});
	}
	
	[RPC]
	public void ReqMedal(int id, NetworkMessageInfo info) {
		if (!netPlayers.ContainsKey(info.sender)) return;
		ServerPlayer player = netPlayers[info.sender];
		if (id == 0) return;
		Web.Call("http://localhost/local/mbo/medal.php?medalId=" + id, (result) => {
			if ((bool)result["success"].Value == true) {
				Medal medal = new Medal(result["medal"]);
				SendRPC("GetMedal", player.netPlayer, medal.ToBytes());
			}else if (result.Contains("error")) Debug.LogWarning("ReqMedal error: " + result["error"].Value);
		});
	}
	
	[RPC]
	public void ReqBots(NetworkMessageInfo info) {
		if (!netPlayers.ContainsKey(info.sender)) return;
		ServerPlayer player = netPlayers[info.sender];
		Web.Call("http://localhost/local/mbo/bots.php?playerId=" + player.dbId, (result) => {
			if ((bool)result["success"].Value == true) {
				List<byte> bytes = new List<byte>();
				foreach (ALDNode botNode in result["bots"]) {
					bytes.AddRange(new Medabot(botNode).ToBytes());
				}
				SendRPC("GetBots", player.netPlayer, bytes.ToArray());
			}else{
				if (result.Contains("error")){
					Debug.LogWarning("ReqBots error: " + result["error"].Value);
				}
			}
		});
	}
	
	[RPC]
	public void ReqEquipItem(int botId, int itemId, NetworkMessageInfo info) {
		if (!netPlayers.ContainsKey(info.sender)) return;
		ServerPlayer player = netPlayers[info.sender];
		Web.Call("http://localhost/local/mbo/updatebot.php?playerId=" + player.dbId + "&botId=" + botId + "&itemId=" + itemId, (result) => {
			if ((bool)result["success"].Value == true) {
				SendRPC("EquipItem", player.netPlayer, botId, itemId);
			}else{
				if (result.Contains("error")){
					Debug.LogWarning("ReqEquipItem error: " + result["error"].Value);
					if(result.Contains("clientMsg")) {
						SendRPC("Message", player.netPlayer, ""+result["clientMsg"].Value);
					}
				}
			}
		});
	}
	
	[RPC]
	public void ReqWearItem(int layer, int item, NetworkMessageInfo info) {
		if (!netPlayers.ContainsKey(info.sender)) return;
		ServerPlayer player = netPlayers[info.sender];
		
		player.outfit[(Avatar.Layers)layer] = (byte)item;
		foreach (ServerPlayer p in players[player.map]) {
			SendRPC("SetOutfit", p.netPlayer, player.dbId, player.outfit.ToBytes());
		}
	}

	[RPC]
	public void ReqChallenge(int playerId, NetworkMessageInfo info) {
		if (!netPlayers.ContainsKey(info.sender)) return;
		ServerPlayer player = netPlayers[info.sender];

		if (player.challenges.Contains(playerId)) {
			SendRPC("Message", player.netPlayer, "<color=red>You have already challenged that player!</color>");
		}else{
			if (dbPlayers.ContainsKey(playerId)) {
				ServerPlayer p = dbPlayers[playerId];
				player.challenges.Add(playerId);
				SendRPC("Message", player.netPlayer, "You challenged " + p.name + " to a Robattle!");
				SendRPC("Message", p.netPlayer, player.name + " challenged you to a Robattle!");
				SendRPC("SetChallenge", p.netPlayer, player.dbId, true);
				Log(player.name + " challenged " + p.name);
			}else{
				SendRPC("Message", player.netPlayer, "<color=red>The player you challenged is offline!</color>");
			}
		}
	}

	[RPC]
	public void ReqBattle(int playerId, NetworkMessageInfo info) {
		if (!netPlayers.ContainsKey(info.sender)) return;
		ServerPlayer player = netPlayers[info.sender];
		if (player.battle != null) {
			SendRPC("Message", player.netPlayer, "<color=red>You are already in a Robattle!</color>");
		}else{
			if (dbPlayers.ContainsKey(playerId)) {
				ServerPlayer p = dbPlayers[playerId];
				Battle battle = new Battle(p, player);
				battle.flags = BattleFlags.BotTop | BattleFlags.BotMiddle | BattleFlags.BotBottom | BattleFlags.WagerNormal | BattleFlags.EnableCmdTimer | BattleFlags.WinTypeLeader | BattleFlags.WinTypeTime;
				if (p.userlevel == UserLevel.NPC) {
					battle.flags |= BattleFlags.AttackerReady;
				}else{
					SendRPC("SetBattle", p.netPlayer, player.dbId, true);
					SendRPC("SetChallenge", p.netPlayer, player.dbId, false);
					SendRPC("SetBattleFlags", p.netPlayer, (int)battle.flags);
					p.challenges.Remove(player.dbId);
				}
				SendRPC("SetBattle", player.netPlayer, p.dbId, false);
				SendRPC("SetChallenge", player.netPlayer, p.dbId, false);
				SendRPC("SetBattleFlags", player.netPlayer, (int)battle.flags);
				player.challenges.Remove(p.dbId);
			}
		}
	}

	[RPC]
	public void ReqBattleFlags(int flags, NetworkMessageInfo info) {
		if (!netPlayers.ContainsKey(info.sender)) return;
		ServerPlayer player = netPlayers[info.sender];
		if (player.battle != null) {
			player.battle.flags = (BattleFlags)flags;
			if (player.battle.attacker == player) {
				SendRPC("SetBattleFlags", player.netPlayer, flags);
				if (player.battle.defender.userlevel != UserLevel.NPC)
					SendRPC("SetBattleFlags", ((ServerPlayer)player.battle.defender).netPlayer, flags);
			}else{
				SendRPC("Message", player.netPlayer, "<color=red>You aren't allowed to change this Robattle's settings!</color>");
			}
		}else{
			SendRPC("Message", player.netPlayer, "<color=red>You aren't in a Robattle!</color>");
		}
	}

	[RPC]
	public void ReqBattleReady(int bot0, int bot1, int bot2, NetworkMessageInfo info) {
		if (!netPlayers.ContainsKey(info.sender)) return;
		ServerPlayer player = netPlayers[info.sender];
		Battle b = player.battle;
		if (b != null) {
			bool attacker = (b.attacker == player);
			bool defender = (b.defender == player);
			if (attacker || defender) {
				Web.Call("http://localhost/local/mbo/battlebots.php?playerId=" + player.dbId, (result) => {
					if ((bool)result["success"].Value == true) {
						foreach (ALDNode botNode in result["bots"]) {
							Medabot bot = Medabot.Generate(player.dbId, botNode);
							if (bot.head == null || bot.lArm == null || bot.rArm == null || bot.legs == null || bot.medal == null || bot.tinpet == null) continue;
							if (attacker) {
								if (bot.dbId == bot0 && b.flags.HasFlag(BattleFlags.BotTop))
									b.bots[0] = new Bot(bot);
								if (bot.dbId == bot1 && b.flags.HasFlag(BattleFlags.BotMiddle))
									b.bots[1] = new Bot(bot);
								if (bot.dbId == bot2 && b.flags.HasFlag(BattleFlags.BotBottom))
									b.bots[2] = new Bot(bot);
							}else if (defender) {
								if (bot.dbId == bot0 && b.flags.HasFlag(BattleFlags.BotTop))
									b.bots[3] = new Bot(bot);
								if (bot.dbId == bot1 && b.flags.HasFlag(BattleFlags.BotMiddle))
									b.bots[4] = new Bot(bot);
								if (bot.dbId == bot2 && b.flags.HasFlag(BattleFlags.BotBottom))
									b.bots[5] = new Bot(bot);
							}
						}
						if (b.attacker is NPC) {
							NPC npc = b.attacker as NPC;
							if (npc.bots.Length > 0 && b.flags.HasFlag(BattleFlags.BotTop)) b.bots[0] = new Bot(npc.bots[0]);
							if (npc.bots.Length > 1 && b.flags.HasFlag(BattleFlags.BotMiddle)) b.bots[1] = new Bot(npc.bots[1]);
							if (npc.bots.Length > 2 && b.flags.HasFlag(BattleFlags.BotBottom)) b.bots[2] = new Bot(npc.bots[2]);
						}
						if (attacker) b.flags |= BattleFlags.AttackerReady;
						else if (defender) b.flags |= BattleFlags.DefenderReady;
						if (b.flags.HasFlag(BattleFlags.AttackerReady) && b.flags.HasFlag(BattleFlags.DefenderReady)) {
							b.flags |= BattleFlags.SetupComplete;
							Log(b.attacker.name + " is now battling " + b.defender.name + ".");
							SendBattleBots(new ServerPlayer[] { (ServerPlayer)b.attacker, (ServerPlayer)b.defender }, b);
							if (b.attacker is NPC) {
								for (int i = 0; i < 3; i++) {
									if (b.bots[i] != null) b.SetAbility(b.bots[i], (PartIndex)UnityEngine.Random.Range(0, 4));
								}
							}
						}
						if (b.attacker == player) SendRPC("SetBattleFlags", ((ServerPlayer)b.defender).netPlayer, (int)b.flags);
						else if (b.defender == player) SendRPC("SetBattleFlags", ((ServerPlayer)b.attacker).netPlayer, (int)b.flags);
						SendRPC("SetBattleFlags", player.netPlayer, (int)b.flags);
					}else{
						if (result.Contains("error")){
							Debug.LogWarning("ReqBattleBots error: " + result["error"].Value);
						}
					}
				});
			}else{
				SendRPC("Message", player.netPlayer, "<color=red>You aren't a participant in this Robattle!</color>");
			}
		}else{
			SendRPC("Message", player.netPlayer, "<color=red>You aren't in a Robattle!</color>");
		}
	}

	[RPC]
	public void ReqQuitBattle(NetworkMessageInfo info) {
		if (!netPlayers.ContainsKey(info.sender)) return;
		ServerPlayer player = netPlayers[info.sender];
		if (player.battle != null) {
			HandleQuitBattle(player.battle, player);
		}else{
			SendRPC("Message", player.netPlayer, "<color=red>You aren't in a Robattle!</color>");
		}
	}

	[RPC]
	public void ReqSetAbility(int bot, int part, NetworkMessageInfo info) {
		if (!netPlayers.ContainsKey(info.sender)) return;
		ServerPlayer player = netPlayers[info.sender];

		if (player.battle != null) {
			Battle b = player.battle;
			if (b.attacker == player || b.defender == player) {
				if (b.bots[bot].owner == player && b.bots[bot].state == BotState.Standby) {
					b.SetAbility(b.bots[bot], (PartIndex)part);
				} else {
					SendRPC("Message", player.netPlayer, "<color=red>You can't command that Medabot!</color>");
				}
			} else {
				SendRPC("Message", player.netPlayer, "<color=red>You aren't a participant in this Robattle!</color>");
			}
		} else {
			SendRPC("Message", player.netPlayer, "<color=red>You aren't in a Robattle!</color>");
		}
	}
	
	void OnLog(string log, string stack, LogType type) {
		switch (type) {
			case LogType.Log:
				//Log("<color=grey>"+log+"</color>");
				break;
			case LogType.Warning:
				Log("<color=orange>"+log+"</color>");
				break;
			case LogType.Error:
			case LogType.Exception:
			case LogType.Assert:
				Log("<color=red>"+log+"\n<size=12>"+stack+"</size></color>");
				break;
		}
	}

	public void SendRPC(string method, NetworkPlayer netPlayer, params object[] args) {
		if (netPlayer == Network.player) return;
		try {
			networkView.RPC(method, netPlayer, args);
		} catch (Exception e) {
			Debug.Log("Failed to send RPC " + method + " to invalid player " + netPlayer.ipAddress + " (" + e.Message + ")");
		}
	}
	
	public List<string> messages = new List<string>();
	
	public void Log(string message) {
		messages.Add(message);
		if (messages.Count > 256){
			messages.RemoveAt(0);
		}
		message = Regex.Replace(message, "<[^>]*>", string.Empty);
		scroll.y = Mathf.Infinity;
		Web.Call("http://localhost/local/mbo/log.php?msg="+WWW.EscapeURL(message), null);
		if (Application.isWebPlayer) Application.ExternalEval("console.log(\"" + message + "\")");
	}

	void Update() {
		if (sendMsg != "" && Input.GetKeyDown(KeyCode.Return)) {
			Commands.Handle(sendMsg);
			sendMsg = "";
		}
		foreach (Battle b in Battle.battles.Values) {
			b.Update();
		}
	}
	
	Vector2 scroll;
	Vector2 playerScroll;
	string sendMsg = "";

	void OnGUI() {
		XGUI.Init();
		GUILayout.BeginArea(new Rect(0f, 0f, Screen.width, Screen.height));
		GUILayout.BeginHorizontal();
		GUILayout.BeginVertical(GUILayout.MinWidth(512f));
		scroll = GUILayout.BeginScrollView(scroll);
		foreach (string msg in messages) {
			GUILayout.Label(msg);
		}
		GUILayout.EndScrollView();
		sendMsg = GUILayout.TextField(sendMsg);
		GUILayout.Space(20f);
		GUILayout.EndVertical();
		GUILayout.FlexibleSpace();
		GUILayout.BeginVertical();
		playerScroll = GUILayout.BeginScrollView(playerScroll);
		foreach (ServerPlayer player in netPlayers.Values) {
			GUILayout.Label(player.name + " (" + Data.mapList[player.map] + ": " + player.loc + ")");
		}
		GUILayout.EndScrollView();
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
		GUILayout.EndArea();
	}
	
	//RPC stubs
	[RPC] public void Join() { }
	[RPC] public void JoinError(string msg) { }
	[RPC] public void Move(int playerId, int direction, bool auto) { }
	[RPC] public void EndMove() { }
	[RPC] public void Warp(int map, int x, int y, int dir) { }
	[RPC] public void CreateMyPlayer(string name, int x, int y, int map, int userlevel, int money, int dbId) { }
	[RPC] public void CreatePlayer(string name, int x, int y, int userlevel, int dbId) { }
	[RPC] public void RemovePlayer(int playerId) { }
	[RPC] public void ChatMsg(string sender, int channels, string msg) { }
	[RPC] public void GetItems(byte[] bytes) { }
	[RPC] public void GetShop(int id, byte[] bytes) { }
	[RPC] public void BuyItem(byte[] bytes) { }
	[RPC] public void SellItem(byte[] bytes) { }
	[RPC] public void GetMedals(byte[] bytes) { }
	[RPC] public void GetMedal(byte[] bytes) { }
	[RPC] public void GetBots(byte[] bytes) { }
	[RPC] public void EquipItem(int botId, int itemId) { }
	[RPC] public void SetOutfit(int playerId, byte[] bytes) { }
	[RPC] public void SetChallenge(int playerId, bool on) { }
	[RPC] public void SetBattle(int playerId, bool attacker) { }
	[RPC] public void SetBattleFlags(int flags) { }
	[RPC] public void QuitBattle() { }
	[RPC] public void GetBattleBots(byte[] bytes) { }
	[RPC] public void UseAbility(int bot, int targ, int part, int r0, int r1) { }
	[RPC] public void SetAbility(int bot, int part) { }
    [RPC] public void SetBotState(int bot, int state) { }
	[RPC] public void Message(string msg) { }
}

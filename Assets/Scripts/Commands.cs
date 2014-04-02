using UnityEngine;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public static class Commands {

	public class CommandHandler {
		public ClientCmd clientCmd;
		public ServerCmd serverCmd;

		public CommandHandler(ClientCmd clientCmd, ServerCmd serverCmd) {
			this.clientCmd = clientCmd;
			this.serverCmd = serverCmd;
		}
	}

	public delegate string ServerCmd(string[] args);
	public delegate string ClientCmd(ServerPlayer player, string[] args);
	
	public static Dictionary<string, CommandHandler> cmds;
	
	static Commands() {
		cmds = new Dictionary<string, CommandHandler>();
		cmds.Add("warp", new CommandHandler((player, args) => {
			if (player.userlevel < UserLevel.Moderator) return "<color=orange>Insufficient permissions for /warp</color>";
			if (args.Length < 4) return "<color=orange>Incorrect number of parameters for /warp</color>";
			int map = -1;
			int x = -1;
			int y = -1;
			if (!int.TryParse(args[1], out map)) return "<color=orange>Invalid mapId for /warp</color>";
			if (!int.TryParse(args[2], out x)) return "<color=orange>Invalid x for /warp</color>";
			if (!int.TryParse(args[3], out y)) return "<color=orange>Invalid y for /warp</color>";
			if (Data.mapList.Count <= map || Data.mapList[map] == null) return "<color=orange>Invalid map for /warp</color>";
			NetServer.use.WarpPlayer(player, map, x, y, -1);
			return "Warped to " + map + " ("+x+","+y+") successfully";
		}, null));
		cmds.Add("maplist", new CommandHandler((player, args) => {
			if (player.userlevel < UserLevel.Moderator) return "<color=orange>Insufficient permissions for /maplist</color>";
			string list = "<color=orange>Maps:\n";
			for (int i = 0; i < Data.maps.Count; i ++) {
				list += "" + i + "=" + Data.maps[i].name + "\n";
			}
			list += "</color>";
			return list;
		}, null));
		cmds.Add("nocollide", new CommandHandler((player, args) => {
			if (player.userlevel < UserLevel.Moderator) return "<color=orange>Insufficient permissions for /nocollide</color>";
			if (player.noCollide) {
				player.noCollide = false;
				return "<color=orange>NoCollide is now off</color>";
			}else{
				player.noCollide = true;
				return "<color=orange>NoCollide is now on</color>";
			}
		}, null));
		cmds.Add("playerlist", new CommandHandler((player, args) => {
			if (player.userlevel < UserLevel.User) return "<color=orange>Insufficient permissions for /playerlist</color>";
			string list = "<color=orange>Online Players: ";
			foreach (List<ServerPlayer> plist in NetServer.use.players.Values) {
				foreach (ServerPlayer p in plist) {
					if (p.userlevel != UserLevel.NPC) {
						list += p.name + ", ";
					}
				}
			}
			list = list.Substring(0, list.Length - 2);
			list += "</color>";
			return list;
		}, null));
		cmds.Add("say", new CommandHandler((player, args) => {
			string sender = "<color="+Player.userColors[(int)player.userlevel]+">"+player.name+"</color>";
			string message = "";
			for (int i = 1; i < args.Length; i ++) message += args[i] + " ";
			foreach (ServerPlayer p in NetServer.use.players[player.map]) {
				NetServer.use.SendRPC("ChatMsg", p.netPlayer, sender, (int)Channels.Map, message);
			}
			return "";
		}, (args) => {
			string message = "";
			for (int i = 1; i < args.Length; i ++) message += args[i] + " ";
			foreach (ServerPlayer p in NetServer.use.netPlayers.Values) {
				NetServer.use.SendRPC("ChatMsg", p.netPlayer, "<color=orange>Server</color>", (int)Channels.World, message);
			}
			return message;
		}));
		cmds.Add("bsay", new CommandHandler((player, args) => {
			string sender = "<color="+Player.userColors[(int)player.userlevel]+">"+player.name+"</color>";
			string message = "[<color=#00ff33ff>Battle</color>] ";
			for (int i = 1; i < args.Length; i ++) message += args[i] + " ";
			if (player.battle != null) {
				if (player.battle.attacker != null)
					NetServer.use.SendRPC("ChatMsg", (player.battle.attacker as ServerPlayer).netPlayer, sender, (int)Channels.Battle, message);
				if (player.battle.defender != null)
					NetServer.use.SendRPC("ChatMsg", (player.battle.defender as ServerPlayer).netPlayer, sender, (int)Channels.Battle, message);
			}
			return "";
		}, null));
		cmds.Add("tell", new CommandHandler((player, args) => {
			string sender = "<color="+Player.userColors[(int)player.userlevel]+">"+player.name+"</color>";
			string message = "[<color=#f000f0ff>Private</color>] ";
			for (int i = 2; i < args.Length; i ++) message += args[i] + " ";
			foreach (ServerPlayer p in NetServer.use.netPlayers.Values) {
				if (p.name == args[1]) {
					NetServer.use.SendRPC("ChatMsg", p.netPlayer, sender, (int)Channels.Private, message);
					return "";
				}
			}
			return "<color=orange>No player online with the name " + args[1] + "</color>";
		}, (args) => {
			string sender = "<color=orange>Server</color>";
			string message = "";
			for (int i = 2; i < args.Length; i ++) message += args[i] + " ";
			foreach (ServerPlayer p in NetServer.use.netPlayers.Values) {
				if (p.name == args[1]) {
					NetServer.use.SendRPC("ChatMsg", p.netPlayer, sender, (int)Channels.Private, message);
					return "";
				}
			}
			return message;
		}));
	}

	public static void Handle(string cmd) {
		string[] args = cmd.Split(' ');
		if (cmds.ContainsKey(args[0]) && cmds[args[0]].serverCmd != null) {
			string result = cmds[args[0]].serverCmd(args);
			if (result != "") {
				NetServer.use.Log(result);
			}
		}else{
			NetServer.use.Log("<color=orange>Unknown command: " + args[0] + "</color>");
		}
	}
	
	public static void Handle(ServerPlayer player, string cmd) {
		string[] args = cmd.Split(' ');
		if (cmds.ContainsKey(args[0]) && cmds[args[0]].clientCmd != null) {
			string result = cmds[args[0]].clientCmd(player, args);
			if (result != "") {
				NetServer.use.MsgPlayer(player, result);
			}
		}else{
			NetServer.use.MsgPlayer(player, "<color=orange>Unknown command: " + args[0] + "</color>");
		}
	}
}

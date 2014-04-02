using UnityEngine;
using System.Collections.Generic;

public class Player {
	public string name;
	public int dbId;
	public int map;
	public Point loc;
	public UserLevel userlevel;
	public int money;
	public Outfit outfit;
	public Battle battle;

	public Player (string name, Point loc, int map, UserLevel userlevel, int money, int dbId) {
		this.name = name;
		this.loc = loc;
		this.map = map;
		this.userlevel = userlevel;
		this.money = money;
		this.dbId = dbId;
		this.outfit = new Outfit();
	}
	
	//Userlevels: 6=admin, 5=dev, 4=mod, 3=NPC, 2=registered, 1=unactivated, 0=banned
	
	public static string[] userColors = {
		"#663333", //Banned
		"#999999", //Unactivated
		"#FFFFFF", //Player
		"#FFCC00", //NPC
		"#FFFFFF", //Moderator
		"#00CCFF", //Developer
		"#FF8800" //Admin
	};
}

public class ServerPlayer : Player {
	public NetworkPlayer netPlayer;
	public float moveTime;
	public float chatTime;
	public bool noCollide;
	public HashSet<int> challenges = new HashSet<int>();
	
	public ServerPlayer (string name, Point loc, int map, UserLevel userlevel, int money, int dbId) : base(name, loc, map, userlevel, money, dbId) { }
}

public class ClientPlayer : Player {
	public static ClientPlayer mine;
	public Avatar avatar;
	
	public ClientPlayer (string name, Point loc, int map, UserLevel userlevel, int money, int dbId) : base(name, loc, map, userlevel, money, dbId) {	}
}

public class NPC : ServerPlayer {
	public ALDNode rawData;
	public static int nextId = -1;
	public Medabot[] bots;
	public NPC(string name, Point loc, int map) : base(name, loc, map, UserLevel.NPC, 0, nextId) {
		nextId --;
		netPlayer = Network.player;
	}
}
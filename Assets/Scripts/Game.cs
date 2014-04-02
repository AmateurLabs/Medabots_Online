using UnityEngine;
using System.Collections;

public static class Game {
	
	public static GameMode mode;
	
	public static NetState netState;

	public static bool isServer {
		get {
			return netState == NetState.Server;
		}
	}

	public static bool isClient {
		get {
			return netState == NetState.LoggedIn;
		}
	}
}

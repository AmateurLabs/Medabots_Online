using UnityEngine;
using System.Collections;

public class Challenge {

	public Player attacker;
	public Player defender;

	public Challenge(Player atk, Player def) {
		attacker = atk;
		defender = def;
	}
}

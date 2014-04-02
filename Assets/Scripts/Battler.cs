using UnityEngine;
using System.Collections;

public class Battler : MonoBehaviour {
	public static Battler use;

	void Awake() {
		use = this;
	}

	public Battle battle;

	BattleFlags flags = BattleFlags.None;

	int nextId = 0;

	public AnimBot[] animBots = new AnimBot[6];
	public Medabot[] bots = new Medabot[6];

	void OnGUI() {
		XGUI.Init();
		if (Game.mode == GameMode.Battle && battle != null) {
			bool attacker = (battle.attacker == ClientPlayer.mine);
			bool defender = (battle.defender == ClientPlayer.mine);
			if (battle.flags.HasFlag(BattleFlags.SetupComplete)) {
				battle.OnGUI();
			}else{
				flags = BattleFlags.None;
				GUILayout.BeginArea(new Rect((Screen.width-Chat.use.window.width)/2f - 200, Screen.height/2f - 150, 400, 300), "Battle Settings", "window");
				GUILayout.BeginHorizontal();
				GUILayout.BeginVertical(GUILayout.Width(160f));
				foreach (Medabot bot in Medawatch.use.bots) {
					int i = -1;
					for (int j = 0; j < 3; j ++) {
						if (bots[j] == bot) {
							i = j;
							break;
						}
					}
					string name = "-----";
					if (bot.medal != null) name = bot.medal.GetName() + ((i > -1) ? ((i == 0) ? " (Leader)" : " (Partner " + i + ")" ) : "");
					if (bot.head != null && bot.lArm != null && bot.rArm != null && bot.legs != null && bot.tinpet != null && bot.medal != null) {
						bool toggle = GUILayout.Toggle(i > -1, name, "button");
						if (toggle && i == -1 && nextId < 3 && bot.medal != null) {
							bots[nextId] = bot;
							nextId++;
						}
						if (!toggle && i == nextId - 1 && i > -1 && bots[i] == bot) {
							bots[i] = null;
							nextId--;
						}
					} else {
						GUILayout.Box(name);
					}
				}
				GUILayout.EndVertical();
				GUI.enabled = attacker;
				GUILayout.BeginVertical(GUILayout.Width(100));
				FlagToggle(BattleFlags.BotTop, "Leader enabled");
				FlagToggle(BattleFlags.BotMiddle, "Partner 1 enabled");
				FlagToggle(BattleFlags.BotBottom, "Partner 2 enabled");
				FlagToggle(BattleFlags.WagerNormal, "Wager normal Medaparts");
				FlagToggle(BattleFlags.WagerSpecial, "Wager rare Medaparts");
				FlagToggle(BattleFlags.WinTypeLeader, "Win when Leader's function ceases");
				FlagToggle(BattleFlags.WinTypeTime, "Enable Robattle timer");
				FlagToggle(BattleFlags.EnableCmdTimer, "Enable command timer");
				GUILayout.EndVertical();
				if (GUI.enabled && GUI.changed) NetClient.use.AskSetBattleFlags(flags);
				GUI.enabled = true;
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUI.enabled = attacker;
				if (!battle.flags.HasFlag(BattleFlags.AttackerReady) && GUILayout.Button("Ready")) {
					NetClient.use.AskBattleReady((bots[0] != null) ? bots[0].dbId : -1,
					                             (bots[1] != null) ? bots[1].dbId : -1,
					                             (bots[2] != null) ? bots[2].dbId : -1);
				}
				if (battle.flags.HasFlag(BattleFlags.AttackerReady)) GUILayout.Box("Ready");
				GUI.enabled = defender;
				if (!battle.flags.HasFlag(BattleFlags.DefenderReady) && GUILayout.Button("Ready")) {
					NetClient.use.AskBattleReady((bots[0] != null) ? bots[0].dbId : -1,
					                             (bots[1] != null) ? bots[1].dbId : -1,
					                             (bots[2] != null) ? bots[2].dbId : -1);
				}
				if (battle.flags.HasFlag(BattleFlags.DefenderReady)) GUILayout.Box("Ready");
				GUI.enabled = true;
				GUILayout.EndHorizontal();
				if (GUILayout.Button("Abandon Battle")) {
					NetClient.use.AskQuitBattle();
				}
				GUILayout.EndArea();
			}
		}
	}

	void Update() {
		if (battle != null && battle.flags.HasFlag(BattleFlags.SetupComplete)) battle.Update();
	}

	public void ClearBattle() {
		nextId = 0;
		battle.attacker.battle = null;
		battle.defender.battle = null;
		battle = null;
		Game.mode = GameMode.Explore;
	}
	
	void FlagToggle(BattleFlags flag, string name) {
		if (GUILayout.Toggle((battle.flags & flag) == flag, name)) flags |= flag;
	}
}

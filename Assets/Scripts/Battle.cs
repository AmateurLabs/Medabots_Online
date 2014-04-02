using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Battle {
	public static Dictionary<int, Battle> battles = new Dictionary<int, Battle>();
	private static int nextId = 0;

	public readonly int id;
	public Player attacker;
	public Player defender;

	public List<Player> spectators = new List<Player>();

	public BattleFlags flags;

	public float pauseTime = 0;

	public Bot[] bots = new Bot[6];

	public Battle(Player atk, Player def) {
		attacker = atk;
		defender = def;
		atk.battle = this;
		def.battle = this;
		battles.Add(nextId, this);
		id = nextId;
		nextId++;
	}

	public void Update() {
		bool paused = false;
		if (pauseTime > 0f) {
			pauseTime -= Time.deltaTime;
			paused = true;
		}
		for (int i = 0; i < 6; i++) {
			if (bots[i] == null) continue;
			if (bots[i].state == BotState.Standby || bots[i].state == BotState.Attacking) {
				paused = true;
			}
		}
		if (paused) return;
		for (int i = 0; i < 6; i++) {
			if (bots[i] == null) continue;
			if (bots[i].state == BotState.Charging && bots[i].charge >= bots[i].maxCharge) {
				bots[i].state = BotState.Attacking;
				if (Game.isServer) {
					int r0 = Random.Range(0, 256);
					int r1 = Random.Range(0, 256);
					Bot target = null;
					int targ = bots[i][(int)bots[i].usePart].t.ability.findTarget(this, bots[i]);
					if (targ >= 0 && targ < 6 && bots[targ] != null) target = bots[targ];
					UseAbility(bots[i], target, bots[i].usePart, (byte)r0, (byte)r1);
				}
				return;
			} else if (bots[i].state == BotState.Cooling && bots[i].charge <= 0) {
				bots[i].state = BotState.Standby;
				if (Game.isServer && bots[i].owner.userlevel == UserLevel.NPC) {
					int part = Random.Range(0, 4);
					SetAbility(bots[i], (PartIndex)part);
				}
				return;
			}
		}
		for (int i = 0; i < 6; i++) {
			if (bots[i] == null) continue;
			if (bots[i].state == BotState.Charging) bots[i].charge += Time.deltaTime;
			else if (bots[i].state == BotState.Cooling) bots[i].charge -= Time.deltaTime;
		}
	}

    public static Texture2D statusTex = Resources.Load<Texture2D>("GUI/statusicons");

	public void OnGUI() {
		GUI.skin.label.alignment = TextAnchor.MiddleCenter;
		GUI.Label(new Rect(0, 0, 192, 64), "<size=24>" + attacker.name + "</size>");
		GUI.Label(new Rect(256 - 64, 0, 128, 64), "<size=32>vs.</size>");
		GUI.Label(new Rect(512 - 192, 0, 192, 64), "<size=24>" + defender.name + "</size>");
		GUI.skin.label.alignment = TextAnchor.UpperLeft;
		for (int i = 0; i < 6; i++) {
			if (bots[i] != null) {
				Bot bot = bots[i];
				Vector2 o = new Vector2((i < 3) ? 0 : 512 - 96, 96 + 8 + 128 * (i % 3));
				GUI.Label(new Rect(o.x, o.y-20f, 96f, 24f), bot.medal.name, "GBA");
				bool flip = i < 3;
				XGUI.ValueBar(new Rect(o.x, o.y, 96f, 24f), ((float)bot.head.armor / (float)bot.head.t.armor), bot.head.t.name, Color.green, !flip);
				XGUI.ValueBar(new Rect(o.x, o.y+16f, 96f, 24f), ((float)bot.lArm.armor / (float)bot.lArm.t.armor), bot.lArm.t.name, Color.green, !flip);
				XGUI.ValueBar(new Rect(o.x, o.y+32f, 96f, 24f), ((float)bot.rArm.armor / (float)bot.rArm.t.armor), bot.rArm.t.name, Color.green, !flip);
				XGUI.ValueBar(new Rect(o.x, o.y+48f, 96f, 24f), ((float)bot.legs.armor / (float)bot.legs.t.armor), bot.legs.t.name, Color.green, !flip);
				XGUI.ValueBar(new Rect(o.x, o.y+64f, 96f, 24f), ((float)bot.medal.medaforce / 80f), bot.medal.t.name, Color.cyan, !flip);
				if (flip) XGUI.ValueBar(new Rect(o.x, o.y + 80f, 192f, 24f), (bot.charge / bot.maxCharge), bot.state.ToString(), Color.yellow, !flip);
				else XGUI.ValueBar(new Rect(o.x - 96f, o.y + 80f, 192f, 24f), (bot.charge / bot.maxCharge), bot.state.ToString(), Color.yellow, !flip);
				if (GUI.Button(new Rect(o.x, o.y + 4f, 96f, 16f), "", "label")) NetClient.use.AskSetAbility(i, PartIndex.Head);
				if (GUI.Button(new Rect(o.x, o.y + 4f + 16f, 96f, 16f), "", "label")) NetClient.use.AskSetAbility(i, PartIndex.LArm);
				if (GUI.Button(new Rect(o.x, o.y + 4f + 32f, 96f, 16f), "", "label")) NetClient.use.AskSetAbility(i, PartIndex.RArm);
				if (GUI.Button(new Rect(o.x, o.y + 4f + 48f, 96f, 16f), "", "label")) NetClient.use.AskSetAbility(i, PartIndex.Legs);
                bot.status = (StatusEffect)(Mathf.RoundToInt(Time.time) % 17);
                GUI.DrawTextureWithTexCoords(new Rect(o.x + (flip ? 192 - 16 : -96), o.y - 16f, 16f, 16f), statusTex, new Rect(0.0625f * ((int)bot.status - 1), 0f, 0.0625f, 1f));
			}
		}
		if (GUI.Button(new Rect(512f - 64f, 512 - 16f, 64f, 16f), "Forfeit")) {
			NetClient.use.AskQuitBattle();
		}
	}

	public void SetAbility(Bot bot, PartIndex part) {
		if (Game.isServer) {
			NetServer.use.SendRPC("SetAbility", ((ServerPlayer)attacker).netPlayer, Find(bot), (int)part);
			NetServer.use.SendRPC("SetAbility", ((ServerPlayer)defender).netPlayer, Find(bot), (int)part);
		}
		bot.usePart = part;
		int chrg = 0;
		if (part == PartIndex.Head) chrg = 10;
		else if (part == PartIndex.LArm) chrg = bot.lArm.t.charge;
		else if (part == PartIndex.RArm) chrg = bot.rArm.t.charge;
		else chrg = 10;
		bot.maxCharge = chrg;
		bot.state = BotState.Charging;
		bot.charge = -bot.charge;
	}

	public void UseAbility(Bot bot, Bot targ, PartIndex part, byte rand0, byte rand1) {
		float diff = bot.charge - bot.maxCharge;
		Part p = bot[(int)part];
		Ability a = p.t.ability;
		bool charge = (part == PartIndex.Legs);
		if (charge) a = Data.abilities["Charge Medaforce"];
		if (Game.isServer) {
			NetServer.use.SendRPC("UseAbility", ((ServerPlayer)attacker).netPlayer, Find(bot), Find(targ), (int)bot.usePart, (int)rand0, (int)rand1);
			NetServer.use.SendRPC("UseAbility", ((ServerPlayer)defender).netPlayer, Find(bot), Find(targ), (int)bot.usePart, (int)rand0, (int)rand1);
		}
		if (charge) Log(bot.medal.name + " focused its energy.");
		else Log(bot.medal.name + " used its " + p.t.name + "! " + p.t.type + "-action " + p.t.ability.name + "!");

		a.execute(this, bot, targ, p, rand0, rand1);
		bot.state = BotState.Cooling;
		bot.charge -= diff;
		pauseTime = 3f;
	}

	public static void Log(string text) {
		//if (Game.isServer) NetServer.use.Log(text);
		if (Game.isClient) NetClient.use.Log(text);
	}

	public int Find(Bot bot) {
		if (bot == null) return -1;
		for (int i = 0; i < 6; i++) {
			if (bots[i] == bot) return i;
		}
		return -1;
	}

	public static int CalcComp(Bot bot) {
		int comp = 0;
		if (bot.medal.t.attr == bot.head.t.attr) comp += bot.medal.t.compatibility;
		if (bot.medal.t.attr == bot.lArm.t.attr) comp += bot.medal.t.compatibility;
		if (bot.medal.t.attr == bot.rArm.t.attr) comp += bot.medal.t.compatibility;
		if (bot.medal.t.attr == bot.legs.t.attr) comp += bot.medal.t.compatibility;
		return comp;
	}

	public static int CalcBaseDamage(Bot attacker, WepPart part) {
		int dmg = 0;
		dmg += CalcROS(attacker, part) / 4;
		dmg += part.t.power;
		if (part.t.ability.name == "Beam" || part.t.ability.name == "Laser") dmg += part.t.power;
		int prop = attacker.legs.t.propulsion;
		if (attacker.legs.armor == 0) prop /= 2;
		if (part.t.type == PartType.Berserk) dmg += prop / 2;
		return dmg;
	}

	public static int CalcDamage(Bot attacker, Bot defender, WepPart part) {
		int dmg = CalcBaseDamage(attacker, part);
		int def = defender.legs.t.defense;
		if (defender.legs.armor == 0) def /= 2;
		dmg -= def / 2;
		dmg -= CalcComp(defender) / 4;
		dmg -= defender.medal.level / 2;
		dmg = Mathf.Max(dmg, CalcBaseDamage(attacker, part));
		return dmg;
	}

	public static int CalcROS(Bot bot, WepPart part) {
		int ros = 0;
		if (bot.medal.t.attr == part.t.attr) ros += CalcComp(bot);
		ros += bot.medal.skills[(int)part.t.type];
		ros += part.t.rateOfSuccess;
		if (part.t.ability.name == "Break" || part.t.ability.name == "Press") ros += part.t.rateOfSuccess;
		int legStat = 0;
		if (part.t.control == Control.Proximity) legStat = bot.legs.t.proximity;
		if (part.t.control == Control.Remoteness) legStat = bot.legs.t.remoteness;
		if (bot.legs.armor == 0) legStat /= 2;
		ros += legStat;
		return ros;
	}

	//End of 'Sacred Ground': the original equations for the following methods are unknown

	public static int CalcSpeed(Bot bot) {
		int speed = bot.medal.level;
		int prop = bot.legs.t.propulsion;
		if (bot.legs.armor == 0) prop /= 2;
		if (bot.head.t.ability.name == "Accel Chrg") speed += 20;
		if (bot.lArm.t.ability.name == "Accel Chrg") speed += 20;
		if (bot.rArm.t.ability.name == "Accel Chrg") speed += 20;
		return Mathf.RoundToInt((prop + speed) / (bot.maxCharge + 5));
	}

	public static bool CalcCrit(Bot attacker, WepPart part, byte rand) {
		int ros = CalcROS(attacker, part);
		if (part.t.type != PartType.AimShot) ros /= 2;
		return (ros >= rand);
	}

	public static bool CalcHit(Bot attacker, Bot defender, WepPart part, byte rand) {
		if (part.t.ability.isMedaforce) return true;
		if (part.t.ability.name == "Missile" || part.t.ability.name == "Napalm") return true;
		int ros = 0;
		ros += CalcROS(attacker, part) * 4;
		int evas = defender.legs.t.evasion;
		if (defender.legs.armor == 0) evas /= 2;
		ros -= evas;
		ros -= defender.medal.level / 4;
		return (ros >= rand);
	}

	public static bool CalcCancel(Bot attacker, Bot defender, string cancelType) {
		if (defender.head.armor > 0 && defender.head.t.ability.name == "Cancel" + cancelType) return true;
		if (defender.lArm.armor > 0 && defender.lArm.t.ability.name == "Cancel" + cancelType) return true;
		if (defender.rArm.armor > 0 && defender.rArm.t.ability.name == "Cancel" + cancelType) return true;
		return false;
	}
}

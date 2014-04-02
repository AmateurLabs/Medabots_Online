using UnityEngine;
using System.Collections.Generic;

public class Ability {
	public string name;
	public TargetCheck isTarget;
	public FindTarget findTarget;
	public ExecuteAbility execute;
	public bool isMedaforce;

	public Ability(string name, TargetCheck isTarget, FindTarget findTarget, ExecuteAbility execute) {
		this.name = name;
		this.isTarget = isTarget;
		this.findTarget = findTarget;
		this.execute = execute;
		Data.abilities.Add(name, this);
	}

	public delegate bool TargetCheck(Battle battle, Bot attacker, Bot defender);
	public delegate int FindTarget(Battle battle, Bot bot);
	public delegate void ExecuteAbility(Battle battle, Bot bot, Bot target, Part part, byte rand0, byte rand1);

	public static void LoadAll() {
		Data.abilities = new Dictionary<string, Ability>();
		FindTarget nullFind = (battle, bot) => { return -1; };
		FindTarget meleeFind = (battle, bot) => {
			float charge = -1f;
			int target = -1;
			int b = battle.Find(bot);
			for (int i = (b < 3 ? 3 : 0); i < (b < 3 ? 6 : 3); i++) {
				if (battle.bots[i] != null) {
					if (battle.bots[i].state == BotState.Dead) continue;
					float chrg = battle.bots[i].charge / battle.bots[i].maxCharge;
					if (chrg > charge) {
						charge = chrg;
						target = i;
					}
				}
			}
			return target;
		};
		FindTarget rangedFind = (battle, bot) => {
			int p = battle.Find(bot);
			int targetParts = -1;
			int target = -1;
			for (int i = ((p < 3) ? 3 : 0); i < ((p < 3) ? 6 : 3); i++) {
				if (battle.bots[i] == null || battle.bots[i].head.armor == 0) continue;
				Bot b = battle.bots[i];
				int targParts = 0;
				if (b.head.armor > 0 && bot.medal.t.aim == b.head.t.type) targParts++;
				if (b.lArm.armor > 0 && bot.medal.t.aim == b.lArm.t.type) targParts++;
				if (b.rArm.armor > 0 && bot.medal.t.aim == b.rArm.t.type) targParts++;
				if (b.legs.armor > 0 && bot.medal.t.aim == b.legs.t.type) targParts++;
				if (targParts > targetParts) {
					targetParts = targParts;
					target = i;
				}
			}
			return target;
		};
		TargetCheck nullCheck = (b, a, d) => { return false; };
		TargetCheck rangedCheck = (b, a, d) => { return rangedFind(b, a) == b.Find(d); };
		ExecuteAbility nullExecute = (battle, bot, targ, part, r0, r1) => {
			Battle.Log(part.t.name + " has no effect!");
		};
		ExecuteAbility attackExecute = (battle, bot, targ, atkPart, r0, r1) => {
			WepPart part = (WepPart)atkPart;
			if (part.armor == 0) {
				Battle.Log(bot.medal.name + " can't use its broken " + part.t.name + "!");
				return;
			}
			if (targ == null) {
				Battle.Log(bot.medal.name + " has no valid targets to use its " + part.t.name + " on!");
				return;
			}
			bool cantDefend = targ.state == BotState.Cooling && targ[(int)targ.usePart].t.type == PartType.Berserk;
			bool crit = Battle.CalcCrit(bot, part, r1) || cantDefend;
			bool hit = Battle.CalcHit(bot, targ, part, r0);
			if (!cantDefend && !hit) {
				Battle.Log(bot.medal.name + " missed!");
				return;
			}
			if (((part.t.ability.name == "Missile" || part.t.ability.name == "Napalm") && Battle.CalcCancel(bot, targ, "Bomb")) ||
				((part.t.ability.name == "Laser" || part.t.ability.name == "Beam") && Battle.CalcCancel(bot, targ, "Optic")) ||
				((part.t.ability.name == "Break" || part.t.ability.name == "Press") && Battle.CalcCancel(bot, targ, "Grav"))) {
					Battle.Log(targ.medal.name + " canceled " + bot.medal.name + "'s attack!");
					return;
			}
			int damage = (crit) ? Battle.CalcBaseDamage(bot, part) : Battle.CalcDamage(bot, targ, part);
			if (crit) Battle.Log("Critical hit!");
			while (damage > 0) {
				int maxHp = 0;
				if (targ.head.armor / 2 > maxHp) maxHp = targ.head.armor / 2;
				if (targ.lArm.armor > maxHp) maxHp = targ.lArm.armor;
				if (targ.rArm.armor > maxHp) maxHp = targ.rArm.armor;
				if (targ.legs.armor > maxHp) maxHp = targ.legs.armor;
				if (maxHp == 0) break;
				if (targ.head.armor / 2 == maxHp) damage = DamagePart(battle, targ, targ.head, damage);
				else if (targ.lArm.armor == maxHp) damage = DamagePart(battle, targ, targ.lArm, damage);
				else if (targ.rArm.armor == maxHp) damage = DamagePart(battle, targ, targ.rArm, damage);
				else if (targ.legs.armor == maxHp) damage = DamagePart(battle, targ, targ.legs, damage);
				if (!part.t.chainDamage) break;
			}
			if (targ.head.armor == 0) {
				Battle.Log(targ.medal.name + "'s function ceased!");
				targ.state = BotState.Dead;
			}
			if (Game.isClient) {
				Battler.use.animBots[battle.Find(targ)].Refresh();
			}
		};
		new Ability("Accel Chrg", nullCheck, nullFind, nullExecute);
		new Ability("Anti-Air", rangedCheck, nullFind, nullExecute);
		new Ability("Anti-Sea", rangedCheck, nullFind, nullExecute);
		new Ability("Attack Clr", nullCheck, nullFind, nullExecute);
		new Ability("AttkChange", nullCheck, nullFind, nullExecute);
		new Ability("AutoRecover", rangedCheck, nullFind, nullExecute);
		new Ability("Beam", rangedCheck, rangedFind, attackExecute);
		new Ability("Boost Chrg", nullCheck, nullFind, nullExecute);
		new Ability("Break", rangedCheck, rangedFind, attackExecute);
		new Ability("Break Form", rangedCheck, nullFind, nullExecute);
		new Ability("Bug", rangedCheck, nullFind, nullExecute);
		new Ability("CancelBomb", rangedCheck, nullFind, nullExecute);
		new Ability("CancelGrav", rangedCheck, nullFind, nullExecute);
		new Ability("CancelOptic", rangedCheck, nullFind, nullExecute);
		new Ability("Chain Gun", rangedCheck, rangedFind, attackExecute);
		new Ability("Change", rangedCheck, nullFind, nullExecute);
		new Ability("Chaos", rangedCheck, nullFind, nullExecute);
		new Ability("CntrAttk", rangedCheck, nullFind, nullExecute);
		new Ability("Conceal", rangedCheck, nullFind, nullExecute);
		new Ability("Confusion", rangedCheck, nullFind, nullExecute);
		new Ability("DEF Change", rangedCheck, nullFind, nullExecute);
		new Ability("Defence", rangedCheck, nullFind, nullExecute);
		new Ability("Destroy", rangedCheck, nullFind, nullExecute);
		new Ability("Drain Chrg", rangedCheck, nullFind, nullExecute);
		new Ability("Fire", rangedCheck, nullFind, nullExecute);
		new Ability("Force Bind", rangedCheck, nullFind, nullExecute);
		new Ability("Forcedrain", rangedCheck, nullFind, nullExecute);
		new Ability("Freeze", rangedCheck, nullFind, nullExecute);
		new Ability("Full Block", rangedCheck, nullFind, nullExecute);
		new Ability("Grap Trap", rangedCheck, nullFind, nullExecute);
		new Ability("Half Block", rangedCheck, nullFind, nullExecute);
		new Ability("Hammer", rangedCheck, meleeFind, attackExecute);
		new Ability("HealChange", rangedCheck, nullFind, nullExecute);
		new Ability("Hold", rangedCheck, nullFind, nullExecute);
		new Ability("Impair", rangedCheck, nullFind, nullExecute);
		new Ability("ItrpChange", rangedCheck, nullFind, nullExecute);
		new Ability("Laser", rangedCheck, rangedFind, attackExecute);
		new Ability("Melt", rangedCheck, nullFind, nullExecute);
		new Ability("Missile", rangedCheck, nullFind, attackExecute);
		new Ability("Napalm", rangedCheck, rangedFind, attackExecute);
		new Ability("No Defense", rangedCheck, nullFind, nullExecute);
		new Ability("No Escape", rangedCheck, nullFind, nullExecute);
		new Ability("Press", rangedCheck, rangedFind, attackExecute);
		new Ability("Pushover", rangedCheck, nullFind, nullExecute);
		new Ability("Rapid Chrg", rangedCheck, nullFind, nullExecute);
		new Ability("Reactivate", rangedCheck, nullFind, nullExecute);
		new Ability("Recovery", rangedCheck, nullFind, nullExecute);
		new Ability("Repair", rangedCheck, nullFind, nullExecute);
		new Ability("Rifle", rangedCheck, rangedFind, attackExecute);
		new Ability("Sacrifice", rangedCheck, nullFind, nullExecute);
		new Ability("Scout", rangedCheck, nullFind, nullExecute);
		new Ability("Shot Trap", rangedCheck, nullFind, nullExecute);
		new Ability("Stability", rangedCheck, nullFind, nullExecute);
		new Ability("Strengthen", rangedCheck, nullFind, nullExecute);
		new Ability("Sword", rangedCheck, meleeFind, attackExecute);
		new Ability("Team Form", rangedCheck, nullFind, nullExecute);
		new Ability("TeamAttack", rangedCheck, nullFind, nullExecute);
		new Ability("Thunder", rangedCheck, nullFind, nullExecute);
		new Ability("Use Drain", rangedCheck, nullFind, nullExecute);
		new Ability("Virus", rangedCheck, nullFind, nullExecute);
		new Ability("Wave", rangedCheck, nullFind, nullExecute);
		new Ability("Charge Medaforce", nullCheck, nullFind, (battle, bot, targ, part, r0, r1) => {
			if (bot.medal.medaforce >= 80) {
				Battle.Log(bot.medal.name + " cannot gather any more Medaforce!");
				return;
			}
			bot.medal.medaforce += Mathf.Min(80 - bot.medal.medaforce, 40);
			Battle.Log(bot.medal.name + " charged its Medaforce!");
		});
	}

	private static int DamagePart(Battle battle, Bot targ, Part part, int damage) {
		int overdmg = Mathf.Max(0, damage - part.armor);
		part.armor -= damage;
		Battle.Log(targ.medal.name + "'s " + part.t.name + " took " + (damage - overdmg) + " damage!");
		part.armor = Mathf.Clamp(part.armor, 0, part.t.armor);
		if (part.armor == 0) Battle.Log(targ.medal.name + "'s " + part.t.name + " was destroyed!");
		return overdmg;
	}

	public class AbilityArgs {
		public int user;
		public int target;
		public float random;
		public PartType part;
	}
}
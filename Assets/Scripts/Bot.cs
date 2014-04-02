using UnityEngine;
using System;
using System.Collections;

public class Bot {
	public Player owner;
	public Medal medal;
	public bool female;
	public Head head;
	public Arm lArm;
	public Arm rArm;
	public Legs legs;
	public Medabot medabot;

	public float charge;
	public float maxCharge;

	public PartIndex usePart;
	public BotState state;

    public StatusEffect status;
    public ushort statusData;

	public Bot (Medabot bot) {
		if (Game.isServer) owner = NetServer.use.dbPlayers[bot.playerId];
		else owner = NetClient.use.PlayerById(bot.playerId);
		if (bot.medal != null) {
			if (bot.medal.metaObj != null) medal = bot.medal.metaObj as Medal;
			else if (Data.medals.ContainsKey(bot.medal.meta)) medal = Data.medals[bot.medal.meta];
		}
		female = bot.tinpet.item == 1;
		head = new Head(Data.tPartSetList[bot.head.item].head);
		lArm = new Arm(Data.tPartSetList[bot.lArm.item].lArm);
		rArm = new Arm(Data.tPartSetList[bot.rArm.item].rArm);
		legs = new Legs(Data.tPartSetList[bot.legs.item].legs);
		medabot = bot;
		medal.medaforce = 0;
		charge = 0f;
		maxCharge = 1;
		state = BotState.Standby;
	}

	public Part this[int index] {
		get {
			PartIndex i = (PartIndex)index;
			if (i == PartIndex.Head) return head;
			if (i == PartIndex.LArm) return lArm;
			if (i == PartIndex.RArm) return rArm;
			if (i == PartIndex.Legs) return legs;
			return null;
		}
		set {
			PartIndex i = (PartIndex)index;
			if (i == PartIndex.Head) head = (Head)value;
			if (i == PartIndex.LArm) lArm = (Arm)value;
			if (i == PartIndex.RArm) rArm = (Arm)value;
			if (i == PartIndex.Legs) legs = (Legs)value;
		}
	}

	public byte[] ToBytes() {
		byte[] medalBytes = medal.ToBytes();
		byte[] bytes = new byte[4+medalBytes.Length+4+4+6*Item.BYTE_LENGTH];
		int i = 0;
		i += Util.SetBytes(BitConverter.GetBytes(medalBytes.Length), bytes, i);
		i += Util.SetBytes(medalBytes, bytes, i);
		i += Util.SetBytes(BitConverter.GetBytes(medabot.dbId), bytes, i);
		i += Util.SetBytes(BitConverter.GetBytes(medabot.playerId), bytes, i);
		i += Util.SetBytes(medabot.head.ToBytes(), bytes, i);
		i += Util.SetBytes(medabot.lArm.ToBytes(), bytes, i);
		i += Util.SetBytes(medabot.rArm.ToBytes(), bytes, i);
		i += Util.SetBytes(medabot.legs.ToBytes(), bytes, i);
		i += Util.SetBytes(medabot.tinpet.ToBytes(), bytes, i);
		i += Util.SetBytes(medabot.medal.ToBytes(), bytes, i);
		return bytes;
	}

	public static Bot FromBytes(byte[] bytes) {
		Medabot mb = new Medabot();
		int i = 0;
		int medalLength = BitConverter.ToInt32(bytes, i); i += 4;
		Medal medal = Medal.FromBytes(Util.CopyBytes(bytes, i, medalLength)); i += medalLength;
		mb.dbId = BitConverter.ToInt32(bytes, i); i += 4;
		mb.playerId = BitConverter.ToInt32(bytes, i); i += 4;
		mb.head = Item.FromBytes(Util.CopyBytes(bytes, i, Item.BYTE_LENGTH)); i += Item.BYTE_LENGTH;
		mb.headId = mb.head.id;
		mb.lArm = Item.FromBytes(Util.CopyBytes(bytes, i, Item.BYTE_LENGTH)); i += Item.BYTE_LENGTH;
		mb.lArmId = mb.lArm.id;
		mb.rArm = Item.FromBytes(Util.CopyBytes(bytes, i, Item.BYTE_LENGTH)); i += Item.BYTE_LENGTH;
		mb.rArmId = mb.rArm.id;
		mb.legs = Item.FromBytes(Util.CopyBytes(bytes, i, Item.BYTE_LENGTH)); i += Item.BYTE_LENGTH;
		mb.legsId = mb.legs.id;
		mb.tinpet = Item.FromBytes(Util.CopyBytes(bytes, i, Item.BYTE_LENGTH)); i += Item.BYTE_LENGTH;
		mb.tinpetId = mb.tinpet.id;
		mb.medal = Item.FromBytes(Util.CopyBytes(bytes, i, Item.BYTE_LENGTH)); i += Item.BYTE_LENGTH;
		mb.medalId = mb.medal.id;
		mb.medal.metaObj = medal;
		Bot bot = new Bot(mb);
		return bot;
	}
}

public abstract class Part {
	public int armor;
	public TPart t;

	public Part(TPart t) {
		this.t = t;
		this.armor = t.armor;
	}
}

public abstract class WepPart : Part {
	public new TWepPart t;

	public WepPart(TWepPart t) : base(t) {
		this.t = t;
	}
}

public class Head : WepPart {
	public new THead t;
	public int uses;
	
	public Head(THead t) : base(t) {
		this.t = t;
	}
}

public class Arm : WepPart {
	public new TArm t;
	
	public Arm(TArm t) : base(t) {
		this.t = t;
	}
}

public class Legs : Part {
	public new TLegs t;
	
	public Legs(TLegs t) : base(t) {
		this.t = t;
	}
}
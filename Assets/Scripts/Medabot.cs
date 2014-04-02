using UnityEngine;
using System;
using System.Collections;

public class Medabot {
	public int dbId;
	public int playerId;
	public Item tinpet;
	public int tinpetId;
	public Item medal;
	public int medalId;
	public Item head;
	public int headId;
	public Item lArm;
	public int lArmId;
	public Item rArm;
	public int rArmId;
	public Item legs;
	public int legsId;
	
	public Item this [Item.Type index] {
		get {
			if (index == Item.Type.Head) return head;
			if (index == Item.Type.LArm) return lArm;
			if (index == Item.Type.RArm) return rArm;
			if (index == Item.Type.Legs) return legs;
			if (index == Item.Type.Medal) return medal;
			if (index == Item.Type.Component) return tinpet;
			return null;
		}
		set {
			throw new NotImplementedException ();
		}
	}
	
	public Medabot() {
		
	}

	public static Medabot Generate(int owner, ALDNode node) {
		Medabot b = new Medabot();
		b.dbId = b.headId = b.lArmId = b.rArmId = b.legsId = b.tinpetId = b.medalId = -1;
		b.playerId = owner;
		if (node.Contains("id")) b.dbId = (int)node["id"].Value;
		if (node.Contains("playerId")) b.playerId = (int)node["playerId"].Value;
		if (node["tinpet"].ChildCount > 0) b.tinpet = new Item(node["tinpet"]);
		if (node["medal"].ChildCount > 0) b.medal = new Item(node["medal"]);
		if (node["head"].ChildCount > 0) b.head = new Item(node["head"]);
		if (node["larm"].ChildCount > 0) b.lArm = new Item(node["larm"]);
		if (node["rarm"].ChildCount > 0) b.rArm = new Item(node["rarm"]);
		if (node["legs"].ChildCount > 0) b.legs = new Item(node["legs"]);
		return b;
	}
	
	public Medabot(ALDNode node) {
		dbId = (int)node["id"].Value;
		playerId = (int)node["playerId"].Value;
		tinpetId = (int)node["tinpet"].Value;
		medalId = (int)node["medal"].Value;
		headId = (int)node["head"].Value;
		lArmId = (int)node["larm"].Value;
		rArmId = (int)node["rarm"].Value;
		legsId = (int)node["legs"].Value;
	}
	
	public byte[] ToBytes() {
		byte[] bytes = new byte[8*4];
		Util.SetBytes(BitConverter.GetBytes(dbId), bytes, 0);
		Util.SetBytes(BitConverter.GetBytes(playerId), bytes, 4);
		Util.SetBytes(BitConverter.GetBytes(tinpetId), bytes, 8);
		Util.SetBytes(BitConverter.GetBytes(medalId), bytes, 12);
		Util.SetBytes(BitConverter.GetBytes(headId), bytes, 16);
		Util.SetBytes(BitConverter.GetBytes(lArmId), bytes, 20);
		Util.SetBytes(BitConverter.GetBytes(rArmId), bytes, 24);
		Util.SetBytes(BitConverter.GetBytes(legsId), bytes, 28);
		return bytes;
	}

	public static Medabot FromBytes(byte[] bytes) {
		Medabot bot = new Medabot();
		bot.dbId = BitConverter.ToInt32(bytes, 0);
		bot.playerId = BitConverter.ToInt32(bytes, 4);
		bot.tinpetId = BitConverter.ToInt32(bytes, 8);
		bot.medalId = BitConverter.ToInt32(bytes, 12);
		bot.headId = BitConverter.ToInt32(bytes, 16);
		bot.lArmId = BitConverter.ToInt32(bytes, 20);
		bot.rArmId = BitConverter.ToInt32(bytes, 24);
		bot.legsId = BitConverter.ToInt32(bytes, 28);
		bot.TryLoadItems();
		return bot;
	}
	
	public void EquipItem(Item item) {
		if (item.id < 0) {
			item.type = (Item.Type)(-item.id);
			item.id = 0;
		}
		if (item.type == Item.Type.Head) {
			headId = item.id;
			head = item;
		}else if (item.type == Item.Type.LArm) {
			lArmId = item.id;
			lArm = item;
		}else if (item.type == Item.Type.RArm) {
			rArmId = item.id;
			rArm = item;
		}else if (item.type == Item.Type.Legs) {
			legsId = item.id;
			legs = item;
		}else if (item.type == Item.Type.Component) {
			tinpetId = item.id;
			tinpet = item;
		}else if (item.type == Item.Type.Medal) {
			medalId = item.id;
			medal = item;
		}
		TryLoadItems();
	}
	
	public void TryLoadItems() {
		if (tinpet == null) Data.items.TryGetValue(tinpetId, out tinpet);
		if (medal == null) Data.items.TryGetValue(medalId, out medal);
		if (head == null) Data.items.TryGetValue(headId, out head);
		if (lArm == null) Data.items.TryGetValue(lArmId, out lArm);
		if (rArm == null) Data.items.TryGetValue(rArmId, out rArm);
		if (legs == null) Data.items.TryGetValue(legsId, out legs);
		RefreshImg();
	}
	
	private Texture2D img;
	
	public Texture2D GetImg() {
		if (img != null) return img;
		string tinpetPath = "Bots/"+((tinpet != null) ? ((tinpet.item == 1) ? "Female Tinpet" : "Male Tinpet") : "Blank") + "/";
		img = (Texture2D)Texture2D.Instantiate(((legs != null) ? Resources.Load("Bots/"+Data.statNode[legs.item].Value+"/legs") : null) ?? Resources.Load(tinpetPath+"legs"));
		img.Layer(((lArm != null) ? (Texture2D)Resources.Load("Bots/"+Data.statNode[lArm.item].Value+"/larm") : null) ?? (Texture2D)Resources.Load(tinpetPath+"larm"));
		img.Layer(((head != null) ? (Texture2D)Resources.Load("Bots/"+Data.statNode[head.item].Value+"/head") : null) ?? (Texture2D)Resources.Load(tinpetPath+"head"));
		img.Layer(((rArm != null) ? (Texture2D)Resources.Load("Bots/"+Data.statNode[rArm.item].Value+"/rarm") : null) ?? (Texture2D)Resources.Load(tinpetPath+"rarm"));
		return img;
	}
	
	public void RefreshImg() {
		if (img == null) return;
		Texture2D.Destroy(img);
		img = null;
	}
}

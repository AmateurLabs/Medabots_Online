using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Item {
	
	public int id;
	public Type type;
	public byte item;
	public ushort amount;
	public int meta;
	public object metaObj;

	public const int BYTE_LENGTH = 12;
	
	public bool IsPart {
		get {
			if (type == Type.Head ||
				type == Type.LArm ||
				type == Type.RArm ||
				type == Type.Legs)
				return true;
			return false;
		}
	}
	
	public bool IsClothing {
		get {
			if (type == Type.Top ||
				type == Type.Bottom ||
				type == Type.Shoes ||
				type == Type.Coat ||
				type == Type.Eyes ||
				type == Type.Mask ||
				type == Type.Hair)
				return true;
			return false;
		}
	}
	
	public Item(int id, Type type, byte item, ushort amount, int meta) {
		this.id = id;
		this.type = type;
		this.item = item;
		this.amount = amount;
		this.meta = meta;
	}
	
	public Item(ALDNode node) {
		if (node.Contains("id")) this.id = (int)node["id"].Value;
		else this.id = -1;
		this.type = (Type)(byte)node["type"].Value;
		this.item = (byte)node["item"].Value;
		if (node.Contains("amount")) this.amount = (ushort)node["amount"].Value;
		else this.amount = 1;
		if (node.Contains("meta")) {
			if (node["meta"].Value != "") this.meta = (int)node["meta"].Value;
			if (node["meta"].ChildCount > 0) {
				if (type == Type.Medal) metaObj = new Medal(node["meta"]);
			}
		}
	}
	
	public byte[] ToBytes() {
		byte[] bytes = new byte[BYTE_LENGTH];
		Util.SetBytes(BitConverter.GetBytes(id), bytes, 0);
		bytes[4] = (byte)type;
		bytes[5] = item;
		Util.SetBytes(BitConverter.GetBytes(amount), bytes, 6);
		Util.SetBytes(BitConverter.GetBytes(meta), bytes, 8);
		return bytes;
	}
	
	public static Item FromBytes(byte[] bytes) {
		return new Item(BitConverter.ToInt32(bytes, 0), (Type)bytes[4], bytes[5], BitConverter.ToUInt16(bytes, 6), BitConverter.ToInt32(bytes, 8));
	}
	
	private bool requestedMeta;
	private string desc = "";
	
	public string GetDescription() {
		if (desc != "") return desc;
		if (Data.itemNode.Contains(""+type) && Data.itemNode[""+type].Contains(""+item) && Data.itemNode[""+type][""+item].Contains("desc"))
			desc = Data.itemNode[""+type][""+item]["desc"].Value;
		if (IsPart) {
			ALDNode partSet = Data.statNode[item];
			ALDNode part = null;
			if (type == Type.Head) part = partSet["Head"];
			if (type == Type.LArm) part = partSet["LArm"];
			if (type == Type.RArm) part = partSet["RArm"];
			if (type == Type.Legs) part = partSet["Legs"];
			desc = "<b>" + part["name"].Value + "</b>\n";
			desc += "Set: " + partSet.Value + "\n";
			desc += "Model: " + partSet["model"].Value + "\n";
			desc += "Type: " + part["type"].Value + "\n";
			desc += "Attribute: " + part["attr"].Value + "\n";
			desc += "Armor: " + part["armr"].Value + "\n";
			if (part.Contains("powr")) {
				desc += "Power: " + part["powr"].Value + "\n";
				desc += "Control: " + part["ctrl"].Value + "\n";
				desc += "ROS: " + part["succ"].Value + "\n";
				if (part["cdmg"].Value == "true") {
					desc += "<i>Chains Damage</i>\n";
				}
			}
			if (part.Contains("defn")) {
				desc += "Defense: " + part["defn"].Value + "\n";
				desc += "Evasion: " + part["evas"].Value + "\n";
				desc += "Propulsion: " + part["prop"].Value + "\n";
				desc += "Proximity: " + part["prox"].Value + "\n";
				desc += "Remoteness: " + part["remo"].Value + "\n";
			}
			if (part.Contains("uses")) desc += "Uses: " + part["uses"].Value + "\n";
			if (part.Contains("chrg")){
				desc += "Charge: " + part["chrg"].Value + "\n";
				desc += "Radiation: " + part["cool"].Value + "\n";
			}
		}else if (type == Type.Medal && meta != 0) {
			if (Data.medals.ContainsKey(meta)) {
				Medal medal = Data.medals[meta];
				desc = "<b>" + medal.name + "</b>\n";
				if (Data.itemNode.Contains(""+type) && Data.itemNode[""+type].Contains(""+item))
					desc += "Type: " + Data.itemNode[""+type][""+item].Value + "\n";
				desc += "Level: " + medal.level + "\n";
				desc += "XP: " + medal.xp + "\n";
				for (int i = 0; i < 8; i ++)
					desc += "" + (PartType)i + " XP: " + medal.skills[i] + "\n";
			}else if (!requestedMeta) {
				NetClient.use.AskMedal(meta);
				requestedMeta = true;
			}
		}
		if (desc == "") desc = "No Description Available.";
		return desc;
	}
	
	private Texture2D previewImg;
	
	public Texture2D GetPreviewImg() {
		if (previewImg != null) return previewImg;
		if (IsPart) {
			string setPath = "Bots/"+Data.statNode[item].Value + "/";
			string tinpetPath = "Bots/"+(Data.statNode[item]["gender"].Value + " Tinpet") + "/";
			if (type == Type.Legs) previewImg = (Texture2D)Texture2D.Instantiate(Resources.Load(setPath+"legs") ?? Resources.Load(tinpetPath+"legs"));
			else previewImg = (Texture2D)Texture2D.Instantiate(Resources.Load(tinpetPath+"legs"));
			if (type == Type.LArm) previewImg.Layer((Texture2D)Resources.Load(setPath+"larm") ?? (Texture2D)Resources.Load(tinpetPath+"larm"));
			else previewImg.Layer((Texture2D)Resources.Load(tinpetPath+"larm"));
			if (type == Type.Head) previewImg.Layer((Texture2D)Resources.Load(setPath+"head") ?? (Texture2D)Resources.Load(tinpetPath+"head"));
			else previewImg.Layer((Texture2D)Resources.Load(tinpetPath+"head"));
			if (type == Type.RArm) previewImg.Layer((Texture2D)Resources.Load(setPath+"rarm") ?? (Texture2D)Resources.Load(tinpetPath+"rarm"));
			else previewImg.Layer((Texture2D)Resources.Load(tinpetPath+"rarm"));
		}else if (IsClothing) {
			previewImg = ((Texture2D)Resources.Load("Players/Base/base")).Crop(0, 0, 32, 48);
			previewImg.Layer(((Texture2D)Resources.Load("Players/"+type+"/"+Data.itemNode[""+type][item].Value)).Crop(0, 0, 32, 48));
		}
		return previewImg;
	}
	
	public string GetName() {
		if (type == Type.Medal) {
			if (Data.medals.ContainsKey(meta)) {
				return Data.medals[meta].name;
			}else if (!requestedMeta && Game.netState != NetState.Server) {
				NetClient.use.AskMedal(meta);
				requestedMeta = true;
			}
		}
		if (Data.itemNode.Contains(""+type) && Data.itemNode[""+type].Contains(""+item)) {
			if (Data.itemNode[""+type][""+item].Contains("name"))
				return Data.itemNode[""+type][""+item]["name"].Value;
			else
				return Data.itemNode[""+type][""+item].Value;
		}
		return "Item (id="+id + " type=" + type + " item=" + item + " amount=" + amount + " meta= " + meta + ")";
	}
	
	public int GetCost() {
		if (Data.itemNode.Contains(""+type) && Data.itemNode[""+type].Contains(""+item) && Data.itemNode[""+type][""+item].Contains("price"))
			return (int)Data.itemNode[""+type][item]["price"].Value;
		return 0;
	}
	
	public override string ToString () {
		string suffix = "";
		if (Game.mode == GameMode.Inventory) suffix = " x" + amount;
		if (Game.mode == GameMode.Shop) suffix = " " + Util.FormatMoney(GetCost());
		return GetName() + suffix;
	}

	public enum Type : byte {
		Invalid=0,
		Head=1,
		LArm=2,
		RArm=3,
		Legs=4,
		Component=5,
		Medal=6,
		Top=7,
		Bottom=8,
		Shoes=9,
		Coat=10,
		Eyes=11,
		Mask=12,
		Hair=13,
		Key=14,
		Length
	}
	
	public static string[] typeNames = new string[]{
		"Invalid",
		"Heads",
		"Left Arms",
		"Right Arms",
		"Legs",
		"Components",
		"Medals",
		"Tops",
		"Bottoms",
		"Shoes",
		"Coats",
		"Eyewear",
		"Masks",
		"Hairstyles",
		"Key Items"
	};
	
	public enum Error : byte {
		Unknown=0,
		NotEnoughCash=1,
		BadShop=2,
		OutOfStock=3
	}
	
	public static Avatar.Layers LayerFromType(Type type) {
		if (type == Type.Top) return Avatar.Layers.Top;
		if (type == Type.Bottom) return Avatar.Layers.Bottom;
		if (type == Type.Shoes) return Avatar.Layers.Shoes;
		if (type == Type.Coat) return Avatar.Layers.Coat;
		if (type == Type.Eyes) return Avatar.Layers.Eyes;
		if (type == Type.Mask) return Avatar.Layers.Mask;
		if (type == Type.Hair) return Avatar.Layers.Hair;
		return Avatar.Layers.Length;
	}
	
	public static Type TypeFromLayer(Avatar.Layers layer) {
		if (layer == Avatar.Layers.Top) return Type.Top;
		if (layer == Avatar.Layers.Bottom) return Type.Bottom;
		if (layer == Avatar.Layers.Shoes) return Type.Shoes;
		if (layer == Avatar.Layers.Coat) return Type.Coat;
		if (layer == Avatar.Layers.Eyes) return Type.Eyes;
		if (layer == Avatar.Layers.Mask) return Type.Mask;
		if (layer == Avatar.Layers.Hair) return Type.Hair;
		return Type.Invalid;
	}
}

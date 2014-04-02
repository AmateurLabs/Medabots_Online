using UnityEngine;
using System;
using System.Collections.Generic;

public class Medal {
	public TMedal t;
	public int dbId;
	public int playerId;
	public string name; //The nickname of this medal, e.g. Rokusho, Metabee, Brass
	public ushort level; //The level of the part, which increases indefinitely
	public int xp; //The total number of experience points this medal has
	public byte[] skills; //The number of times this medal has used each type of ability
	public int medaforce; //The amount of medaforce charge the bot has
	
	public int XPForNextLevel() { //Returns the amount of xp needed to level up
		return (int)Mathf.Pow((float)level / 25f, (float)level / 250f);
	}
	
	public Medal() {
		
	}
	
	public Medal(ALDNode node) {
		if (node.Contains("id")) this.dbId = (int)node["id"].Value;
		else this.dbId = -1;
		if (node.Contains("playerId")) this.playerId = (int)node["playerId"].Value;
		this.name = (string)node["name"].Value;
		this.t = Data.tMedalList[(int)node["type"].Value];
		this.level = (ushort)node["level"].Value;
		if (node.Contains("xp")) this.xp = (int)node["xp"].Value;
		this.skills = new byte[8];
		if (node.Contains("skills")) {
			for (int i = 0; i < 8; i ++)
				this.skills[i] = (byte)node["skills"][i].Value;
		}
	}
	
	public Medal(TMedal t) {
		this.t = t;
	}
	
	public byte[] ToBytes() {
		byte[] stringBytes = System.Text.Encoding.UTF8.GetBytes(name);
		byte[] bytes = new byte[4+4+4+stringBytes.Length+1+2+4+8];
		int i = 0;
		Util.SetBytes(BitConverter.GetBytes(dbId), bytes, i); i += 4;
		Util.SetBytes(BitConverter.GetBytes(playerId), bytes, i); i += 4;
		Util.SetBytes(BitConverter.GetBytes(stringBytes.Length), bytes, i); i += 4;
		Util.SetBytes(stringBytes, bytes, i); i += stringBytes.Length;
		bytes[i] = (byte)Data.tMedalList.IndexOf(t); i ++;
		Util.SetBytes(BitConverter.GetBytes(level), bytes, i); i += 2;
		Util.SetBytes(BitConverter.GetBytes(xp), bytes, i); i += 4;
		for (int j = 0; j < 8; j ++) {
			bytes[i] = skills[j]; i ++;
		}
		return bytes;
	}
	
	public static Medal FromBytes(byte[] bytes) {
		Medal medal = new Medal();
		int i = 0;
		medal.dbId = BitConverter.ToInt32(bytes, i); i += 4;
		medal.playerId = BitConverter.ToInt32(bytes, i); i += 4;
		int nameLength = BitConverter.ToInt32(bytes, i); i += 4;
		medal.name = System.Text.Encoding.UTF8.GetString(bytes, i, nameLength); i += nameLength;
		medal.t = Data.tMedalList[(int)bytes[i]]; i ++;
		medal.level = BitConverter.ToUInt16(bytes, i); i += 2;
		medal.xp = BitConverter.ToInt32(bytes, i); i += 4;
		medal.skills = new byte[8];
		for (int j = 0; j < 8; j ++) {
			medal.skills[j] = bytes[i]; i ++;
		}
		return medal;
	}
}
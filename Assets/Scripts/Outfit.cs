using UnityEngine;
using System;
using System.Collections;

public class Outfit {
	
	public string name;
	public int playerId;
	public byte baseId;
	public byte topId;
	public byte bottomId;
	public byte shoesId;
	public byte coatId;
	public byte eyesId;
	public byte maskId;
	public byte hairId;
	
	public Outfit() {
		
	}
	
	public byte this[Avatar.Layers index] {
		get {
			switch (index) {
			case Avatar.Layers.Base:
				return baseId;
			case Avatar.Layers.Top:
				return topId;
			case Avatar.Layers.Bottom:
				return bottomId;
			case Avatar.Layers.Shoes:
				return shoesId;
			case Avatar.Layers.Coat:
				return coatId;
			case Avatar.Layers.Eyes:
				return eyesId;
			case Avatar.Layers.Mask:
				return maskId;
			case Avatar.Layers.Hair:
				return hairId;
			}
			return 0;
		}
		set {
			switch(index) {
			case Avatar.Layers.Base:
				baseId = value;
				break;
			case Avatar.Layers.Top:
				topId = value;
				break;
			case Avatar.Layers.Bottom:
				bottomId = value;
				break;
			case Avatar.Layers.Shoes:
				shoesId = value;
				break;
			case Avatar.Layers.Coat:
				coatId = value;
				break;
			case Avatar.Layers.Eyes:
				eyesId = value;
				break;
			case Avatar.Layers.Mask:
				maskId = value;
				break;
			case Avatar.Layers.Hair:
				hairId = value;
				break;
			}
		}
	}
	
	public Outfit(ALDNode node) {
		name = node["name"].Value;
		playerId = (int)node["playerId"].Value;
		baseId = (byte)node["base"].Value;
		topId = (byte)node["top"].Value;
		bottomId = (byte)node["bottom"].Value;
		shoesId = (byte)node["shoes"].Value;
		coatId = (byte)node["coat"].Value;
		eyesId = (byte)node["eyes"].Value;
		maskId = (byte)node["mask"].Value;
		hairId = (byte)node["hair"].Value;
	}
	
	public static Outfit FromBytes(byte[] bytes) {
		Outfit outfit = new Outfit();
		int i = 0;
		int strLength = BitConverter.ToInt32(bytes, i); i += 4;
		outfit.name = System.Text.Encoding.UTF8.GetString(bytes, i, strLength); i += strLength;
		outfit.playerId = BitConverter.ToInt32(bytes, i); i += 4;
		outfit.baseId = bytes[i]; i ++;
		outfit.topId = bytes[i]; i ++;
		outfit.bottomId = bytes[i]; i ++;
		outfit.shoesId = bytes[i]; i ++;
		outfit.coatId = bytes[i]; i ++;
		outfit.eyesId = bytes[i]; i ++;
		outfit.maskId = bytes[i]; i ++;
		outfit.hairId = bytes[i]; i ++;
		return outfit;
	}
	
	public byte[] ToBytes() {
		byte[] stringBytes = System.Text.Encoding.UTF8.GetBytes(name);
		byte[] bytes = new byte[4 + stringBytes.Length + 4 + 8];
		int i = 0;
		Util.SetBytes(BitConverter.GetBytes(stringBytes.Length), bytes, i); i += 4;
		Util.SetBytes(stringBytes, bytes, i); i += stringBytes.Length;
		Util.SetBytes(BitConverter.GetBytes(playerId), bytes, i); i += 4;
		bytes[i] = baseId; i ++;
		bytes[i] = topId; i ++;
		bytes[i] = bottomId; i ++;
		bytes[i] = shoesId; i ++;
		bytes[i] = coatId; i ++;
		bytes[i] = eyesId; i ++;
		bytes[i] = maskId; i ++;
		bytes[i] = hairId; i ++;
		return bytes;
	}
}
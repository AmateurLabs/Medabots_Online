using System;
using UnityEngine;
using System.Collections;

public static class Util {

	/*public static Texture2D GenSprite(int head, int lArm, int rArm, int legs, int tinpet) {
		string tinpetPath = "Bots/"+((tinpet != -1) ? ((tinpet == 1) ? "Female Tinpet" : "Male Tinpet") : "Blank") + "/";
		Texture2D img = new Texture2D(96, 128);
		Texture2D img = (Texture2D)Texture2D.Instantiate(((legs != -1) ? Resources.Load("Bots/"+Data.statNode[legs].Value+"/legs") : null) ?? Resources.Load(tinpetPath+"legs"));
		img.Layer(((lArm != -1) ? (Texture2D)Resources.Load("Bots/"+Data.statNode[lArm].Value+"/larm") : null) ?? (Texture2D)Resources.Load(tinpetPath+"larm"));
		img.Layer(((head != -1) ? (Texture2D)Resources.Load("Bots/"+Data.statNode[head].Value+"/head") : null) ?? (Texture2D)Resources.Load(tinpetPath+"head"));
		img.Layer(((rArm != -1) ? (Texture2D)Resources.Load("Bots/"+Data.statNode[rArm].Value+"/rarm") : null) ?? (Texture2D)Resources.Load(tinpetPath+"rarm"));
		return img;
	}*/
	
	public static void Layer(this Texture2D tex, Texture2D other) {
		Color32[] tColors = tex.GetPixels32();
		Color32[] oColors = other.GetPixels32();
		for (int i = 0; i < oColors.Length; i ++)
			if (oColors[i].a < 127 && tColors[i].a >= 127) oColors[i] = tColors[i];
		tex.SetPixels32(oColors);
		tex.Apply();
	}

	public static void Layer(this Texture2D tex, Texture2D other, int srcX, int srcY, int srcW, int srcH, int dstX, int dstY) {
		Color[] tColors = tex.GetPixels(dstX, dstY, srcW, srcH);
		Color[] oColors = other.GetPixels(srcX, srcY, srcW, srcH);
		for (int i = 0; i < oColors.Length; i ++)
			if (oColors[i].a < 0.5f && tColors[i].a >= 0.5f) oColors[i] = tColors[i];
		tex.SetPixels(dstX, dstY, srcW, srcH, oColors);
		tex.Apply();
	}
	
	public static Texture2D Crop(this Texture2D tex, int x, int y, int width, int height) {
		Texture2D newTex = new Texture2D(width, height, TextureFormat.RGBA32, false);
		Color[] colors = tex.GetPixels(x, y, width, height);
		newTex.SetPixels(colors);
		newTex.Apply();
		return newTex;
	}

	public static double TripTime(this NetworkMessageInfo info) {
		return Network.time - info.timestamp;
	}
	
	public static string FormatMoney(int i) {
		float money = i;
		money /= 100f;
		return string.Format("{0:C}", money);
	}
	
	public static int SetBytes(byte[] bytes, byte[] array, int start) {
		for (int i = 0; i < bytes.Length; i ++) array[i+start] = bytes[i];
		return bytes.Length;
	}

	public static byte[] CopyBytes(byte[] array, int start, int length) {
		byte[] bytes = new byte[length];
		for (int i = 0; i < length; i ++) {
			bytes[i] = array[i+start];
		}
		return bytes;
	}
	
	/*public static void SetBytes(byte[] bytes, byte[] array, ref int startIndex) {
		SetBytes(bytes, array, startIndex);
		startIndex += bytes.Length;
	}*/
	
	public static T ParseEnum<T>(string val) {
		val = val.Replace(" ", "");
		val = val.Replace("_", "");
		val = val.Replace("-", "_");
		return (T)Enum.Parse(typeof(T), val, true);
	}

	public static bool HasFlag(this Enum value, Enum flag) {
		ulong a = Convert.ToUInt64(value);
		ulong b = Convert.ToUInt64(flag);
		return (a & b) == b;
	}
}

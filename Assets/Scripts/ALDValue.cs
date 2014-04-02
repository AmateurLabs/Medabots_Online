using UnityEngine;
using System.Collections;

public struct ALDValue {
	private string _value;
	
	private ALDValue(string val) {
		_value = val;
	}
	
	public override string ToString ()
	{
		return _value;
	}
	
	public static explicit operator byte(ALDValue v) {
		return byte.Parse(v._value);
	}
	
	public static explicit operator ushort(ALDValue v) {
		return ushort.Parse(v._value);
	}
	
	public static explicit operator int(ALDValue v) {
		return int.Parse(v._value);
	}
	
	public static explicit operator float(ALDValue v) {
		return float.Parse(v._value);
	}
	
	public static explicit operator bool(ALDValue v) {
		return bool.Parse(v._value);
	}
	
	public static explicit operator Vector2(ALDValue v) {
		string[] bits = v._value.Split(" ".ToCharArray());
		return new Vector2(float.Parse(bits[0]), float.Parse(bits[1]));
	}
	
	public static explicit operator Vector3(ALDValue v) {
		string[] bits = v._value.Split(" ".ToCharArray());
		Vector3 val = Vector3.zero;
		float.TryParse(bits[0], out val.x);
		float.TryParse(bits[1], out val.y);
		float.TryParse(bits[2], out val.z);
		return val;
	}
	
	public static explicit operator Color(ALDValue v) {
		string[] bits = v._value.Split(" ".ToCharArray());
		return new Color(float.Parse(bits[0]), float.Parse(bits[1]), float.Parse(bits[2]), ((bits.Length > 3) ? float.Parse(bits[3]) : 1f));
	}
	
	public static implicit operator string(ALDValue v) {
		return v._value;
	}
	
	public static implicit operator ALDValue(string v) {
		return new ALDValue(v);
	}
}
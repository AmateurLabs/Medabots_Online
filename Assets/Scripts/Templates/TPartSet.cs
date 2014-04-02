using UnityEngine;
using System.Collections;

[System.Serializable]
public class TPartSet {
	public int id;
	public string name;
	public string designation;
	public bool female;
	public AnimType anim;
	public THead head;
	public TArm lArm;
	public TArm rArm;
	public TLegs legs;
}

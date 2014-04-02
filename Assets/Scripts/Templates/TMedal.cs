using UnityEngine;
using System.Collections;

[System.Serializable]
public class TMedal {
	public string name;
	public string designation; //The short letter designation, e.g. A, B, ?, !
	public PartAttr attr; //The attribute that this medal is compatible with
	public PartType aim; //The type of part that this medal targets
	public int compatibility; //The compatibility bonus this medal grants
	public Ability[] medaforces; //The medaforce abilities this medal gets access to
}
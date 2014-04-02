using UnityEngine;
using System.Collections;

[System.Serializable]
public abstract class TPart {
	public string name;
	public Ability ability;
	public PartType type;
	public PartAttr attr;
	public int armor;
	
	public TPartSet partSet;
}

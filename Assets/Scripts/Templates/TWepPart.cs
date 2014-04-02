using UnityEngine;
using System.Collections;

[System.Serializable]
public abstract class TWepPart : TPart {
	public Control control;
	public int rateOfSuccess;
	public int power;
	public bool chainDamage;
}

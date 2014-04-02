using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Shop : MonoBehaviour {
	public static Shop use;
	public static int id;
	public List<Item> items = new List<Item>();
	
	public void Awake() {
		use = this;
	}
}

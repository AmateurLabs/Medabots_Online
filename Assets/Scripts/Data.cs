using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Data : MonoBehaviour {
	public static Data use;
	public List<string> _maps;
	public TMedal[] _tMedals;
	
	public static List<TMedal> tMedalList;
	public static List<TPartSet> tPartSetList;
	
	public static Dictionary<int, Item> items;
	
	public static Dictionary<int, Medal> medals;

	public static Dictionary<string, Ability> abilities;
	
	public static List<MapData> maps;
	public static List<string> mapList;
	
	public static ALDNode statNode;
	public static ALDNode itemNode;
	public static ALDNode offsetNode;
	
	public static bool isReady;

	public void Awake() {
		use = this;
	}
	
	public IEnumerator Start() {
		if (statNode == null) {
            Debug.Log("Downloading Medastats");
			WWW www = new WWW("http://www.amateurlabs.com/mbo/MedaStats.ald");
			yield return www;
			statNode = ALDNode.ParseString(www.text);
            Debug.Log("Finished downloading Medastats");
		}
		if (itemNode == null) {
            Debug.Log("Downloading Items");
			WWW www = new WWW("http://www.amateurlabs.com/mbo/Items.ald");
			yield return www;
			itemNode = ALDNode.ParseString(www.text);
            Debug.Log("Finished downloading Items");
		}
		Load();
		isReady = true;
	}
		
	public void Load() {
		Ability.LoadAll();
		tPartSetList = new List<TPartSet>();
		foreach (ALDNode node in statNode) {
			TPartSet partSet = new TPartSet();
			partSet.id = int.Parse(node.Name);
			partSet.name = node.Value;
			partSet.designation = node["model"].Value;
			partSet.female = node["gender"].Value == "Female";
			partSet.head = new THead();
			partSet.head.name = node["Head"]["name"].Value;
			partSet.head.type = Util.ParseEnum<PartType>(node["Head"]["type"].Value);
			partSet.head.attr = Util.ParseEnum<PartAttr>(node["Head"]["attr"].Value);
			partSet.head.ability = abilities[node["Head"]["abil"].Value];
			partSet.head.control = Util.ParseEnum<Control>(node["Head"]["ctrl"].Value);
			partSet.head.armor = (int)node["Head"]["armr"].Value;
			partSet.head.rateOfSuccess = (int)node["Head"]["succ"].Value;
			partSet.head.power = (int)node["Head"]["powr"].Value;
			partSet.head.chainDamage = (bool)node["Head"]["cdmg"].Value;
			partSet.head.uses = (int)node["Head"]["uses"].Value;
			partSet.head.partSet = partSet;
			partSet.lArm = new TArm();
			partSet.lArm.name = node["LArm"]["name"].Value;
			partSet.lArm.type = Util.ParseEnum<PartType>(node["LArm"]["type"].Value);
			partSet.lArm.attr = Util.ParseEnum<PartAttr>(node["LArm"]["attr"].Value);
			partSet.lArm.ability = abilities[node["LArm"]["abil"].Value];
			partSet.lArm.control = Util.ParseEnum<Control>(node["LArm"]["ctrl"].Value);
			partSet.lArm.armor = (int)node["LArm"]["armr"].Value;
			partSet.lArm.rateOfSuccess = (int)node["LArm"]["succ"].Value;
			partSet.lArm.power = (int)node["LArm"]["powr"].Value;
			partSet.lArm.chainDamage = (bool)node["LArm"]["cdmg"].Value;
			partSet.lArm.charge = (int)node["LArm"]["chrg"].Value;
			partSet.lArm.radiation = (int)node["LArm"]["cool"].Value;
			partSet.lArm.partSet = partSet;
			partSet.rArm = new TArm();
			partSet.rArm.name = node["RArm"]["name"].Value;
			partSet.rArm.type = Util.ParseEnum<PartType>(node["RArm"]["type"].Value);
			partSet.rArm.attr = Util.ParseEnum<PartAttr>(node["RArm"]["attr"].Value);
			partSet.rArm.ability = abilities[node["RArm"]["abil"].Value];
			partSet.rArm.control = Util.ParseEnum<Control>(node["RArm"]["ctrl"].Value);
			partSet.rArm.armor = (int)node["RArm"]["armr"].Value;
			partSet.rArm.rateOfSuccess = (int)node["RArm"]["succ"].Value;
			partSet.rArm.power = (int)node["RArm"]["powr"].Value;
			partSet.rArm.chainDamage = (bool)node["RArm"]["cdmg"].Value;
			partSet.rArm.charge = (int)node["RArm"]["chrg"].Value;
			partSet.rArm.radiation = (int)node["RArm"]["cool"].Value;
			partSet.rArm.partSet = partSet;
			partSet.legs = new TLegs();
			partSet.legs.name = node["Legs"]["name"].Value;
			partSet.legs.type = Util.ParseEnum<PartType>(node["Legs"]["type"].Value);
			partSet.legs.attr = Util.ParseEnum<PartAttr>(node["Legs"]["attr"].Value);
			partSet.legs.ability = abilities[node["RArm"]["abil"].Value];
			partSet.legs.armor = (int)node["Legs"]["armr"].Value;
			partSet.legs.propulsion = (int)node["Legs"]["prop"].Value;
			partSet.legs.evasion = (int)node["Legs"]["evas"].Value;
			partSet.legs.defense = (int)node["Legs"]["defn"].Value;
			partSet.legs.proximity = (int)node["Legs"]["prox"].Value;
			partSet.legs.remoteness = (int)node["Legs"]["remo"].Value;
			partSet.legs.partSet = partSet;
			partSet.anim = AnimType.Bipedal;
			if (node.Contains("anim")) {
				partSet.anim = (AnimType)(int)node["anim"].Value;
			}
			tPartSetList.Add(partSet);
		}
		
		tMedalList = new List<TMedal>(_tMedals);
		items = new Dictionary<int, Item>();
		medals = new Dictionary<int, Medal>();
		
		mapList = _maps;
		
		maps = new List<MapData>();
		foreach (string mapName in _maps) maps.Add(new MapData(mapName));
	}
}

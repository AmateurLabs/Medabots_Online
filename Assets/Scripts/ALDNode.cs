using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ALDNode {
	public static ALDNode Blank = new ALDNode();
	
	private Dictionary<string, ALDNode> _nodeDict;
	
	public string Name {
		get;
		private set;
	}
	
	public ALDValue Value {
		get;
		set;
	}
	
	public ALDNode Parent {
		get;
		private set;
	}
	
	public int Depth {
		get {
			if (Parent != null)
				return Parent.Depth + 1;
			else
				return 0;
		}
	}
	
	public int ChildCount {
		get {
			return _nodeDict.Count;
		}
	}
	
	public ALDNode() {
		_nodeDict = new Dictionary<string, ALDNode>();
	}
	
	public ALDNode(string input) {
		_nodeDict = new Dictionary<string, ALDNode>();
		Deserialize(input);
	}
	
	public ALDNode(string name, string val) {
		_nodeDict = new Dictionary<string, ALDNode>();
		Name = name;
		Value = val;
	}
	
	public void AddNode(ALDNode node) {
		node.Parent = this;
		_nodeDict[node.Name] = node;
	}
	
	public bool Contains(ALDNode node) {
		return _nodeDict.ContainsValue(node);
	}
	
	public bool Contains(string name) {
		return _nodeDict.ContainsKey(name);
	}
	
	public ALDNode this [int index] {
		get {
			if (!_nodeDict.ContainsKey(""+index)) return null;
			return _nodeDict[""+index];
		}
		set {
			throw new System.NotSupportedException();
		}
	}
	
	public ALDNode this [string index] {
		get {
			if (!_nodeDict.ContainsKey(index)) return null;
			return _nodeDict[index];
		}
		set {
			throw new System.NotSupportedException();
		}
	}
	
	public IEnumerator<ALDNode> GetEnumerator() {
		return _nodeDict.Values.GetEnumerator();
	}
	
	public override string ToString ()
	{
		return "{" + Name + ((Value != "") ? "=" + Value : "") + "}";
	}
	
	public string Serialize() {
		string output = "";
		foreach (ALDNode node in _nodeDict.Values) {
			output += new string('\t', Depth);
			if (node.Value == "")
				output += node.Name;
			else
				output += node.Name + "=" + node.Value;
			output += "\n";
			if (node.ChildCount > 0)
				output += node.Serialize();
		}
		return output;
	}
	
	public void Deserialize(string input) {
		input = input.Replace("\t", "");
		if (input.Contains("=")) {
			string[] bits = input.Split("=".ToCharArray());
			Name = bits[0];
			Value = bits[1];
		}else{
			Name = input;
			Value = "";
		}
	}
	
	public static ALDNode ParseFile(string path) {
		ALDNode root = new ALDNode();
		string[] lines = System.IO.File.ReadAllLines(path);
		root.ParseLines(lines, 0);
		return root;
	}
	
	public static ALDNode ParseString(string input) {
		ALDNode root = new ALDNode();
		input = input.Replace("\r\n", "\n");
		string[] lines = input.Split('\n');
		root.ParseLines(lines, 0);
		return root;
	}
	
	public int ParseLines(string[] lines, int i) {
		ALDNode n = null;
		while (i < lines.Length) {
			if (lines[i] == ""){
				i ++;
				continue;
			}
			lines[i] = lines[i].Replace("    ", "\t");
			int lineDepth = StringDepth(lines[i]);
			if (lineDepth == Depth) {
				n = new ALDNode(lines[i]);
				AddNode(n);
			}else if (lineDepth > Depth) {
				if (n != null)
					i = n.ParseLines(lines, i) - 1;
			}else{
				break;
			}
			i ++;
		}
		return i;
	}
	
	private static int StringDepth(string line) {
		int depth = 0;
		foreach (char c in line) {
			if (c != '\t') break;
			depth ++;
		}
		return depth;
	}
}
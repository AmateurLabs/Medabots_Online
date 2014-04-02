using UnityEngine;
using System.Collections.Generic;

public class MapData {
	public string name;
	public bool[,] walls;
	public int width;
	public int height;
	public List<Warp> warps;
	public List<ShopPoint> shops;
	public ALDNode raw;
	
	public MapData(string name) {
		this.name = name;
		Texture2D wallImg = (Texture2D)Resources.Load("Maps/" + name + "_wall");		
		TextAsset data = (TextAsset)Resources.Load("Maps/" + name);
		
		ALDNode node = ALDNode.ParseString(data.text);
		width = (int)node["width"].Value;
		height = (int)node["height"].Value;
		//width = wallImg.width;
		//height = wallImg.height;
		
		warps = new List<Warp>();
		if (node.Contains("warps"))
			foreach (ALDNode warp in node["warps"])
				warps.Add(new Warp(new Point((int)warp["x"].Value, (int)warp["y"].Value), Data.mapList.IndexOf(warp["tMap"].Value), new Point((int)warp["tX"].Value, (int)warp["tY"].Value)));
		
		shops = new List<ShopPoint>();
		if (node.Contains("shops"))
			foreach (ALDNode shop in node["shops"])
				shops.Add(new ShopPoint(shop["name"].Value, (int)shop["id"].Value, new Point((int)shop["x"].Value, (int)shop["y"].Value)));
		
		walls = new bool[width, height];
		for (int x = 0; x < width; x ++) {
			for (int y = 0; y < height; y ++) {
				if (wallImg.GetPixel(x, wallImg.height - y).grayscale == 0f)
					walls[x,y] = true;
				else
					walls[x,y] = false;
			}
		}
		raw = node;
	}
	
	public bool CanMove(Point p) {
		if (p.x < 0 || p.x >= width || p.y < 0 || p.y >= height) return false;
		return !walls[p.x, p.y];
	}
}

public class Warp {
	public Point loc;
	public int map;
	public Point tLoc;
	
	public Warp(Point loc, int map, Point tLoc) {
		this.loc = loc;
		this.map = map;
		this.tLoc = tLoc;
	}
	
	public override string ToString ()
	{
		return "Warp from " + loc + " to " + Data.mapList[map] + ": " + tLoc;
	}
}

public class ShopPoint {
	public string name;
	public int id;
	public int map;
	public Point loc;
	
	public ShopPoint(string name, int id, Point loc) {
		this.name = name;
		this.id = id;
		this.loc = loc;
	}
}
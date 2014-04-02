using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Map : MonoBehaviour {
	public static Map use;
	public static int current;
	public Renderer baseRenderer;
	public Renderer fringeRenderer;
	public Renderer wallRenderer;
	
	void Awake() {
		use = this;
	}
	
	public static void Load (int id) {
		current = id;
		string name = Data.maps[id].name;
		Texture2D baseImg = (Texture2D)Resources.Load("Maps/" + name + "_base");
		Texture2D fringeImg = (Texture2D)Resources.Load("Maps/" + name + "_fringe");
		Texture2D wallImg = (Texture2D)Resources.Load("Maps/" + name + "_wall");
		use.transform.localScale = new Vector3(baseImg.width, baseImg.height, 1f);

		use.baseRenderer.material.mainTexture = baseImg;
		use.fringeRenderer.material.mainTexture = fringeImg;
		use.wallRenderer.material.mainTexture = wallImg;
	}

	public static Point PointFromWorld(Vector3 v) {
		v.y -= 32;
		return new Point(Mathf.FloorToInt(v.x / 32f), Mathf.FloorToInt(-v.y / 32f));
	}

	public static List<Point> GetPath(Point start, Point goal) {
		int width = Data.maps[current].width;
		int height = Data.maps[current].height;
		int[,] costs = new int[width, height];
		Point[,] parents = new Point[width, height];
		for (int x = 0; x < width; x ++) {
			for (int y = 0; y < height; y++) {
				parents[x, y] = Point.Max;
				if (Data.maps[current].CanMove(new Point(x, y))) costs[x, y] = int.MaxValue;
			}
		}

		costs[start.x, start.y] = 0;
		Point end = goal;
		List<Point> open = new List<Point>();
		open.Add(start);
		while (open.Count > 0) {
			List<Point> newOpen = new List<Point>();
			foreach (Point p in open) {
				if (p == goal) {
					end = goal;
					break;
				}
				int pCost = costs[p.x, p.y] + 1;
				foreach (Point dir in Point.GetDirs()) {
					Point move = p + dir;
					if (move.x < 0 || move.x >= width || move.y < 0 || move.y >= height) continue;
					if (costs[move.x, move.y] > pCost) {
						parents[move.x, move.y] = p;
						costs[move.x, move.y] = pCost;
						newOpen.Add(move);
					}
				}
				if (Point.Distance(p, goal) < Point.Distance(end, goal)) end = p;
			}
			open = newOpen;
		}
		List<Point> path = new List<Point>();
		Point pp = end;
		if (pp.x < 0 || pp.x >= width || pp.y < 0 || pp.y >= height) return path;
		while (pp != start && parents[pp.x, pp.y] != Point.Max) {
			path.Add(pp);
			pp = parents[pp.x, pp.y];
		}
		path.Reverse();
		return path;
	}
}
using UnityEngine;
using System.Collections;

public struct Point {
	public int x, y;
	
	public Point (int x, int y) {
		this.x = x;
		this.y = y;
	}

	public static Point Max = new Point(int.MaxValue, int.MaxValue);
	public static Point Zero = new Point();
	
	static Point[] dirs = new Point[] {
		new Point(0, 1),
		new Point(0, -1),
		new Point(-1, 0),
		new Point(1, 0)
	};

	public static Point FromDir(Dir dir) {
		switch (dir) {	
		case Dir.Down:
			return dirs[0];
		case Dir.Up:
			return dirs[1];
		case Dir.Left:
				return dirs[2];
		case Dir.Right:
			return dirs[3];
		default:
			return Point.Zero;
		}
	}

	public static Point[] GetDirs() {
		return dirs;
	}

	public static int Distance(Point a, Point b) {
		return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
	}
	
	public override bool Equals (object obj)
	{
		if (obj == null) return false;
		Point p = (Point)obj;
		return p == this;
	}
	
	public override int GetHashCode ()
	{
		return (y << 16) ^ x;
	}
	
	public override string ToString ()
	{
		return string.Format("({0},{1})", x, y);
	}
	
	public static explicit operator Vector2(Point p) {
		return new Vector2(p.x, -p.y);
	}
	
	public static explicit operator Vector3(Point p) {
		return new Vector3(p.x, -p.y, 0f);
	}
	
	public static explicit operator Point(Vector2 p) {
		return new Point(Mathf.RoundToInt(p.x), Mathf.RoundToInt(p.y));
	}
	
	public static explicit operator Point(Vector3 p) {
		return new Point(Mathf.RoundToInt(p.x), Mathf.RoundToInt(p.y));
	}
	
	public Dir dir {
		get {
			if (x >= 1) return Dir.Right;
			if (x <= -1) return Dir.Left;
			if (y >= 1) return Dir.Down;
			if (y <= -1) return Dir.Up;
			return Dir.None;
		}
	}
	
	public static Point operator +(Point a, Point b) {
		return new Point(a.x + b.x, a.y + b.y);
	}
	
	public static Point operator -(Point a, Point b) {
		return new Point(a.x - b.x, a.y - b.y);
	}
	
	public static bool operator ==(Point a, Point b) {
		return (a.x == b.x && a.y == b.y);
	}
	
	public static bool operator !=(Point a, Point b) {
		return !(a == b);
	}
}

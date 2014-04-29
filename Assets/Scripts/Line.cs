using UnityEngine;
using System.Collections;

public class Line : MonoBehaviour {

    public Vector2 texScale;
    public Vector2 texSpeed;
    public LineRenderer line;

	void Awake () {
        line = gameObject.GetComponent<LineRenderer>();
	}

    public static Line Create() {
        GameObject newLine = (GameObject)Instantiate(Resources.Load<GameObject>("Prefabs/Line"));
        return newLine.GetComponent<Line>();
    }

    public void Set(Vector3 p0, Vector3 p1, Color color) {
        line.SetPosition(0, p0);
        line.SetPosition(1, p1);
        line.SetColors(color, color);
        line.material.mainTextureScale = new Vector2(texScale.x * (p0 - p1).magnitude, texScale.y);
    }
	
	// Update is called once per frame
	void Update () {
        line.material.mainTextureOffset += texSpeed * Time.deltaTime;
	}
}

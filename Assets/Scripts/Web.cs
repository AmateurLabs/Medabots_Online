using UnityEngine;
using System.Collections;

public class Web : MonoBehaviour {
	public static Web use;
	
	public delegate void WebCallback(ALDNode result);
	
	void Awake() {
		use = this;
	}
	
	public static void Call(string url, WebCallback callback) {
		use.StartCoroutine(DoWWW(new WWW(url), callback));
	}
	
	public static void Call(string url, ALDNode data, WebCallback callback) {
		WWWForm form = new WWWForm();
		foreach (ALDNode node in data) {
			form.AddField(node.Name, node.Value);
		}
		use.StartCoroutine(DoWWW(new WWW(url, form), callback));
	}
	
	private static IEnumerator DoWWW(WWW www, WebCallback callback) {
		www.threadPriority = ThreadPriority.High;
		float startTime = Time.time;
		Debug.Log("Requested " + www.url);
		yield return www;
		//Debug.Log("Raw: " + www.text);
		Debug.Log("Recieved " + www.url + " ("+(Time.time-startTime)+" secs)");
		if (www.error != null && www.error != "")
			Debug.Log("WWW Error: " + www.error);
		if (callback != null)
			callback(ALDNode.ParseString(www.text));
	}
}
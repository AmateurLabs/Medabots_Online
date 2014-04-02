using UnityEngine;
using System.Collections;

public class Boot : MonoBehaviour {
	public static bool firstRun = true;
	public bool isServer;
	public bool go = false;
	public bool loading = false;

    void Awake()
    {
        Application.RegisterLogCallback((string msg, string stack, LogType type) => {
            Application.ExternalEval("if (console) { console.log(\"" + type + "|" + msg + "|" + stack + "\")}");
        });
    }
	
	void Update() {
		if (!Data.isReady) return;
		if (isServer) {
			loading = true;
			if (Application.CanStreamedLevelBeLoaded(2))
				Application.LoadLevel(2);
		}else{
			if (Application.CanStreamedLevelBeLoaded(1) && (go || firstRun)){
				loading = true;
				firstRun = false;
				Application.LoadLevel(1);
			}
		}
	}
	
	void OnGUI() {
		if (Application.CanStreamedLevelBeLoaded(1) && !isServer && !firstRun && !loading){
			GUILayout.Label("Lost connection to server.");
			if (GUILayout.Button("Connect"))
				go = true;
		}else if (loading) {
			GUILayout.Label("Loading game...");
		}else{
			GUILayout.Label("Loading assets...");
		}
	}
}

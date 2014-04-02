using UnityEngine;
using System.Collections;

public class PersistantObject : MonoBehaviour {
	void Awake() {
		DontDestroyOnLoad(gameObject);
	}
}

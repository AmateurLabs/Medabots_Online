using UnityEngine;
using System.Collections.Generic;

public class Chat : MonoBehaviour {
	public static Chat use;
	public Rect window;
	public List<ChatMsg> output = new List<ChatMsg>();
	Vector2 scroll;
	public string chatMsg;

	public Channels channels = Channels.All;
	
	void Awake() {
		use = this;
	}

	void OnGUI() {
		if (Game.netState != NetState.LoggedIn) return;
		GUI.depth = 50;
		XGUI.Init();
		if (Event.current.isKey && Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return){
			if (chatMsg == "") {
				if (GUI.GetNameOfFocusedControl() == "ChatField"){
					GUIUtility.hotControl = 0;
					GUIUtility.keyboardControl = 0;
				}else{
					GUI.FocusControl("ChatField");
				}
			}else{
				NetClient.use.AskChat(chatMsg);
				chatMsg = "";
			}
		}
		GUILayout.BeginArea(window, "", "box");
		GUILayout.BeginVertical();
		scroll = GUILayout.BeginScrollView(scroll);
		foreach (ChatMsg msg in output) {
			if (((byte)channels & (byte)msg.channels) > 0)
				GUILayout.Label("[" + msg.sender + "] " + msg.text);
		}
		GUILayout.EndScrollView();
		GUI.SetNextControlName("ChatField");
		chatMsg = GUILayout.TextField(""+chatMsg);
		Channels cs = Channels.None;
		GUILayout.BeginHorizontal();
		cs = ChannelToggle(cs, Channels.Private);
		cs = ChannelToggle(cs, Channels.Trade);
		cs = ChannelToggle(cs, Channels.Battle);
		cs = ChannelToggle(cs, Channels.Team);
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		cs = ChannelToggle(cs, Channels.Map);
		cs = ChannelToggle(cs, Channels.World);
		cs = ChannelToggle(cs, Channels.Game);
		cs = ChannelToggle(cs, Channels.System);
		GUILayout.EndHorizontal();
		channels = cs;
		GUILayout.EndVertical();
		GUILayout.EndArea();
	}

	public Channels ChannelToggle(Channels cs, Channels c) {
		if (GUILayout.Toggle((channels & c) == c, ""+c)) cs |= c;
		return cs;
	}

	public void AddMsg(ChatMsg msg) {
		output.Add(msg);
		scroll.y = Mathf.Infinity;
	}
}

public struct ChatMsg {
	public string sender;
	public Channels channels;
	public string text;
	
	public ChatMsg(string sender, Channels channels, string text) {
		this.sender = sender;
		this.channels = channels;
		this.text = text;
	}
}
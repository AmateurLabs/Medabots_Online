using UnityEngine;
using System.Collections;

public enum BotState : byte {
	Standby,
	Charging,
	Attacking,
	Cooling,
	Dead
}

public enum StatusEffect : byte
{
    None,
    AutoRecover,
    Bind,
    CantDefend,
    CantEvade,
    Chaos,
    Counter,
    Defend,
    Flow,
    FullBlock,
    HalfBlock,
    Movement,
    Stop,
    Time,
    Useless
}

[System.Flags]
public enum BattleFlags : int {
	None=0,
	BotTop=1,
	BotMiddle=2,
	BotBottom=4,
	WagerNormal=8,
	WagerSpecial=16,
	AttackerReady=32,
	DefenderReady=64,
	WinTypeLeader=128,
	WinTypeTime=256,
	EnableCmdTimer=512,
	SetupComplete=1024
}

[System.Flags]
public enum Channels : byte {
	None = 0,
	Game = 1,
	Private = 2,
	Trade = 4,
	Map = 8,
	World = 16,
	System = 32,
	Battle = 64,
	Team = 128,
	All = 255
}

public enum UserLevel {
	Banned=0,
	Unactivated=1,
	User=2,
	NPC=3,
	Moderator=4,
	Developer=5,
	Admin=6
}

public enum Dir {
	None,
	Down,
	Up,
	Left,
	Right
}

public enum Control {
	None = -1,
	Proximity,
	Remoteness
}

public enum PartIndex {
	None = -1,
	Head = 0,
	LArm = 1,
	RArm = 2,
	Legs = 3,
}

public enum PartType {
	None = -1,
	Strike = 0,
	Berserk = 1,
	Defend = 2,
	Heal = 3,
	Shoot = 4,
	AimShot = 5,
	Support = 6,
	Interrupt = 7,
	Air = 8,
	Float = 9,
	Multi_Legged = 10,
	Two_Legged = 11,
	Wheels = 12,
	Tank = 13,
	Sea = 14
}

public enum PartAttr {
	None = -1,
	Grapple = 0,
	Shoot = 1,
	Optic = 2,
	Bomb = 3,
	Gravity = 4,
	Formation = 5,
	Movement = 6,
	Stop = 7,
	Bind = 8,
	Flow = 9,
	Release = 10,
	Cancel = 11,
	Defense = 12,
	Heal = 13,
	Revive = 14,
	Anti_Air = 15,
	Anti_Sea = 16,
	Scout = 17,
	Conceal = 18,
	Time = 19,
	Interrupt = 20,
	Destroy = 21,
	Regenerate = 22,
	Teamwork = 23,
	Counterattack = 24,
	Transform = 25
}

public enum AnimType {
	Bipedal = 0,
	Quadrupedal = 1,
	Tail = 2
}

public enum NetState {
	Disconnected,
	Connected,
	LoggedIn,
	Server
}

public enum GameMode {
	Login,
	Explore,
	Battle,
	Inventory,
	Shop,
	Medawatch
}
using UnityEngine;
using System.Collections;

public class AnimBot : MonoBehaviour {
	
	public static AnimBot use;
	
	public AnimType animType;
	public Texture2D tinpetTex;
	public Texture2D headTex;
	public Texture2D lArmTex;
	public Texture2D rArmTex;
	public Texture2D legsTex;
	public bool female;
	public Bot bot;
	public bool flipped;
	public UVScroll backdropScroll;
	public GameObject torso;
	public GameObject head;
	public GameObject leftArm;
	public GameObject rightArm;
	public GameObject leftHand;
	public GameObject rightHand;
	public GameObject hips;
	public GameObject leftLeg;
	public GameObject rightLeg;
	public GameObject leftFoot;
	public GameObject rightFoot;
	public GameObject leftFrontLeg;
	public GameObject rightFrontLeg;
	public GameObject leftBackLeg;
	public GameObject rightBackLeg;
	public GameObject upperTail;
	public GameObject middleTail;
	public GameObject lowerTail;
	
	public void Awake() {
		use = this;
		animation["Walk"].speed = 2f;
		Refresh();
	}
	
	public void Refresh() {
		if (bot != null) {
			animType = bot.legs.t.partSet.anim;
			female = bot.head.t.partSet.female;
			headTex = (bot.head.armor > 0) ? (Texture2D)Resources.Load("Bots/"+bot.head.t.partSet.name) : null;
			lArmTex = (bot.lArm.armor > 0) ? (Texture2D)Resources.Load("Bots/"+bot.lArm.t.partSet.name) : null;
			rArmTex = (bot.rArm.armor > 0) ? (Texture2D)Resources.Load("Bots/"+bot.rArm.t.partSet.name) : null;
			legsTex = (bot.legs.armor > 0) ? (Texture2D)Resources.Load("Bots/"+bot.legs.t.partSet.name) : null;
		}
		
		tinpetTex = (Texture2D)Resources.Load("Bots/"+((female) ? "Female Tinpet" : "Male Tinpet"));
		Texture2D headImg = (headTex == null) ? tinpetTex : headTex;
		Texture2D lArmImg = (lArmTex == null) ? tinpetTex : lArmTex;
		Texture2D rArmImg = (rArmTex == null) ? tinpetTex : rArmTex;
		Texture2D legsImg = (legsTex == null) ? tinpetTex : legsTex;

		if (legsImg == tinpetTex) {
			animType = AnimType.Bipedal;
		}
		
		head.renderer.material.mainTexture = headImg;
		torso.renderer.material.mainTexture = headImg;
		leftArm.renderer.material.mainTexture = lArmImg;
		leftHand.renderer.material.mainTexture = lArmImg;
		rightArm.renderer.material.mainTexture = rArmImg;
		rightHand.renderer.material.mainTexture = rArmImg;
		hips.renderer.material.mainTexture = legsImg;
		leftLeg.renderer.material.mainTexture = legsImg;
		leftFoot.renderer.material.mainTexture = legsImg;
		rightLeg.renderer.material.mainTexture = legsImg;
		rightFoot.renderer.material.mainTexture = legsImg;
		leftFrontLeg.renderer.material.mainTexture = legsImg;
		rightFrontLeg.renderer.material.mainTexture = legsImg;
		leftBackLeg.renderer.material.mainTexture = legsImg;
		rightBackLeg.renderer.material.mainTexture = legsImg;
		upperTail.renderer.material.mainTexture = legsImg;
		middleTail.renderer.material.mainTexture = legsImg;
		lowerTail.renderer.material.mainTexture = legsImg;
		
		if (flipped) {
			transform.eulerAngles = Vector3.zero;
			transform.localScale = new Vector3(1f, 1f, -0.1f);
			leftHand.renderer.material.mainTextureOffset = new Vector2(0.75f, 0.5f);
			rightHand.renderer.material.mainTextureOffset = new Vector2(0.25f, 0.5f);
			//backdropScroll.speed = Vector2.right * -1.5f;
		}else{
			transform.eulerAngles = Vector3.up * 180f;
			transform.localScale = new Vector3(1f, 1f, 0.1f);
			leftHand.renderer.material.mainTextureOffset = new Vector2(0.5f, 0.5f);
			rightHand.renderer.material.mainTextureOffset = new Vector2(0.0f, 0.5f);
			//backdropScroll.speed = Vector2.right * 1.5f;
		}
		
		leftLeg.gameObject.SetActive(animType == AnimType.Bipedal);
		rightLeg.gameObject.SetActive(animType == AnimType.Bipedal);
		leftFoot.gameObject.SetActive(animType == AnimType.Bipedal);
		rightFoot.gameObject.SetActive(animType == AnimType.Bipedal);
		leftFrontLeg.gameObject.SetActive(animType == AnimType.Quadrupedal);
		rightFrontLeg.gameObject.SetActive(animType == AnimType.Quadrupedal);
		leftBackLeg.gameObject.SetActive(animType == AnimType.Quadrupedal);
		rightBackLeg.gameObject.SetActive(animType == AnimType.Quadrupedal);
		upperTail.gameObject.SetActive(animType == AnimType.Tail);
		middleTail.gameObject.SetActive(animType == AnimType.Tail);
		lowerTail.gameObject.SetActive(animType == AnimType.Tail);

		head.transform.localPosition = new Vector3(4f, 8f, 1f);
		torso.transform.localPosition = new Vector3(0f, 8f, 0f);
		leftArm.transform.localPosition = new Vector3(4f, -8f, -5f);
		leftHand.transform.localPosition = new Vector3(-4f, -8f, -4f);
		rightArm.transform.localPosition = new Vector3(4f, -8f, 2f);
		rightHand.transform.localPosition = new Vector3(4f, -8f, 3f);
		hips.transform.localPosition = new Vector3(0f, -16f, -1f);
		leftLeg.transform.localPosition = new Vector3(0f, -4f, -3f);
		leftFoot.transform.localPosition = new Vector3(0f, -16f, -2f);
		rightLeg.transform.localPosition = new Vector3(-4f, -4f, -2f);
		rightFoot.transform.localPosition = new Vector3(-4f, -16f, -1f);
		leftFrontLeg.transform.localPosition = new Vector3(0f, -8f, -3f);
		rightFrontLeg.transform.localPosition = new Vector3(-4f, -8f, 0f);
		leftBackLeg.transform.localPosition = new Vector3(-4f, -4f, -4f);
		rightBackLeg.transform.localPosition = new Vector3(0f, -4f, -1f);
		upperTail.transform.localPosition = new Vector3(4f, -4f, -3f);
		middleTail.transform.localPosition = new Vector3(4f, 4f, -4f);
		lowerTail.transform.localPosition = new Vector3(4f, 4f, -5f);

		ALDNode d = Paperdoll.offsetData;
		if (headTex != null && d.Contains(headTex.name)) {
			if (d[headTex.name].Contains("Head")) head.transform.localPosition = (Vector3)d[headTex.name]["Head"].Value;
			if (d[headTex.name].Contains("Torso")) torso.transform.localPosition = (Vector3)d[headTex.name]["Torso"].Value;
		}
		if (lArmTex != null && d.Contains(lArmTex.name)) {
			if (d[lArmTex.name].Contains("Left Arm")) leftArm.transform.localPosition = (Vector3)d[lArmTex.name]["Left Arm"].Value;
		}
	}
}
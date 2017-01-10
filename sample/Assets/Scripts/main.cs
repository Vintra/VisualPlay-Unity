using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class main : MonoBehaviour {
	
	// Use this for initialization
	public string appID;
	void Start () {
		VisualPlay.Challenges.Instance.Configure (appID);
		Debug.Log ("Starting VisualPlay");
	}

	// Callbacks definitions
	void OnEnable() {
		VisualPlay.Challenges.Instance.OnInit += VisualPlayInstanceOnInit;
		VisualPlay.Challenges.Instance.OnAvailableChallenges += VisualPlayInstanceOnAvailableChallenges;
		VisualPlay.Challenges.Instance.OnGetChallengesFailed += VisualPlayInstanceOnGetChallengesFailed;
		VisualPlay.Challenges.Instance.OnStartChallenge += VisualPlayInstanceOnStartChallenge;
		VisualPlay.Challenges.Instance.OnSolution += VisualPlayInstanceOnSolution;
		VisualPlay.Challenges.Instance.OnCancel += VisualPlayInstanceOnCancel;
	}
	void OnDisable() {
		VisualPlay.Challenges.Instance.OnInit -= VisualPlayInstanceOnInit;
		VisualPlay.Challenges.Instance.OnAvailableChallenges -= VisualPlayInstanceOnAvailableChallenges;
		VisualPlay.Challenges.Instance.OnGetChallengesFailed -= VisualPlayInstanceOnGetChallengesFailed;
		VisualPlay.Challenges.Instance.OnStartChallenge -= VisualPlayInstanceOnStartChallenge;
		VisualPlay.Challenges.Instance.OnSolution -= VisualPlayInstanceOnSolution;
		VisualPlay.Challenges.Instance.OnCancel -= VisualPlayInstanceOnCancel;
	}

	void VisualPlayInstanceOnInit(VPCINIT isInit){
		switch (isInit) {
		case VPCINIT.Success:
			Debug.Log ("VPCHALLENGEs init ok");
			textMessage = "INIT SUCCESS";
			showStart = true;
			break;
		case VPCINIT.Failed:
			Debug.Log ("VPCHALLENGEs init fail");
			textMessage = "INIT FAIL";
			showStart = false;
			break;
		}
	}
	void VisualPlayInstanceOnSolution (VPCHALLENGE ok) {
		switch (ok) {
		case VPCHALLENGE.Correct:
			Debug.Log ("CHALLENGE ACCEPTED -> REWARD!");
			textMessage = "CHALLENGE ACCEPTED -> REWARD!";
			break;
		case VPCHALLENGE.Incorrect:
			Debug.Log ("CHALLENGE FAILED -> NO REWARDS!");
			textMessage = "CHALLENGE FAILED -> NO REWARDS!";
			break;
		}
	}
	void VisualPlayInstanceOnAvailableChallenges(VPCHALLENGE isAvailable){
		Debug.Log ("CHALLENGES AVAILABLE? -> " + isAvailable);
		textMessage = "AVAILABLE: " + isAvailable;
	}
	void VisualPlayInstanceOnCancel(){
		Debug.Log ("USER CANCEL THE CHALLENGE");
		textMessage = "USER CANCEL THE CHALLENGE";
	}
	void VisualPlayInstanceOnGetChallengesFailed(){
		Debug.Log ("CHALLENGE FAILED WHILE OPENING");
		textMessage = "FAILED WHILE OPENING";
	}
	void VisualPlayInstanceOnStartChallenge(){
		Debug.Log ("CHALLENGE IS READY TO SOLVE");
		textMessage = "CHALLENGE IS READY TO SOLVE";
	}



	// GUI definitions
	public bool showStart = false;
	public string textMessage = "VisualPlay Challenge";
	void OnGUI() {
		var w = Screen.width;	
		var h = Screen.height;
		int wbutton = (int) (w * 0.33);
		int hbutton = (int) (h * 0.2);
		int wbutton2 = (int) (w * 0.5);
		int fsize = (int) (w * 0.07);

		GUIStyle style = new GUIStyle ();
		style.richText = true;
		GUI.Label (new Rect (0, (h - (2 * hbutton)), w, hbutton), "<size="+fsize+"><color=white>"+textMessage+"</color></size>", style);

		if (showStart) {
			if (GUI.Button (new Rect (0, (h-hbutton), wbutton2, hbutton), "OPEN CHALLENGE")) {
				VisualPlay.Challenges.Instance.ShowChallenge ();
			}
			if (GUI.Button (new Rect (wbutton2, (h-hbutton), wbutton2, hbutton), "CHALLENGES AVAILABLE?")) {
				VisualPlay.Challenges.Instance.AvailableChallenges ();
				Debug.Log ("Run AvailableChallenges, waiting for callback ");
			}
		}

		if (GUI.Button (new Rect (0, 0, wbutton, hbutton), "AR")) {
			VisualPlay.Challenges.Instance.Configure (appID)
				.SetLanguage("ar");
			showStart = false;
			textMessage = "INITIALIZING . . .";
		}
		if (GUI.Button (new Rect ((int) (w*0.33), 0, wbutton,  hbutton), "ES")) {
			VisualPlay.Challenges.Instance.Configure (appID)
				.SetLanguage("es");
			showStart = false;
			textMessage = "INITIALIZING . . .";
		}
		if (GUI.Button (new Rect ((int) (w*0.66), 0, wbutton,  hbutton), "EN")) {
			VisualPlay.Challenges.Instance.Configure (appID)
				.SetLanguage("en");
			showStart = false;
			textMessage = "INITIALIZING . . .";
		}
		if (GUI.Button (new Rect (0, hbutton, wbutton,  hbutton), "JP")) {
			VisualPlay.Challenges.Instance.Configure (appID)
				.SetLanguage("jp");
			showStart = false;
			textMessage = "INITIALIZING . . .";
		}
		if (GUI.Button (new Rect ((int) (w*0.33), hbutton, wbutton, hbutton), "CA")) {
			VisualPlay.Challenges.Instance.Configure (appID)
				.SetLanguage("ca");
			showStart = false;
			textMessage = "INITIALIZING . . .";
		}
	}
}

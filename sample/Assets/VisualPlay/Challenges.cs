using UnityEngine;
using System.Collections;
using System.Collections.Specialized;

public enum VPCHALLENGE { Correct, Incorrect, Available, NotAvailable }
public enum VPCINIT { Success, Failed }

namespace VisualPlay {
	public class Challenges : MonoBehaviour {
		
		#region Singleton
		private static Challenges _instance;
		void Awake() {
			if (_instance == null) {
				_instance = this;
				DontDestroyOnLoad (this.gameObject);
			} else {
				Destroy (this.gameObject);
			}
		}

		public static Challenges Instance {
			get {
				if (_instance == null)
					new GameObject ("VisualPlayChallenges", new System.Type[]{ typeof(Challenges) });
				return _instance;
			}
			set {
				_instance = value;
			}
		}
		#endregion
		private WebViewObject webViewObject;
		private float userTimeScale;
		private ScreenOrientation userLastScreenOrientation;
		private string userScreenOrientation;
		const string apiBase = "http://webview.visualplay.io";
		private bool isInitialitzed = false;
		public bool IsInitialitzed {
			get {
				return isInitialitzed;
			}
		}

		public class VisualPlayOptions {
			private static string Get2LetterISOCodeFromSystemLanguage() {
				SystemLanguage lang = Application.systemLanguage;
				string res = "EN";
				switch (lang) {
					case SystemLanguage.Afrikaans: res = "AF"; break;
					case SystemLanguage.Arabic: res = "AR"; break;
					case SystemLanguage.Basque: res = "EU"; break;
					case SystemLanguage.Belarusian: res = "BY"; break;
					case SystemLanguage.Bulgarian: res = "BG"; break;
					case SystemLanguage.Catalan: res = "CA"; break;
					case SystemLanguage.Chinese: res = "ZH"; break;
					case SystemLanguage.Czech: res = "CS"; break;
					case SystemLanguage.Danish: res = "DA"; break;
					case SystemLanguage.Dutch: res = "NL"; break;
					case SystemLanguage.English: res = "EN"; break;
					case SystemLanguage.Estonian: res = "ET"; break;
					case SystemLanguage.Faroese: res = "FO"; break;
					case SystemLanguage.Finnish: res = "FI"; break;
					case SystemLanguage.French: res = "FR"; break;
					case SystemLanguage.German: res = "DE"; break;
					case SystemLanguage.Greek: res = "EL"; break;
					case SystemLanguage.Hebrew: res = "IW"; break;
					case SystemLanguage.Hungarian: res = "HU"; break;
					case SystemLanguage.Icelandic: res = "IS"; break;
					case SystemLanguage.Indonesian: res = "IN"; break;
					case SystemLanguage.Italian: res = "IT"; break;
					case SystemLanguage.Japanese: res = "JA"; break;
					case SystemLanguage.Korean: res = "KO"; break;
					case SystemLanguage.Latvian: res = "LV"; break;
					case SystemLanguage.Lithuanian: res = "LT"; break;
					case SystemLanguage.Norwegian: res = "NO"; break;
					case SystemLanguage.Polish: res = "PL"; break;
					case SystemLanguage.Portuguese: res = "PT"; break;
					case SystemLanguage.Romanian: res = "RO"; break;
					case SystemLanguage.Russian: res = "RU"; break;
					case SystemLanguage.SerboCroatian: res = "SH"; break;
					case SystemLanguage.Slovak: res = "SK"; break;
					case SystemLanguage.Slovenian: res = "SL"; break;
					case SystemLanguage.Spanish: res = "ES"; break;
					case SystemLanguage.Swedish: res = "SV"; break;
					case SystemLanguage.Thai: res = "TH"; break;
					case SystemLanguage.Turkish: res = "TR"; break;
					case SystemLanguage.Ukrainian: res = "UK"; break;
					case SystemLanguage.Unknown: res = "EN"; break;
					case SystemLanguage.Vietnamese: res = "VI"; break;
				}
				Debug.Log ("[[VISUALPLAY UNITY]] Lang: " + res + "langSys: " + lang);
				return res;
			}
			private string accessToken;
			private string userId;
			private string userProfile = null;
			private bool challengeFinalizeFeedback = true;
			private string language = Get2LetterISOCodeFromSystemLanguage();

			public VisualPlayOptions(string accesToken) {
				this.accessToken = accesToken;
				this.userId = SystemInfo.deviceUniqueIdentifier;
			}

			// SET CUSTOM OPTIONS
			public VisualPlayOptions SetLanguage(string language) {
				this.language = language;
				return this;
			}
			public VisualPlayOptions SetUserId(string id) {
				this.userId = id;
				return this;
			}
			public VisualPlayOptions SetUserProfile(string jsonUserProfile) {
				this.userProfile = jsonUserProfile;
				return this;
			}
			public VisualPlayOptions SetChallengeFinalizeFeedback(bool challengeFinalizeFeedback) {
				this.challengeFinalizeFeedback = challengeFinalizeFeedback;
				return this;
			}

			// GETTERS FOR OWN CLASS
			public string AccessToken {
				get {
					return accessToken;
				}
			}
			public string UserId {
				get {
					return userId;
				}
			}
			public string Language {
				get {
					return language;
				}
			}
			public string UserProfile {
				get {
					return userProfile;
				}
			}
			public bool ChallengeFinalizeFeedback {
				get {
					return challengeFinalizeFeedback;
				}
			}
		}

		private VisualPlayOptions _options;
		public VisualPlayOptions Configure (string accessToken) {
			this.isInitialitzed = false;
			if (!accessToken.Equals ("")) {
				this._options = new VisualPlayOptions (accessToken);
				Debug.Log ("[[VISUALPLAY UNITY]] WEBVIEW CHALLENGES INIT: " + this._options.AccessToken + ", userId: " + this._options.UserId + ", userProfile: " + this._options.UserProfile + ", language: " + this._options.Language);
				StartCoroutine (InitWebView ());
			} else {
				Debug.Log ("[[VISUALPLAY UNITY]] appID Error ");
				if(OnInit != null) OnInit(VPCINIT.Failed);
			}
			return this._options;
		}

		private void saveActualOrientation(){
			userLastScreenOrientation = Screen.orientation;
			userScreenOrientation = "";

			if (Screen.autorotateToPortrait == true) {
				userScreenOrientation += "P-";
				Debug.Log ("[[VISUALPLAY UNITY]] Adding P-");
			} 
			if (Screen.autorotateToPortraitUpsideDown == true) {
				userScreenOrientation += "UD-";
				Debug.Log ("[[VISUALPLAY UNITY]] Adding UD-");
			}
			if (Screen.autorotateToLandscapeRight == true) {
				userScreenOrientation += "LR-";
				Debug.Log ("[[VISUALPLAY UNITY]] Adding LR-");
			}
			if (Screen.autorotateToLandscapeLeft == true) {
				userScreenOrientation += "LL-";
				Debug.Log ("[[VISUALPLAY UNITY]] Adding LL-");
			}

			if (userLastScreenOrientation != ScreenOrientation.Landscape && userLastScreenOrientation != ScreenOrientation.LandscapeLeft && userLastScreenOrientation != ScreenOrientation.LandscapeRight) {
				Screen.orientation = ScreenOrientation.LandscapeLeft;
				Debug.Log ("[[VISUALPLAY UNITY]] Change to landscape LL-");
			}
			StartCoroutine("ChangeToAutoOrientation", "landscape");
		}
		private void restoreScreenOrientation(){
			Debug.Log ("[[VISUALPLAY UNITY]] Is back, restore: " + userLastScreenOrientation);
			Screen.orientation = userLastScreenOrientation;
			Debug.Log ("[[VISUALPLAY UNITY]] Restoring with " + userScreenOrientation);
			StartCoroutine("ChangeToAutoOrientation", "backToGame");
		}
		IEnumerator ChangeToAutoOrientation(string mode){
			yield return new WaitForSecondsRealtime(1.0F);
			if(mode == "landscape"){
				Screen.autorotateToPortrait = false;
				Screen.autorotateToPortraitUpsideDown = false;
				Screen.autorotateToLandscapeRight = true;
				Screen.autorotateToLandscapeLeft = true;
				Screen.orientation = ScreenOrientation.AutoRotation;
				webViewObject.SetMargins(0, 0, 0, 0);
			} else {
				if (userScreenOrientation.Contains ("P-"))
					Screen.autorotateToPortrait = true;
				else
					Screen.autorotateToPortrait = false;
				if (userScreenOrientation.Contains ("UD-")) 
					Screen.autorotateToPortraitUpsideDown = true;
				else 
					Screen.autorotateToPortraitUpsideDown = false;
				if (userScreenOrientation.Contains ("LR-")) 
					Screen.autorotateToLandscapeRight = true;
				else 
					Screen.autorotateToLandscapeRight = false;
				if (userScreenOrientation.Contains ("LL-")) 
					Screen.autorotateToLandscapeLeft = true;
				else 
					Screen.autorotateToLandscapeLeft = false;
				Screen.orientation = ScreenOrientation.AutoRotation;

			}
		}

		public void ShowChallenge() {
			if (!IsInitialitzed) {
				Debug.Log ("You need configure VisualPlay Challanges before call ShowChallenge");
				if(OnAvailableChallenges != null) OnAvailableChallenges(VPCHALLENGE.NotAvailable);
				return;
			}
			userTimeScale = Time.timeScale;
			Time.timeScale = 0;
			saveActualOrientation();
			webViewObject.SetMargins(0, 0, 0, 0);
			webViewObject.EvaluateJS("showChallenge()");
			webViewObject.SetVisibility(true);
		}
		IEnumerator InitWebView(){
			if(webViewObject!=null) 
				Destroy (webViewObject.gameObject);

			webViewObject = (new GameObject("WebViewObject")).AddComponent<WebViewObject>();
			DontDestroyOnLoad (webViewObject.gameObject);
			webViewObject.Init(
				cb: (msg) => {
					Debug.Log(string.Format("[[VISUALPLAY WEBVIEW]] CallFromJS[{0}]", msg));
					if (!msg.Contains("visualplay://")) {
						Debug.Log(string.Format("error command in message: {0}", msg));
					} else {
						msg = msg.Replace("visualplay://", "");
						string cmd = msg.Split('?')[0];
						string resp;
						switch (cmd) {
						case "onLoad":
							string initData = @"{
								user_id: '"+this._options.UserId+@"',
								language: '"+this._options.Language+@"',
								user_profile: "+((this._options.UserProfile!=null) ? this._options.UserProfile : "''")+@",
								challenge_finalize_feedback: " + this._options.ChallengeFinalizeFeedback.ToString().ToLower() + @"
							}";
							webViewObject.EvaluateJS("Init("+initData+");");
							break;
						case "isInit":
							resp = msg.Split('?')[1].Split('=')[1];
							if(resp.Equals("true")) {
								this.isInitialitzed = true;
								if(OnInit != null) OnInit(VPCINIT.Success);
							} else {
								this.isInitialitzed = false;
								if(OnInit != null) OnInit(VPCINIT.Failed);
							}
							break;
						case "onSolution":
							if(OnSolution != null){
								resp = msg.Split('?')[1].Split('=')[1];
								if(resp.Equals("true")) OnSolution(VPCHALLENGE.Correct);
								else OnSolution(VPCHALLENGE.Incorrect);
							}
							else Debug.Log("OnResponse callback is mandatory");
							this.CloseWebView();
							break;
						case "isAvailable":
							if(OnAvailableChallenges != null){
								resp = msg.Split('?')[1].Split('=')[1];
								if(resp.Equals("true")) OnAvailableChallenges(VPCHALLENGE.Available);
								else OnAvailableChallenges(VPCHALLENGE.NotAvailable);
							}
							break;
						case "startChallenge":
							if(OnStartChallenge != null) OnStartChallenge();
							break;
						case "challengeNotAvailable":
							if(OnGetChallengesFailed != null) OnGetChallengesFailed();
							this.CloseWebView();
							break;
						case "cancelChallenge":
							if(OnCancel != null) OnCancel();
							this.CloseWebView();
							break;
						case "exit":
							this.CloseWebView();
							break;
						}
					}
				},
				err: (msg) => {
					Debug.Log(string.Format("[[VISUALPLAY WEBVIEW]] FAILED LOAD [{0}]", msg));
					this.isInitialitzed = false;
					OnInit(VPCINIT.Failed);
				},
				ld: (msg) => {
					Debug.Log(string.Format("[[VISUALPLAY WEBVIEW]] Ready to use [{0}]", msg));

				},
				enableWKWebView: true);
			webViewObject.SetMargins(0, 0, 0, 0);
			webViewObject.SetVisibility(false);

			var epochStart = new System.DateTime(1970, 1, 1, 8, 0, 0, System.DateTimeKind.Utc);
			string timestamp = ((System.DateTime.UtcNow - epochStart).TotalSeconds).ToString();
			string Url = apiBase + "?access_token=" + this._options.AccessToken + "&platform=unity&width=" + Screen.width + "&height=" + Screen.height + "&dpi=" + Screen.dpi + "&ts=" + timestamp;

			if (Url.StartsWith("http")) {
				Debug.Log("[[VISUALPLAY WEBVIEW]] CARGA DIRECTE URL");
				webViewObject.LoadURL(Url.Replace(" ", "%20"));
			} else {
				Debug.Log("[[VISUALPLAY WEBVIEW]] CARGA EN LOCAL URL");
				var exts = new string[]{
					".jpg",
					".html"  // should be last
				};
				foreach (var ext in exts) {
					var url = Url.Replace(".html", ext);
					var src = System.IO.Path.Combine(Application.streamingAssetsPath, url);
					var dst = System.IO.Path.Combine(Application.persistentDataPath, url);
					byte[] result = null;
					if (src.Contains("://")) {  // for Android
						var www = new WWW(src);
						yield return www;
						result = www.bytes;
					} else {
						result = System.IO.File.ReadAllBytes(src);
					}
					System.IO.File.WriteAllBytes(dst, result);
					if (ext == ".html") {
						webViewObject.LoadURL("file://" + dst.Replace(" ", "%20"));
						break;
					}
				}
			}
			#if !UNITY_ANDROID
			Debug.Log("[[VISUALPLAY WEBVIEW]] IOS -> ES CREA EL CALL DE TORNADA!");
			webViewObject.EvaluateJS(
			"window.addEventListener('load', function() {" +
			"   window.Unity = {" +
			"       call:function(msg) {" +
			"           var iframe = document.createElement('IFRAME');" +
			"           iframe.setAttribute('src', 'unity:' + msg);" +
			"           document.documentElement.appendChild(iframe);" +
			"           iframe.parentNode.removeChild(iframe);" +
			"           iframe = null;" +
			"       }" +
			"   }" +
			"}, false);");
			#endif

			yield break;
		}
		private void CloseWebView() {
			Debug.Log ("[[VISUALPLAY UNITY]] CLOSING WEBVIEW");
			restoreScreenOrientation();
			Time.timeScale = userTimeScale;

			if(webViewObject != null)
				webViewObject.SetVisibility(false);
		}

		public bool AvailableChallenges(){
			if (!IsInitialitzed) {
				Debug.Log ("You need configure VisualPlay Challanges before call AvailableChallenges");
				if(OnAvailableChallenges != null) OnAvailableChallenges(VPCHALLENGE.NotAvailable);
				return false;
			}
			webViewObject.EvaluateJS("availableChallenge()");
			return true;
		}


		// CALLBACKS
		// for Challenges
		public delegate void VPChallengesInit(VPCINIT isInit);
		public event VPChallengesInit OnInit;
		public delegate void VPChallengesAvChallanges(VPCHALLENGE isAvailable);
		public event VPChallengesAvChallanges OnAvailableChallenges;
		public delegate void VPChallengesGetChallenges();
		public event VPChallengesGetChallenges OnGetChallengesFailed;
		public delegate void VPChallengesStartChallenge();
		public event VPChallengesStartChallenge OnStartChallenge;
		public delegate void VPChallengesSolution(VPCHALLENGE status);
		public event VPChallengesSolution OnSolution;
		public delegate void VPChallengesCancel();
		public event VPChallengesCancel OnCancel;

		void OnDestroy() {
			
		}
	}
}
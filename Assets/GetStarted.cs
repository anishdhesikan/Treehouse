using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class GetStarted : MonoBehaviour {

	public CanvasGroup myCg;
	public CanvasGroup overlayCg;
	public NetworkManager manager;

	private bool fading;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (myCg.blocksRaycasts) {
			if (myCg.alpha >= 1f) {
				myCg.alpha = 1f;
			} else {
				myCg.alpha += 1f * Time.deltaTime;
			}
		} else {
			if (myCg.alpha <= 0f) {
				myCg.alpha = 0f;
			} else {
				myCg.alpha -= 1f * Time.deltaTime;
			}
		}
//		if (fading) {
//			if (myCg.alpha <= 0f) {
//				myCg.alpha = 0f;
//				overlayCg.alpha = 1f;
//				fading = false;
//			} else {
//				myCg.alpha -= 1f * Time.deltaTime;
//				overlayCg.alpha += 1f * Time.deltaTime;
//			}
//		}
	}

	public void FadeToCG () {
		fading = true;
		myCg.blocksRaycasts = false;
		overlayCg.blocksRaycasts = true;
	}

	IEnumerator Fade () {
		for (int i = 0; i < 20; i++) {
			myCg.alpha -= 1f / 20f;
			overlayCg.alpha += 1f / 20f;
			yield return 0;
		}
	}

	public void StartHost () {
		manager.StartHost();
	}
}

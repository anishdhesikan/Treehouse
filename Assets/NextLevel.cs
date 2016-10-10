using UnityEngine;
using System.Collections;

public class NextLevel : MonoBehaviour {

	// Use this for initialization
	void Start () {
		StartCoroutine ("NextLevel1");
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	IEnumerator NextLevel1 () {
		yield return new WaitForSeconds (3f);
		Application.LoadLevel (Application.loadedLevel + 1);
	}
}

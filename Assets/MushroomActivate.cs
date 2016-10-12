using UnityEngine;
using System.Collections;

public class MushroomActivate : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnCollisionEnter (Collision col) {
		if (col.transform.tag == "Snowman") {
			Application.LoadLevel (1);
		}
	}
}

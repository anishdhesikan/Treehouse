using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class SImpleMotion : NetworkBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (!isLocalPlayer)
			return;

		transform.position = transform.position + new Vector3 (Input.GetAxis("Horizontal"),
			Input.GetAxis("Vertical"),
			0f) * 2f * Time.deltaTime;
	}
}

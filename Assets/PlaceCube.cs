using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class PlaceCube : NetworkBehaviour {

	public GameObject cubePrefab;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown(0)) {
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit = new RaycastHit ();
			if (Physics.Raycast(ray, out hit, 1000f)) {
				GameObject cube = (GameObject)Instantiate(cubePrefab, hit.point, transform.rotation);
				NetworkServer.Spawn(cube);
			}
		}
	}
}

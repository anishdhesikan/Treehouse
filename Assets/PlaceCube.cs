﻿using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class PlaceCube : NetworkBehaviour {

	public GameObject cubePrefab;
	public GameObject giftPrefab;

	public GameObject giftSpawnPoint;

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

		if (Input.GetMouseButtonDown(1)) {
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit = new RaycastHit ();
			if (Physics.Raycast(ray, out hit, 1000f)) {
				GameObject gift = (GameObject)Instantiate(giftPrefab, hit.point + Vector3.up, Quaternion.identity);
				NetworkServer.Spawn(gift);
			}
		}
	}

	public void PlaceGift () {
		GameObject gift = (GameObject)Instantiate(giftPrefab, giftSpawnPoint.transform.position, Quaternion.identity);
		NetworkServer.Spawn(gift);
	}
}

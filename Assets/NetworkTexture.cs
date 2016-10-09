using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Runtime.InteropServices;
using System;

public class NetworkTexture : NetworkBehaviour {

	Renderer rend;
	WebCamTexture webcamTexture;
	NetworkView view;
	NetworkTransmitter networkTransmitter;
	Texture2D tex;

	int transmissionId = 0;
	bool startedRecording;
	float timeSinceSentData = 0f;

	private Texture2D prevTex;

	// Use this for initialization
	void Awake () {
		webcamTexture = new WebCamTexture ();

		view = GetComponent<NetworkView>();

		rend = GetComponent<Renderer>();
		networkTransmitter = GetComponent<NetworkTransmitter>();
		networkTransmitter.OnDataCompletelyReceived += MyCompletelyReceivedHandler;
		networkTransmitter.OnDataFragmentReceived += MyFragmentReceivedHandler;
		
	}
	
	// Update is called once per frame
//	[ServerCallback]
	void Update () {
		if (!startedRecording && isServer) {
//			Debug.Log("Hello");
			startedRecording = true;

			rend.material.mainTexture = webcamTexture;
			WebCamDevice[] devices = WebCamTexture.devices;

			webcamTexture = null;
			WebCamDevice[] wdcs = WebCamTexture.devices;
			for (int n = 0; n < wdcs.Length; ++n) {
				if (wdcs[n].isFrontFacing) {
					webcamTexture = new WebCamTexture(wdcs[n].name);
				}
			}

			if (webcamTexture == null) {
				webcamTexture = new WebCamTexture();
			}

			webcamTexture.Play();

//			StartCoroutine(SendData(0.5f));
		}

		timeSinceSentData += Time.deltaTime;

		if (isServer && timeSinceSentData > 0.1f) {

//			Debug.Log("Sending data.. " + Time.time);
			//		byte[] bytes = Color32ArrayToByteArray(webcamTexture.GetPixels32());
//			tex.width = webcamTexture.width;
//			tex.height = webcamTexture.height;
			tex = new Texture2D (webcamTexture.width, webcamTexture.height);
			tex.SetPixels(webcamTexture.GetPixels());
			tex.Apply();

			int wid = webcamTexture.width / 5;
			int hei = webcamTexture.height / 5;

			byte[] bytesToSend = new byte[0];

			if (wid > 0 && hei > 0) {
				TextureScale.Bilinear(tex, wid, hei);


				bytesToSend = tex.EncodeToJPG();

				if (bytesToSend.Length > 0) {
					StartCoroutine(networkTransmitter.SendBytesToClientsRoutine(1, transmissionId, bytesToSend));
					transmissionId++;
					timeSinceSentData = 0f;
				}
			} else {
				timeSinceSentData = 0f;
			}

			Destroy (tex);
		}

//		if (startedRecording && isServer) {
//				RpcReceiveWebcamPNG(bytesToSend);
//				view.RPC("ReceiveWebcamPNG", RPCMode.Others, bytesToSend);
//			} else {
//				Debug.LogError("Bad length of bytes to send.");
//			}
//		}
	}

//	IEnumerator SendData (float interval) {
//		Debug.Log("Sending data.. " + Time.time);
////		byte[] bytes = Color32ArrayToByteArray(webcamTexture.GetPixels32());
//		Texture2D tex = new Texture2D (webcamTexture.width, webcamTexture.height);
//		tex.SetPixels(webcamTexture.GetPixels());
//		tex.Apply();
//
//		int wid = tex.width / 10;
//		int hei = tex.height / 10;
//
//		byte[] bytesToSend = new byte[0];
//
//		if (wid > 0 && hei > 0) {
//			TextureScale.Bilinear(tex, wid, hei);
//
//
//			bytesToSend = tex.EncodeToJPG();
//
//			if (bytesToSend.Length > 0) {
//				StartCoroutine(networkTransmitter.SendBytesToClientsRoutine(0, bytesToSend));
//				yield return new WaitForSeconds (interval);
//			}
//		} else {
//			yield return new WaitForSeconds (interval);
//		}
//
//		Destroy (tex);
//
//		StartCoroutine(SendData (interval));
//	}

//	[ClientRpc]
//	public void RpcReceiveWebcamPNG(byte[] bytes)
//	{
//		if (bytes.Length < 1)
//		{
//			Debug.LogError("Received bad byte count from network.");
//			return;
//		}
//
//		Texture2D tex = new Texture2D (2, 2);
//		tex.LoadImage(bytes);
//
//		Destroy (rend.material.mainTexture);
//		rend.sharedMaterial.mainTexture = tex;
//	}

	private static byte[] Color32ArrayToByteArray(Color32[] colors)
	{
		if (colors == null || colors.Length == 0)
			return null;

		int lengthOfColor32 = Marshal.SizeOf(typeof(Color32));
		int length = lengthOfColor32 * colors.Length;
		byte[] bytes = new byte[length];

		GCHandle handle = default(GCHandle);
		try
		{
			handle = GCHandle.Alloc(colors, GCHandleType.Pinned);
			IntPtr ptr = handle.AddrOfPinnedObject();
			Marshal.Copy(ptr, bytes, 0, length);
		}
		finally
		{
			if (handle != default(GCHandle))
				handle.Free();
		}

		return bytes;
	}

	//on client this will be called once the complete data array has been received
	[Client]
	private void MyCompletelyReceivedHandler(int dataType, int transmissionId, byte[] data){
//		Debug.Log("Received All");


		// If this is the live stream texture data
		if (dataType == 1) {
			if (data.Length < 1) {
//			Debug.LogError("Received bad byte count from network.");
				return;
			}

			Texture2D tex = new Texture2D (2, 2);
			tex.LoadImage(data);

			Texture2D.DestroyImmediate(prevTex, true);
			prevTex = tex;

			rend.sharedMaterial.mainTexture = tex;
		}


	}

	//on clients this will be called every time a chunk (fragment of complete data) has been received
	[Client]
	private void MyFragmentReceivedHandler(int transmissionId, byte[] data){
//		Debug.Log("Received Fragment");
		//update a progress bar or do something else with the information
	}
}

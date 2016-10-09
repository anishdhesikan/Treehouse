using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class NetworkTransmitter : NetworkBehaviour {

	private static readonly string LOG_PREFIX = "[" + typeof(NetworkTransmitter).Name + "]: ";
//	public const int RELIABLE_SEQUENCED_CHANNEL = MyNetworkManager.CHANNEL_RELIABLE_SEQUENCED;
	public const int RELIABLE_SEQUENCED_CHANNEL = 0;
	private static int defaultBufferSize = 1024; //max ethernet MTU is ~1400

	private class TransmissionData{
		public int curDataIndex; //current position in the array of data already received.
		public byte[] data;

		public TransmissionData(byte[] _data){
			curDataIndex = 0;
			data = _data;
		}
	}

	//list of transmissions currently going on. a transmission id is used to uniquely identify to which transmission a received byte[] belongs to.
	List<int> serverTransmissionIds = new List<int>();

	//maps the transmission id to the data being received.
	Dictionary<int, TransmissionData> clientTransmissionData = new Dictionary<int,TransmissionData>();

	//callbacks which are invoked on the respective events. int = transmissionId. byte[] = data sent or received.
	public event UnityAction<int, byte[]> OnDataComepletelySent;
	public event UnityAction<int, byte[]> OnDataFragmentSent;
	public event UnityAction<int, byte[]> OnDataFragmentReceived;
	public event UnityAction<int, int, byte[]> OnDataCompletelyReceived;

	private float timeSinceReset = 0f;
	private bool triggerReset = false;

	void Update () {
		timeSinceReset += Time.deltaTime;
		if (triggerReset && timeSinceReset < 2f) {
			StopAllCoroutines();
		} else {
			triggerReset = false;
		}
	}

//	[Server]
//	public void SendBytesToClients(int transmissionId, byte[] data)
//	{
//		Debug.Assert(!serverTransmissionIds.Contains(transmissionId));
//		StartCoroutine(SendBytesToClientsRoutine(transmissionId, data));
//	}

	/*
	 * dataType:
	 * 1 - Live stream Texture
	 * 
	 */

	[Server]
	public IEnumerator SendBytesToClientsRoutine(int dataType, int transmissionId, byte[] data)
	{
		Debug.Assert(!serverTransmissionIds.Contains(transmissionId));

		if (serverTransmissionIds.Count > 0 && transmissionId > serverTransmissionIds [0] + 1) {
			serverTransmissionIds.Remove(transmissionId);
		}

//		Debug.Log(LOG_PREFIX + "SendBytesToClients processId=" + transmissionId + " | datasize=" + data.Length);

		//tell client that he is going to receive some data and tell him how much it will be.
		RpcPrepareToReceiveBytes(transmissionId, data.Length);
		yield return null;

		//begin transmission of data. send chunks of 'bufferSize' until completely transmitted.
		serverTransmissionIds.Add(transmissionId);
		TransmissionData dataToTransmit = new TransmissionData(data);
		int bufferSize = defaultBufferSize; 
		while (dataToTransmit.curDataIndex < dataToTransmit.data.Length-1)
		{
			//determine the remaining amount of bytes, still need to be sent.
			int remaining = dataToTransmit.data.Length - dataToTransmit.curDataIndex;
			if (remaining < bufferSize)
				bufferSize = remaining;

			//prepare the chunk of data which will be sent in this iteration
			byte[] buffer = new byte[bufferSize];

			System.Array.Copy(dataToTransmit.data, dataToTransmit.curDataIndex, buffer, RELIABLE_SEQUENCED_CHANNEL, bufferSize);

			//send the chunk
			RpcReceiveBytes(dataType, transmissionId, buffer);
			dataToTransmit.curDataIndex += bufferSize;

			yield return null;

			if (null != OnDataFragmentSent) {
				OnDataFragmentSent.Invoke(transmissionId, buffer);
				if (timeSinceReset > 10f) {
					timeSinceReset = 0f;
					triggerReset = true;
				}
			}
		}

		//transmission complete.
		serverTransmissionIds.Remove(transmissionId);

		if (null != OnDataComepletelySent)
			OnDataComepletelySent.Invoke(transmissionId, dataToTransmit.data);
	}

	[ClientRpc]
	private void RpcPrepareToReceiveBytes(int transmissionId, int expectedSize)
	{
		if (clientTransmissionData.ContainsKey(transmissionId))
			return;

		//prepare data array which will be filled chunk by chunk by the received data
		TransmissionData receivingData = new TransmissionData(new byte[expectedSize]);
		lock(clientTransmissionData)
			clientTransmissionData.Add(transmissionId, receivingData);
	}

	//use reliable sequenced channel to ensure bytes are sent in correct order
	[ClientRpc(channel = 1)]
	private void RpcReceiveBytes(int dataType, int transmissionId, byte[] recBuffer)
	{
		//already completely received or not prepared?
		if (!clientTransmissionData.ContainsKey(transmissionId))
			return;

		//copy received data into prepared array and remember current dataposition
		TransmissionData dataToReceive = clientTransmissionData[transmissionId];
		System.Array.Copy(recBuffer, 0, dataToReceive.data, dataToReceive.curDataIndex, recBuffer.Length);
		dataToReceive.curDataIndex += recBuffer.Length;

		if (null != OnDataFragmentReceived)
			OnDataFragmentReceived(transmissionId, recBuffer);

		if (dataToReceive.curDataIndex < dataToReceive.data.Length - 1)
			//current data not completely received
			return;

		//current data completely received
//		Debug.Log(LOG_PREFIX + "Completely Received Data at transmissionId=" + transmissionId);
		lock(clientTransmissionData)
			clientTransmissionData.Remove(transmissionId);

		if (null != OnDataCompletelyReceived) {
			OnDataCompletelyReceived.Invoke(dataType, transmissionId, dataToReceive.data);
		} else {
//			Debug.Log("HEYOOO");
		}
	}
}
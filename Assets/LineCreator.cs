using UnityEngine;
using System.Collections;

public class LineCreator : MonoBehaviour {

	// Attach this to the "Pen Tip"

    public SteamVR_TrackedObject trackedObj;
    public Material Mat;
    private LineRenderer NewLine;
    private int numClicks = 0;
	private Vector3 prevPos;

	void Start () {
		prevPos = Vector3.zero;
	}

    // Update is called once per frame
    void Update () {
        SteamVR_Controller.Device Controller = SteamVR_Controller.Input((int)trackedObj.index);
        if (Controller.GetTouchDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            GameObject NewGameObject = new GameObject();
            NewLine = NewGameObject.AddComponent<LineRenderer>();

            NewLine.material = Mat;

            NewLine.SetWidth(.001f, .02f);

            numClicks = 0;
        } else if (Controller.GetTouch(SteamVR_Controller.ButtonMask.Trigger))
		{
			Vector3 pos = transform.position;
			if (Vector3.Distance (pos, prevPos) > 0.02f) {
				NewLine.SetVertexCount (numClicks + 1);
				// NewLine.SetPosition(numClicks, Controller.transform.pos) = global position not local possition
				NewLine.SetPosition (numClicks, pos);
				prevPos = pos;
				numClicks++;
			}
        }
    }

//	void OnTriggerStay (Collider col) {
//		Ray ray = new Ray (transform.position - transform.up, transform.up);
//		RaycastHit hit = new RaycastHit ();
//
//		if (Physics.Raycast (ray, out hit, 1f)) {
//
//		}
//	}
}
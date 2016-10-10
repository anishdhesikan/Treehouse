using UnityEngine;
using System.Collections;

public class LineCreator : MonoBehaviour {

    public SteamVR_TrackedObject trackedObj;
    public Transform penTip;
    public Material Mat;
    private LineRenderer NewLine;
    private int numClicks = 0;

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
            NewLine.SetVertexCount(numClicks + 1);
            // NewLine.SetPosition(numClicks, Controller.transform.pos) = global position not local possition
            NewLine.SetPosition(numClicks, penTip.transform.position);
            numClicks++;
        }
        }
	}
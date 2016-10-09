//========= Copyright 2016, Sam Tague, All rights reserved. ===========
//
// Attach to either or both tracked controller objects in SteamVR camera rig
//
//=============================================================================

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ArcTeleporter : MonoBehaviour 
{
	public enum ArcMaterial
	{
		MATERIAL,
		COLOUR
	}

	public bool onlyLandOnFlat = true;
	//Anywhere flat slot limit
	public float slopeLimit = 20;
	public bool onlyLandOnTag = false;
	//Tags object has to have to be valid
	public List<string> tags = new List<string>();
	//Approximate max distance in unity units
	public float maxDistance = 5f;
	public bool disablePreMadeControls = false;
	public ArcMaterial arcMat = ArcMaterial.MATERIAL;
	// Material for the arc leave null to use colour
	public Material goodTeleMat;
	public Material badTeleMat;
	// if material used the scale the texture should use
	public float matScale = 5;
	//	texture animated speed
	public Vector2 texMovementSpeed = Vector2.zero;
	//	arc colours for good and bad spots if no material is used
	public Color goodSpotCol = new Color(0, 0.6f, 1f, 0.2f);
	public Color badSpotCol = new Color(0.8f, 0, 0, 0.2f);
	public float arcLineWidth = 0.05f;
	//Leave empty to collide with everything
	public List<string> raycastLayer = new List<string>();
	//Collide only with selected layers or only ignore selected layers
	public bool ignoreRaycastLayers = false;
	private LayerMask raycastLayerMask;
	//	Teleport and room highlight, leave blank to not show
	public GameObject teleportHighlight;
	GameObject _teleportHighlightInstance;
	public GameObject roomShape;
	GameObject _roomShapeInstance;

	private Transform _vrCamera;
	private Transform _vrPlayArea;
	private LineRenderer _lineRenderer;
	private LineRenderer _lineRenderer2;
	private Vector3 _teleportSpot;
	private bool _goodSpot;
	private bool _teleportActive;
	public bool teleportActive
	{
		get { return _teleportActive; }
	}

	void Start ()
	{
		SteamVR_Camera steamVR_Camera = transform.parent.gameObject.GetComponentInChildren<SteamVR_Camera>();
		if (steamVR_Camera == null) _vrCamera = Camera.main.transform;
		else _vrCamera = steamVR_Camera.transform;
		if (_vrCamera == null)
		{
			Debug.LogError("ArcTeleporter can't find camera!");
			enabled = false;
			return;
		}
		SteamVR_PlayArea steamVR_PlayArea = transform.parent.gameObject.GetComponent<SteamVR_PlayArea>();
		if (steamVR_PlayArea == null) _vrPlayArea = transform.parent;
		else _vrPlayArea = steamVR_PlayArea.transform;
		if (_vrPlayArea == null)
		{
			Debug.LogError("ArcTeleporter must be a child of the steam vr play area attached to the controller tracked object");
			enabled = false;
			return;
		}

		SetControls();

		GameObject arcParentObject = new GameObject("ArcTeleporter");
		arcParentObject.transform.localScale = _vrPlayArea.localScale;
		GameObject arcLine1 = new GameObject("ArcLine1");
		arcLine1.transform.SetParent(arcParentObject.transform);
		_lineRenderer = arcLine1.AddComponent<LineRenderer>();
		GameObject arcLine2 = new GameObject("ArcLine2");
		arcLine2.transform.SetParent(arcParentObject.transform);
		_lineRenderer2 = arcLine2.AddComponent<LineRenderer>();
		_lineRenderer.SetWidth(arcLineWidth*_vrPlayArea.localScale.magnitude, arcLineWidth*_vrPlayArea.localScale.magnitude);
		_lineRenderer2.SetWidth(arcLineWidth*_vrPlayArea.localScale.magnitude, arcLineWidth*_vrPlayArea.localScale.magnitude);
		if (arcMat == ArcMaterial.COLOUR || goodTeleMat == null)
		{
			_lineRenderer.SetColors(goodSpotCol, goodSpotCol);
			_lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
			_lineRenderer2.SetColors(goodSpotCol, goodSpotCol);
			_lineRenderer2.material = new Material(Shader.Find("Sprites/Default"));
		} else
		{
			_lineRenderer.material = goodTeleMat;
			_lineRenderer2.material = goodTeleMat;
		}
		_lineRenderer.enabled = false;
		_lineRenderer2.enabled = false;

		if (roomShape != null)
		{
			_roomShapeInstance = (GameObject)Instantiate(roomShape, Vector3.zero, Quaternion.identity);
			_roomShapeInstance.transform.SetParent(arcParentObject.transform);
			_roomShapeInstance.transform.rotation = _vrPlayArea.rotation;
			_roomShapeInstance.transform.localScale = Vector3.one;
			_roomShapeInstance.SetActive(false);
		}
		if (teleportHighlight != null)
		{
			_teleportHighlightInstance = (GameObject)Instantiate(teleportHighlight, Vector3.zero, Quaternion.identity);
			_teleportHighlightInstance.transform.SetParent(arcParentObject.transform);
			_teleportHighlightInstance.transform.localScale = Vector3.one;
			_teleportHighlightInstance.SetActive(false);
		}
	}

	void Update()
	{
		if (!teleportActive) return;
		CalculateArc();
	}

	//	Overide and change to suit your custom needs
	virtual protected void SetControls()
	{
		if (!disablePreMadeControls)
		{
			if (GetComponent<SteamVR_TrackedController>() == null)
			{
				Debug.LogError("ArcTeleporter must be on a SteamVR_TrackedController");
				enabled = false;
				return;
			}
			GetComponent<SteamVR_TrackedController>().PadClicked += new ClickedEventHandler(PadClicked);
			GetComponent<SteamVR_TrackedController>().PadUnclicked += new ClickedEventHandler(PadUnClicked);
			GetComponent<SteamVR_TrackedController>().TriggerClicked += new ClickedEventHandler(TriggerClicked);
		}
	}

	private void CalculateArc()
	{
		//	Line renderer position storage (two because line renderer texture will stretch if one is used)
		List<Vector3> positions1 = new List<Vector3>();
		List<Vector3> positions2 = new List<Vector3>();

		//	Variables need for curve
		Quaternion currentRotation = transform.rotation;
		Vector3 currentPosition = transform.position;
		positions1.Add(currentPosition);
		Vector3 lastPostion = transform.position-transform.forward;
		Vector3 currentDirection = transform.forward;
		Vector3 downForward = new Vector3(transform.forward.x*0.01f, -1, transform.forward.z*0.01f);
		RaycastHit hit = new RaycastHit();
		float totalDistance1 = 0;
		float totalDistance2 = 0;

		//	first Vector3 positions array will be used for the curve and the second line renderer is used for the straight down after the curve
		bool useFirstArray = true;

		//	Should never come close to 500 iterations but just as safety to avoid indefinite looping
		int i=0;
		while(i<500)
		{
			i++;

			//	Rotate the current rotation toward the downForward rotation
			Quaternion downQuat = Quaternion.LookRotation(downForward);
			currentRotation = Quaternion.RotateTowards(currentRotation, downQuat, 1f);

			//	Make ray for new direction
			Ray newRay = new Ray(currentPosition, currentPosition-lastPostion);
			float length = (maxDistance*0.01f)*_vrPlayArea.localScale.magnitude;
			if (currentRotation == downQuat)
			{
				//We have finished the arc and are facing down
				//So were going to use the second line renderer and extend the normal length as a last effort to hit something
				useFirstArray = false;
				length = (maxDistance*matScale)*_vrPlayArea.localScale.magnitude;
				positions2.Add(currentPosition);
			}

			//	Check if we hit something
			bool hitSomething = false;
			if (raycastLayer == null || raycastLayer.Count == 0)
				hitSomething = Physics.Raycast(newRay, out hit, length);
			else
			{
				LayerMask raycastLayerMask = 1 << LayerMask.NameToLayer(raycastLayer[0]);
				for(int j=1; j<raycastLayer.Count ; j++)
				{
					raycastLayerMask |= 1 << LayerMask.NameToLayer(raycastLayer[j]);
				}
				if (ignoreRaycastLayers) raycastLayerMask = ~raycastLayerMask;
				hitSomething = Physics.Raycast(newRay, out hit, length, raycastLayerMask);
			}

			if (hitSomething)
			{
				//	Depending on whether we had switched to the first or second line renderer
				//	add the point and finish calculating the total distance
				if (useFirstArray)
				{
					totalDistance1 += (currentPosition-hit.point).magnitude;
					positions1.Add(hit.point);
				} else
				{
					totalDistance2 += (currentPosition-hit.point).magnitude;
					positions2.Add(hit.point);
				}
				//	And we're done
				break;
			}

			//	Convert the rotation to a forward vector and apply to our current position
			currentDirection = currentRotation * Vector3.forward;
			lastPostion = currentPosition;
			currentPosition += currentDirection*length;

			//	Depending on whether we have switched to the second line renderer add this point and update total distance
			if (useFirstArray)
			{
				totalDistance1 += length;
				positions1.Add(currentPosition);
			} else
			{
				totalDistance2 += length;
				positions2.Add(currentPosition);
			}

			//	If we're pointing down then we did the whole arc and down without hitting anything so we're done
			if (currentRotation == downQuat) break;
		}

		//	Toggle the second line renderer and get the last position from the appropirate array for our destination
		if (useFirstArray)
		{
			_lineRenderer2.enabled = false;
			_teleportSpot = positions1[positions1.Count-1];
		} else
		{
			_lineRenderer2.enabled = true;
			_teleportSpot = positions2[positions2.Count-1];
		}

		//	Decide using the current teleport rule whether this is a good teleporting spot or not
		_goodSpot = IsGoodSpot(hit);

		//	Update line, teleport highlight and room highlight based on it being a good spot or bad
		if (_goodSpot)
		{
			if (arcMat == ArcMaterial.COLOUR || goodTeleMat == null)
			{
				_lineRenderer.SetColors(goodSpotCol, goodSpotCol);
				_lineRenderer2.SetColors(goodSpotCol, goodSpotCol);
			} else
			{
				if (_lineRenderer.material.mainTexture.name != goodTeleMat.mainTexture.name) _lineRenderer.material = goodTeleMat;
				if (_lineRenderer2.material.mainTexture.name != goodTeleMat.mainTexture.name) _lineRenderer2.material = goodTeleMat;
			}
			if (_roomShapeInstance != null)
			{
				_roomShapeInstance.SetActive(true);
				Vector3 camSpot = new Vector3(_vrCamera.position.x, 0, _vrCamera.position.z);
				Vector3 roomSpot = new Vector3(_vrPlayArea.position.x, 0, _vrPlayArea.position.z);
				Vector3 offset = roomSpot - camSpot;
				_roomShapeInstance.transform.position = (_teleportSpot + offset)+hit.normal*0.05f;
			}
		} else
		{
			if (arcMat == ArcMaterial.COLOUR || badTeleMat == null)
			{
				_lineRenderer.SetColors(badSpotCol, badSpotCol);
				_lineRenderer2.SetColors(badSpotCol, badSpotCol);
			} else
			{
				if (_lineRenderer.material.mainTexture.name != badTeleMat.mainTexture.name) _lineRenderer.material = badTeleMat;
				if (_lineRenderer2.material.mainTexture.name != badTeleMat.mainTexture.name) _lineRenderer2.material = badTeleMat;
			}
			if (_roomShapeInstance != null) _roomShapeInstance.SetActive(false);
		}

		if (_teleportHighlightInstance != null)
		{
			_teleportHighlightInstance.transform.position = _teleportSpot+(hit.normal*0.05f);
			if (hit.normal == Vector3.zero)
				_teleportHighlightInstance.transform.rotation = Quaternion.identity;
			else 
				_teleportHighlightInstance.transform.rotation = Quaternion.LookRotation(hit.normal);
		}

		_lineRenderer.SetVertexCount(positions1.Count);
		_lineRenderer.SetPositions(positions1.ToArray());
		_lineRenderer.material.mainTextureScale = new Vector2((totalDistance1*matScale)/_vrPlayArea.localScale.magnitude, 1);
		_lineRenderer.material.mainTextureOffset = new Vector2(_lineRenderer.material.mainTextureOffset.x+texMovementSpeed.x, _lineRenderer.material.mainTextureOffset.y+texMovementSpeed.y);

		if (_lineRenderer2.enabled)
		{
			_lineRenderer2.SetVertexCount(positions2.Count);
			_lineRenderer2.SetPositions(positions2.ToArray());
			_lineRenderer2.material.mainTextureScale = new Vector2((totalDistance2*matScale)/_vrPlayArea.localScale.magnitude, 1);
			_lineRenderer2.material.mainTextureOffset = new Vector2(_lineRenderer2.material.mainTextureOffset.x+texMovementSpeed.x, _lineRenderer2.material.mainTextureOffset.y+texMovementSpeed.y);
		}
	}

	//	Overide and change to customize good landing spots
	virtual protected bool IsGoodSpot(RaycastHit hit)
	{
		if (hit.transform == null) return false;
		if (onlyLandOnFlat)
		{
			float angle = Vector3.Angle(Vector3.up, hit.normal);
			if (angle > slopeLimit)
				return false;
		}
		if (onlyLandOnTag)
		{
			foreach(string tag in tags)
			{
				if (hit.transform.tag == tag)
					return true;
			}
			return false;
		}
		return true;
	}
		
	void PadClicked(object sender, ClickedEventArgs e)
	{
		if (disablePreMadeControls) return;
		EnableTeleport();
	}

	virtual public void EnableTeleport()
	{
		_lineRenderer.enabled = true;
		_teleportActive = true;
		if (_teleportHighlightInstance != null) _teleportHighlightInstance.SetActive(true);
		if (_roomShapeInstance != null) _roomShapeInstance.SetActive(true);
		CalculateArc();
	}

	void PadUnClicked(object sender, ClickedEventArgs e)
	{
		if (disablePreMadeControls) return;
		DisableTeleport();
	}

	virtual public void DisableTeleport()
	{
		_lineRenderer.enabled = false;
		_lineRenderer2.enabled = false;
		_teleportActive = false;
		if (_teleportHighlightInstance != null) _teleportHighlightInstance.SetActive(false);
		if (_roomShapeInstance != null) _roomShapeInstance.SetActive(false);
	}

	void TriggerClicked(object sender, ClickedEventArgs e)
	{
		if (disablePreMadeControls) return;
		Teleport();
	}

	virtual public void Teleport()
	{
		if (teleportActive && _goodSpot)
		{
			Vector3 camSpot = new Vector3(_vrCamera.position.x, 0, _vrCamera.position.z);
			Vector3 roomSpot = new Vector3(_vrPlayArea.position.x, 0, _vrPlayArea.position.z);
			Vector3 offset = roomSpot - camSpot;
			_vrPlayArea.position = _teleportSpot + offset;
		}
	}
}

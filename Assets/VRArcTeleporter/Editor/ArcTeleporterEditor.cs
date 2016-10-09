using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(ArcTeleporter))]
public class ArcTeleporterEditor : Editor
{
	// target component
	public ArcTeleporter m_Component = null;
	static bool raycastLayerFoldout = false;
	int raycastLayersSize = 0;
	static bool tagsFoldout = false;
	int tagsSize = 0;

	public void OnEnable()
	{
		m_Component = (ArcTeleporter)target;
		if (m_Component.raycastLayer != null)
			raycastLayersSize = m_Component.raycastLayer.Count;
		else raycastLayersSize = 0;
		if (m_Component.tags != null)
			tagsSize = m_Component.tags.Count;
		else tagsSize = 0;
	}

	public override void OnInspectorGUI()
	{
		bool changed = false;

		var oldLandOnFlat = m_Component.onlyLandOnFlat;
		m_Component.onlyLandOnFlat = EditorGUILayout.Toggle("Only land on flat", m_Component.onlyLandOnFlat);
		if (oldLandOnFlat != m_Component.onlyLandOnFlat) changed = true;
		if (m_Component.onlyLandOnFlat)
		{
			var oldSlopeLimit = m_Component.slopeLimit;
			m_Component.slopeLimit = EditorGUILayout.FloatField("Slope limit", m_Component.slopeLimit);
			if (oldSlopeLimit != m_Component.slopeLimit) changed = true;
		}

		var oldOnlyLandOnTag = m_Component.onlyLandOnTag;
		m_Component.onlyLandOnTag = EditorGUILayout.Toggle("Only land on tagged", m_Component.onlyLandOnTag);
		if (oldOnlyLandOnTag != m_Component.onlyLandOnTag) changed = true;
		if (m_Component.onlyLandOnTag)
		{
			tagsFoldout = EditorGUILayout.Foldout(tagsFoldout, "Tags");
			if (tagsFoldout)
			{
				EditorGUI.indentLevel++;
				var oldTagSize = tagsSize;
				tagsSize = EditorGUILayout.IntField("Size", tagsSize);
				if (oldTagSize != tagsSize) 
				{
					if (m_Component.tags == null) m_Component.tags = new System.Collections.Generic.List<string>();
					changed = true;
				}
				if (tagsSize > m_Component.tags.Count)
				{
					int newFields = tagsSize - m_Component.tags.Count;
					for (int i=0 ; i<newFields ; i++)
						m_Component.tags.Add("");
				} else if (tagsSize < m_Component.tags.Count)
				{
					int fieldsToRemove = m_Component.tags.Count - tagsSize;
					m_Component.tags.RemoveRange(m_Component.tags.Count-fieldsToRemove, fieldsToRemove);
				}
				for (int i=0 ; i<tagsSize ; i++)
				{
					var oldTag = m_Component.tags[i];
					m_Component.tags[i] = EditorGUILayout.TextField("Element "+i, m_Component.tags[i]);
					if (oldTag != m_Component.tags[i]) changed = true;
				}
				EditorGUI.indentLevel--;
			}
		}

		raycastLayerFoldout = EditorGUILayout.Foldout(raycastLayerFoldout, "Raycast Layers");
		if (raycastLayerFoldout)
		{
			EditorGUI.indentLevel++;
			var oldRaycastLayersSize = raycastLayersSize;
			raycastLayersSize = EditorGUILayout.IntField("Size", raycastLayersSize);
			if (oldRaycastLayersSize != raycastLayersSize)
			{
				if (m_Component.raycastLayer == null) m_Component.raycastLayer = new System.Collections.Generic.List<string>();
				changed = true;
			}
			if (raycastLayersSize > m_Component.raycastLayer.Count)
			{
				int newFields = raycastLayersSize - m_Component.raycastLayer.Count;
				for(int i=0 ; i<newFields ; i++)
					m_Component.raycastLayer.Add("");

			} else if (raycastLayersSize < m_Component.raycastLayer.Count)
			{
				int fieldsToRemove = m_Component.raycastLayer.Count - raycastLayersSize;
				m_Component.raycastLayer.RemoveRange(m_Component.raycastLayer.Count-fieldsToRemove, fieldsToRemove);
			}
			for(int i=0 ; i<raycastLayersSize ; i++)
			{
				var oldRaycastLayer = m_Component.raycastLayer[i];
				m_Component.raycastLayer[i] = EditorGUILayout.TextField("Element "+i, m_Component.raycastLayer[i]);
				if (oldRaycastLayer != m_Component.raycastLayer[i]) changed = true;
			}
			EditorGUILayout.HelpBox("Leave raycast layers empty to collide with everything", MessageType.Info);
			if (m_Component.raycastLayer != null && m_Component.raycastLayer.Count > 0)
			{
				var oldIgnoreRaycastLayer = m_Component.ignoreRaycastLayers;
				m_Component.ignoreRaycastLayers = EditorGUILayout.Toggle("Ignore raycast layers", m_Component.ignoreRaycastLayers);
				if (oldIgnoreRaycastLayer != m_Component.ignoreRaycastLayers) changed = true;
				EditorGUILayout.HelpBox("Ignore raycast layers True: Ignore anything on the layers specified. False: Ignore anything on layers not specified", MessageType.Info);
			}
			EditorGUI.indentLevel--;
		}

		var oldMaxDistance = m_Component.maxDistance;
		m_Component.maxDistance = EditorGUILayout.FloatField("Max Distance", m_Component.maxDistance);
		if (oldMaxDistance != m_Component.maxDistance) changed = true;
		var oldPremadeControls = m_Component.disablePreMadeControls;
		m_Component.disablePreMadeControls = EditorGUILayout.Toggle("Disable Pre Made Controls", m_Component.disablePreMadeControls);
		if (oldPremadeControls != m_Component.disablePreMadeControls) changed = true;
		var oldArcLineWidth = m_Component.arcLineWidth;
		m_Component.arcLineWidth = EditorGUILayout.FloatField("Arc Width", m_Component.arcLineWidth);
		if (oldArcLineWidth != m_Component.arcLineWidth) changed = true;

		var oldArcMat = m_Component.arcMat;
		m_Component.arcMat = (ArcTeleporter.ArcMaterial)EditorGUILayout.EnumPopup("Use Material", m_Component.arcMat);
		if (oldArcMat != m_Component.arcMat) changed = true;

		if (m_Component.arcMat == ArcTeleporter.ArcMaterial.MATERIAL)
		{
			var oldGoodTeleMat = m_Component.goodTeleMat;
			m_Component.goodTeleMat = (Material)EditorGUILayout.ObjectField("Good Material", m_Component.goodTeleMat, typeof(Material), false);
			if (oldGoodTeleMat != m_Component.goodTeleMat) changed = true;
			var oldBadTeleMat = m_Component.badTeleMat;
			m_Component.badTeleMat = (Material)EditorGUILayout.ObjectField("Bad Material", m_Component.badTeleMat, typeof(Material), false);
			if (oldBadTeleMat != m_Component.badTeleMat) changed = true;
			var oldMatScale = m_Component.matScale;
			m_Component.matScale = EditorGUILayout.FloatField("Material scale", m_Component.matScale);
			if (oldMatScale != m_Component.matScale) changed = true;
			var oldTexMovementSpeed = m_Component.texMovementSpeed;
			m_Component.texMovementSpeed = EditorGUILayout.Vector2Field("Material Movement Speed", m_Component.texMovementSpeed);
			if (oldTexMovementSpeed != m_Component.texMovementSpeed) changed = true;
		} else
		{
			var oldGoodSpotCol = m_Component.goodSpotCol;
			m_Component.goodSpotCol = EditorGUILayout.ColorField("Good Colour", m_Component.goodSpotCol);
			if (oldGoodSpotCol != m_Component.goodSpotCol) changed = true;
			var oldBadSpotCol = m_Component.badSpotCol;
			m_Component.badSpotCol = EditorGUILayout.ColorField("Bad Colour", m_Component.badSpotCol);
			if (oldBadSpotCol != m_Component.badSpotCol) changed = true;
		}

		var oldTeleportHighlight = m_Component.teleportHighlight;
		m_Component.teleportHighlight = (GameObject)EditorGUILayout.ObjectField("Teleport Highlight", m_Component.teleportHighlight, typeof(GameObject), false);
		if (oldTeleportHighlight != m_Component.teleportHighlight) changed = true;
		var oldRoomShape = m_Component.roomShape;
		m_Component.roomShape = (GameObject)EditorGUILayout.ObjectField("Room Highlight", m_Component.roomShape, typeof(GameObject), false);
		if (oldRoomShape != m_Component.roomShape) changed = true;

		if (changed) EditorUtility.SetDirty(m_Component);
	}
}

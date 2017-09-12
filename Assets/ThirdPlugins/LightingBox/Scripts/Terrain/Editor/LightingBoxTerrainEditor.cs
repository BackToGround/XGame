﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LightingBoxTerrain))]
public class LightingBoxTerrainEditor : Editor 
{
	LightingBoxTerrain targetObject;

	int currentLayer;

	void OnEnable()
	{
		targetObject = (LightingBoxTerrain)target;

		targetObject.splats = targetObject.GetComponent<Terrain> ().terrainData.splatPrototypes;

		targetObject.LoadSettings ();
	}

	void OnDisable()
	{
		targetObject.UpdateTerrain ();
	}

	public override void OnInspectorGUI()
	{
		
		serializedObject.Update();


		targetObject = (LightingBoxTerrain)target;

		if (!targetObject.initialized) 
		{
			
			targetObject.tMaterial = (Material)EditorGUILayout.ObjectField ("Material", targetObject.tMaterial, typeof(Material), true);
			EditorGUILayout.Space ();

			if (GUILayout.Button ("Initialize"))
				targetObject.Init ();
		} 
		else
		{
			LightingBoxTerrain mTarget = (LightingBoxTerrain)target;

			EditorGUILayout.Space ();
			EditorGUILayout.Space ();

			EditorGUILayout.LabelField ("LightingBox Terrain 6-Layers");

			EditorGUILayout.Space ();
			EditorGUILayout.Space ();
			targetObject.autoUpdate = EditorGUILayout.Toggle ("Auto Update", targetObject.autoUpdate);
			EditorGUILayout.Space ();
			EditorGUILayout.Space ();

////		------------------------------------------------------------------
			GUILayout.BeginVertical ("Layer", GUI.skin.box);
			GUILayout.Space (20);

			//-----------------------------------------------------------------------------------------
			EditorGUILayout.BeginHorizontal ();

			if (GUILayout.Button (targetObject.splats [0].texture, GUILayout.Width (53), GUILayout.Height (53)))
				currentLayer = 0;
			if (GUILayout.Button (targetObject.splats [1].texture, GUILayout.Width (53), GUILayout.Height (53)))
				currentLayer = 1;
			if (GUILayout.Button (targetObject.splats [2].texture, GUILayout.Width (53), GUILayout.Height (53)))
				currentLayer = 2;
			if (GUILayout.Button (targetObject.splats [3].texture, GUILayout.Width (53), GUILayout.Height (53)))
				currentLayer = 3;
			EditorGUILayout.EndHorizontal ();

			//-----------------------------------------------------------------------------------------
			EditorGUILayout.BeginHorizontal ();

			if (GUILayout.Button (targetObject.splats [4].texture, GUILayout.Width (53), GUILayout.Height (53)))
				currentLayer = 4;
			if (GUILayout.Button (targetObject.splats [5].texture, GUILayout.Width (53), GUILayout.Height (53)))
				currentLayer = 5;

			EditorGUILayout.EndHorizontal ();
			//-----------------------------------------------------------------------------------------
			// ... your box content ...
			GUILayout.EndVertical ();
			//-----------------------------------------------------------------------------------------

			if (currentLayer == 0) {
				//-----------------------------------------------------------------------------------------
				GUILayout.BeginVertical ("Texture", GUI.skin.box);
				GUILayout.Space (20);
				targetObject.layer0 = (Texture2D)EditorGUILayout.ObjectField ("Albedo", targetObject.layer0, typeof(Texture2D), true);
				targetObject.layer_Normal_0 = (Texture2D)EditorGUILayout.ObjectField ("Normal", targetObject.layer_Normal_0, typeof(Texture2D), true);
				GUILayout.EndVertical ();
				//-----------------------------------------------------------------------------------------
				GUILayout.BeginVertical ("Property", GUI.skin.box);
				GUILayout.Space (20);
				targetObject.sUV0 = EditorGUILayout.FloatField ("UV Tile", targetObject.sUV0);
				targetObject.sSmoothness0 = EditorGUILayout.Slider ("Smoothness", targetObject.sSmoothness0, 0, 10);
				targetObject.sDisplacement0 = EditorGUILayout.Slider ("Displacement", targetObject.sDisplacement0, 0, 3);
				targetObject.sNormal0 = EditorGUILayout.Slider ("Normal Power", targetObject.sNormal0, 0, 1);
				GUILayout.EndVertical ();

			}
			if (currentLayer == 1) {
				//-----------------------------------------------------------------------------------------
				GUILayout.BeginVertical ("Texture", GUI.skin.box);
				GUILayout.Space (20);
				targetObject.layer1 = (Texture2D)EditorGUILayout.ObjectField ("Albedo", targetObject.layer1, typeof(Texture2D), true);
				targetObject.layer_Normal_1 = (Texture2D)EditorGUILayout.ObjectField ("Normal", targetObject.layer_Normal_1, typeof(Texture2D), true);
				GUILayout.EndVertical ();
				//-----------------------------------------------------------------------------------------
				GUILayout.BeginVertical ("Property", GUI.skin.box);
				GUILayout.Space (20);
				targetObject.sUV1 = EditorGUILayout.FloatField ("UV Tile", targetObject.sUV1);
				targetObject.sSmoothness1 = EditorGUILayout.Slider ("Smoothness", targetObject.sSmoothness1, 0, 10);
				targetObject.sDisplacement1 = EditorGUILayout.Slider ("Displacement", targetObject.sDisplacement1, 0, 3);
				targetObject.sNormal1 = EditorGUILayout.Slider ("Normal Power", targetObject.sNormal1, 0, 1);
				GUILayout.EndVertical ();

			}
			if (currentLayer == 2) {
				//-----------------------------------------------------------------------------------------
				GUILayout.BeginVertical ("Texture", GUI.skin.box);
				GUILayout.Space (20);
				targetObject.layer2 = (Texture2D)EditorGUILayout.ObjectField ("Albedo", targetObject.layer2, typeof(Texture2D), true);
				targetObject.layer_Normal_2 = (Texture2D)EditorGUILayout.ObjectField ("Normal", targetObject.layer_Normal_2, typeof(Texture2D), true);
				GUILayout.EndVertical ();
				//-----------------------------------------------------------------------------------------
				GUILayout.BeginVertical ("Property", GUI.skin.box);
				GUILayout.Space (20);
				targetObject.sUV2 = EditorGUILayout.FloatField ("UV Tile", targetObject.sUV2);
				targetObject.sSmoothness2 = EditorGUILayout.Slider ("Smoothness", targetObject.sSmoothness2, 0, 10);
				targetObject.sDisplacement2 = EditorGUILayout.Slider ("Displacement", targetObject.sDisplacement2, 0, 3);
				targetObject.sNormal2 = EditorGUILayout.Slider ("Normal Power", targetObject.sNormal2, 0, 1);
				GUILayout.EndVertical ();

			}
			if (currentLayer == 3) {
				//-----------------------------------------------------------------------------------------
				GUILayout.BeginVertical ("Texture", GUI.skin.box);
				GUILayout.Space (20);
				targetObject.layer3 = (Texture2D)EditorGUILayout.ObjectField ("Albedo", targetObject.layer3, typeof(Texture2D), true);
				targetObject.layer_Normal_3 = (Texture2D)EditorGUILayout.ObjectField ("Normal", targetObject.layer_Normal_3, typeof(Texture2D), true);
				GUILayout.EndVertical ();
				//-----------------------------------------------------------------------------------------
				GUILayout.BeginVertical ("Property", GUI.skin.box);
				GUILayout.Space (20);
				targetObject.sUV3 = EditorGUILayout.FloatField ("UV Tile", targetObject.sUV3);
				targetObject.sSmoothness3 = EditorGUILayout.Slider ("Smoothness", targetObject.sSmoothness3, 0, 10);
				targetObject.sDisplacement3 = EditorGUILayout.Slider ("Displacement", targetObject.sDisplacement3, 0, 3);
				targetObject.sNormal3 = EditorGUILayout.Slider ("Normal Power", targetObject.sNormal3, 0, 1);
				GUILayout.EndVertical ();

			}
			if (currentLayer == 4) {
				//-----------------------------------------------------------------------------------------
				GUILayout.BeginVertical ("Texture", GUI.skin.box);
				GUILayout.Space (20);
				targetObject.layer4 = (Texture2D)EditorGUILayout.ObjectField ("Albedo", targetObject.layer4, typeof(Texture2D), true);
				targetObject.layer_Normal_4 = (Texture2D)EditorGUILayout.ObjectField ("Normal", targetObject.layer_Normal_4, typeof(Texture2D), true);
				GUILayout.EndVertical ();
				//-----------------------------------------------------------------------------------------
				GUILayout.BeginVertical ("Property", GUI.skin.box);
				GUILayout.Space (20);
				targetObject.sUV4 = EditorGUILayout.FloatField ("UV Tile", targetObject.sUV4);
				targetObject.sSmoothness4 = EditorGUILayout.Slider ("Smoothness", targetObject.sSmoothness4, 0, 10);
				targetObject.sDisplacement4 = EditorGUILayout.Slider ("Displacement", targetObject.sDisplacement4, 0, 3);
				targetObject.sNormal4 = EditorGUILayout.Slider ("Normal Power", targetObject.sNormal4, 0, 1);
				GUILayout.EndVertical ();

			}
			if (currentLayer == 5) {
				//-----------------------------------------------------------------------------------------
				GUILayout.BeginVertical ("Texture", GUI.skin.box);
				GUILayout.Space (20);
				targetObject.layer5 = (Texture2D)EditorGUILayout.ObjectField ("Albedo", targetObject.layer5, typeof(Texture2D), true);
				targetObject.layer_Normal_5 = (Texture2D)EditorGUILayout.ObjectField ("Normal", targetObject.layer_Normal_5, typeof(Texture2D), true);
				GUILayout.EndVertical ();
				//-----------------------------------------------------------------------------------------
				GUILayout.BeginVertical ("Property", GUI.skin.box);
				GUILayout.Space (20);
				targetObject.sUV5 = EditorGUILayout.FloatField ("UV Tile", targetObject.sUV5);
				targetObject.sSmoothness5 = EditorGUILayout.Slider ("Smoothness", targetObject.sSmoothness5, 0, 10);
				targetObject.sDisplacement5 = EditorGUILayout.Slider ("Displacement", targetObject.sDisplacement5, 0, 3);
				targetObject.sNormal5 = EditorGUILayout.Slider ("Normal Power", targetObject.sNormal5, 0, 1);
				GUILayout.EndVertical ();

			}
			EditorGUILayout.Space ();
			EditorGUILayout.Space ();
			//-----------------------------------------------------------------------------------------
			if (mTarget.autoUpdate) {
				mTarget.UpdateTerrain ();
			} else {
				if (GUILayout.Button ("Update Terrain"))
					mTarget.UpdateTerrain (); ///	mTarget.UpdateTerrain ();
			}
		}
		serializedObject.Update ();
		serializedObject.ApplyModifiedProperties ();
	}
}

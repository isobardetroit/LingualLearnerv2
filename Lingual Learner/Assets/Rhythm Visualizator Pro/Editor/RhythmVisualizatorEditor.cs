// Created by Carlos Arturo Rodriguez Silva (Legend) https://twitter.com/xLegendx97 or https://www.facebook.com/legendxh
// Thread: http://forum.unity3d.com/threads/rhythm-visualizator.423168/ 
// Video: https://www.youtube.com/watch?v=i5uRU45fi8U

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RhythmVisualizator))]
public class RhythmVisualizatorEditor : Editor
{
	public override void OnInspectorGUI()
	{
		var rhythmVisualizator = (RhythmVisualizator)target;

		if (GUILayout.Button ("Update Script")) {
			rhythmVisualizator.UpdateVisualizations ();
		}

		if (EditorApplication.isPlaying) {
			if (DrawDefaultInspector ()) {
				rhythmVisualizator.UpdateVisualizations ();
			}

		} else {
			DrawDefaultInspector ();
		}
	}
}
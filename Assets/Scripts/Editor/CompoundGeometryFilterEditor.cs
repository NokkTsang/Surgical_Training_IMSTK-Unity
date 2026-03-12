using ImstkUnity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace ImstkEditor
{
    [CustomEditor(typeof(ImstkUnity.CompoundGeometryFilter), false)]
    [InitializeOnLoad]
    class CompoundGeometryFilterEditor : Editor
    {
        bool _showHandles = false;
        public override void OnInspectorGUI()
        {
            var script = target as CompoundGeometryFilter;
            EditorGUI.BeginChangeCheck();

            _showHandles = EditorGUILayout.Toggle("Show Handle", script.showHandles);

            var geometryFilters = new List<GeometryFilter>(script.filters);
            for (int i = 0; i < geometryFilters.Count; i++)
            {
                var newFilter = EditorGUILayout.ObjectField(geometryFilters[i], typeof(GeometryFilter), true) as GeometryFilter;
                if (newFilter != null)
                {
                    if (geometryFilters[i] == newFilter)
                    {
                        continue;
                    }
                    if (newFilter.type == GeometryType.CompoundGeometry)
                    {
                        EditorUtility.DisplayDialog("Invalid Geometry Filter",
                            "Can't use the Compound Geometry as part of itself", "Ok");
                        continue;
                    }
                    if (newFilter.transform != script.transform && !newFilter.transform.IsChildOf(script.transform))
                    {
                        EditorUtility.DisplayDialog("Invalid Geometry Filter",
                            "Geometry Filter used in Compound Geometry needs on the same level as the Compound Geometry " +
                            "or underneath", "Ok");
                        continue;
                    }
                    
                }
                geometryFilters[i] = newFilter;
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+"))
            {
                geometryFilters.Add(null);
            }
            if (GUILayout.Button("-"))
            {
                geometryFilters.Remove(geometryFilters.Last());
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.HelpBox("When using the compound geometry you should only position " +
                "sub-geometries with unity transforms. Using position or orientation UI fields " +
                "may cause misalignment.", MessageType.Info);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RegisterCompleteObjectUndo(script, "Changeof GeomFilter Geom");
                script.filters = geometryFilters;
                script.RefreshGeometry();
                script.showHandles = _showHandles;
                script.SetSubShowHandles(script.showHandles);
                SceneView.RepaintAll();
            }
        }
    }
}

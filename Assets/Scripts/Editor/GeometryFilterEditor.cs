/*=========================================================================

   Library: iMSTK-Unity

   Copyright (c) Kitware, Inc. 

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

      http://www.apache.org/licenses/LICENSE-2.0.txt

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.

=========================================================================*/

using ImstkUnity;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ImstkEditor
{
    /// <summary>
    /// Used to display geometry components in the editor view and inspector
    /// </summary>
    [CustomEditor(typeof(GeometryFilter), false)]
    [InitializeOnLoad]
    class GeometryFilterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            OnGeomGUI();

            OnPrimitiveGeomGUI();
        }

        protected void OnGeomGUI()
        {
            GeometryFilter geomFilter = target as GeometryFilter;

            EditorGUI.BeginChangeCheck();

            bool showHandles = EditorGUILayout.Toggle("Show Handles", geomFilter.showHandles);
            GeometryType newType = (GeometryType)EditorGUILayout.EnumPopup("Geom Type", geomFilter.type);
            UnityEngine.Object abstractMesh = null;

            // If unity mesh, let user slot an asset
            if (newType == GeometryType.UnityMesh)
            {
                // Accepts either a Mesh or MeshFilter. If MeshFilter, then pulls the mesh out and uses that
                UnityEngine.Object obj = EditorGUILayout.ObjectField("Mesh", geomFilter.inputUnityGeom, typeof(UnityEngine.Object), true);
                if (obj as MeshFilter != null)
                {
                    abstractMesh = (obj as MeshFilter).sharedMesh;
                }
                else if (obj as Mesh != null)
                {
                    abstractMesh = obj as Mesh;
                }
                else
                {
                    if (abstractMesh != null)
                    {
                        Debug.LogWarning("Tried to set object of type " + abstractMesh.GetType().Name + " on GeometryFilter");
                    }
                }
            }
            // If an imstk mesh, let user slot an asset for it
            else if (newType == GeometryType.SurfaceMesh ||
                newType == GeometryType.LineMesh ||
                newType == GeometryType.TetrahedralMesh ||
                newType == GeometryType.HexahedralMesh ||
                newType == GeometryType.PointSet)
            {
                abstractMesh = EditorGUILayout.ObjectField("iMSTKMesh", geomFilter.inputImstkGeom, typeof(Geometry), true);
            }



            if (EditorGUI.EndChangeCheck())
            {
                Undo.RegisterCompleteObjectUndo(geomFilter, "Change of GeomFilter");

                geomFilter.showHandles = showHandles;

                // If the type changed unslot the geometry from the component
                if (newType != geomFilter.type)
                {
                    geomFilter.inputImstkGeom = null;
                    geomFilter.inputUnityGeom = null;
                    geomFilter.type = newType;
                    abstractMesh = null;
                    geomFilter.geometryJson = "";
                }

                // If the mesh changed, call the appropriate setter for it
                if (abstractMesh != null)
                {
                    if ((abstractMesh as Mesh) != null)
                        geomFilter.SetGeometry(abstractMesh as Mesh);
                    else if ((abstractMesh as Geometry) != null)
                        geomFilter.SetGeometry(abstractMesh as Geometry);
                }
                SceneView.RepaintAll();
            }


            if (geomFilter.type == GeometryType.SurfaceMesh ||
                geomFilter.type == GeometryType.UnityMesh)
            {
                ShowSaveMeshFoldout(geomFilter);
            }
        }

        protected void OnPrimitiveGeomGUI()
        {
            GeometryFilter geomFilter = target as GeometryFilter;

            if (geomFilter.type == GeometryType.Capsule)
            {
                if (geomFilter.inputImstkGeom == null)
                {
                    geomFilter.SetGeometry(CreateInstance<Capsule>());
                }
                if (geomFilter.geometryJson != "")
                {
                    JsonUtility.FromJsonOverwrite(geomFilter.geometryJson, geomFilter.inputImstkGeom);
                }

                Capsule source = geomFilter.inputImstkGeom as Capsule;
                Capsule target = CreateInstance<Capsule>();

                EditorGUI.BeginChangeCheck();

                target.center = EditorGUILayout.Vector3Field("Center", source.center);
                target.radius = Mathf.Max(EditorGUILayout.FloatField("Radius", source.radius), float.Epsilon);
                target.length = Mathf.Max(EditorGUILayout.FloatField("Length", source.length), float.Epsilon);
                target.orientation = EditorGUILayout.Vector4Field("Orientation", source.orientation.ToVector4()).ToQuat();

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RegisterCompleteObjectUndo(geomFilter, "Change of GeomFilter Geom");
                    source.center = target.center;
                    source.radius = target.radius;
                    source.length = target.length;
                    source.orientation = target.orientation;
                    geomFilter.geometryJson = JsonUtility.ToJson(target);
                    SceneView.RepaintAll();
                }
            }
            else if (geomFilter.type == GeometryType.Cylinder)
            {
                if (geomFilter.inputImstkGeom == null)
                {
                    geomFilter.SetGeometry(CreateInstance<Cylinder>());
                }
                if (geomFilter.geometryJson != "")
                {
                    JsonUtility.FromJsonOverwrite(geomFilter.geometryJson, geomFilter.inputImstkGeom);
                }
                Cylinder source = geomFilter.inputImstkGeom as Cylinder;
                Cylinder target = CreateInstance<Cylinder>();

                EditorGUI.BeginChangeCheck();

                target.center = EditorGUILayout.Vector3Field("Center", source.center);
                target.radius = Mathf.Max(EditorGUILayout.FloatField("Radius", source.radius), float.Epsilon);
                target.length = Mathf.Max(EditorGUILayout.FloatField("Length", source.length), float.Epsilon);
                target.orientation = EditorGUILayout.Vector4Field("Orientation", source.orientation.ToVector4()).ToQuat();

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RegisterCompleteObjectUndo(geomFilter, "Change of GeomFilter Geom");
                    source.center = target.center;
                    source.radius = target.radius;
                    source.length = target.length;
                    source.orientation = target.orientation;
                    geomFilter.geometryJson = JsonUtility.ToJson(target);
                    SceneView.RepaintAll();
                }
            }
            else if (geomFilter.type == GeometryType.OrientedBox)
            {
                if (geomFilter.inputImstkGeom == null)
                {
                    geomFilter.SetGeometry(CreateInstance<OrientedBox>());
                }
                if (geomFilter.geometryJson != "")
                {
                    JsonUtility.FromJsonOverwrite(geomFilter.geometryJson, geomFilter.inputImstkGeom);
                }
                OrientedBox source = geomFilter.inputImstkGeom as OrientedBox;
                OrientedBox target = CreateInstance<OrientedBox>();

                EditorGUI.BeginChangeCheck();

                target.center = EditorGUILayout.Vector3Field("Center", source.center);
                target.extents = EditorGUILayout.Vector3Field("Extents", source.extents).cwiseMax(new Vector3(float.Epsilon, float.Epsilon, float.Epsilon)); ;
                target.orientation = EditorGUILayout.Vector4Field("Orientation", source.orientation.ToVector4()).ToQuat();

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RegisterCompleteObjectUndo(geomFilter, "Change of GeomFilter Geom");
                    source.center = target.center;
                    source.extents = target.extents;
                    source.orientation = target.orientation;
                    geomFilter.geometryJson = JsonUtility.ToJson(target);
                    SceneView.RepaintAll();
                }
            }
            else if (geomFilter.type == GeometryType.Plane)
            {
                if (geomFilter.inputImstkGeom == null)
                {
                    geomFilter.SetGeometry(CreateInstance<ImstkUnity.Plane>());
                }
                if (geomFilter.geometryJson != "")
                {
                    JsonUtility.FromJsonOverwrite(geomFilter.geometryJson, geomFilter.inputImstkGeom);
                }
                ImstkUnity.Plane source = geomFilter.inputImstkGeom as ImstkUnity.Plane;
                ImstkUnity.Plane target = CreateInstance<ImstkUnity.Plane>();

                EditorGUI.BeginChangeCheck();

                target.center = EditorGUILayout.Vector3Field("Center", source.center);
                target.normal = EditorGUILayout.Vector3Field("Normal", source.normal);
                target.visualWidth = Mathf.Max(EditorGUILayout.FloatField("Visual Width", source.visualWidth), float.Epsilon);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RegisterCompleteObjectUndo(geomFilter, "Change of GeomFilter Geom");
                    source.center = target.center;
                    source.normal = target.normal;
                    source.visualWidth = target.visualWidth;
                    geomFilter.geometryJson = JsonUtility.ToJson(target);
                    SceneView.RepaintAll();
                }
            }
            else if (geomFilter.type == GeometryType.Sphere)
            {
                if (geomFilter.inputImstkGeom == null)
                {
                    geomFilter.SetGeometry(CreateInstance<Sphere>());
                }
                if (geomFilter.geometryJson != "")
                {
                    JsonUtility.FromJsonOverwrite(geomFilter.geometryJson, geomFilter.inputImstkGeom);
                }
                Sphere source = geomFilter.inputImstkGeom as Sphere;
                Sphere target = CreateInstance<Sphere>();

                EditorGUI.BeginChangeCheck();

                target.center = EditorGUILayout.Vector3Field("Center", source.center);
                target.radius = Mathf.Max(EditorGUILayout.FloatField("Radius", source.radius), float.Epsilon);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RegisterCompleteObjectUndo(geomFilter, "Change of GeomFilter Geom");
                    source.center = target.center;
                    source.radius = target.radius;
                    geomFilter.geometryJson = JsonUtility.ToJson(target);
                    SceneView.RepaintAll();
                }
            }
            else if (geomFilter.type == GeometryType.CompoundGeometry)
            {
                EditorGUILayout.HelpBox("Use the CompoundGeometryFilter Component for modelling " +
                    "compound geometries",
                    MessageType.Warning);
            }
        }

        [DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.NotInSelectionHierarchy)]
        static void DrawHandles(GeometryFilter geomFilter, GizmoType gizmoType)
        {
            if (!geomFilter.showHandles) return;
            if (geomFilter.inputUnityGeom == null && geomFilter.inputImstkGeom == null) return;

            if (geomFilter.type == GeometryType.UnityMesh)
            {
                Transform transform = geomFilter.gameObject.GetComponent<Transform>();
                Mesh mesh = geomFilter.inputUnityGeom;
                ImstkGizmos.DrawMesh(mesh, transform.position, transform.rotation, transform.lossyScale);
            }
            else
            {
                Transform transform = geomFilter.gameObject.GetComponent<Transform>();
                Geometry geom = geomFilter.inputImstkGeom;
                if (geom.geomType == GeometryType.Capsule)
                {
                    Capsule capsule = geom as Capsule;

                    Mesh displayMesh = capsule.GetMesh();
                    Gizmos.DrawWireMesh(displayMesh, 0, transform.position, transform.rotation, new Vector3(1.0f, 1.0f, 1.0f));
                }
                else if (geom.geomType == GeometryType.Cylinder)
                {
                    Cylinder cylinder = geom as Cylinder;

                    Mesh displayMesh = cylinder.GetMesh();
                    Gizmos.DrawWireMesh(displayMesh, 0, transform.position, transform.rotation, new Vector3(1.0f, 1.0f, 1.0f));
                }
                else if (geom.geomType == GeometryType.OrientedBox)
                {
                    OrientedBox orientedBox = geom as OrientedBox;

                    Mesh displayMesh = orientedBox.GetMesh();
                    Gizmos.DrawWireMesh(displayMesh, 0, transform.position, transform.rotation, transform.localScale);
                }
                else if (geom.geomType == GeometryType.Plane)
                {
                    // Unity's planes default config for drawing is normal along z. So we rotate from z to normal
                    ImstkUnity.Plane plane = geom as ImstkUnity.Plane;
                    Handles.RectangleHandleCap(0, plane.GetTransformedCenter(transform),
                       Quaternion.FromToRotation(Vector3.forward, plane.GetTransformedNormal(transform)), plane.visualWidth, EventType.Repaint);
                }
                else if (geom.geomType == GeometryType.Sphere)
                {
                    Sphere sphere = geom as Sphere;
                    Mesh displayMesh = sphere.GetMesh();
                    Gizmos.DrawWireMesh(displayMesh, 0, transform.position, transform.rotation, transform.lossyScale);
                }
                else if (geom.geomType == GeometryType.PointSet ||
                        geom.geomType == GeometryType.LineMesh ||
                        geom.geomType == GeometryType.SurfaceMesh)
                {
                    ImstkMesh mesh = geom as ImstkMesh;
                    ImstkGizmos.DrawMesh(mesh.ToMesh(), transform.position, transform.rotation, transform.localScale);
                }
                else if (geom.geomType == GeometryType.TetrahedralMesh)
                {
                    var tetMesh = Imstk.Utils.CastTo<Imstk.TetrahedralMesh>(geomFilter.GetOutputGeometry());
                    Imstk.SurfaceMesh surfMesh = tetMesh.extractSurfaceMesh();
                    Debug.Assert(tetMesh != null);
                    var toWorld = transform.localToWorldMatrix;
                    surfMesh.transform(toWorld.ToMat4d());
                    surfMesh.updatePostTransformData();
                    // Note probably not really performant ...
                    Gizmos.DrawWireMesh(surfMesh.ToMesh());
                }
            }
        }

        private void WriteMesh(GeometryFilter script, string type)
        {
            var components = script.GetComponents<GeometryFilter>();
            int i = Array.IndexOf(components, script);
            script.WriteMesh(script.gameObject.name + "_" + i.ToString() + type);
        }

        private void ShowSaveMeshFoldout(GeometryFilter geomFilter)
        {
            _foldoutOpen = EditorGUILayout.Foldout(_foldoutOpen, "Save Mesh");
            if (_foldoutOpen)
            {
                _saveName = EditorGUILayout.TextField("Filename (.obj)", _saveName);
                _applyTransform = EditorGUILayout.Toggle("Apply Transform", _applyTransform);

                if (GUILayout.Button("Write Mesh"))
                {
                    var parts = _saveName.Split('.');
                    if (parts.Length > 1)
                    {
                        var ending = parts[^1];
                        var validEndings = new string[] { "obj", "stl", "ply" };
                        if (!validEndings.Contains(ending))
                        {
                            Debug.LogWarning($"Invalid Ending {ending} only .obj, .stl, .ply are valid, defaulting to .obj");
                            var period = _saveName.LastIndexOf(".");
                            _saveName = _saveName.Substring(0, period) + ".obj";
                        }
                    } else
                    {
                        _saveName += ".obj";
                    }
                    geomFilter.WriteMesh(_saveName, _applyTransform);
                }
            }
        }

        bool _foldoutOpen = false;
        string _saveName = "mesh";
        bool _applyTransform = true;
    }
}
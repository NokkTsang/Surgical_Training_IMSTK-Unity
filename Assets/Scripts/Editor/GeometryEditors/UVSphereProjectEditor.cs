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
using UnityEngine;
using UnityEditor;

namespace ImstkEditor
{
    /// <summary>
    /// Generates UV sphere tcoords via imgui
    /// </summary>
    public class UVSphereProjectEditor : EditorWindow
    {
        public float radius = 1.0f;
        public float uvScale = 2.0f;
        public Vector3 center = new Vector3(0.0f, 0.0f, 0.0f);
        public Mesh inputMesh = null;
        public Mesh outputMesh = null;

        public static void Init(Mesh inputMesh, Mesh outputMesh)
        {
            UVSphereProjectEditor window = GetWindow(typeof(UVSphereProjectEditor)) as UVSphereProjectEditor;
            window.inputMesh = inputMesh;
            window.outputMesh = outputMesh;
            // Initialize to the bounds of the input mesh
            window.center = inputMesh.bounds.center;
            window.radius = inputMesh.bounds.size.magnitude * 0.5f;
            window.UpdateEditorResults();
            window.Show();
        }

        void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            inputMesh = EditorGUILayout.ObjectField("Input Mesh: ", inputMesh, typeof(Mesh), true) as Mesh;
            Vector3 tCenter = EditorGUILayout.Vector3Field("Center: ", center);
            float tRadius = EditorGUILayout.FloatField("Radius: ", radius);
            float tUvScale = EditorGUILayout.FloatField("UV Scale: ", uvScale);

            // \todo: How to get undo to also call UpdateInputObj?
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RegisterCompleteObjectUndo(this, "Change of Parameters");
                radius = Mathf.Max(tRadius, 0.0f);
                uvScale = tUvScale;
                center = tCenter;

                UpdateEditorResults();
            }
        }

        private void UpdateEditorResults()
        {
            GeomUtil.CopyMesh(inputMesh, outputMesh);

            Vector3[] vertices = outputMesh.vertices;
            Vector2[] uvs = new Vector2[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 diff = vertices[i] - center;

                // Compute phi and theta on the sphere
                float theta = Mathf.Asin(diff.x / radius);
                float phi = Mathf.Atan2(diff.y, diff.z);
                uvs[i] = new Vector2(phi / (Mathf.PI * 2.0f) + 0.5f, theta / (Mathf.PI * 2.0f) + 0.5f) * uvScale;
            }
            outputMesh.SetUVs(0, uvs);
        }
    }
}
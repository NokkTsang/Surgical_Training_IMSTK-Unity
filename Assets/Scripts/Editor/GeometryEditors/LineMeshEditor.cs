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
    /// Generates a plane mesh for MeshFilter
    /// </summary>
    public class LineMeshEditor : EditorWindow
    {
        public Vector3 start = Vector3.zero;
        public Vector3 direction = new Vector3(1.0f, 0.0f, 0.0f);
        public int divisions = 10;
        public double length = 1.0;
        public Mesh outputMesh = null;

        public static void Init(Mesh outputMesh)
        {
            LineMeshEditor window = GetWindow(typeof(LineMeshEditor)) as LineMeshEditor;
            window.outputMesh = outputMesh;
            window.UpdateEditorResults();
            window.Show();
        }

        void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            outputMesh = EditorGUILayout.ObjectField("Input Mesh: ", outputMesh, typeof(Mesh), true) as Mesh;
            Vector3 tStart = EditorGUILayout.Vector3Field("Start", start);
            Vector3 tDirection = EditorGUILayout.Vector3Field("Direction", direction);
            int tDivisions = EditorGUILayout.IntField("Divisions", divisions);
            double tLength = EditorGUILayout.DoubleField("Length", length);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RegisterCompleteObjectUndo(this, "Change of Parameters");
                start = tStart;
                direction = tDirection;
                if (direction == Vector3.zero)
                {
                    direction = new Vector3(1.0f, 0.0f, 0.0f);
                }
                divisions = Mathf.Max(tDivisions, 1);
                length = MathUtil.Max(tLength, 0.0);

                UpdateEditorResults();
            }
        }

        private void UpdateEditorResults()
        {
            int numVerts = divisions + 1;
            int numCells = divisions;

            Vector3[] vertices = new Vector3[numVerts];
            int[] indices = new int[numCells * 2];

            direction = direction.normalized;
            for (int i = 0; i < numVerts; i++)
            {
                float t = (float)i / (numVerts - 1); // 0-1
                vertices[i] = start + direction * t * (float)length;
            }

            for (int i = 0; i < numCells; i++)
            {
                indices[i * 2] = i;
                indices[i * 2 + 1] = i + 1;
            }

            outputMesh.triangles = null;
            outputMesh.normals = null;
            outputMesh.tangents = null;

            outputMesh.vertices = vertices;
            outputMesh.SetIndices(indices, MeshTopology.Lines, 0);

            outputMesh.RecalculateBounds();
        }
    }
}
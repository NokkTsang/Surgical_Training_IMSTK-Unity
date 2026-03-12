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
    public enum UVPlaneOptions
    {
        XY,
        YZ,
        XZ,
        Custom
    }

    /// <summary>
    /// Generates UV plane coords
    /// </summary>
    public class UVPlaneProjectEditor : EditorWindow
    {
        public Vector3 planeNormal = new Vector3(0.0f, 1.0f, 0.0f);
        public UVPlaneOptions planeOption = UVPlaneOptions.XZ;
        public Vector2 uvScale = new Vector2(1.0f, 1.0f);
        public Vector2 uvShift = Vector2.zero;
        public Mesh inputMesh = null;
        public Mesh outputMesh = null;

        public static void Init(Mesh inputMesh, Mesh outputMesh)
        {
            UVPlaneProjectEditor window = GetWindow(typeof(UVPlaneProjectEditor)) as UVPlaneProjectEditor;
            window.inputMesh = inputMesh;
            window.outputMesh = outputMesh;
            window.UpdateEditorResults();
            window.Show();
        }

        void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            inputMesh = EditorGUILayout.ObjectField("Input Mesh: ", inputMesh, typeof(Mesh), true) as Mesh;
            Vector3 tPlaneNormal = EditorGUILayout.Vector3Field("Plane Normal: ", planeNormal);
            UVPlaneOptions tPlaneOption = (UVPlaneOptions)EditorGUILayout.EnumPopup("Plane: ", planeOption);
            Vector2 tUvScale = EditorGUILayout.Vector2Field("UV Scale", uvScale);
            Vector2 tUvShift = EditorGUILayout.Vector2Field("UV Shift", uvShift);

            // \todo: How to get undo to also call UpdateInputObj?
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RegisterCompleteObjectUndo(this, "Change of Parameters");

                // IF the plane normal was changed
                if (tPlaneNormal != planeNormal)
                {
                    if (tPlaneNormal == new Vector3(0.0f, 1.0f, 0.0f))
                    {
                        tPlaneOption = UVPlaneOptions.XZ;
                    }
                    else if (tPlaneNormal == new Vector3(1.0f, 0.0f, 0.0f))
                    {
                        tPlaneOption = UVPlaneOptions.YZ;
                    }
                    else if (tPlaneNormal == new Vector3(0.0f, 0.0f, 1.0f))
                    {
                        tPlaneOption = UVPlaneOptions.XY;
                    }
                    else
                    {
                        tPlaneOption = UVPlaneOptions.Custom;
                    }
                }
                if (tPlaneOption != planeOption)
                {
                    if (tPlaneOption == UVPlaneOptions.XY)
                    {
                        tPlaneNormal = new Vector3(0.0f, 0.0f, 1.0f);
                    }
                    else if (tPlaneOption == UVPlaneOptions.XZ)
                    {
                        tPlaneNormal = new Vector3(0.0f, 1.0f, 0.0f);
                    }
                    else if (tPlaneOption == UVPlaneOptions.YZ)
                    {
                        tPlaneNormal = new Vector3(1.0f, 0.0f, 0.0f);
                    }
                }

                planeNormal = tPlaneNormal;
                planeOption = tPlaneOption;
                uvScale = tUvScale;
                uvShift = tUvShift;

                UpdateEditorResults();
            }
        }

        private void UpdateEditorResults()
        {
            GeomUtil.CopyMesh(inputMesh, outputMesh);

            Bounds bounds = outputMesh.bounds;
            Vector3 size = bounds.size;

            Vector3[] vertices = outputMesh.vertices;
            Vector2[] uvs = new Vector2[vertices.Length];

            for (int i = 0; i < vertices.Length; i++)
            {
                uvs[i] = new Vector2(vertices[i].x / size.x, vertices[i].z / size.z) * uvScale + uvShift;
            }
            outputMesh.SetUVs(0, uvs);
        }
    }
}
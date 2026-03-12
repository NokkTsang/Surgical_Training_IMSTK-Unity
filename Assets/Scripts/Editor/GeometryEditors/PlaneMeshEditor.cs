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
    public class PlaneMeshEditor : EditorWindow
    {
        //public Vector3 size = new Vector3(1.0f, 0.25f, 1.0f);
        public Vector2Int dim = new Vector2Int(8, 8);
        //public Vector3 center = new Vector3(0.0f, 0.0f, 0.0f);
        public Mesh outputMesh = null;

        public static void Init(Mesh outputMesh)
        {
            PlaneMeshEditor window = GetWindow(typeof(PlaneMeshEditor)) as PlaneMeshEditor;
            window.outputMesh = outputMesh;
            if (outputMesh.name.Length == 0) outputMesh.name = "plane mesh";
            window.UpdateEditorResults();
            window.Show();
        }

        void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            outputMesh = EditorGUILayout.ObjectField("Input Mesh: ", outputMesh, typeof(Mesh), true) as Mesh;
            name = EditorGUILayout.TextField("Name: ", outputMesh.name);
            //Vector3 tSize = EditorGUILayout.Vector3Field("Size: ", size);
            Vector2Int tDim = EditorGUILayout.Vector2IntField("Grid Dimensions: ", dim);
            //Vector3 tCenter = EditorGUILayout.Vector3Field("Center: ", center);

            // \todo: How to get undo to also call UpdateInputObj?
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RegisterCompleteObjectUndo(this, "Change of Parameters");
                //size = tSize.cwiseMax(new Vector3(0.0f, 0.0f, 0.0f));
                dim = tDim.cwiseMax(new Vector2Int(2, 2));
                //center = tCenter;
                outputMesh.name = name;
                UpdateEditorResults();
            }
        }

        private void UpdateEditorResults()
        {
            ImstkMesh planeMesh = Utility.GetXYPlane(dim.x, dim.y);
            GeomUtil.CopyMesh(planeMesh.ToMesh(), outputMesh);
        }
    }
}
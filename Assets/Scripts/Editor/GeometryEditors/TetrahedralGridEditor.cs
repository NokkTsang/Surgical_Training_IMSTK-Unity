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
    /// Generates a tetrahedral mesh PhysicsGeometry
    /// </summary>
    public class TetrahedralGridEditor : EditorWindow
    {
        public Vector3 size = new Vector3(1.0f, 0.25f, 1.0f);
        public Vector3Int dim = new Vector3Int(8, 4, 8);
        public Vector3 center = new Vector3(0.0f, 0.0f, 0.0f);
        public Mesh outputSurfMesh = null;
        public ImstkMesh outputTetMesh = null;

        public static void Init(
            Mesh outputSurfMesh,
            ImstkMesh outputTetMesh)
        {
            TetrahedralGridEditor window = GetWindow(typeof(TetrahedralGridEditor)) as TetrahedralGridEditor;
            window.outputSurfMesh = outputSurfMesh;
            window.outputTetMesh = outputTetMesh;
            window.UpdateEditorResults();
            window.Show();
        }

        void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            outputSurfMesh = EditorGUILayout.ObjectField("Input Surface MeshFilter: ", outputSurfMesh, typeof(Mesh), true) as Mesh;
            outputTetMesh = EditorGUILayout.ObjectField("Input Tet GeometryFilter: ", outputTetMesh, typeof(ImstkMesh), true) as ImstkMesh;
            Vector3 tSize = EditorGUILayout.Vector3Field("Size: ", size);
            Vector3Int tDim = EditorGUILayout.Vector3IntField("Grid Dimensions: ", dim);
            Vector3 tCenter = EditorGUILayout.Vector3Field("Center: ", center);

            // \todo: How to get undo to also call UpdateInputObj?
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RegisterCompleteObjectUndo(this, "Change of Parameters");
                size = tSize.cwiseMax(new Vector3(0.0f, 0.0f, 0.0f));
                dim = tDim.cwiseMax(new Vector3Int(2, 2, 2));
                center = tCenter;

                UpdateEditorResults();
            }
        }

        private void UpdateEditorResults()
        {
            ImstkMesh tetMesh = Utility.GetTetGridMesh(size, dim, center);
            GeomUtil.CopyMesh(tetMesh, outputTetMesh);

            Imstk.TetrahedralMesh imstkTetMesh = Imstk.Utils.CastTo<Imstk.TetrahedralMesh>(tetMesh.ToImstkGeometry());
            Imstk.SurfaceMesh surfMesh = imstkTetMesh.extractSurfaceMesh();
            GeomUtil.CopyMesh(surfMesh.ToMesh(), outputSurfMesh);
        }
    }
}
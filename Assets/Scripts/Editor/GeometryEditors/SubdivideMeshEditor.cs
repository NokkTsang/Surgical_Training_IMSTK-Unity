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
    public enum SubdivideMeshType
    {
        Linear,
        Loop,
        Butterfly
    }

    /// <summary>
    /// Laplacian smoothens an input unity Mesh providing
    /// an output one
    /// </summary>
    public class SubdivideMeshEditor : EditorWindow
    {
        public int numSubdivisions = 1;
        public Imstk.SurfaceMeshSubdivide.Type subdivType = Imstk.SurfaceMeshSubdivide.Type.LINEAR;
        public Mesh inputMesh = null;
        public Mesh outputMesh = null;

        public static void Init(Mesh inputMesh, Mesh outputMesh)
        {
            SubdivideMeshEditor window = GetWindow(typeof(SubdivideMeshEditor)) as SubdivideMeshEditor;
            window.inputMesh = inputMesh;
            window.outputMesh = outputMesh;
            window.UpdateEditorResults();
            window.Show();
        }

        void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            inputMesh = EditorGUILayout.ObjectField("Input Mesh: ", inputMesh, typeof(Mesh), true) as Mesh;
            int tNumSubdivisions = EditorGUILayout.IntField("Number Of Subdivisions: ", numSubdivisions);
            Imstk.SurfaceMeshSubdivide.Type tSubdivType =
                (Imstk.SurfaceMeshSubdivide.Type)EditorGUILayout.EnumPopup("Subdivision Type: ", subdivType);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RegisterCompleteObjectUndo(this, "Change of Parameters");
                numSubdivisions = MathUtil.Max(tNumSubdivisions, 0);
                subdivType = tSubdivType;

                UpdateEditorResults();
            }
        }

        private void UpdateEditorResults()
        {
            Imstk.SurfaceMesh surfMesh = inputMesh.ToImstkGeometry() as Imstk.SurfaceMesh;

            Imstk.SurfaceMeshSubdivide subdiv = new Imstk.SurfaceMeshSubdivide();
            subdiv.setInputMesh(surfMesh);
            subdiv.setNumberOfSubdivisions(numSubdivisions);
            subdiv.setSubdivisionType(subdivType);
            subdiv.update();

            Imstk.SurfaceMesh outputSurfMesh = Imstk.Utils.CastTo<Imstk.SurfaceMesh>(subdiv.getOutput());
            GeomUtil.CopyMesh(outputSurfMesh.ToMesh(), outputMesh);
        }
    }
}
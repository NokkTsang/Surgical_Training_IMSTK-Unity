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
    /// For menu'd geometry operations. Could spin off into own GUI as it gets larger.
    /// GUI for parameters needs to be done up as well for some
    /// </summary>
    class GeometryMenuItems
    {
        /// <summary>
        /// Creates UVs per vertex on plane for GameObjects MeshFilter mesh
        /// </summary>
        [MenuItem("iMSTK/Geometry/ProjectUVPlane")]
        private static void ProjectUVPlane()
        {
            Vector2 scale = new Vector2(1.0f, 1.0f);

            // \todo: Plane option
            GameObject inputObj = Selection.activeObject as GameObject;
            if (inputObj == null)
            {
                Debug.LogWarning("ProjectUVPlane failed, no object selected.");
                return;
            }
            MeshFilter meshFilter = inputObj.GetComponentOrCreate<MeshFilter>();
            Mesh inputMesh = meshFilter.sharedMesh;
            meshFilter.sharedMesh = new Mesh();
            meshFilter.sharedMesh.name = inputMesh.name;

            UVPlaneProjectEditor.Init(inputMesh, meshFilter.sharedMesh);
        }

        /// <summary>
        /// Creates UVs per vertex on sphere for GameObjects MeshFilter mesh
        /// </summary>
        [MenuItem("iMSTK/Geometry/ProjectUVSphere")]
        private static void ProjectUVSphere()
        {
            GameObject inputObj = Selection.activeObject as GameObject;
            if (inputObj == null)
            {
                Debug.LogWarning("ProjectUVSphere failed, no object selected.");
                return;
            }
            MeshFilter meshFilter = inputObj.GetComponentOrCreate<MeshFilter>();
            Mesh inputMesh = meshFilter.sharedMesh;
            meshFilter.sharedMesh = new Mesh();
            meshFilter.sharedMesh.name = inputMesh.name;

            UVSphereProjectEditor.Init(inputMesh, meshFilter.sharedMesh);
        }

        /// <summary>
        /// Laplacian smoothens a mesh
        /// </summary>
        [MenuItem("iMSTK/Geometry/LaplaceSmoothMesh")]
        private static void LaplaceSmoothMesh()
        {
            GameObject inputObj = Selection.activeObject as GameObject;
            if (inputObj == null)
            {
                Debug.LogWarning("LaplaceSmoothMesh failed, no object selected.");
                return;
            }
            MeshFilter meshFilter = inputObj.GetComponentOrCreate<MeshFilter>();
            Mesh inputMesh = meshFilter.sharedMesh;
            meshFilter.sharedMesh = new Mesh();
            meshFilter.sharedMesh.name = inputMesh.name;

            LaplaceSmoothEditor.Init(inputMesh, meshFilter.sharedMesh);
        }

        /// <summary>
        /// Subdivides a triangle mesh
        /// </summary>
        [MenuItem("iMSTK/Geometry/SubdivideMesh")]
        private static void SubdivideMesh()
        {
            GameObject inputObj = Selection.activeObject as GameObject;
            if (inputObj == null)
            {
                Debug.LogWarning("LaplaceSmoothMesh failed, no object selected.");
                return;
            }
            MeshFilter meshFilter = inputObj.GetComponentOrCreate<MeshFilter>();
            Mesh inputMesh = meshFilter.sharedMesh;
            meshFilter.sharedMesh = new Mesh();
            meshFilter.sharedMesh.name = inputMesh.name;

            SubdivideMeshEditor.Init(inputMesh, meshFilter.sharedMesh);
        }

        /// <summary>
        /// Generates a tet grid on the currently selected object
        /// </summary>
        [MenuItem("iMSTK/Geometry/Generate TetGrid")]
        private static void GenerateTetGrid()
        {
            GameObject inputObj = Selection.activeObject as GameObject;
            if (inputObj == null)
            {
                Debug.LogWarning("TetGrid generation failed, no object selected.");
                return;
            }
            MeshFilter meshFilter = inputObj.GetComponentOrCreate<MeshFilter>();
            Mesh inputMesh = meshFilter.sharedMesh;
            meshFilter.sharedMesh = new Mesh();
            meshFilter.sharedMesh.name = inputMesh.name;

            GeometryFilter tetGridFilter = inputObj.AddComponent<GeometryFilter>();
            ImstkMesh tetMesh = ScriptableObject.CreateInstance<ImstkMesh>();
            tetMesh.geomType = GeometryType.TetrahedralMesh;
            tetGridFilter.SetGeometry(tetMesh);

            // Operates on the whole object as it provides the tetrahedral mesh on a separate geometry
            TetrahedralGridEditor.Init(meshFilter.sharedMesh, tetGridFilter.inputImstkGeom as ImstkMesh);
        }

        /// <summary>
        /// Generates a plane mesh on the currently selected object
        /// </summary>
        [MenuItem("iMSTK/Geometry/Generate Plane")]
        private static void GeneratePlane()
        {
            GameObject inputObj = Selection.activeObject as GameObject;
            if (inputObj == null)
            {
                Debug.LogWarning("Plane generation failed, no object selected.");
                return;
            }
            MeshFilter meshFilter = inputObj.GetComponentOrCreate<MeshFilter>();
            PlaneMeshEditor.Init(meshFilter.sharedMesh);
        }

        /// <summary>
        /// Generates a line mesh on the currently selected object
        /// </summary>
        [MenuItem("iMSTK/Geometry/Generate LineMesh")]
        private static void GenerateLineMesh()
        {
            GameObject inputObj = Selection.activeObject as GameObject;
            if (inputObj == null)
            {
                Debug.LogError("Select an object before opening the LineMesh Editor");
                return;
            }
            MeshFilter meshFilter = inputObj.GetComponentOrCreate<MeshFilter>();
            inputObj.GetComponentOrCreate<MeshRenderer>();
            if (meshFilter.sharedMesh == null)
            {
                meshFilter.sharedMesh = new Mesh();
                meshFilter.sharedMesh.name = inputObj.name + "_mesh";
            } else
            {
                Debug.LogWarning("This will overwrite the current mesh.");
            }
            LineMeshEditor.Init(meshFilter.sharedMesh);
        }
    }
}
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


using Imstk;
using UnityEngine;

namespace ImstkUnity
{
    /// <summary>
    /// Similar to a MeshFilter in Unity.It provides an input and output geometry. 
    /// It may take in any iMSTK geometry, as well as a Unity Mesh
    /// (one can also drag/drop a MeshFilter to it). These are instances of geometries
    /// used in all of iMSTK unity scripts.Instances of this class fit into 
    /// the ``Visual Geometry``, ``Physics Geometry``, and ``Collision Geometry`` 
    /// slots on the model  components.
    /// </summary>
    [AddComponentMenu("iMSTK/GeometryFilter")]
    public class GeometryFilter : ImstkBehaviour
    {
        // GeometryFilter supports either type
        public Geometry inputImstkGeom = null;

        // HS 2023-nov-6
        // Don't set, used by the editor to be able to create prefabs with 
        // the scriptable object hierarchy, issues with copying and prefab creation
        // seem to be related to the fact that we are using virtual inheritance _and_
        // scriptable objects here
        public string geometryJson = "";
        
        public Mesh inputUnityGeom = null;

        // It can output this type
        // Note: Imstk.Geometry cannot transition over editor and runtime
        public Imstk.Geometry outputImstkGeom = null;

        // Used to denote which should be seated, not what is currently
        public GeometryType type = GeometryType.UnityMesh;

        // Used to toggle drawing of it in the editor, ideally this would not
        // exist in runtime code, but unity's architecture doesn't allow
        public bool showHandles = true;
        private bool inGlobalSpace = false;

        /// <summary>
        /// Moves the geometry to a global transform
        /// </summary>
        public void MoveToGlobalSpace()
        {
            // Models in Unity are provided in Mesh pre transformed.
            // We move the entire model to world space for simulation. Meaning the transform
            // is applied before giving the simulation geometry. Here we store that initial
            // transform and reset the local transform

            if (inGlobalSpace) return;

            var localToWorld = gameObject.transform.localToWorldMatrix.ToMat4d();
            var filters = gameObject.GetComponents<GeometryFilter>();
            foreach (var filter in filters)
            {
                if (filter.inGlobalSpace) continue;
                // World coordinates
                var geometry = filter.GetOutputGeometry();
                geometry.transform(localToWorld, Imstk.Geometry.TransformType.ApplyToData);
                geometry.setTransform(Imstk.Mat4d.Identity());
                filter.inGlobalSpace = true;
            }

            gameObject.transform.localPosition = Vector3.zero;
            gameObject.transform.localScale = Vector3.one;
            gameObject.transform.localRotation = Quaternion.identity;

            // Re-parent to itself for the period of the simulation run, this avoids having 
            // to recalculate the local transforms when the object is in a hierarchy
            gameObject.transform.SetParent(null, false);
        }

        public void OnValidate()
        {
            // This fixes issues with copying between scenes, i don't know where
            // the json information gets lots of if it just never gets correctly generated
            var isMesh = (type == GeometryType.SurfaceMesh ||
                type == GeometryType.LineMesh ||
                type == GeometryType.TetrahedralMesh ||
                type == GeometryType.HexahedralMesh ||
                type == GeometryType.PointSet ||
                type == GeometryType.UnityMesh);
            if (geometryJson == "" && !isMesh && inputImstkGeom != null)
            {
                geometryJson = JsonUtility.ToJson(inputImstkGeom);
            }
        }

        public void SetGeometry(Geometry geom)
        {
            inputImstkGeom = geom;
            type = geom.geomType;
        }
        public void SetGeometry(Mesh geom)
        {
            inputUnityGeom = geom;
            type = GeometryType.UnityMesh;
        }

        /// <summary>
        /// This will convert the input to output, allocating in native code
        /// </summary>
        virtual public Imstk.Geometry GetOutputGeometry() 
        {
            if (outputImstkGeom == null)
            {
                if (type == GeometryType.UnityMesh)
                {
                    if (inputUnityGeom == null)
                    {
                        Debug.LogError($"GeometryFilter on {gameObject.name}: inputUnityGeom is null but type is set to UnityMesh");
                        return null;
                    }
                    outputImstkGeom = inputUnityGeom.ToImstkGeometry();
                }
                else
                {
                    if (inputImstkGeom == null)
                    {
                        Debug.LogError($"GeometryFilter on {gameObject.name}: inputImstkGeom is null but type is set to {type}");
                        return null;
                    }
                    outputImstkGeom = inputImstkGeom.ToImstkGeometry();
                }
            }
            return outputImstkGeom;
        }

        /// <summary>
        /// Write mesh in this filter to file, useful for debugging
        /// </summary>
        public void WriteMesh(string filename, bool applyTransform = false)
        {
            var mesh = Imstk.Utils.CastTo<Imstk.SurfaceMesh>(GetOutputGeometry());
            if (mesh == null)
            {
                Debug.LogError("Could not generate output mesh");
                return;
            }
            if (applyTransform)
            {
                var mat = transform.localToWorldMatrix.ToMat4d();
                mesh.transform(mat, Imstk.Geometry.TransformType.ApplyToData);
            }
            //mesh.flipNormals();
            mesh.correctWindingOrder();
            mesh.computeVertexNormals();
            mesh.computeTrianglesNormals();
            Imstk.MeshIO.write(mesh, filename);
        }
    }
}
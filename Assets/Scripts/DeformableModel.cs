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
using System.Linq;
using Unity.Profiling;
using UnityEngine;

namespace ImstkUnity
{


    /// <summary>
    /// Used for models with deformable vertices. That is the vertices are changing
    /// per update of the model in global configuration.
    /// </summary>
    public abstract class DeformableModel : DynamicalModel
    {
        static readonly ProfilerMarker s_UpdatePerfMarker = new ProfilerMarker(ProfilerCategory.Physics, "Imstk.DeformableUpdate");
        private TimingBuffer _updateTimes = new TimingBuffer(100);

        // These filters can accept either imstk or unity geometry input
        // and output imstk geometry
        public GeometryFilter visualGeomFilter;
        public GeometryFilter physicsGeomFilter;
        public GeometryFilter collisionGeomFilter;

        public bool cleanVisualMesh;

        public TimingBuffer UpdateTimes => _updateTimes;

        private delegate void DynamicUpdateDelegate();
        private DynamicUpdateDelegate _updateMesh;
        private Vector3[] _vertices;
        private Vector3[] _normals;
        private Vector4[] _tangents;
        private int[] _indices;
        private bool _didShift = false;
        protected override void OnImstkInit()
        {
            _updateMesh = StaticMeshUpdate;

            if (imstkObject != null) return;
            // Get dependencies
            meshFilter = visualGeomFilter.gameObject.GetComponentFatal<MeshFilter>();

            if (meshFilter.mesh == null)
            {
                meshFilter.mesh = new Mesh();
            }
            // Make sure mesh filter is read/writable
            meshFilter.mesh.MarkDynamic();
            if (!meshFilter.mesh.isReadable)
            {
                Debug.LogError(gameObject.name + "'s MeshFilter Mesh must be readable (check the meshes import settings)");
                return;
            }

            imstkObject = InitObject();

            // TODO move to simulation manager 
            SimulationManager.sceneManager.getActiveScene().addSceneObject(imstkObject);
            InitGeometry();
            InitGeometryMaps();
            ProcessBoundaryConditions(gameObject.GetComponents<BoundaryCondition>());
            Configure();
        }

        protected override void InitGeometry()
        {
            // Copy all the geometries over to imstk, set the transform and
            // apply later. (to avoid applying transform twice *since two
            // geometries could point to the same one*)

            Imstk.Geometry visualGeom = null;
            // Setup the visual geometry
            if (visualGeomFilter != null)
            {
                visualGeom = GetVisualGeometry();
                visualGeomFilter.MoveToGlobalSpace();
                imstkObject.setVisualGeometry(visualGeom);
            }

            // Visual geometry is Unity this also means it's either a surface or a line mesh
            // For either we do want to clean up the mesh.
            // As the indices not going to match after cleanup a map from the (cleaned) physics mesh 
            // to the visual mesh needs to be added as well
            Imstk.Geometry physicsGeom = null;
            Imstk.PointwiseMap physicsToUnityMeshMap = null;
            if (visualGeomFilter == physicsGeomFilter && cleanVisualMesh)
            {
                var visualMesh = Imstk.Utils.CastTo<Imstk.SurfaceMesh>(visualGeom);
                Debug.Assert(visualMesh != null);

                Debug.Log("Cleaning Mesh");
                Debug.Log("Visual Mesh " + visualMesh.getNumVertices() + " vertices");

                var cleaner = new Imstk.CleanMesh();
                cleaner.setInputMesh(visualMesh);
                cleaner.setTolerance(0.001);
                cleaner.update();
                var physicsMesh = cleaner.getOutputMesh();
                physicsGeom = physicsMesh;
                Debug.Log("Physics Mesh " + physicsMesh.getNumVertices() + " vertices");

                physicsGeomFilter.outputImstkGeom = physicsMesh;
                
                physicsToUnityMeshMap = new Imstk.PointwiseMap(physicsGeom, visualGeom);
                // Tolerance needed to avoid issues with double/float conversions
                physicsToUnityMeshMap.setTolerance(1e-4);
                physicsToUnityMeshMap.compute();
                
                // TODO Should warn the user if the map doesn't actually map any points

                (imstkObject as Imstk.DynamicObject).setPhysicsGeometry(physicsGeom);
                (imstkObject as Imstk.DynamicObject).getDynamicalModel().setModelGeometry(physicsGeom);
                (imstkObject as Imstk.DynamicObject).setPhysicsToVisualMap(physicsToUnityMeshMap);
            }
            else if (physicsGeomFilter != null)
            {
                physicsGeom = GetPhysicsGeometry();
                physicsGeomFilter.MoveToGlobalSpace();
                (imstkObject as Imstk.DynamicObject).setPhysicsGeometry(physicsGeom);
                (imstkObject as Imstk.DynamicObject).getDynamicalModel().setModelGeometry(physicsGeom);
            }
            else
            {
                Debug.LogError("No physics geometry provided to DynamicalModel on object " + gameObject.name);
            }


            if (collisionGeomFilter != null)
            {
                if (visualGeomFilter == collisionGeomFilter && visualGeomFilter == physicsGeomFilter)
                {
                    imstkObject.setCollidingGeometry(physicsGeom);
                }
                else
                {
                    Imstk.Geometry colGeom = GetCollidingGeometry();
                    collisionGeomFilter.MoveToGlobalSpace();
                    imstkObject.setCollidingGeometry(colGeom);
                }
            }
            else
            {
                Debug.LogError("No collision geometry provided to DynamicalModel on object " + gameObject.name);
            }

            if (dynamicGeometry)
            {
                if (imstkObject.getVisualGeometry().getTypeName() == SurfaceMesh.getStaticTypeName())
                {
                    _updateMesh = DynamicSurfaceMeshUpdate;
                }
            }
        }

        protected void InitGeometryMaps()
        {
            // Setup any geometry maps on the object
            // \todo: Generalize geometry maps in imstk, currently
            // well test geometry types to figure out maps
            DeformableMap[] geomMaps = gameObject.GetComponents<DeformableMap>();

            // \todo: Currently imstk only supports physicstovisual and physicstocollision
            // this needs to be generalized. For now we will only support two maps. There are
            // some other minute but tricky details to be worked out here.
            DynamicObject dynObj = imstkObject as DynamicObject;
            foreach (var map in geomMaps)
            {
                if (!map.enabled) continue;
                Imstk.GeometryMap geomMap = map.GetMap();
                if (dynObj == null || map == null || geomMap == null)
                {
                    continue;
                }
                geomMap.compute();
                string mapTypeName = "";
                // Test if map contains physics or visual
                if (map.parentGeom == physicsGeomFilter &&
                    map.childGeom == collisionGeomFilter)
                {
                    dynObj.setPhysicsToCollidingMap(geomMap);
                    mapTypeName = "Physics to Collision";
                    Debug.Log("Set up Physics to Collision Map");
                }
                else if (map.parentGeom == physicsGeomFilter &&
                         map.childGeom == visualGeomFilter)
                {
                    dynObj.setPhysicsToVisualMap(geomMap);
                    mapTypeName = "Physics to Visual";
                    Debug.Log("Set up Physics to Visual Map");
                }
                else if (map.parentGeom == collisionGeomFilter &&
                         map.childGeom == visualGeomFilter)
                {
                    imstkObject.setCollidingToVisualMap(geomMap);
                    mapTypeName = "Collision to Visual";
                    Debug.Log("Set up Collision to Visual Map");
                }
                
                if (map.forceOneOne)
                {
                    var pointWiseMap = Utils.CastTo<PointwiseMap>(geomMap);
                    var childGeometry = Utils.CastTo<PointSet>(pointWiseMap.getChildGeometry());
                    if (childGeometry != null && childGeometry.getNumVertices() > 0)
                    {
                        var mapping = pointWiseMap.getMap();
                        var ratio =  mapping.Count() / (float)childGeometry.getNumVertices();
                        var message =
                            $"{gameObject.name} {mapTypeName}: {mapping.Count}/{childGeometry?.getNumVertices()} points mapped.";
                        if (ratio < .75f)
                        {
                            Debug.LogWarning(message);
                        }
                        else
                        {
                            Debug.Log(message);
                        }
                    }
                }
                
            }
        }

        /// <summary>
        /// Visual update of the geometry (only needs to call before render)
        /// </summary>
        public void Update()
        {
            if (SimulationManager.sceneManager == null)
            {
                Debug.LogWarning("Failed to update dynamical model on " + gameObject.name + " no sceneManager from the SimulationManager");
                return;
            }

            if (!SimulationManager.HasFrame) return;

            s_UpdatePerfMarker.Begin();
            _updateTimes.Begin();

            if (imstkObject != null && imstkObject.getVisualGeometry() != null && 
                dynamicGeometry && imstkObject.getVisualGeometry().getTypeName() == SurfaceMesh.getStaticTypeName())
            {
                DynamicSurfaceMeshUpdate();
            }
            else
            {
                StaticMeshUpdate();
                if (meshFilter != null && meshFilter.mesh != null &&
                    (meshFilter.mesh.GetTopology(0) == MeshTopology.Triangles ||
                     meshFilter.mesh.GetTopology(0) == MeshTopology.Quads))
                {
                    meshFilter.mesh.RecalculateNormals();
                }
                meshFilter.mesh.RecalculateBounds();
            }
            
            _updateTimes.End();
            s_UpdatePerfMarker.End();
        }

        private void StaticMeshUpdate()
        {
            if (imstkObject == null || imstkObject.getVisualGeometry() == null)
            {
                Debug.LogWarning($"StaticMeshUpdate: Visual geometry not available for {gameObject.name}");
                return;
            }
            var visualGeom = Imstk.Utils.CastTo<Imstk.PointSet>(imstkObject.getVisualGeometry());
            if (visualGeom == null)
            {
                Debug.LogWarning($"StaticMeshUpdate: Failed to cast visual geometry to PointSet for {gameObject.name}");
                return;
            }
            MathUtil.ToVector3Array(visualGeom.getVertexPositions(), ref _vertices);
            meshFilter.mesh.vertices = _vertices;
        }

        private void DynamicSurfaceMeshUpdate()
        {
            if (imstkObject == null)
            {
                Debug.LogWarning($"DynamicSurfaceMeshUpdate: imstkObject is null for {gameObject.name}");
                return;
            }
            var dynamicObject = Imstk.Utils.CastTo<Imstk.DynamicObject>(imstkObject);
            if (dynamicObject == null)
            {
                Debug.LogWarning($"DynamicSurfaceMeshUpdate: Failed to cast to DynamicObject for {gameObject.name}");
                return;
            }
            var geometry = Imstk.Utils.CastTo<Imstk.SurfaceMesh>(dynamicObject.getVisualGeometry());
            if (geometry == null)
            {
                Debug.LogWarning($"DynamicSurfaceMeshUpdate: Failed to cast visual geometry to SurfaceMesh for {gameObject.name}");
                return;
            }
            MathUtil.ToVector3Array(geometry.getVertexPositions(), ref _vertices);
            var mesh = meshFilter.sharedMesh;
            mesh.vertices = _vertices;

            // Two modifications will happen to the vertex count
            // - A new point is added at 0 that is used to deleted triangles
            // - New Vertices are added at the end 
            // This is going to be brittle ..
            if (mesh.uv.Length > 0 && (mesh.uv.Length != _vertices.Length || !_didShift))
            {
                var offset = (_didShift) ? 0 : -1;
                var oldCount = mesh.uv.Length;

                _didShift = true;
                var newUV = new Vector2[_vertices.Length];

                newUV[0] = new Vector2(0, 0);
                for (int i = 1; i < _vertices.Length - 1; i++)
                {
                    newUV[i] = mesh.uv[i + offset];
                    mesh.uv = newUV;
                }
            }


            // TODO Deal with normals

            //             if (_mesh.vertexCount > 0 && _needUV)
            //             {
            //                 GenerateUVAndNormals(_mesh);
            //             }

            //             if (_normals.Length != _vertices.Length)
            //             {
            //                 _normals = new Vector3[_vertices.Length];
            //                 _tangents = new Vector4[_vertices.Length];
            //             }

            // HS 2024-03-15 Imstk and Unity operate in two different coordinate systems, RightHanded vs LeftHanded
            // when we sent the mesh to imstk we changed the winding order, if a mesh is read back here that is 
            // visualized in unity the winding order is going to be incorrect. This could be fixed here by swapping 
            // two indices in each triangle or by just telling the shader to render back-facing triangles rather than
            // front facing
            MathUtil.ToIntArray(geometry.getTriangleIndices(), ref _indices);
            mesh.SetIndices(_indices, MeshTopology.Triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.MarkModified();
        }

        public override Imstk.Geometry GetVisualGeometry()
        {
            return visualGeomFilter != null ? visualGeomFilter.GetOutputGeometry() : null;
        }
        public override Imstk.Geometry GetPhysicsGeometry()
        {
            return physicsGeomFilter != null ? physicsGeomFilter.GetOutputGeometry() : null;
        }
        public override Imstk.Geometry GetCollidingGeometry()
        {
            return collisionGeomFilter != null ? collisionGeomFilter.GetOutputGeometry() : null;
        }
        public override ImstkUnity.Geometry GetUnityColisionGeometry()
        {
            return collisionGeomFilter != null ? collisionGeomFilter.inputImstkGeom : null;
        }
    }
}
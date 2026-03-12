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

using UnityEngine;

namespace ImstkUnity
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    /// <summary>
    /// This component represents connective tissue as a multitude of strands between
    /// opposing surfaces.
    /// </summary>
    /// Given two opposing geometries strands will be generated with
    /// configurable parameters. The generated object is physical and can be interacted
    /// with. The connective tissue will consist of multiple "strands" each going from 
    /// one of the reference objects to the other. Each strand will be made up of the give
    /// number of segments. The amount of strands is roughly NumberOfFaces(ObjectA) * strandsPerFace
    /// Note that increasing the density and/or the number of segments per strand will also
    /// increase the computational load to simulation this object.
    public class ConnectiveTissue : DynamicalModel
    {
        /// <value>objectA and deformable are the objects that delimit the connective tissue</value>
        public Deformable objectA;
        public Deformable objectB;

        /// <value>
        /// <c>maxDistance</c> represents the maximum distance between A and B where conn
        /// connective tissue will be generated
        /// </value>
        public double maxDistance = 10;

        /// <value>
        /// <c>strandsPerFace</c> indicates the amount of strands total number will be roughly  
        /// NumberOfFaces(ObjectA) * strandsPerFace
        /// </value>
        public double strandsPerFace = 2.0;

        /// <value>
        /// <c>segmentsPerStrand</c> indicates how many subdivisions each strand has
        /// </value>
        public int segmentsPerStrand = 2;

        /// <value>
        /// <c>distanceStiffness</c> how much (or little) give the connective tissue has (0-infinity)
        /// </value>
        public double distanceStiffness = 10;
        public double uniformMassValue = 0.1;
        public double viscousDampingCoeff = 0.01;
        public double strandAngleDeviation = 90.0;
        public double stretch = 1.0;

        private Mesh _mesh;
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;

        private Imstk.PbdObject _connectiveTissue;
        private bool _needUV = true;

        private Vector3[] _normals;
        private Vector3[] _vertices;
        private Vector4[] _tangents;

#if UNITY_EDITOR
        private Imstk.LineMesh _editorCollisionMesh;
#endif

        private void Awake()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();
        }

        private void Start()
        {

            if (objectA == null || objectB == null)
            {
                Debug.LogError("Connective Tissue needs two objects to span");
            }
        }

        protected override void OnImstkInit()
        {
            if (_connectiveTissue != null) return;

            objectA.ImstkInit();
            objectB.ImstkInit();

            var aPbdObject = objectA.GetDynamicObject() as Imstk.PbdObject;
            var bPbdObject = objectB.GetDynamicObject() as Imstk.PbdObject;

            // Using separate `makeConnectiveTissue` call that doesn't have selector in it
            _connectiveTissue = Imstk.Utils.makeConnectiveTissue(aPbdObject, bPbdObject, SimulationManager.pbdModel,
                maxDistance, strandsPerFace, segmentsPerStrand, uniformMassValue, distanceStiffness, (System.Math.PI / 180) * strandAngleDeviation);

            var bodyId = _connectiveTissue.getPbdBody().bodyHandle;

            // Superclass object
            imstkObject = _connectiveTissue;

            var visualGeometry = Imstk.Utils.CastTo<Imstk.PointSet>(_connectiveTissue.getPhysicsGeometry());
            _mesh = new Mesh();
            _mesh.name = "Connective Tissue Mesh (Imstk)";
            _mesh.MarkDynamic();
            GeomUtil.CopyMesh(visualGeometry.ToMesh(), _mesh);
            _meshFilter.mesh = _mesh;

            var config = SimulationManager.pbdModel.getConfig();
            //             config.enableConstraint(Imstk.PbdModelConfig.ConstraintGenType.Distance, distanceStiffness,
            //                 _connectiveTissue.getPbdBody().bodyHandle);
            config.enableDistanceConstraint(distanceStiffness, stretch, bodyId);
//          Could add bend constraint here ... 
            _connectiveTissue.getPbdBody().bodyGravity = false;
            config.setBodyDamping(bodyId, viscousDampingCoeff, 0.0001);

            
            SimulationManager.pbdModel.configure(config);

            // TODO refactor to move to simulation _manager
            SimulationManager.sceneManager.getActiveScene().addSceneObject(GetSceneObject());
        }

        public void Update()
        {
            var geometry = Imstk.Utils.CastTo<Imstk.LineMesh>(_connectiveTissue.getPhysicsGeometry());

            MathUtil.ToVector3Array(geometry.getVertexPositions(), ref _vertices);

            _mesh.vertices = _vertices;
            if (_mesh.vertexCount > 0 && _needUV)
            {
                GenerateUVAndNormals(_mesh);
            }

            if (dynamicGeometry)
            {
                if (_normals.Length != _vertices.Length)
                {
                    _normals= new Vector3[_vertices.Length];
                    _tangents = new Vector4[_vertices.Length];
                }

                int[] indices = MathUtil.ToIntArray(geometry.getLinesIndices());
                _mesh.SetIndices(indices, MeshTopology.Lines, 0);
            }
            _mesh.RecalculateBounds();
            UpdateNormals(_mesh);
            _mesh.MarkModified();
        }

        public Imstk.SceneObject GetSceneObject()
        {
            return Imstk.Utils.CastTo<Imstk.SceneObject>(_connectiveTissue);
        }


        private void GenerateUVAndNormals(Mesh mesh)
        {
            var vertices = mesh.vertices;
            var uvs = new Vector2[vertices.Length];
            _normals = new Vector3[vertices.Length];
            _tangents = new Vector4[vertices.Length];

            int pointsPerStrand = segmentsPerStrand + 1;
            for (int strand = 0; strand < mesh.vertices.Length / pointsPerStrand; ++strand)
            {
                float rand = Random.Range(0.1f, 0.9f);
                for (int i = 0; i < pointsPerStrand; ++i)
                {
                    Vector3 dir;
                    var index = strand * pointsPerStrand + i;
                    if (i < pointsPerStrand - 1)
                    {
                        dir = vertices[index + 1] - vertices[index];
                    }
                    else
                    {
                        dir = vertices[index] - vertices[index - 1];
                    }

                    uvs[index] = new Vector2(rand, (float)i / (float)pointsPerStrand);

                    // For now just connect through the origin
                    _normals[index] = vertices[index] - _meshRenderer.bounds.center;

                    // Just a guess for now
                    Vector3 tangent = Vector3.Cross(_normals[index], dir);
                    _tangents[index] = new Vector4(tangent.x, tangent.y, tangent.z, -1);
                }
            }

            mesh.SetUVs(0, uvs);
            mesh.SetNormals(_normals);
            mesh.SetTangents(_tangents);
            _needUV = false;
        }

        private void UpdateNormals(Mesh mesh)
        {
            int pointsPerStrand = segmentsPerStrand + 1;

            Vector3 dir;
            Vector4 tangent;
            for (int strand = 0; strand < _vertices.Length / pointsPerStrand; ++strand)
            {
                for (int i = 0; i < pointsPerStrand; ++i)
                {

                    var index = strand * pointsPerStrand + i;
                    if (i < pointsPerStrand - 1)
                    {
                        dir = _vertices[index + 1] - _vertices[index];
                    }
                    else
                    {
                        dir = _vertices[index] - _vertices[index - 1];
                    }
                    
                    // Normalize 
                    _normals[index] = _vertices[index];

                    // Just a guess for now
                    tangent = Vector3.Cross(_normals[index], dir);
                    _tangents[index].x = tangent.x;
                    _tangents[index].y = tangent.y;
                    _tangents[index].z = tangent.x;
                }
            }
            mesh.SetNormals(_normals);
            mesh.SetTangents(_tangents);
        }

        protected override Imstk.CollidingObject InitObject()
        {
            return imstkObject;
        }

        protected override void Configure()
        {
            // Don't need this for connective tissue
        }

        protected override void InitGeometry()
        {
            // Don't need this for connective tissue
        }

        public override Imstk.Geometry GetVisualGeometry()
        {
            return null;
        }
        public override Imstk.Geometry GetPhysicsGeometry()
        {
            return _connectiveTissue.getPhysicsGeometry();
        }
        public override Imstk.Geometry GetCollidingGeometry()
        {
#if UNITY_EDITOR
            // Editor only, provide the _editorCollisionMesh for 
            // the purpose of detecting the collision type
            if (_connectiveTissue != null)
            {
                return _connectiveTissue.getCollidingGeometry();
            }
            else
            {
                if (_editorCollisionMesh == null)
                {
                    _editorCollisionMesh = new Imstk.LineMesh();
                }
                return _editorCollisionMesh;
            }
#else
            if (_connectiveTissue != null)
            {
                return _connectiveTissue.getCollidingGeometry();
            }
            else return null;
#endif
        }

        public override ImstkUnity.Geometry GetUnityColisionGeometry()
        {
            return new ImstkUnity.ImstkMesh();
        }
    }
}
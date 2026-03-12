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
using System.Collections.Generic;
using UnityEngine;

namespace ImstkUnity
{
    /// <summary>
    /// Various geometryic utility functions
    /// </summary>
    public static class GeomUtil
    {
        /// <summary>
        /// Map of GeometryType to MeshTopology
        /// </summary>
        public static Dictionary<GeometryType, MeshTopology> geomTypeToMeshTopology = new Dictionary<GeometryType, MeshTopology>()
        {
            { GeometryType.PointSet, MeshTopology.Points },
            { GeometryType.LineMesh, MeshTopology.Lines },
            { GeometryType.SurfaceMesh, MeshTopology.Triangles }
        };

        /// <summary>
        /// Map of MeshTopology to GeometryType
        /// </summary>
        public static Dictionary<MeshTopology, GeometryType> meshTopologyToGeomType = new Dictionary<MeshTopology, GeometryType>()
        {
            { MeshTopology.Points, GeometryType.PointSet },
            { MeshTopology.Lines, GeometryType.LineMesh },
            { MeshTopology.Triangles, GeometryType.SurfaceMesh }
        };

        /// <summary>
        /// Preforms a simple mesh copy (only works with triangles)
        /// </summary>
        public static void CopyMesh(Mesh srcMesh, Mesh destMesh)
        {
            destMesh.triangles = null;
            destMesh.vertices = srcMesh.vertices;
            destMesh.uv = srcMesh.uv;
            destMesh.normals = srcMesh.normals;
            destMesh.colors = srcMesh.colors;
            destMesh.tangents = srcMesh.tangents;

            for (int i = 0; i < srcMesh.subMeshCount; i++)
            {
                destMesh.SetIndices(srcMesh.GetIndices(i),
                    srcMesh.GetTopology(i), i);
            }
        }

        public static void CopyMesh(ImstkMesh srcMesh, ImstkMesh destMesh)
        {
            destMesh.vertices = srcMesh.vertices;
            destMesh.indices = srcMesh.indices;
            destMesh.texCoords = srcMesh.texCoords;
            destMesh.geomType = srcMesh.geomType;
        }

        /// <summary>
        /// Converts Imstk.PointSet to Unity Mesh
        /// SurfaceMesh
        /// </summary>
        public static Mesh ToMesh(this Imstk.PointSet geom)
        {
            string typeName = geom.getTypeName();
            if (typeName != "LineMesh" &&
                typeName != "SurfaceMesh" &&
                typeName != "PointSet")
            {
                return null;
            }

            Imstk.PointSet pointSet = Imstk.Utils.CastTo<Imstk.PointSet>(geom);

            Mesh results = new Mesh();

            Vector3[] vertices = MathUtil.ToVector3Array(pointSet.getVertexPositions());
            results.SetVertices(vertices);

            if (typeName == "PointSet")
            {
                int[] indices = new int[vertices.Length];
                for (int i = 0; i < indices.Length; i++)
                {
                    indices[i] = i;
                }
                results.SetIndices(indices, MeshTopology.Points, 0);
            }
            else if (typeName == "LineMesh")
            {
                Imstk.LineMesh lineMesh = Imstk.Utils.CastTo<Imstk.LineMesh>(geom);
                int[] indices = MathUtil.ToIntArray(lineMesh.getLinesIndices());
                results.SetIndices(indices, MeshTopology.Lines, 0);
            }
            else if (typeName == "SurfaceMesh")
            {
                Imstk.SurfaceMesh surfMesh = Imstk.Utils.CastTo<Imstk.SurfaceMesh>(geom);
                int[] indices = MathUtil.ToIntArray(surfMesh.getTriangleIndices());
                results.SetIndices(indices, MeshTopology.Triangles, 0);
                results.RecalculateNormals();
            }
            
            if (pointSet.getVertexTCoords() != null)
            {
                Vector2[] tCoords = MathUtil.ToVector2Array(pointSet.getVertexTCoords());
                results.SetUVs(0, tCoords);
            }

            results.RecalculateBounds();
            return results;
        }

        /// <summary>
        /// Converts ImstkMesh to Unity Mesh
        /// </summary>
        public static Mesh ToMesh(this ImstkMesh geom)
        {
            if (geom.IsVolume)
                return null;

            Mesh results = new Mesh();
            results.name = geom.name;

            results.SetVertices(geom.vertices);
            results.SetIndices(geom.indices, geomTypeToMeshTopology[geom.geomType], 0);

            if (geom.texCoords.Length > 0)
            {
                results.SetUVs(0, geom.texCoords);
            }

            results.RecalculateBounds();
            if (results.GetTopology(0) == MeshTopology.Triangles ||
                results.GetTopology(0) == MeshTopology.Quads)
            {
                results.RecalculateNormals();
            }
            return results;
        }

        /// <summary>
        /// Converts Unity Mesh to ImstkUnity Geometry
        /// </summary>
        public static ImstkMesh ToImstkMesh(this Mesh mesh)
        {
            // A Unity mesh can be made up multiple submeshes we only support 1
            ImstkMesh geom = ScriptableObject.CreateInstance<ImstkMesh>();
            geom.name = mesh.name;
            geom.SetVertices(mesh.vertices);
            geom.SetIndices(mesh.GetIndices(0));

            geom.texCoords = new Vector2[mesh.uv.Length];
            mesh.uv.CopyTo(geom.texCoords, 0);
            geom.geomType = meshTopologyToGeomType[mesh.GetTopology(0)];

            return geom;
        }

        /// <summary>
        /// Converts Unity Mesh to ImstkUnity Geometry with transform applied
        /// </summary>
        public static ImstkMesh ToImstkMesh(this Mesh mesh, Matrix4x4 transform)
        {
            ImstkMesh geometry = ScriptableObject.CreateInstance<ImstkMesh>();
            geometry.name = mesh.name;
            geometry.geomType = meshTopologyToGeomType[mesh.GetTopology(0)];

            geometry.SetVertices(mesh.vertices, transform);

            int[] indices = mesh.GetIndices(0);
            geometry.indices = new int[indices.Length];
            mesh.triangles.CopyTo(geometry.indices, 0);

            geometry.texCoords = new Vector2[mesh.uv.Length];
            mesh.uv.CopyTo(geometry.texCoords, 0);

            return geometry;
        }

        /// <summary>
        /// Converts ImstkMesh to Imstk PointSet
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        public static Imstk.PointSet ToPointSet(this ImstkMesh geometry)
        {
            Imstk.VecDataArray3d vertices = MathUtil.ToVecDataArray3d(geometry.vertices);
            if (geometry.geomType == GeometryType.PointSet)
            {
                Imstk.PointSet pointSet = new Imstk.PointSet();
                pointSet.initialize(vertices);
                return pointSet;
            }
            else if (geometry.geomType == GeometryType.LineMesh)
            {
                Imstk.LineMesh mesh = new Imstk.LineMesh();
                Imstk.VecDataArray2i indices = MathUtil.ToVecDataArray2i(geometry.indices);
                mesh.initialize(vertices, indices);
                return mesh;
            }
            else if (geometry.geomType == GeometryType.SurfaceMesh)
            {
                Imstk.SurfaceMesh mesh = new Imstk.SurfaceMesh();
                Imstk.VecDataArray3i indices = MathUtil.ToVecDataArray3i(geometry.indices);
                mesh.initialize(vertices, indices);
                return mesh;
            }
            else if (geometry.geomType == GeometryType.TetrahedralMesh)
            {
                Imstk.TetrahedralMesh mesh = new Imstk.TetrahedralMesh();
                Imstk.VecDataArray4i indices = MathUtil.ToVecDataArray4i(geometry.indices);
                mesh.initialize(vertices, indices);
                return mesh;
            }
            return null;
        }

        /// <summary>
        /// Converts Geometry to Imstk Geometry
        /// </summary>
        public static Imstk.Geometry ToImstkGeometry(this Geometry geom)
        {
            if (geom == null)
            {
                Debug.LogError("ToImstkgeometry called with null");
                return null;
            }
            if (geom.IsMesh)
            {
                return (geom as ImstkMesh).ToPointSet();
            }
            else if (geom.geomType == GeometryType.Capsule)
            {
                Capsule capsule = geom as Capsule;
                Imstk.Capsule imstkCapsule = new Imstk.Capsule();
                imstkCapsule.setPosition(capsule.center.ToImstkVec());
                imstkCapsule.setRadius(capsule.radius);
                imstkCapsule.setLength(capsule.length);
                imstkCapsule.setOrientation(capsule.orientation.ToImstkQuat());
                imstkCapsule.updatePostTransformData();
                return imstkCapsule;
            }
            else if (geom.geomType == GeometryType.Cylinder)
            {
                Cylinder cylinder = geom as Cylinder;
                Imstk.Cylinder imstkCylinder = new Imstk.Cylinder();
                imstkCylinder.setPosition(cylinder.center.ToImstkVec());
                imstkCylinder.setRadius(cylinder.radius);
                imstkCylinder.setLength(cylinder.length);
                imstkCylinder.setOrientation(cylinder.orientation.ToImstkQuat());
                imstkCylinder.updatePostTransformData();
                return imstkCylinder;
            }
            else if (geom.geomType == GeometryType.OrientedBox)
            {
                OrientedBox orientedBox = geom as OrientedBox;
                Imstk.OrientedBox imstkOrientedBox = new Imstk.OrientedBox();
                imstkOrientedBox.setPosition(orientedBox.center.ToImstkVec());
                imstkOrientedBox.setOrientation(orientedBox.orientation.ToImstkQuat());
                imstkOrientedBox.setExtents(orientedBox.extents.ToImstkVec());
                imstkOrientedBox.updatePostTransformData();
                return imstkOrientedBox;
            }
            else if (geom.geomType == GeometryType.Plane)
            {
                Plane plane = geom as Plane;
                Imstk.Plane imstkPlane = new Imstk.Plane();
                imstkPlane.setPosition(plane.center.ToImstkVec());
                imstkPlane.setNormal(plane.normal.ToImstkVec());
                imstkPlane.updatePostTransformData();
                return imstkPlane;
            }
            else if (geom.geomType == GeometryType.Sphere)
            {
                Sphere sphere = geom as Sphere;
                Imstk.Sphere imstkSphere = new Imstk.Sphere();
                imstkSphere.setPosition(sphere.center.ToImstkVec());
                imstkSphere.setRadius(sphere.radius);
                imstkSphere.updatePostTransformData();
                return imstkSphere;
            } 
            else if (geom.geomType == GeometryType.CompoundGeometry)
            {
                CompoundGeometry compound = geom as CompoundGeometry;
                Imstk.CompoundGeometry imstkCompound = new Imstk.CompoundGeometry();

                foreach(GeometryFilter filter in compound.geometryFilters)
                {
                    if (filter.isActiveAndEnabled)
                    {
                        var inputGeom = filter.GetOutputGeometry();
                        var analyticalGeom = Imstk.Utils.CastTo<Imstk.AnalyticalGeometry>(inputGeom);
                        Debug.Assert(analyticalGeom != null);

                        imstkCompound.add(analyticalGeom);
                        Matrix4x4 m = compound.parent.transform.localToWorldMatrix.inverse * filter.transform.localToWorldMatrix;
                        imstkCompound.setLocalTransform(imstkCompound.count() - 1, m.ToMat4d());
                    }
                }
                return imstkCompound;
            }
            return null;
        }

        /// <summary>
        /// Lossy conversion, doesn't support mixed topologies
        /// </summary>
        public static Imstk.Geometry ToImstkGeometry(this Mesh mesh)
        {
            if (mesh.GetTopology(0) == MeshTopology.Points)
            {
                Imstk.PointSet geom = new Imstk.PointSet();
                geom.initialize(MathUtil.ToVecDataArray3d(mesh.vertices));
                return geom;
            }
            else if (mesh.GetTopology(0) == MeshTopology.Lines)
            {
                Imstk.LineMesh geom = new Imstk.LineMesh();
                geom.initialize(
                    MathUtil.ToVecDataArray3d(mesh.vertices),
                    MathUtil.ToVecDataArray2i(mesh.GetIndices(0)));
                return geom;
            }
            else if (mesh.GetTopology(0) == MeshTopology.Triangles)
            {
                Imstk.SurfaceMesh geom = new Imstk.SurfaceMesh();
                geom.initialize(
                    MathUtil.ToVecDataArray3d(mesh.vertices),
                    MathUtil.ToVecDataArray3i(mesh.GetIndices(0)));
                return geom;
            }
            return null;
        }

        /// <summary>
        /// Converts Imstk PointSet to ImstkMesh
        /// </summary>
        public static ImstkMesh ToImstkMesh(this Imstk.PointSet pointSet)
        {
            ImstkMesh geom = ScriptableObject.CreateInstance<ImstkMesh>();
            geom.vertices = MathUtil.ToVector3Array(pointSet.getVertexPositions());
            string pointSetName = pointSet.getTypeName();
            if (pointSetName == "PointSet")
            {
                geom.geomType = GeometryType.PointSet;
                geom.indices = new int[geom.vertices.Length];
                for (int i = 0; i < geom.indices.Length; i++)
                {
                    geom.indices[i] = i;
                }
            }
            else if (pointSetName == "LineMesh")
            {
                geom.geomType = GeometryType.LineMesh;
                Imstk.LineMesh mesh = Imstk.Utils.CastTo<Imstk.LineMesh>(pointSet);
                geom.indices = MathUtil.ToIntArray(mesh.getLinesIndices());
            }
            else if (pointSetName == "SurfaceMesh")
            {
                geom.geomType = GeometryType.SurfaceMesh;
                Imstk.SurfaceMesh mesh = Imstk.Utils.CastTo<Imstk.SurfaceMesh>(pointSet);
                geom.indices = MathUtil.ToIntArray(mesh.getTriangleIndices());
            }
            else if (pointSetName == "TetrahedralMesh")
            {
                geom.geomType = GeometryType.TetrahedralMesh;
                Imstk.TetrahedralMesh mesh = Imstk.Utils.CastTo<Imstk.TetrahedralMesh>(pointSet);
                geom.indices = MathUtil.ToIntArray(mesh.getTetrahedraIndices());
            }
            return geom;
        }

        public static Geometry ToGeometry(this Mesh mesh)
        {
            return mesh.ToImstkMesh();
        }

        /// <summary>
        /// Converts Imstk Geometry to Geometry
        /// </summary>
        public static Geometry ToGeometry(Imstk.Geometry geom)
        {
            string name = geom.getTypeName();
            if (name == "PointSet" || name == "LineMesh" ||
                name == "SurfaceMesh" || name == "TetrahedralMesh")
            {
                Imstk.PointSet pointSet = Imstk.Utils.CastTo<Imstk.PointSet>(geom);
                return pointSet.ToImstkMesh();
            }
            else if (name == "Capsule")
            {
                Imstk.Capsule imstkCapsule = Imstk.Utils.CastTo<Imstk.Capsule>(geom);
                Imstk.Vec3d center = imstkCapsule.getCenter();
                Imstk.Quatd quat = imstkCapsule.getOrientation();

                Capsule capsule = ScriptableObject.CreateInstance<Capsule>();
                capsule.center = center.ToUnityVec();
                capsule.radius = (float)imstkCapsule.getRadius();
                capsule.length = (float)imstkCapsule.getLength();
                capsule.orientation = quat.ToUnityQuat();
                return capsule;
            }
            else if (name == "Cylinder")
            {
                Imstk.Cylinder imstkCapsule = Imstk.Utils.CastTo<Imstk.Cylinder>(geom);
                Imstk.Vec3d center = imstkCapsule.getCenter();
                Imstk.Quatd quat = imstkCapsule.getOrientation();

                Cylinder cylinder = ScriptableObject.CreateInstance<Cylinder>();
                cylinder.center = center.ToUnityVec();
                cylinder.radius = (float)imstkCapsule.getRadius();
                cylinder.length = (float)imstkCapsule.getLength();
                cylinder.orientation = quat.ToUnityQuat();
                return cylinder;
            }
            else if (name == "OrientedBox")
            {
                Imstk.OrientedBox imstkOrientedBox = Imstk.Utils.CastTo<Imstk.OrientedBox>(geom);
                Imstk.Vec3d center = imstkOrientedBox.getCenter();
                Imstk.Vec3d extents = imstkOrientedBox.getExtents();
                Imstk.Quatd orientation = imstkOrientedBox.getOrientation();

                OrientedBox orientedBox = ScriptableObject.CreateInstance<OrientedBox>();
                orientedBox.center = center.ToUnityVec();
                orientedBox.extents = extents.ToUnityVec();
                orientedBox.orientation = orientation.ToUnityQuat();
                return orientedBox;
            }
            else if (name == "Plane")
            {
                Imstk.Plane imstkPlane = Imstk.Utils.CastTo<Imstk.Plane>(geom);
                Imstk.Vec3d center = imstkPlane.getCenter();
                Imstk.Vec3d normal = imstkPlane.getNormal();

                Plane plane = ScriptableObject.CreateInstance<Plane>();
                plane.center = center.ToUnityVec();
                plane.normal = normal.ToUnityVec();
                return plane;
            }
            else if (name == "Sphere")
            {
                Imstk.Sphere imstkSphere = Imstk.Utils.CastTo<Imstk.Sphere>(geom);
                Imstk.Vec3d center = imstkSphere.getCenter();

                Sphere sphere = ScriptableObject.CreateInstance<Sphere>();
                sphere.center = center.ToUnityVec();
                sphere.radius = (float)imstkSphere.getRadius();
                return sphere;
            }
            return null;
        }

        /// <summary>Calculate list of vertices inside of a given mesh contained by unity meshfilter.
        /// The points sampled need to be in world space as the enclosing mesh will also be transfered
        /// into world space.</summary>
        /// <param name="enclosingMesh">Unity.MeshFilter for enclosing mesh</param> 
        /// <param name="sampleMesh">Imstk.PointSet for sample mesh</param>
        /// <returns>Return list of indices of of the sample mesh that are contained within the enclosing mesh</returns>
        public static List<uint> PointsInside(MeshFilter enclosingMesh, PointSet sampleMesh)
        {
            Debug.Assert(enclosingMesh != null && sampleMesh != null);
            ImstkMesh imstkEnclosingMesh = enclosingMesh.sharedMesh.ToImstkMesh(enclosingMesh.transform.localToWorldMatrix);
            return PointsInside(imstkEnclosingMesh.ToImstkGeometry() as Imstk.SurfaceMesh, sampleMesh);
        }

        public static List<uint> PointsInside(SurfaceMesh enclosingMesh, PointSet sampleMesh)
        {
            var result = new List<uint>();
            // Compute mask of enclosed points
            Imstk.SelectEnclosedPoints selectEnclosed = new Imstk.SelectEnclosedPoints();
            selectEnclosed.setInputMesh(enclosingMesh);
            selectEnclosed.setInputPoints(sampleMesh);
            selectEnclosed.setUsePruning(false);
            selectEnclosed.update();

            Imstk.DataArrayuc isInside = selectEnclosed.getIsInsideMask();
            byte[] isInsideBytes = new byte[isInside.size()];
            isInside.getValues(isInsideBytes);
            for (int i = 0; i < isInsideBytes.Length; i++)
            {
                if (isInsideBytes[i] == 1)
                    result.Add((uint)i);
            }

            return result;
        }
    };
}
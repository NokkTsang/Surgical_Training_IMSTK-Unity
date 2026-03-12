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
    /// <summary>
    /// This class reads in various types of geometry utilized by Imstk
    /// </summary>
    [UnityEditor.AssetImporters.ScriptedImporter(1, new string[] { "vtk", "vtu", "vtp", "stl", "ply", "veg", "msh", "mhd" })]
    public class GeometryImporter : UnityEditor.AssetImporters.ScriptedImporter
    {
        /// <summary>
        /// When on, reverses winding of cells (currently only works for surface meshes)
        /// </summary>
        public bool reverseWinding = false;

        // Default is mirror along the X-Axis
        public Matrix4x4 transform = Matrix4x4.Scale(new Vector3(-1, 1, 1));

        [Tooltip("If true, will center the mesh around the origin")]
        public bool centerMesh = false;

        public override void OnImportAsset(UnityEditor.AssetImporters.AssetImportContext ctx)
        {
            string assetsPath = Application.dataPath;
            assetsPath = assetsPath.Replace("Assets", "");
            string filePath = assetsPath + ctx.assetPath;
            string fileName = System.IO.Path.GetFileNameWithoutExtension(ctx.assetPath);

            // Read the geometry using imstk, warning: type not correct use conversion funcs
            Imstk.PointSet pointSet = Imstk.MeshIO.read(filePath);

            // Create resource to load into
            // Setup a default cube, remove BoxCollider, replace mesh, add Geometry (Imstk), replace Mesh (Unity)
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            DestroyImmediate(obj.GetComponent<BoxCollider>());
            obj.name = fileName;

            if (centerMesh)
            {
                var vertices = pointSet.getVertexPositions();
                var center = new Imstk.Vec3d(0, 0, 0);
                for (uint i = 0; i < vertices.size(); ++i)
                {
                    center += vertices[i];
                }
                center = center / vertices.size();
                pointSet.translate(center*-1.0, Imstk.Geometry.TransformType.ApplyToData);
            }

            if (pointSet.getTypeName() == Imstk.LineMesh.getStaticTypeName())
            {
                Imstk.LineMesh lineMesh = Imstk.Utils.CastTo<Imstk.LineMesh>(pointSet);
                lineMesh.transform(transform.ToMat4d(), Imstk.Geometry.TransformType.ApplyToData);
                lineMesh.updatePostTransformData();

                Mesh mesh = lineMesh.ToMesh();
                foreach (var v in mesh.vertices)
                {
                    Debug.Log(v);
                }
                mesh.name = fileName + "_mesh";
                obj.GetComponent<MeshFilter>().sharedMesh = mesh;

                ctx.AddObjectToAsset(obj.name, obj);
                ctx.AddObjectToAsset(mesh.name, mesh); // Add to the load asset
            }
            else if (pointSet.getTypeName() == Imstk.SurfaceMesh.getStaticTypeName())
            {
                Imstk.SurfaceMesh surfMesh = Imstk.Utils.CastTo<Imstk.SurfaceMesh>(pointSet);
                surfMesh.transform(transform.ToMat4d(), Imstk.Geometry.TransformType.ApplyToData);
                surfMesh.updatePostTransformData();

                if (reverseWinding)
                {
                    surfMesh.flipNormals();
                }

                Mesh mesh = surfMesh.ToMesh();
                mesh.name = fileName + "_mesh";
                obj.GetComponent<MeshFilter>().sharedMesh = mesh;

                ctx.AddObjectToAsset(obj.name, obj);
                ctx.AddObjectToAsset(mesh.name, mesh); // Add to the load asset
            }
            else if (pointSet.getTypeName() == Imstk.TetrahedralMesh.getStaticTypeName())
            {
                Imstk.TetrahedralMesh tetMesh = Imstk.Utils.CastTo<Imstk.TetrahedralMesh>(pointSet);
                if (tetMesh == null)
                {
                    return;
                }
                tetMesh.transform(transform.ToMat4d(), Imstk.Geometry.TransformType.ApplyToData);
                tetMesh.updatePostTransformData();

                Imstk.SurfaceMesh surfMesh = tetMesh.extractSurfaceMesh();
                
                if (reverseWinding)
                {
                    surfMesh.flipNormals();
                }

                Geometry tetGeom = tetMesh.ToImstkMesh();
                tetGeom.name = fileName + "_mesh";
                Mesh mesh = surfMesh.ToMesh();
                mesh.name = fileName + "_mesh_surface";
                obj.GetComponent<MeshFilter>().sharedMesh = mesh;
                ctx.AddObjectToAsset(obj.name, obj);
                ctx.AddObjectToAsset(mesh.name, mesh); // Add to the load asset
                // \todo: Add a custom thumbnail for the tetrahedral mesh
                ctx.AddObjectToAsset(tetGeom.name, tetGeom);
            }

            ctx.SetMainObject(obj);
        }
    }
}
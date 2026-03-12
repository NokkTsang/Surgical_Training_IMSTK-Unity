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

using System.Collections.Generic;
using UnityEngine;

namespace ImstkUnity
{
    /// <summary>
    /// Roles all meshes into one class. ie: PointSet, LineMesh, SurfaceMesh,
    /// TetMesh, HexMesh
    /// </summary>
    public class ImstkMesh : Geometry
    {
        /// <summary>
        /// Map of some geometry types to number of cell points
        /// </summary>
        public static Dictionary<GeometryType, int> typeToNumPts = new Dictionary<GeometryType, int>()
        {
            { GeometryType.PointSet, 1 },
            { GeometryType.LineMesh, 2 },
            { GeometryType.SurfaceMesh, 3 },
            { GeometryType.TetrahedralMesh, 4 },
            { GeometryType.HexahedralMesh, 8 }
        };

        public int[] indices = new int[0];
        public Vector3[] vertices = new Vector3[0];
        public Vector2[] texCoords = new Vector2[0];

        //public void CopyTo(ImstkMesh geometry)
        //{
        //    geometry.type = type;
        //    geometry.vertices = new Vector3[vertices.Length];
        //    vertices.CopyTo(geometry.vertices, 0);
        //    geometry.texCoords = new Vector2[texCoords.Length];
        //    texCoords.CopyTo(geometry.texCoords, 0);
        //    geometry.indices = new int[indices.Length];
        //    indices.CopyTo(geometry.indices, 0);
        //}

        public void SetVertices(Vector3[] vertices, Matrix4x4 transform)
        {
            this.vertices = new Vector3[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                this.vertices[i] = transform.MultiplyPoint(vertices[i]);
            }
        }
        public void SetVertices(Vector3[] vertices) { this.vertices = vertices; }

        public void SetTexCoords(Vector2[] texCoords) { this.texCoords = texCoords; }

        public void SetIndices(int[] indices) { this.indices = indices; }

        public void Transform(Matrix4x4 transform)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = transform.MultiplyPoint(vertices[i]);
            }
        }
    }
}

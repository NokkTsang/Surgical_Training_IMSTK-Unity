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
    /// Handles drawing of meshes in the editor, can draw line or triangle meshes
    /// </summary>
    public static class ImstkGizmos
    {
        static public void DrawPointMesh(Mesh mesh, Vector3 position, Quaternion rotation, Vector3 localScale, Color? color = null)
        {
            var actualColor = color ?? Color.white;
            for (int i = 0; i < mesh.vertices.Length - 1; ++i)
            {
                Vector3 v0 = mesh.vertices[i];
                v0 = rotation * v0;
                for (int j = 0; j < 3; ++j)
                {
                    v0[j] = v0[j] * localScale[j];
                }
                v0 = v0 + position;
                Gizmos.color = actualColor;
                Gizmos.DrawLine(v0, v0);
            }
        }
        static public void DrawLineMesh(Mesh mesh, Vector3 position, Quaternion rotation, Vector3 localScale, Color? color = null)
        {
            var actualColor = color ?? Color.white;
            for (int i = 0; i < mesh.vertices.Length - 1; ++i)
            {
                Vector3 v0 = mesh.vertices[i];
                Vector3 v1 = mesh.vertices[i + 1];
                v0 = rotation * v0;
                v1 = rotation * v1;
                for (int j = 0; j < 3; ++j)
                {
                    v0[j] = v0[j] * localScale[j];
                    v1[j] = v1[j] * localScale[j];
                }
                v0 = v0 + position;
                v1 = v1 + position;

                Gizmos.color = actualColor;
                Gizmos.DrawLine(v0, v1);
            }
        }

        static public void DrawMesh(Imstk.TetrahedralMesh mesh, Matrix4x4 t, Color? color = null)
        {
            var actualColor = color ?? Color.white;
            var positions = mesh.getVertexPositions();
            Vector3[] vertices = null;
            MathUtil.ToVector3Array(positions, ref vertices);
            var tets = mesh.getTetrahedraIndices();
            for (int i = 0; i < tets.size(); ++i)
            {
                var tet = tets[(uint)i];
                Gizmos.color = actualColor;
                Gizmos.DrawLine(t * vertices[(uint)tet[0]], t * vertices[(uint)tet[1]]);
                Gizmos.DrawLine(t * vertices[(uint)tet[1]], t * vertices[(uint)tet[2]]);
                Gizmos.DrawLine(t * vertices[(uint)tet[2]], t * vertices[(uint)tet[0]]);
                Gizmos.DrawLine(t * vertices[(uint)tet[0]], t * vertices[(uint)tet[3]]);
                Gizmos.DrawLine(t * vertices[(uint)tet[1]], t * vertices[(uint)tet[3]]);
                Gizmos.DrawLine(vertices[(uint)tet[2]], vertices[(uint)tet[3]]);
            }
        }

        static public void DrawLineMesh(Mesh mesh, Color? color = null)
        {
            var actualColor = color ?? Color.white;
            for (int i = 0; i < mesh.vertices.Length - 1; ++i)
            {
                Vector3 v0 = mesh.vertices[i];
                Vector3 v1 = mesh.vertices[i + 1];
                Gizmos.color = actualColor;
                Gizmos.DrawLine(v0, v1);
            }
        }

        static public void DrawMesh(Mesh mesh, Color? color = null)
        {
             DrawMesh(mesh, Vector3.zero, Quaternion.identity, Vector3.one, color);
        }

        static public void DrawMesh(Mesh mesh, Vector3 position, Quaternion rotation, Vector3 localScale, Color? color = null)
        {
            if (mesh)
            {
                // Only check for the first sub-mesh
                switch (mesh.GetTopology(0))
                {
                    case MeshTopology.Quads:
                    case MeshTopology.Triangles:
                        Gizmos.color = color ?? Color.white;
                        Gizmos.DrawWireMesh(mesh, position, rotation, localScale);
                        break;
                    case MeshTopology.Lines:
                    case MeshTopology.LineStrip:
                        DrawLineMesh(mesh, position, rotation, localScale, color);
                        break;
                    case MeshTopology.Points:
                        DrawPointMesh(mesh, position, rotation, localScale, color);
                        break;
                }
            }
        }
        static public void DrawMesh(Imstk.PointSet pointSet, Color? color = null)
        {
            var mesh = pointSet.ToMesh();
            DrawMesh(mesh, color);
        }
    }
}
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
using System;
using System.Collections.Generic;

namespace ImstkUnity
{
    public static class Utility
    {
        /// <summary>
        /// Tries to get a component, if it doesn't exists, it creates one
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gameObj"></param>
        /// <returns></returns>
        public static T GetComponentOrCreate<T>(this GameObject gameObj) where T : Component
        {
            T comp = gameObj.GetComponent<T>();
            if (comp == null)
                return gameObj.AddComponent<T>();
            else
                return comp;
        }
        /// <summary>
        /// Tries to get a component, if it doesn't exists, throws exception
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gameObj"></param>
        /// <returns></returns>
        /// <exception cref="MissingComponentException"></exception>
        public static T GetComponentFatal<T>(this GameObject gameObj) where T : Component
        {
            T comp = gameObj.GetComponent<T>();
            if (comp == null)
                throw new MissingComponentException();
            return comp;
        }

        /// <summary>
        /// Creates a tetrahedral mesh in the shape of a cube
        /// </summary>
        /// Mesh coordinates will be in world space with regards to the center parameter
        /// <param name="size">extents of the cube</param>
        /// <param name="dim">subdivisions in each axis</param>
        /// <param name="center">center of the cube</param>
        /// <returns></returns>
        public static ImstkMesh GetTetGridMesh(Vector3 size, Vector3Int dim, Vector3 center)
        {
            // Make a tetrahedral cube
            ImstkMesh tetMesh = ScriptableObject.CreateInstance<ImstkMesh>();
            tetMesh.name = "GridTetMesh";
            tetMesh.geomType = GeometryType.TetrahedralMesh;

            Vector3[] vertices = new Vector3[dim[0] * dim[1] * dim[2]];

            Vector3 div = dim - new Vector3Int(1, 1, 1);
            Vector3 dx = Vector3.Scale(size, div.Invert());
            for (int z = 0; z < dim[2]; z++)
            {
                for (int y = 0; y < dim[1]; y++)
                {
                    for (int x = 0; x < dim[0]; x++)
                    {
                        vertices[x + dim[0] * (y + dim[1] * z)] = Vector3.Scale(new Vector3Int(x, y, z), dx) - size * 0.5f + center;
                    }
                }
            }
            tetMesh.SetVertices(vertices);

            List<int> indices = new List<int>();
            for (int z = 0; z < dim[2] - 1; z++)
            {
                for (int y = 0; y < dim[1] - 1; y++)
                {
                    for (int x = 0; x < dim[0] - 1; x++)
                    {
                        int[] cubeIndices =
                        {
                            x + dim[0] * (y + dim[1] * z),
                            (x + 1) + dim[0] * (y + dim[1] * z),
                            (x + 1) + dim[0] * (y + dim[1] * (z + 1)),
                            x + dim[0] * (y + dim[1] * (z + 1)),
                            x + dim[0] * ((y + 1) + dim[1] * z),
                            (x + 1) + dim[0] * ((y + 1) + dim[1] * z),
                            (x + 1) + dim[0] * ((y + 1) + dim[1] * (z + 1)),
                            x + dim[0] * ((y + 1) + dim[1] * (z + 1))
                        };

                        // Alternate the pattern so the edges line up on the sides of each voxel
                        bool a = Convert.ToBoolean(z % 2);
                        bool b = Convert.ToBoolean(x % 2);
                        bool c = Convert.ToBoolean(y % 2);
                        if ((a ^ b) ^ c)
                        {
                            indices.InsertRange(indices.Count, new int[] { cubeIndices[0], cubeIndices[7], cubeIndices[5], cubeIndices[4] });
                            indices.InsertRange(indices.Count, new int[] { cubeIndices[3], cubeIndices[7], cubeIndices[2], cubeIndices[0] });
                            indices.InsertRange(indices.Count, new int[] { cubeIndices[2], cubeIndices[7], cubeIndices[5], cubeIndices[0] });
                            indices.InsertRange(indices.Count, new int[] { cubeIndices[1], cubeIndices[2], cubeIndices[0], cubeIndices[5] });
                            indices.InsertRange(indices.Count, new int[] { cubeIndices[2], cubeIndices[6], cubeIndices[7], cubeIndices[5] });
                        }
                        else
                        {
                            indices.InsertRange(indices.Count, new int[] { cubeIndices[3], cubeIndices[7], cubeIndices[6], cubeIndices[4] });
                            indices.InsertRange(indices.Count, new int[] { cubeIndices[1], cubeIndices[3], cubeIndices[6], cubeIndices[4] });
                            indices.InsertRange(indices.Count, new int[] { cubeIndices[3], cubeIndices[6], cubeIndices[2], cubeIndices[1] });
                            indices.InsertRange(indices.Count, new int[] { cubeIndices[1], cubeIndices[6], cubeIndices[5], cubeIndices[4] });
                            indices.InsertRange(indices.Count, new int[] { cubeIndices[0], cubeIndices[3], cubeIndices[1], cubeIndices[4] });
                        }
                    }
                }
            }
            tetMesh.SetIndices(indices.ToArray());

            // Ensure correct windings
            //for (int i = 0; i < indices.Count; i++)
            //{
            //    if (tetVolume(vertices[indices[i][0]], vertices[indices[i][1]], vertices[indices[i][2]], vertices[indices[i][3]]) < 0.0)
            //    {
            //        std::swap(indices[i][0], indices[i][2]);
            //    }
            //}
            return tetMesh;
        }

        public static ImstkMesh GetTetCubeMesh()
        {
            // Make a tetrahedral cube
            ImstkMesh cubeTetMesh = ScriptableObject.CreateInstance<ImstkMesh>();
            cubeTetMesh.name = "CubeTetMesh";
            cubeTetMesh.geomType = GeometryType.TetrahedralMesh;
            Vector3[] vertices = {
                new Vector3(-0.5f, 0.5f, -0.5f),
                new Vector3(0.5f, 0.5f, -0.5f),
                new Vector3(-0.5f, 0.5f, 0.5f),
                new Vector3(0.5f, 0.5f, 0.5f),
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, 0.5f) };
            cubeTetMesh.SetVertices(vertices);
            int[] indices = {
                4, 1, 2, 7,
                4, 5, 1, 7,
                4, 1, 0, 2,
                1, 7, 3, 2,
                6, 4, 2, 7 };
            cubeTetMesh.SetIndices(indices);
            return cubeTetMesh;
        }

        public static ImstkMesh GetXYPlane(int numXDivisions = 0, int numYDivisions = 0)
        {
            int numCellsX = numXDivisions + 1;
            int numCellsY = numYDivisions + 1;
            int numCells = numCellsX * numCellsY;
            int numPointsX = numXDivisions + 2;
            int numPointsY = numYDivisions + 2;
            int numPoints = numPointsX * numPointsY;
            Vector2 spacing = new Vector2(1.0f / numCellsX, 1.0f / numCellsY);

            Vector3[] vertices = new Vector3[numPoints];
            Vector2[] texCoords = new Vector2[numPoints];
            int[] indices = new int[numCells * 6];

            ImstkMesh planeMesh = ScriptableObject.CreateInstance<ImstkMesh>();
            planeMesh.name = "PlaneMesh";
            planeMesh.geomType = GeometryType.SurfaceMesh;

            int iter = 0;
            for (int j = 0; j < numPointsY; j++)
            {
                for (int i = 0; i < numPointsX; i++)
                {
                    texCoords[iter] = new Vector2(i, j) * spacing;
                    Vector2 pos = texCoords[iter] - new Vector2(0.5f, 0.5f);
                    vertices[iter++] = new Vector3(pos.x, pos.y, 0.0f);
                }
            }

            iter = 0;
            for (int j = 0; j < numCellsY; j++)
            {
                for (int i = 0; i < numCellsX; i++)
                {
                    int index1 = j * numPointsX + i;
                    int index2 = index1 + numPointsX;
                    int index3 = index1 + 1;
                    int index4 = index2 + 1;

                    if ((i % 2 == 0) ^ (j % 2 == 0))
                    {
                        indices[iter++] = index1;
                        indices[iter++] = index2;
                        indices[iter++] = index3;

                        indices[iter++] = index4;
                        indices[iter++] = index3;
                        indices[iter++] = index2;
                    }
                    else
                    {
                        indices[iter++] = index2;
                        indices[iter++] = index4;
                        indices[iter++] = index1;

                        indices[iter++] = index4;
                        indices[iter++] = index3;
                        indices[iter++] = index1;
                    }
                }
            }

            planeMesh.SetVertices(vertices);
            planeMesh.SetTexCoords(texCoords);
            planeMesh.SetIndices(indices);
            return planeMesh;
        }

        /// <summary>
        /// Returns a compound name for a component made up of the name of 
        /// the component itself and the names of all the parent objects
        /// </summary>
        /// <param name="behavior">Behavior that we need the name for</param>
        /// <returns>A long name for the component</returns>
        public static string GetFullName(MonoBehaviour behavior)
        {
            var gameObject = behavior.gameObject;
            var stack = new System.Collections.Generic.Stack<string>();
            stack.Push(behavior.GetType().Name);
            stack.Push(gameObject.name);
            Transform parent = gameObject.transform.parent;
            while (parent != null)
            {
                stack.Push(parent.gameObject.name);
                parent = parent.transform.parent;
            }
            var newName = stack.Pop();
            while (stack.Count != 0)
            {
                newName = newName + "/" + stack.Pop();
            }
            return newName;
        }
    }
}
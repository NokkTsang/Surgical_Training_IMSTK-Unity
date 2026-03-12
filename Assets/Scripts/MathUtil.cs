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

using System;
using UnityEngine;

namespace ImstkUnity
{
    /// <summary>
    /// Extensions to Imstk.Vec3d
    /// </summary>
    public static class Vec3dExt
    {
        public static Vector3 ToUnityVec(this Imstk.Vec3d vec)
        {
            return new Vector3((float)vec[0], (float)vec[1], (float)vec[2]);
        }
    }
    /// <summary>
    /// Extensions to Imstk.Quatd
    /// </summary>
    public static class QuatdExt
    {
        public static Quaternion ToUnityQuat(this Imstk.Quatd quat)
        {
            return new Quaternion((float)quat.x(), (float)quat.y(), (float)quat.z(), (float)quat.w());
        }
    }

    /// <summary>
    /// Extensions to Unity Vector3
    /// </summary>
    public static class Vector3Ext
    {
        public static Vector3 Invert(this Vector3 vec)
        {
            return new Vector3(1.0f / vec.x, 1.0f / vec.y, 1.0f / vec.z);
        }
        public static Vector3 cwiseMax(this Vector3 vec, Vector3 max)
        {
            return new Vector3(
                vec.x > max.x ? vec.x : max.x,
                vec.y > max.y ? vec.y : max.y,
                vec.z > max.z ? vec.z : max.z);
        }
        public static Vector3 cwiseMin(this Vector3 vec, Vector3 min)
        {
            return new Vector3(
                vec.x < min.x ? vec.x : min.x,
                vec.y < min.y ? vec.y : min.y,
                vec.z < min.z ? vec.z : min.z);
        }
        public static Imstk.Vec3d ToImstkVec(this Vector3 vec)
        {
            return new Imstk.Vec3d(vec.x, vec.y, vec.z);
        }
    }

    /// <summary>
    /// Extensions to Unity Vector2Int
    /// </summary>
    public static class Vector2IntExt
    {
        public static Vector2Int cwiseMax(this Vector2Int vec, Vector2Int max)
        {
            return new Vector2Int(
                vec.x > max.x ? vec.x : max.x,
                vec.y > max.y ? vec.y : max.y);
        }
        public static Vector2Int cwiseMin(this Vector2Int vec, Vector2Int min)
        {
            return new Vector2Int(
                vec.x < min.x ? vec.x : min.x,
                vec.y < min.y ? vec.y : min.y);
        }
        public static Imstk.Vec2i ToImstkVec(this Vector2Int vec)
        {
            return new Imstk.Vec2i(vec.x, vec.y);
        }
    }

    /// <summary>
    /// Extensions to Unity Vector3Int
    /// </summary>
    public static class Vector3IntExt
    {
        public static Vector3Int cwiseMax(this Vector3Int vec, Vector3Int max)
        {
            return new Vector3Int(
                vec.x > max.x ? vec.x : max.x,
                vec.y > max.y ? vec.y : max.y,
                vec.z > max.z ? vec.z : max.z);
        }
        public static Vector3Int cwiseMin(this Vector3Int vec, Vector3Int min)
        {
            return new Vector3Int(
                vec.x < min.x ? vec.x : min.x,
                vec.y < min.y ? vec.y : min.y,
                vec.z < min.z ? vec.z : min.z);
        }
        public static Imstk.Vec3i ToImstkVec(this Vector3Int vec)
        {
            return new Imstk.Vec3i(vec.x, vec.y, vec.z);
        }
    }

    /// <summary>
    /// Extensions to Unity Quaternion
    /// </summary>
    public static class QuaternionExt
    {
        public static Imstk.Quatd ToImstkQuat(this Quaternion quat)
        {
            return new Imstk.Quatd(quat.x, quat.y, quat.z, quat.w);
        }
        public static Vector4 ToVector4(this Quaternion quat)
        {
            return new Vector4(quat.x, quat.y, quat.z, quat.w);
        }
    }

    /// <summary>
    /// Extensions to Unity Quaternion
    /// </summary>
    public static class Vector4Ext
    {
        public static Quaternion ToQuat(this Vector4 vec)
        {
            return new Quaternion(vec.x, vec.y, vec.z, vec.w);
        }
    }

    /// <summary>
    /// Extensions to Unity Transform
    /// </summary>
    public static class Matrix4x4Ext
    {
        public static Imstk.Mat4d ToMat4d(this Matrix4x4 m)
        {
            Imstk.Mat4d results = new Imstk.Mat4d();
            for (int c = 0; c < 4; c++)
            {
                for (int r = 0; r < 4; r++)
                {
                    results.setValue(r, c, m[r, c]);
                }
            }
            return results;
        }
    }

    public static class MathUtil
    {
        public static T Max<T>(T lhs, T rhs) where T : struct, IComparable<T>
        {
            return (lhs.CompareTo(rhs) > 0 ? lhs : rhs);
        }

        /// <summary>
        /// Convert imstk vertices to Vector3 array
        /// </summary>
        public static Vector3[] ToVector3Array(Imstk.VecDataArray3d vecDataArray)
        {
            Vector3[] vector3Array = new Vector3[vecDataArray.size()];
            double[] dArray = new double[vecDataArray.size() * 3];
            vecDataArray.getValues(dArray);
            for (uint i = 0, j = 0; i < vecDataArray.size(); i++)
            {
                vector3Array[i].x = (float)dArray[j++];
                vector3Array[i].y = (float)dArray[j++];
                vector3Array[i].z = (float)dArray[j++];
            }
            return vector3Array;
        }


        /// <summary>
        /// Convert imstk vertices to Vector3 array avoiding allocations, not reentrant !
        /// </summary>
        private static double[] _doubleArrayBuffer = new double[4096];
        public static void ToVector3Array(Imstk.VecDataArray3d vecDataArray, ref Vector3[] vector3Array)
        {
            if (vector3Array == null || vector3Array.Length < vecDataArray.size())
            {
                vector3Array = new Vector3[vecDataArray.size()];
            }
            if (_doubleArrayBuffer.Length < vecDataArray.size() * 3)
            {
                _doubleArrayBuffer = new double[vecDataArray.size() * 3];
            }

            vecDataArray.getValues(_doubleArrayBuffer);
            for (uint i = 0, j = 0; i < vecDataArray.size(); i++)
            {
                vector3Array[i].x = (float)_doubleArrayBuffer[j++];
                vector3Array[i].y = (float)_doubleArrayBuffer[j++];
                vector3Array[i].z = (float)_doubleArrayBuffer[j++];
            }
        }


        /// <summary>
        /// Convert Vector3 array to imstk data array
        /// </summary>
        public static Imstk.VecDataArray3d ToVecDataArray3d(Vector3[] array)
        {
            double[] dArray = new double[array.Length * 3];
            for (uint i = 0, j = 0; i < array.Length; i++)
            {
                dArray[j++] = array[i].x;
                dArray[j++] = array[i].y;
                dArray[j++] = array[i].z;
            }
            Imstk.VecDataArray3d vecDataArray = new Imstk.VecDataArray3d(array.Length);
            vecDataArray.setValues(dArray);
            return vecDataArray;
        }

        /// <summary>
        /// Convert Vector2 array to imstk data array
        /// </summary>

        public static Imstk.VecDataArray2f ToVecDataArray2f(Vector2[] array)
        {
            float[] fArray = new float[array.Length * 2];
            for (uint i = 0, j = 0; i < array.Length; i++)
            {
                fArray[j++] = array[i].x;
                fArray[j++] = array[i].y;
            }
            Imstk.VecDataArray2f vecDataArray = new Imstk.VecDataArray2f(array.Length);
            vecDataArray.setValues(fArray);
            return vecDataArray;
        }

        /// <summary>
        /// Convert int array to imstk data array
        /// </summary>
        public static Imstk.VecDataArray2i ToVecDataArray2i(int[] array)
        {
            if (array.Length % 2 != 0)
            {
                Debug.LogError("array must be divisible by 2");
            }

            Imstk.VecDataArray2i vecDataArray = new Imstk.VecDataArray2i(array.Length / 2);
            vecDataArray.setValues(array);
            return vecDataArray;
        }



        /// <summary>
        /// Convert int array to imstk data array
        /// </summary>
        public static Imstk.VecDataArray3i ToVecDataArray3i(int[] array)
        {
            if (array.Length % 3 != 0)
            {
                Debug.LogError("array must be divisible by 3");
            }

            Imstk.VecDataArray3i vecDataArray = new Imstk.VecDataArray3i(array.Length / 3);
            vecDataArray.setValues(array);
            return vecDataArray;
        }

        /// <summary>
        /// Convert int array to imstk data array
        /// </summary>
        public static Imstk.VecDataArray4i ToVecDataArray4i(int[] array)
        {
            if (array.Length % 4 != 0)
            {
                Debug.LogError("array must be divisible by 4");
            }

            Imstk.VecDataArray4i vecDataArray = new Imstk.VecDataArray4i(array.Length / 4);
            vecDataArray.setValues(array);
            return vecDataArray;
        }

        /// <summary>
        /// Converts VecDataArray2f to int array
        /// </summary>
        public static Vector2[] ToVector2Array(Imstk.VecDataArray2f vecDataArray)
        {
            float[] fArray = new float[vecDataArray.size() * 2];
            vecDataArray.getValues(fArray);

            Vector2[] array = new Vector2[vecDataArray.size()];
            for (int i = 0, j = 0; i < vecDataArray.size(); i++)
            {
                array[i].x = fArray[j++];
                array[i].y = fArray[j++];
            }
            return array;
        }

        /// <summary>
        /// Converts VecDataArray2i to int array
        /// </summary>
        public static int[] ToIntArray(Imstk.VecDataArray2i vecDataArray)
        {
            int[] array = new int[vecDataArray.size() * 2];
            vecDataArray.getValues(array);
            return array;
        }

        /// <summary>
        /// Converts VecDataArray3i to int array
        /// </summary>
        public static int[] ToIntArray(Imstk.VecDataArray3i vecDataArray)
        {
            int[] array = new int[vecDataArray.size() * 3];
            vecDataArray.getValues(array);
            return array;
        }

        /// <summary>
        /// Converts VecDataArray4i to int array
        /// </summary>
        public static int[] ToIntArray(Imstk.VecDataArray4i vecDataArray)
        {
            int[] array = new int[vecDataArray.size() * 4];
            vecDataArray.getValues(array);
            return array;
        }

        public static void ToIntArray(Imstk.VecDataArray3i vecDataArray, ref int[] inArray)
        {
            if (inArray == null || inArray.Length !=  vecDataArray.size() * 3)
            {
                inArray = new int[vecDataArray.size() * 3];
            }
            vecDataArray.getValues(inArray);
        }
    }
}

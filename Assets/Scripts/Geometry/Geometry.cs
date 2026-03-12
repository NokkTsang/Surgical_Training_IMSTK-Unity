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
    public enum GeometryType
    {
        Capsule,
        Cylinder,
        HexahedralMesh,
        ImageData,
        LineMesh,
        OrientedBox,
        Plane,
        PointSet,
        Sphere,
        SurfaceMesh,
        TetrahedralMesh,
        UnityMesh,
        CompoundGeometry
    };

    /// <summary>
    /// We need to reimplement storage for all geometry classes in iMSTK as Unity
    /// import and transfer from editor to runtime works via serialization
    /// afaik it would be quite significant undertaking, and very possibly impossible
    /// to serialize non C# classes, not to mention sensitive to changes. Thus the
    /// base iMSTK geometry cannot be used for geometry storage
    /// </summary>
    public class Geometry : ScriptableObject
    {
        public GeometryType geomType = GeometryType.Plane;

        public bool IsMesh { get { return
                    (geomType == GeometryType.PointSet ||
                    geomType == GeometryType.LineMesh ||
                    geomType == GeometryType.SurfaceMesh ||
                    geomType == GeometryType.TetrahedralMesh ||
                    geomType == GeometryType.HexahedralMesh); } }
        public bool IsVolume { get { return (geomType == GeometryType.TetrahedralMesh || geomType == GeometryType.HexahedralMesh); } }
    };
}
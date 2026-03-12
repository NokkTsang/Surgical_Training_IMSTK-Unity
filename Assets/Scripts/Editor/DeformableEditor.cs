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
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ImstkUnity;
using UnityEngine;
using UnityEditor;
using Imstk;

namespace ImstkEditor
{
    [CustomEditor(typeof(Deformable))]
    public class DeformableEditor : DynamicalModelEditor
    {
        bool _cleanVisualMesh = false;

        bool _bodyDamping = false;
        double _linearDampingCoeff = 0.0;
        double _angularDampingCoeff = 0.0;

        bool _meshInfoOpen = false;

        enum MeshType
        {
            None,
            Line,
            Surface,
            Tetrahedral
        }

        private static Dictionary<MeshType, List<bool>> enabledConstraints = new Dictionary<MeshType, List<bool>>()
        {
            // Dist, Bend, Dihedral, Area, Volume, Fem
            { MeshType.None, new List<bool>() { false, false, false, false, false, false } },
            { MeshType.Line, new List<bool>() { true, true, false, false, false, false } },
            { MeshType.Surface, new List<bool>() { true, false, true, true, false, false } },
            { MeshType.Tetrahedral, new List<bool>() { true, false, false, false, true, true } }
        };
        
        static MeshType GetMeshType(GeometryFilter filter)
        {
            if (filter == null)
                return MeshType.None;

            if (filter.type == GeometryType.TetrahedralMesh)
                return MeshType.Tetrahedral;

            if (filter.type == GeometryType.SurfaceMesh)
                return MeshType.Surface;

            if (filter.type == GeometryType.UnityMesh)
            {
                var mesh = filter.inputUnityGeom;
                if (mesh == null) return MeshType.None;
                switch (mesh.GetTopology(0))
                {
                    case MeshTopology.Triangles:
                        return MeshType.Surface;
                    case MeshTopology.Lines:
                    case MeshTopology.LineStrip:
                        return MeshType.Line;
                    default:
                        Debug.LogWarning("Unhandeled Unity Mesh Topology");
                        return MeshType.None;
                }
            }
            return MeshType.None;
        }

        public override void OnInspectorGUI()
        {
            Deformable script = target as Deformable;
            EditorGUI.BeginChangeCheck();

            GeometryFilter visualGeomFilter = EditorUtils.GeomFilterField("Visual Geometry", script.visualGeomFilter);
            GeometryFilter physicsGeomFilter = EditorUtils.GeomFilterField("Physics Geometry", script.physicsGeomFilter);            
            GeometryFilter collisionGeomFilter = EditorUtils.GeomFilterField("Collision Geometry", script.collisionGeomFilter);

            if (visualGeomFilter != null && visualGeomFilter == physicsGeomFilter)
            {
                bool val = script.cleanVisualMesh;
                if (visualGeomFilter != script.visualGeomFilter)
                {
                    val = visualGeomFilter.type == GeometryType.UnityMesh;
                }

                GUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.HelpBox(
                   "Some Mesh import may create `duplicate vertices` this will cause your physics object" +
                    " to fall apart at seams that might not be visible in the data. use the `Clean Visual Mesh` option" +
                    " to prevent this." ,MessageType.Warning);
                _cleanVisualMesh = EditorGUILayout.Toggle("Clean Visual Mesh", val);
                GUILayout.EndVertical();
            }
            _meshInfoOpen = EditorGUILayout.Foldout(_meshInfoOpen,"Mesh Stats");
            if (_meshInfoOpen )
            {
                EditorGUILayout.BeginVertical();

                string[] names = { "Visual Geom", "Pysics Geom", "Collision Geom"};
                GeometryFilter[] filters = {script.visualGeomFilter, script.physicsGeomFilter, script.collisionGeomFilter};

                for (int i = 0; i < names.Length; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(names[i]);
                    if (filters[i] != null)
                    {
                        ShowGeometryStats(filters[i]);
                    }
                    EditorGUILayout.EndHorizontal();

                }


                EditorGUILayout.EndVertical();
            }

            var enables = enabledConstraints[GetMeshType(physicsGeomFilter)]; 
            
            GUI.enabled = enables[0];
            
            GUILayout.BeginVertical(EditorStyles.helpBox);
            bool useDistanceConstraint = EditorGUILayout.Toggle("Distance Stiffness", script.useDistanceConstraint);
            double distanceStiffness = script.distanceStiffness;
            if (useDistanceConstraint)
                distanceStiffness = EditorGUILayout.DoubleField("Stiffness", script.distanceStiffness);
            GUILayout.EndVertical();

            GUI.enabled = enables[1];
            GUILayout.BeginVertical(EditorStyles.helpBox);
            bool useBendConstraint = EditorGUILayout.Toggle("Bend Stiffness", script.useBendConstraint);
            double bendStiffness = script.bendStiffness;
            int bendStride = script.maxBendStride;
            if (useBendConstraint)
            {
                bendStiffness = EditorGUILayout.DoubleField("Stiffness", script.bendStiffness);
                bendStride = EditorGUILayout.IntField("Stride", script.maxBendStride);
            }
            GUILayout.EndVertical();

            GUI.enabled = enables[2];
            GUILayout.BeginVertical(EditorStyles.helpBox);
            bool useDihedralConstraint = EditorGUILayout.Toggle("Dihedral Stiffness", script.useDihedralConstraint);
            double dihedralStiffness = script.dihedralStiffness;
            if (useDihedralConstraint)
                dihedralStiffness = EditorGUILayout.DoubleField("Stiffness", script.dihedralStiffness);
            GUILayout.EndVertical();

            GUI.enabled = enables[3];
            GUILayout.BeginVertical(EditorStyles.helpBox);
            bool useAreaConstraint = EditorGUILayout.Toggle("Area Stiffness", script.useAreaConstraint);
            double areaStiffness = script.areaStiffness;
            if (useAreaConstraint)
                areaStiffness = EditorGUILayout.DoubleField("Stiffness", script.areaStiffness);
            GUILayout.EndVertical();

            GUI.enabled = enables[4];
            GUILayout.BeginVertical(EditorStyles.helpBox);
            bool useVolumeConstraint = EditorGUILayout.Toggle("Volume Stiffness", script.useVolumeConstraint);
            double volumeStiffness = script.volumeStiffness;
            if (useVolumeConstraint)
                volumeStiffness = EditorGUILayout.DoubleField("Stiffness", script.volumeStiffness);
            GUILayout.EndVertical();

            GUI.enabled = enables[5];
            GUILayout.BeginVertical(EditorStyles.helpBox);
            bool useFEMConstraint = EditorGUILayout.Toggle("FEM", script.useFEMConstraint);
            double youngsModulus = script.youngsModulus;
            double possionsRatio = script.possionsRatio;
            double mu = script.mu;
            double lambda = script.lambda;
            Imstk.PbdFemConstraint.MaterialType materialType = script.materialType;
            if (useFEMConstraint)
            {
                youngsModulus = EditorGUILayout.DoubleField("Youngs Modulus", script.youngsModulus);
                possionsRatio = EditorGUILayout.DoubleField("Possions Ratio", script.possionsRatio);                
                materialType = (Imstk.PbdFemConstraint.MaterialType)EditorGUILayout.EnumPopup("Material Type", script.materialType);
            }
            GUILayout.EndVertical();
            
            GUI.enabled = true;
            
            GUILayout.BeginVertical(EditorStyles.helpBox);

            bool ignoreGravity = EditorGUILayout.Toggle("Ignore Gravity", script.ignoreGravity);

            double uniformMassValue = EditorGUILayout.DoubleField("Uniform Mass Value", script.uniformMassValue);
            if (script.physicsGeomFilter != null)
            {
                var physicsGeom = Imstk.Utils.CastTo<Imstk.PointSet>(script.GetPhysicsGeometry());
                if (physicsGeom != null)
                {
                    var count = physicsGeom.getNumVertices();
                    double mass = EditorGUILayout.DoubleField("Mass ", count * uniformMassValue);
                    if (mass != count * uniformMassValue)
                    {
                        uniformMassValue = mass / count;
                    }
                }
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical(EditorStyles.helpBox);
            _bodyDamping = EditorGUILayout.Toggle("Use Body Damping", script.useBodyDamping);
            if (_bodyDamping)
            {
                _linearDampingCoeff = EditorGUILayout.Slider("Linear Damping Coeff", (float)script.linearDampingCoeff, 0, 1);
                _angularDampingCoeff = EditorGUILayout.Slider("Angular Damping Coeff", (float)script.angularDampingCoeff, 0, 1);
            }
            GUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RegisterCompleteObjectUndo(script, "Change of Parameters");
                script.useDistanceConstraint = useDistanceConstraint && enables[0];
                script.distanceStiffness = distanceStiffness;
                script.useBendConstraint = useBendConstraint && enables[1];
                script.bendStiffness = bendStiffness;
                script.maxBendStride = bendStride;
                script.useDihedralConstraint = useDihedralConstraint && enables[2];
                script.dihedralStiffness = dihedralStiffness;
                script.useAreaConstraint = useAreaConstraint && enables[3];
                script.areaStiffness = areaStiffness;
                script.useVolumeConstraint = useVolumeConstraint && enables[4];
                script.volumeStiffness = volumeStiffness;
                script.useFEMConstraint = useFEMConstraint && enables[5];
                script.youngsModulus = youngsModulus;
                script.possionsRatio = possionsRatio;
                script.mu = mu;
                script.lambda = lambda;
                script.uniformMassValue = uniformMassValue;
                script.materialType = materialType;

                script.useBodyDamping = _bodyDamping;
                script.linearDampingCoeff = _linearDampingCoeff;
                script.angularDampingCoeff = _angularDampingCoeff;

                script.visualGeomFilter = visualGeomFilter;
                script.cleanVisualMesh = _cleanVisualMesh;
                script.physicsGeomFilter = physicsGeomFilter;
                script.collisionGeomFilter = collisionGeomFilter;

                script.ignoreGravity = ignoreGravity;
            }

            base.HandleColliders(script);

        }

        void ShowGeometryStats(GeometryFilter filter)
        {

            int vertices = -1;
            int triangles = -1;
            int tetrahedra = -1;

            var mesh = filter.GetOutputGeometry();

            if (filter.type == GeometryType.TetrahedralMesh) 
            {
                var tetMesh = Imstk.Utils.CastTo<TetrahedralMesh>(mesh);
                tetrahedra = tetMesh.getNumTetrahedra();
                vertices = tetMesh.getNumVertices();
            }

            if (filter.type == GeometryType.SurfaceMesh || filter.type == GeometryType.UnityMesh) 
            {
                var surfaceMesh = Imstk.Utils.CastTo<SurfaceMesh>(mesh);
                triangles = surfaceMesh.getNumTriangles();
                vertices = surfaceMesh.getNumVertices();
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Ver:{(vertices >= 0 ? vertices.ToString() : "N/A")}", GUILayout.Width(60));
            EditorGUILayout.LabelField($"Tri:{(triangles >= 0 ? triangles.ToString() : "N/A")}", GUILayout.Width(60));
            EditorGUILayout.LabelField($"Tet:{(tetrahedra >= 0 ? tetrahedra.ToString() : "N/A")}", GUILayout.Width(60));
            EditorGUILayout.EndHorizontal();

        }
    }
}
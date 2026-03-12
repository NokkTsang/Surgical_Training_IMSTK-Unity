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

using ImstkUnity;
using UnityEngine;
using UnityEditor;
using Imstk;

namespace ImstkEditor
{
    [CustomEditor(typeof(Rigid))]
    class RigidEditor : DynamicalModelEditor
    {
        GeometryFilter _physicsGeometryFilter;
        bool _drawPhysicsGeometry = false;

        GeometryFilter _collisionGeometryFilter;
        bool _drawCollisionGeometry = false;

        bool _bodyDamping = false;
        double _linearDampingCoeff = 0.0;
        double _angularDampingCoeff = 0.0;

        readonly GUIContent _debugContent = new GUIContent("","Draw debug mesh");
        readonly GUILayoutOption _checkBoxWidth = GUILayout.Width(20);
        public override void OnInspectorGUI()
        {
            Rigid script = target as Rigid;
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginHorizontal();
            _physicsGeometryFilter = EditorUtils.GeomFilterField("Physics Geometry", script.physicsGeomFilter);
            _drawPhysicsGeometry = EditorGUILayout.Toggle(_debugContent, script.drawPhysicsGeometry, _checkBoxWidth);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            _collisionGeometryFilter = EditorUtils.GeomFilterField("Collision Geometry", script.collisionGeomFilter);
            _drawCollisionGeometry = EditorGUILayout.Toggle(_debugContent, script.drawCollisionGeometry, _checkBoxWidth);
            EditorGUILayout.EndHorizontal();

            GUILayout.BeginVertical(EditorStyles.helpBox);
            double mass = EditorGUILayout.DoubleField("Mass", script.mass);
            if (mass < 0.0)
            {
                Debug.LogWarning("Mass cannot be negative!");
                mass = 0.0;
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Inertia Tensor");
            Vector3 inertiaRow1 = EditorGUILayout.Vector3Field("", script.inertia[0]);
            Vector3 inertiaRow2 = EditorGUILayout.Vector3Field("", script.inertia[1]);
            Vector3 inertiaRow3 = EditorGUILayout.Vector3Field("", script.inertia[2]);
            GUILayout.EndVertical();

            var ignoreGravity = EditorGUILayout.Toggle("Ignore Gravity", script.ignoreGravity);

            GUILayout.BeginVertical(EditorStyles.helpBox);
            _bodyDamping = EditorGUILayout.Toggle("Use Body Damping", script.useBodyDamping);
            if (_bodyDamping)
            {
                _linearDampingCoeff = EditorGUILayout.Slider("Linear Damping Coeff", (float)script.linearDampingCoeff, 0, 1);
                _angularDampingCoeff = EditorGUILayout.Slider("Angular Damping Coeff", (float)script.angularDampingCoeff, 0, 1);
            }
            GUILayout.EndVertical();

            //          Need to update PbdBody class to better support this when accessing it through the wrapper
            //             GUILayout.BeginVertical(EditorStyles.helpBox);
            //             Vector3 initVel = EditorGUILayout.Vector3Field("Initial Linear Velocity", script.initVelocity);
            //             Vector3 initAngularVel = EditorGUILayout.Vector3Field("Initial Angular Velocity", script.initAngularVelocity);
            //             GUILayout.EndVertical();
            // 
            //             GUILayout.BeginVertical(EditorStyles.helpBox);
            //             Vector3 initForce = EditorGUILayout.Vector3Field("Initial Force", script.initForce);
            //             Vector3 initTorque = EditorGUILayout.Vector3Field("Initial Torque", script.initTorque);
            //             GUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RegisterCompleteObjectUndo(script, "Change of Parameters");
                
                script.physicsGeomFilter = _physicsGeometryFilter;
                script.drawPhysicsGeometry = _drawPhysicsGeometry;

                script.collisionGeomFilter = _collisionGeometryFilter;
                script.drawCollisionGeometry = _drawCollisionGeometry;

                script.mass = mass;
                script.inertia[0] = inertiaRow1;
                script.inertia[1] = inertiaRow2;
                script.inertia[2] = inertiaRow3;

                script.ignoreGravity = ignoreGravity;

                script.useBodyDamping = _bodyDamping;
                script.linearDampingCoeff = _linearDampingCoeff;
                script.angularDampingCoeff = _angularDampingCoeff;


                //                 script.initVelocity = initVel;
                //                 script.initAngularVelocity = initAngularVel;
                //                 script.initForce = initForce;
                //                 script.initTorque = initTorque;
            }

            base.HandleColliders(script);

        }

        [DrawGizmo(GizmoType.Selected | GizmoType.Active)]
        public static void DrawRigidGizmo(Rigid rigid, GizmoType gizmoType)
        {
            var dyn = rigid.GetDynamicObject();
            var pbdObj = Imstk.Utils.CastTo<PbdObject>(dyn);
            if (pbdObj != null)
            {
                if (rigid.drawCollisionGeometry)
                {
                    DrawGeometryGizmo(pbdObj.getCollidingGeometry());
                }
                if (rigid.drawPhysicsGeometry)
                {
                    DrawGeometryGizmo(pbdObj.getPhysicsGeometry());
                }
            }
        }

        public static void DrawGeometryGizmo(Imstk.Geometry geom)
        {
            var pointSet = Imstk.Utils.CastTo<PointSet>(geom);
            if (pointSet != null)
            {
                ImstkGizmos.DrawMesh(pointSet, UnityEngine.Color.red);
            }
        }
    }
}
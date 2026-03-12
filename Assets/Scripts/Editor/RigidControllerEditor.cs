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

namespace ImstkEditor
{
    [CustomEditor(typeof(RigidController))]
    public class RigidControllerEditor : Editor
    {
        Vector3 _attachmentPoint;
        Vector3 _translationalOffset;
        Vector3 _rotationalOffset;
        Vector3 _localRotationalOffset;
        double _translationScaling;
        public override void OnInspectorGUI()
        {
            RigidController script = target as RigidController;

            EditorGUI.BeginChangeCheck();

            GUILayout.BeginVertical(EditorStyles.helpBox);
            TrackingDevice results = script.device;
            Object obj = EditorGUILayout.ObjectField("Device", results, typeof(Object), true) as Object;
            if (obj is TrackingDevice)
            {
                results = obj as TrackingDevice;
            }
            else if (obj is GameObject)
            {
                results = (obj as GameObject).GetComponent<TrackingDevice>();
            }
            else
            {
                Debug.LogWarning("Cannot set object on field, expects TrackingDevice or game object with TrackingDevice");
            }
            Rigid rigid = EditorGUILayout.ObjectField("Rigid", script.rigid, typeof(Rigid), true) as Rigid;
            GUILayout.EndVertical();


            GUILayout.BeginVertical(EditorStyles.helpBox);
            bool useCriticalDamping = EditorGUILayout.Toggle("Critical Damping", script.useCriticalDamping);
            double linearKs = EditorGUILayout.DoubleField("Linear Spring Constant", script.linearKs);
            double linearKd = script.linearKd;
            if (!useCriticalDamping)
            {
                linearKd = EditorGUILayout.DoubleField("Linear Damping", script.linearKd);
            }

            double angularKs = EditorGUILayout.DoubleField("Angular Spring Constant", script.angularKs);
            double angularKd = script.angularKd;
            if (!useCriticalDamping)
            {
                angularKd = EditorGUILayout.DoubleField("Angular Damping", script.angularKd);
            }
            GUILayout.EndVertical();

            script._forceFoldout = EditorGUILayout.Foldout(script._forceFoldout, "Force Settings");
            double forceScaling = script.forceScale;
            bool useForceSmoothing = script.useForceSmoothing;
            int forceSmoothKernelSize = script.forceSmoothingKernelSize;
            if (script._forceFoldout)
            {
                GUILayout.BeginVertical(EditorStyles.helpBox);
                forceScaling = EditorGUILayout.DoubleField("Force Scaling", script.forceScale);
                useForceSmoothing = EditorGUILayout.Toggle("Use Force Smoothing", script.useForceSmoothing);
                forceSmoothKernelSize =
                    EditorGUILayout.IntField("Force Smooth Kernel Size", script.forceSmoothingKernelSize);
                GUILayout.EndVertical();
            }

            var offsetContent = new GUIContent("Offsets", "Settings that will transform the controlled objects coordinate system in " +
                    "relation to the device coordinate system");
            script._offsetFoldout = EditorGUILayout.Foldout(script._offsetFoldout, offsetContent);

            // Initialize in case the foldout is closed

            _attachmentPoint = script.attachmentPoint;
            _translationScaling = script.translationScaling;
            _translationalOffset = script.translationalOffset;
            _rotationalOffset = script.rotationalOffset.eulerAngles;
            _localRotationalOffset = script.localRotationalOffset.eulerAngles;

            if (script._offsetFoldout)
            {
                GUILayout.BeginVertical(EditorStyles.helpBox);

                _attachmentPoint = EditorGUILayout.Vector3Field("Attachment Point", script.attachmentPoint);

                if (GUILayout.Button("Copy Translational Offset from Transform"))
                {
                    _translationalOffset = script.transform.position;
                } else
                {
                    _translationalOffset =
                        EditorGUILayout.Vector3Field("Translational Offset", script.translationalOffset);
                }

                _rotationalOffset =
                    EditorGUILayout.Vector3Field("Rotational Offset", script.rotationalOffset.eulerAngles);
                _localRotationalOffset =
                    EditorGUILayout.Vector3Field("Local Rotational Offset", script.localRotationalOffset.eulerAngles);

                _translationScaling =
                    EditorGUILayout.DoubleField("Translation Scaling", script.translationScaling);
                GUILayout.EndVertical();
            }

            script._axisMappingFoldout = EditorGUILayout.Foldout(script._axisMappingFoldout, "Coordinate Transforms");
            bool invertX = script.invertX;
            bool invertY = script.invertY;
            bool invertZ = script.invertZ;
            bool rotInvertX = script.invertRotX;
            bool rotInvertY = script.invertRotY;
            bool rotInvertZ = script.invertRotZ;
            if (script._axisMappingFoldout)
            {
                GUILayout.BeginVertical(EditorStyles.helpBox);
                invertX = EditorGUILayout.Toggle("Invert X", script.invertX);
                invertY = EditorGUILayout.Toggle("Invert Y", script.invertY);
                invertZ = EditorGUILayout.Toggle("Invert Z", script.invertZ);
                rotInvertX = EditorGUILayout.Toggle("Rotation Invert X", script.invertRotX);
                rotInvertY = EditorGUILayout.Toggle("Rotation Invert Y", script.invertRotY);
                rotInvertZ = EditorGUILayout.Toggle("Rotation Invert Z", script.invertRotZ);
                GUILayout.EndVertical();
            }

            bool debugController = EditorGUILayout.Toggle("Write Controller transform", script.debugController);


            if (EditorGUI.EndChangeCheck())
            {
                Undo.RegisterCompleteObjectUndo(script, "Change of Parameters");
                script.device = results;
                script.rigid = rigid;

                script.angularKd = angularKd;
                script.angularKs = angularKs;
                script.linearKd = linearKd;
                script.linearKs = linearKs;

                script.useCriticalDamping = useCriticalDamping;

                script.forceScale = forceScaling;
                script.useForceSmoothing = useForceSmoothing;
                script.forceSmoothingKernelSize = forceSmoothKernelSize;

                script.attachmentPoint = _attachmentPoint;
                script.translationalOffset = _translationalOffset;
                script.rotationalOffset.eulerAngles = _rotationalOffset;
                script.localRotationalOffset.eulerAngles = _localRotationalOffset;
                script.translationScaling = _translationScaling;

                script.invertX = invertX;
                script.invertY = invertY;
                script.invertZ = invertZ;
                script.invertRotX = rotInvertX;
                script.invertRotY = rotInvertY;
                script.invertRotZ = rotInvertZ;

                script.debugController = debugController;
            }
        }

        [DrawGizmo(GizmoType.InSelectionHierarchy)]
        static void DrawHandles(RigidController scr, GizmoType gizmoType)
        {
            
            var pos = scr.transform.localToWorldMatrix.MultiplyPoint(scr.attachmentPoint);

            // Draw a sphere at the haptic attachment point
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(pos, 0.02f * scr.transform.lossyScale.magnitude);

        }
    }
}
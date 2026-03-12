using ImstkUnity;
using UnityEditor;
using UnityEngine;

namespace ImstkEditor
{
    [CustomEditor(typeof(ConnectiveTissue))]
    public class ConnectiveTissueEditor : Editor
    {
        // Local variables for caching editor results
        Deformable _sideA;
        Deformable _sideB;

        double _maxDistance;
        double _strandsPerFace;
        int _segmentsPerStrand;
        double _distanceStiffness;
        double _uniformMassValue;
        double _viscousDampingCoeff;
        double _strandAngleDeviation;
        double _stretch;

        GUIContent sideAContent = new GUIContent("Side A", "One side of the objects that should be connected.");
        GUIContent sideBContent = new GUIContent("Side B", "One side of the objects that should be connected.");
        GUIContent maxDistContent = new GUIContent("Maximum Distance", "If side a and b are closer than this value" +
            " connective tissue strands will be generated. If 0 the distance between the centers will be used");
        GUIContent strandsPerFaceContent = new GUIContent("Strands per Face", "Indicates the density of strands " +
            "that are being generated, fractions can be used e.g. 0.5 will generate a strand for half the faces");
        GUIContent segmentsPerStrandContent = new GUIContent("Segments per Strand", "Determines the number of " +
            "segments for each strand");
        GUIContent distanceStiffnessContent = new GUIContent("Distance Stiffness", "Determines how much the " +
            "connective tissue will resist extension.");
        GUIContent massValueContent = new GUIContent("Uniform Mass Value", "Mass per vertex of the object");
        GUIContent viscousDampingContent = new GUIContent("Viscous Damping", "Dampens the system");
        GUIContent _strandAngleDeviationContent = new GUIContent("Max Angle Deviation (Deg)", "Strands are allowed to deviate by this amount");
        GUIContent _stretchContent = new GUIContent("Stretch", "On generation the target length of tissue strand is multiplied by this");
        public override void OnInspectorGUI()
        {
            var script = target as ConnectiveTissue;

            EditorGUI.BeginChangeCheck();

            _sideA = EditorGUILayout.ObjectField(sideAContent, script.objectA, typeof(ImstkUnity.Deformable), true) as ImstkUnity.Deformable;
            _sideB = EditorGUILayout.ObjectField(sideBContent,script.objectB, typeof(ImstkUnity.Deformable), true) as ImstkUnity.Deformable;

            _maxDistance = EditorGUILayout.DoubleField(maxDistContent, script.maxDistance);
            _strandsPerFace = EditorGUILayout.DoubleField(strandsPerFaceContent, script.strandsPerFace);
            _segmentsPerStrand = EditorGUILayout.IntField(segmentsPerStrandContent, script.segmentsPerStrand);
            _distanceStiffness = EditorGUILayout.DoubleField(distanceStiffnessContent, script.distanceStiffness);
            _uniformMassValue = EditorGUILayout.DoubleField(massValueContent, script.uniformMassValue);
            _viscousDampingCoeff = EditorGUILayout.DoubleField(viscousDampingContent, script.viscousDampingCoeff);
            _strandAngleDeviation = EditorGUILayout.DoubleField(_strandAngleDeviationContent, script.strandAngleDeviation);
            if (_strandAngleDeviation < 10)
            {
                EditorGUILayout.HelpBox("If the angle gets to small the random generation may have a hard time maintaining " +
                    "this constraint. Strands will be allowed to be further away if no good solution can be found", MessageType.Info);
            }
            _stretch = EditorGUILayout.DoubleField(_stretchContent, script.stretch); 

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RegisterCompleteObjectUndo(script, "Change of Parameters");
                if ((_sideA == null && _sideB == null) || _sideA != _sideB)
                {
                    script.objectA = _sideA;
                    script.objectB = _sideB;
                }

                script.maxDistance = _maxDistance;
                script.strandsPerFace = _strandsPerFace;
                script.segmentsPerStrand = _segmentsPerStrand;
                script.distanceStiffness = _distanceStiffness;
                script.uniformMassValue = _uniformMassValue;
                script.viscousDampingCoeff = _viscousDampingCoeff;
                script.strandAngleDeviation = _strandAngleDeviation;
                script.stretch = _stretch;
            }
        }
    }
}
using Imstk;
using ImstkUnity;
using UnityEditor;
using UnityEngine;

namespace ImstkEditor
{
    [CustomEditor(typeof(ImstkUnity.CollisionInteraction), true)]
    public class CollisionInteractionEditor : Editor
    {
        double _friction = 0.0;
        double _restitution = 0.0;

        double _deformableStiffness1 = 0.2;
        double _deformableStiffness2 = 0.2;

        double _rigidBodyCompliance = 0.0001;

        DynamicalModel _model1;
        DynamicalModel _model2;

        static string[] _options;
        public static string[] CDOptions
        {
            get
            {
                if (_options == null) 
                {
                    var names = CDObjectFactory.getNames();
                    _options = new string[names.Count + 1];
                    _options[0] = "Auto";
                    for (int i = 0; i < names.Count; ++i)
                    {
                        _options[i + 1] = names[i];
                    }
                }
                return _options;
            }
        }

        string _message;

        public override void OnInspectorGUI()
        {
            var script = target as ImstkUnity.CollisionInteraction;

            EditorGUI.BeginChangeCheck();

            _message = "Model 1 " + GetGeometryType(script.model1);
            _model1 = EditorGUILayout.ObjectField(_message, script.model1, typeof(DynamicalModel), true) as DynamicalModel;


            _message = "Model 2 " + GetGeometryType(script.model2);
            _model2 = EditorGUILayout.ObjectField(_message, script.model2, typeof(DynamicalModel), true) as DynamicalModel;

            var selected = System.Array.IndexOf(CDOptions, script.collisionTypeName);
            selected = EditorGUILayout.Popup("Detection Type", selected, CDOptions);

            _message = "Auto Type: ";
            if (_model1 != null && _model1.GetCollidingGeometry() != null &&
                _model2 != null && _model2.GetCollidingGeometry() != null)
            {
                _message += ImstkUnity.CollisionInteraction.GetCDType(_model1, _model2);
            }
            else
            {
                _message += " None";
            }
            EditorGUILayout.LabelField(_message);

            _friction = EditorGUILayout.DoubleField("Friction", script.friction);
            _restitution = EditorGUILayout.DoubleField("Restitution", script.restitution);

            var guiContent = new GUIContent("Deform. Stiffness 1", "A good default value is 1/number of iterations from the simulation manager");
            _deformableStiffness1 = EditorGUILayout.DoubleField(guiContent, script.deformableStiffness1);

            guiContent = new GUIContent("Deform. Stiffness 2", "A good default value is 1/number of iterations from the simulation manager");
            _deformableStiffness2 = EditorGUILayout.DoubleField(guiContent, script.deformableStiffness2);

            _rigidBodyCompliance = EditorGUILayout.DoubleField("Rigid Compliance", script.rigidBodyCompliance);

            if (EditorGUI.EndChangeCheck())
            {
                script.friction = _friction;
                script.restitution = _restitution;

                script.model1 = _model1;
                script.model2 = _model2;

                script.collisionTypeName = CDOptions[selected];

                script.deformableStiffness1 = _deformableStiffness1;
                script.deformableStiffness2 = _deformableStiffness2;

                script.rigidBodyCompliance = _rigidBodyCompliance;
            }
        }

        private string GetGeometryType(DynamicalModel model)
        {
            if (model == null)
            {
                return "";
            }
            Imstk.Geometry geom = model.GetCollidingGeometry();
            if (geom == null)
            {
                return "<no geometry>";
            }
            else
            {
                return "<" + geom.getTypeName() + ">";
            }
        }

    }
}
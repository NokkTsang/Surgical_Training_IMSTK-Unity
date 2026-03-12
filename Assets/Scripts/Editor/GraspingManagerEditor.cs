using ImstkUnity;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ImstkEditor
{
    [CustomEditor(typeof(GraspingManager))]
    public class GraspingManagerEditor : Editor
    {
        bool _folded = true;
        Rigid _rigid;
        List<GeometryFilter> _graspingGeometries;
        List<GraspingManager.GraspingData> _graspingData;

        List<DynamicalModel> _allDeformables = new List<DynamicalModel>();

        GUIContent _warning;

        double _deformableGraspingStiffness;
        double _rigidGraspingStiffness;
        bool _useUnityTransform;
        bool _requireTouch;

        GUIContent _disableColContent = new GUIContent("x", "Disable Collision On Grasping");
        GraspingManagerEditor()
        {
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
        }

        private void OnHierarchyChanged()
        {
            _allDeformables = (Resources.FindObjectsOfTypeAll(typeof(DynamicalModel)) as DynamicalModel[]).ToList<DynamicalModel>();
        }

        public override void OnInspectorGUI()
        {
            if (_allDeformables.Count == 0) OnHierarchyChanged();

            var script = target as GraspingManager;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            var obj = EditorGUILayout.ObjectField("Grasping Object", 
                script.rigid, typeof(ImstkUnity.Rigid), true);
            _rigid = obj as ImstkUnity.Rigid;
            if (script.rigid == null)
            {
                _warning = EditorGUIUtility.IconContent("Error", "|Grasping Object Can't be null");
                EditorGUILayout.LabelField(_warning, GUILayout.Width(_warning.image.width));
            } 
            EditorGUILayout.EndHorizontal();
            
            _useUnityTransform = EditorGUILayout.Toggle("Use Unity Transform", script.useUnityTransform);
            _requireTouch = EditorGUILayout.Toggle("Only grasp when touching", script.requireTouch);
            
            if (_rigid != null)
            {
                _graspingGeometries = EditorTools.ListField("Grasping Geometry (Optional)", script.graspingGeometries);

                GUILayout.BeginVertical(EditorStyles.helpBox);

                _deformableGraspingStiffness = EditorGUILayout.DoubleField("Deformable Grasping Stiffness", script.deformableGraspingStiffness);
                _rigidGraspingStiffness = EditorGUILayout.DoubleField("Rigid Grasping Stiffness", script.rigidGraspingStiffness);

                GUILayout.EndVertical();
            }
            // Maybe this is something we should do for all imstk components 
            // as we can't edit them during runtime anyway
            if (!Application.isPlaying)
            {
                if (_rigid != null)
                {
                    HandleGraspingData(script);
                }
                // Takes up a lot of resources when playing, just don't execute 
                UpdateScript(script);
            }
            else
            {
                EditorGUI.EndChangeCheck();
            }
        }

        private void UpdateScript(GraspingManager script)
        {
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RegisterCompleteObjectUndo(script, "Change of Parameters");
                script.rigid = _rigid;
                script.graspingGeometries = _graspingGeometries;
                script.graspingData = new List<GraspingManager.GraspingData>(_graspingData);
                script.deformableGraspingStiffness = _deformableGraspingStiffness;
                script.rigidGraspingStiffness = _rigidGraspingStiffness;
                script.useUnityTransform = _useUnityTransform;
                script.requireTouch = _requireTouch;
            }
        }

        private void HandleGraspingData(GraspingManager script)
        {
            // if there aren't any items to grasp, make sure ...
            if (_allDeformables.Count == 0) OnHierarchyChanged();

            _graspingData = new List<GraspingManager.GraspingData>(script.graspingData);

            foreach (var item in _allDeformables)
            {
                if (item is StaticModel || item == _rigid) continue;

                if (_graspingData.FindIndex(x => x.target == item) < 0)
                {
                    GraspingManager.GraspingData graspingData = new GraspingManager.GraspingData();
                    graspingData.enabled = false;
                    graspingData.disableCollisionOnGrasp = true;
                    graspingData.type = Grasping.GraspType.Cell;
                    graspingData.target = item;
                    _graspingData.Add(graspingData);
                }
            }

            // Remove stale Objects
            _graspingData.RemoveAll(x => x.target == null || !_allDeformables.Contains(x.target));
            _graspingData.Sort( (a,b) => a.target.name.CompareTo(b.target.name));
            _folded = EditorGUILayout.Foldout(_folded, "Graspable Objects");
            if (_folded)
            {
                EditorGUILayout.BeginVertical();
                for (int i = 0; i < _graspingData.Count; ++i)
                {
                    var item = _graspingData[i];
                    EditorGUILayout.BeginHorizontal();
                    item.enabled = EditorGUILayout.Toggle(item.enabled, GUILayout.Width(20));
                    EditorGUI.BeginDisabledGroup(!item.enabled);
                    EditorGUILayout.LabelField(item.target.name);
                    item.type = (Grasping.GraspType)EditorGUILayout.EnumPopup(item.type);
                    item.disableCollisionOnGrasp = EditorGUILayout.Toggle(item.disableCollisionOnGrasp);
                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.EndHorizontal();
                    _graspingData[i] = item;
                }
                EditorGUILayout.EndVertical();
            }
        }
    }
}
using ImstkUnity;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ImstkEditor
{
    public class DynamicalModelEditor : Editor
    {
        private GUIContent _dropdownClosed;
        private GUIContent _dropdownOpened;
        private GUIStyle _disclosureStyle;
        private Texture2D _backgroundTexture;
		bool _collisionPanelOpen = false;
        List<DynamicalModel> _allDeformables;
        Comparer<DynamicalModel> _comparer = Comparer<DynamicalModel>.Create((a, b) => a.gameObject.name.CompareTo(b.gameObject.name));

        public DynamicalModelEditor()
        {
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
        }

        private Texture2D MakeBackgroundTexture(int width, int height, Color color)
        {
            Color[] pixels = new Color[width * height];
 
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
 
            Texture2D backgroundTexture = new Texture2D(width, height);
 
            backgroundTexture.SetPixels(pixels);
            backgroundTexture.Apply();
 
            return backgroundTexture;
        }
        
        ~DynamicalModelEditor() 
        {
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
        }

        public void OnEnable()
        {
            _dropdownClosed = EditorGUIUtility.IconContent("Animation.Play");
            _dropdownOpened = EditorGUIUtility.IconContent("icon dropdown@2x");
            _disclosureStyle = new GUIStyle();
            _backgroundTexture = MakeBackgroundTexture(1, 1, new Color32(0, 0, 0, 0));
            _disclosureStyle.normal.background = _backgroundTexture;
            _disclosureStyle.margin = new RectOffset(4,4,2,2);
            _disclosureStyle.alignment = TextAnchor.MiddleCenter;
            _disclosureStyle.fixedHeight = 20;
            _disclosureStyle.fixedWidth = 24;
            _disclosureStyle.stretchWidth = false;
            _disclosureStyle.stretchHeight = false;

        }

        private void OnHierarchyChanged()
        {
            _allDeformables = (Resources.FindObjectsOfTypeAll(typeof(DynamicalModel)) as DynamicalModel[]).ToList<DynamicalModel>();

            // Removes Prefabs 
            _allDeformables.RemoveAll(x => EditorUtility.IsPersistent(x.gameObject));
            _allDeformables.Sort(_comparer);
            SimulationManager.Instance().collisions.RemoveAllNull();
        }

        public void HandleColliders(DynamicalModel script) 
		{
            if (Application.isPlaying) return;

            if (_allDeformables == null) 
            {
                OnHierarchyChanged();
            }

			EditorGUI.BeginChangeCheck();
            var editorData = DrawColliders(script);
            if (EditorGUI.EndChangeCheck())
            {
                var simulationManager = SimulationManager.Instance();
                Undo.RegisterCompleteObjectUndo(simulationManager, "Update Collisions");
                script._collisionPanelOpen = _collisionPanelOpen;
                simulationManager.collisions.UpdateFrom(editorData);
            }
		}
		
        protected CollisionInteractions DrawColliders(DynamicalModel script)
        {
            var allCollisions = SimulationManager.Instance().collisions;
            var editorData = new CollisionInteractions(allCollisions);
            editorData.RemoveAllNull();

            _collisionPanelOpen = EditorGUILayout.Foldout(script._collisionPanelOpen, "Colliding Objects");
            if (_collisionPanelOpen)
            {
                EditorGUILayout.BeginVertical();
                for (int i = 0; i < _allDeformables.Count; ++i)
                {
                    var item = _allDeformables[i];
                    if (item == script) continue;
                    EditorGUILayout.BeginVertical();

                    // Title line with Toggle
                    EditorGUILayout.BeginHorizontal();
                    var enabled = allCollisions.IsEnabled(script, item);

                    var autoType = ImstkUnity.CollisionInteraction.GetCDType(script, item);

                    var newEnabled = false;

                    EditorGUI.BeginDisabledGroup(autoType == "");
                    bool oldVisible = true;
                    script._subPanelOpen.TryGetValue(item, out oldVisible);
                    var newVisible = GUILayout.Button((oldVisible) ? _dropdownOpened : _dropdownClosed , _disclosureStyle);
                    newVisible = (newVisible) ? !oldVisible : oldVisible;
                    script._subPanelOpen[item] = newVisible;
                    
                    newEnabled = EditorGUILayout.Toggle(enabled, GUILayout.Width(20));
                    EditorGUI.EndDisabledGroup();
                    if (enabled != newEnabled)
                    {
                        if (newEnabled)
                        {
                            editorData.Add(script, item);
                        }
                        else
                        {
                            editorData.Remove(script, item);
                        }
                    }
                    EditorGUI.BeginDisabledGroup(!newEnabled && autoType == "");
                    EditorGUILayout.LabelField(item.name);
                    if (autoType == "")
                    {
                        EditorGUILayout.LabelField("No collision type available");
                    }
                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.EndHorizontal();
                    if (newEnabled && newVisible)
                    {
                        EditorGUILayout.BeginVertical();
                        var d = editorData.GetData(script, item);

                        var _message = "Auto Type: ";
                        if (d.model1 != null && d.model1.GetCollidingGeometry() != null &&
                            d.model2 != null && d.model2.GetCollidingGeometry() != null)
                        {
                            _message += ImstkUnity.CollisionInteraction.GetCDType(d.model1, d.model2);
                        }
                        else
                        {
                            _message += " None ";
                        }
                        EditorGUILayout.LabelField(_message);

                        var selected = System.Array.IndexOf(CollisionInteractionEditor.CDOptions, d.collisionTypeName);
                        
                        if (selected < 0) selected = 0;

                        selected = EditorGUILayout.Popup("Detection Type", selected, CollisionInteractionEditor.CDOptions);
                        d.collisionTypeName = CollisionInteractionEditor.CDOptions[selected];

                        d.friction = EditorGUILayout.DoubleField("Friction", d.friction);
                        d.restitution = EditorGUILayout.DoubleField("Restitution", d.restitution);

                        var guiContent = new GUIContent("Deform. Stiffness 1", "A good default value is 1/number of iterations from the simulation manager");
                        d.deformableStiffness1 = EditorGUILayout.DoubleField(guiContent, d.deformableStiffness1);

                        guiContent = new GUIContent("Deform. Stiffness 2", "A good default value is 1/number of iterations from the simulation manager");
                        d.deformableStiffness2 = EditorGUILayout.DoubleField(guiContent, d.deformableStiffness2);

                        d.rigidBodyCompliance = EditorGUILayout.DoubleField("Rigid Compliance", d.rigidBodyCompliance);

                        EditorGUILayout.EndVertical();
                    }

                    EditorGUILayout.EndVertical();
                    // Needs to move into the undo section


                }
                EditorGUILayout.EndVertical();
            }

            return editorData;
        }
    }
}


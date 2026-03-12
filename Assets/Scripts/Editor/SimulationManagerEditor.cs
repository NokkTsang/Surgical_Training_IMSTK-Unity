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
    [CustomEditor(typeof(SimulationManager))]
    public class SimulationManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // Define a style for centering text
            var centeredStyle = GUI.skin.GetStyle("Label");
            centeredStyle.alignment = TextAnchor.UpperCenter;

            // Begin the script modification!
            SimulationManager script = target as SimulationManager;
            EditorGUI.BeginChangeCheck();

            GUILayout.Label("Scene Settings", centeredStyle);
            GUILayout.BeginVertical(EditorStyles.helpBox);
            bool writeTaskGraph = EditorGUILayout.Toggle("Write Task Graph", script.writeTaskGraph);
            EditorGUILayout.HelpBox(
               "This is the timestep that iMSTK will used to advance the simulation, use the stats" +
               " display and testing to arrive at a reasonable value for your simulation." ,
               MessageType.Warning);
            float fixedTimestep = EditorGUILayout.FloatField("Fixed Timestep", script.fixedTimestep);
            
            GUILayout.EndVertical();

            GUILayout.Label("Pbd Simulation Settings", centeredStyle);
            GUILayout.BeginVertical(EditorStyles.helpBox);
            var c = new PbdModelConfiguration();
            c.gravity = EditorGUILayout.Vector3Field("Gravity", script.pbdModelConfiguration.gravity);
            c.iterations = EditorGUILayout.IntField("Max # Iterations", script.pbdModelConfiguration.iterations);
            c.useRealtime = EditorGUILayout.Toggle("Use Realtime", script.pbdModelConfiguration.useRealtime);
            if (!script.pbdModelConfiguration.useRealtime)
            {
                c.dt = EditorGUILayout.DoubleField("Delta Time", script.pbdModelConfiguration.dt);
            }
            c.linearDampingCoeff = EditorGUILayout.DoubleField("Linear Damping Coefficient", 
                script.pbdModelConfiguration.linearDampingCoeff);
            c.angularDampingCoeff = EditorGUILayout.DoubleField("Angular Damping Coefficient",
            script.pbdModelConfiguration.angularDampingCoeff);
            c.doPartitioning = EditorGUILayout.Toggle("Partitioning", script.pbdModelConfiguration.doPartitioning);

            GUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RegisterCompleteObjectUndo(script, "Change of Parameters");
                script.writeTaskGraph = writeTaskGraph;
                script.pbdModelConfiguration = c;
                script.fixedTimestep = fixedTimestep;
            }
        }
    }
}
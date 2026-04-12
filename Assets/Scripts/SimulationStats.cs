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
    /// Add this class to the scene to show simple stats about the runtime
    /// performance of the critical parts of iMSTK.
    /// </summary>
    [RequireComponent(typeof(ImstkUnity.SimulationManager))]
    public class SimulationStats : MonoBehaviour
    {
        /// <summary>
        /// Number of frames between recalculation of displayed values
        /// </summary>
        public int calculateRate = 30;

        /// <summary>
        /// Simple Graphics Frame Rate as measured in the Simulation _manager
        /// </summary>
        private float _avgFrameRate = 0.0f;

        /// <summary>
        /// Average time to do one physics update `simulationManager->advance()`
        /// </summary>
        private float _avgPhysicsTime = 0.0f;

        private float _avgEngineTime = 0.0f;

        /// <summary>
        /// Time taken for all Mesh updates (copying imstk mesh data to unity)
        /// </summary>
        private float _meshUpdateTime = 0.0f;

        private int _counter;

        private ImstkUnity.SimulationManager _manager;
        private DeformableModel[] _deformables;

        public bool showStats = false;
        public string key = "p";

        private GUIStyle _greyBackground = new GUIStyle();

        private void Awake()
        {
            _greyBackground.normal.background = MakeTex(2, 2, new Color(0, 0, 0, 0.5f));
            _manager = GetComponent<ImstkUnity.SimulationManager>();
            _deformables = FindObjectsOfType<DeformableModel>();
        }

        private void Update()
        {
            showStats = Input.GetKeyDown(key) ? !showStats : showStats;
        }

        private void OnGUI()
        {

            if (!showStats) return;

            

            if (_counter == 0)
            {
                _counter = calculateRate;
                _avgFrameRate = 1.0f / _manager.FrameTimes.Average;
                _avgPhysicsTime = _manager.PhysicsTimes.AverageTimeMs;
                _avgEngineTime = _manager.EngineTime;

                _meshUpdateTime = 0.0f;
                foreach (var def in _deformables)
                {
                    _meshUpdateTime += def.UpdateTimes.AverageTimeMs;
                }
            }
            --_counter;

            var rect = new Rect(20, Screen.height - 200, 350, 150);
            GUILayout.BeginArea(rect, _greyBackground);
            var fontSize = GUI.skin.label.fontSize;
            GUI.skin.label.fontSize = 24;
            GUILayout.BeginVertical();
            GUILayout.Label(String.Format("Graphics {0:F2} fps", _avgFrameRate),GUILayout.Height(36));
            GUILayout.Label(String.Format("Physics  {0:F2} ms | {1:F2} ms", _avgPhysicsTime, _avgEngineTime), GUILayout.Height(36));
            GUILayout.Label(String.Format("Mesh [{0}]   {1:F2} ms", _deformables.Length, _meshUpdateTime), GUILayout.Height(36));
            
            // Safely get constraint count
            int constraintCount = 0;
            if (SimulationManager.pbdModel != null && SimulationManager.pbdModel.getConstraints() != null)
            {
                constraintCount = SimulationManager.pbdModel.getConstraints().getConstraints().Count;
            }
            GUILayout.Label(String.Format("Constraints (not-part) {0}", constraintCount), GUILayout.Height(36));
            
            GUILayout.EndVertical();
            GUILayout.EndArea();

            GUI.skin.label.fontSize = fontSize;
        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];

            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;

            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();

            return result;
        }
    }


}

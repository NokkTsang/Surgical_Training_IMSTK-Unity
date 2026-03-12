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
    public class TaskGraphStats : MonoBehaviour
    {
        Imstk.UnorderedMapStringDouble _stats;

        // Start is called before the first frame update

        private GUIStyle _greyBackground = new GUIStyle();

        private void Awake()
        {
            _greyBackground.normal.background = MakeTex(2, 2, new Color(0, 0, 0, 0.5f));
        }

        // Update is called once per frame
        void Update()
        {
            ImstkUnity.SimulationManager.sceneManager.getActiveScene().setEnableTaskTiming(true);
            _stats = new Imstk.UnorderedMapStringDouble(ImstkUnity.SimulationManager.sceneManager.getActiveScene().getTaskComputeTimes());
        }

        private void OnGUI()
        {
            var rect = new Rect(20, 0, 800, 200);
            GUILayout.BeginArea(rect, _greyBackground);
            GUILayout.BeginVertical();
            foreach (var task in _stats)
            {
                if (task.Value > 0.01)
                {
                    GUILayout.Label(String.Format("{0} {1:F2}ms", task.Key, task.Value));
                    GUILayout.Space(-5);
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndArea();

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



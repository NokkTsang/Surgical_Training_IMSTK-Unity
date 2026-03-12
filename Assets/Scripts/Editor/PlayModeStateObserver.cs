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
using UnityEditor;

namespace ImstkEditor
{
    /// <summary>
    /// This class observes the play mode state to properly pause and step
    /// Imstk in the Unity editor
    /// </summary>
    [InitializeOnLoad]
    class PlayModeStateObserver
    {
        static PlayModeStateObserver()
        {
#if UNITY_EDITOR
            // Perform imstk install if developer mode is on
            ImstkSettings settings = ImstkSettings.Instance();
            if (settings.useDeveloperMode &&
                EditorUtility.DisplayDialog("Do you want to install imstk",
                "To install imstk select yes and provide your imstk build/install directory. If you would not " +
                "like to be prompted again, please turn off developer mode in imstk settings. Note: " +
                "This prompt will be displayed upon installation again, select no second time.", "Yes", "No"))
            {
                settings.installSourcePath =
                    EditorUtility.OpenFolderPanel("iMSTK Install Directory (Development Use)", settings.installSourcePath, "");
                EditorUtils.InstallImstk(settings.installSourcePath);
                EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssets();
            }
#endif

            //EditorApplication.playModeStateChanged += playModeStateChanged;
            EditorApplication.pauseStateChanged += pauseStateChanged;
        }

        private static void pauseStateChanged(PauseState state)
        {
            if (SimulationManager.sceneManager != null)
            {
                bool paused = SimulationManager.sceneManager.getPaused();
                if (paused)
                {
                    SimulationManager.sceneManager.setPaused(true);
                }
                else
                {
                    SimulationManager.sceneManager.setPaused(false);
                }
            }
        }

        //private static void playModeStateChanged(PlayModeStateChange state) { }
    }
}

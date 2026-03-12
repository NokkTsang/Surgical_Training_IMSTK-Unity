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
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ImstkEditor
{
    /// <summary>
    /// Defines the gui to fill ImstkSettings
    /// </summary>
    class ImstkSettingsProvider : SettingsProvider
    {
        private ImstkSettings settings;

        public ImstkSettingsProvider(string path, SettingsScope scope = SettingsScope.User) : base(path, scope) { }

        public static bool IsSettingsAvailable() { return File.Exists(ImstkSettings.settingsPath); }

        // Read existing settings or create settings if they don't exist when user clicks them
        public override void OnActivate(string searchContext, VisualElement rootElement) { settings = ImstkSettings.Instance(); }

        public override void OnGUI(string searchContext)
        {
            EditorGUI.BeginChangeCheck();
            bool useOptimalNumberOfThreads = EditorGUILayout.Toggle("Use Optimal # Threads", settings.useOptimalNumberOfThreads);
            //int numThreads = settings.numThreads;
           
            //if (!useOptimalNumberOfThreads)
            //    numThreads = EditorGUILayout.IntField("# Threads", numThreads);

            EditorGUILayout.HelpBox(
              "When developer mode is on you will be prompted to install imstk " +
              " this can only be done by restarting unity as the editor will may load dll's preventing you from installing imstk" +
              " while the editor is running.", MessageType.Warning);
            bool useDevMode = EditorGUILayout.Toggle("Use Developer Mode", settings.useDeveloperMode);

            string installPath = settings.installSourcePath;
            if (useDevMode)
            {
                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.BeginHorizontal();
                GUILayout.Label("iMSTK Install Directory");
                installPath = EditorGUILayout.TextField(installPath);

                if (GUILayout.Button("Open Directory"))
                {
                    installPath = EditorUtility.OpenFolderPanel("iMSTK Install Directory (Development Use)", settings.installSourcePath, "");
                }
                GUILayout.EndHorizontal();

                EditorGUILayout.HelpBox("Force install will attempt to perform an install now. This may not work if " +
                    "any imstk libraries are already loaded.",
                    MessageType.Warning);

                if (GUILayout.Button("Force Install"))
                {
                    EditorUtils.InstallImstk(installPath);
                }
                GUILayout.EndVertical();
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RegisterCompleteObjectUndo(settings, "Change of Settings");
                settings.useDeveloperMode = useDevMode;
                settings.useOptimalNumberOfThreads = useOptimalNumberOfThreads;
                //settings.numThreads = numThreads;
                settings.installSourcePath = installPath;

                if (!System.IO.Directory.Exists(ImstkSettings.settingsDirectory))
                {
                    System.IO.Directory.CreateDirectory(ImstkSettings.settingsDirectory);
                }

                EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssets();
            }
        }

        /// <summary>
        /// Register the SettingsProvider so it can be found in Editor->Project Settings
        /// </summary>
        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            var provider = new ImstkSettingsProvider("Project/iMSTK Settings", SettingsScope.Project);
            provider.keywords = new HashSet<string>(new string[] { "Debug", "Logger", "Threads", "iMSTK" });
            return provider;
        }
    }
}

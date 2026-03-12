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

using UnityEditor;
using UnityEngine;

namespace ImstkUnity
{
    /// <summary>
    /// Deprecated needs to be removed
    /// </summary>
    [System.Serializable]
    public class ImstkSettings : ScriptableObject
    {
        // This is absolute path, does not support moving the directory
        // Expects Imstk to be under "Imstk"
        public const string settingsDirectory = "Assets/Imstk/Resources/";
        public const string settingsPath = settingsDirectory+"ImstkSettings.asset";

        [SerializeField]
        public bool useDeveloperMode = false;

        [SerializeField]
        public string installSourcePath = "";

        [SerializeField]
        public bool useOptimalNumberOfThreads = true;

        public static ImstkSettings Instance()
        {
        //
#if UNITY_EDITOR
            var settings = AssetDatabase.LoadAssetAtPath<ImstkSettings>(settingsPath);
            // If it doesn't exist, create default
            if (settings == null)
            {
                if (!System.IO.Directory.Exists(settingsDirectory))
                {
                    System.IO.Directory.CreateDirectory(settingsDirectory);
                }
                settings = CreateInstance<ImstkSettings>();
                AssetDatabase.CreateAsset(settings, settingsPath);
            }

            //string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
            //settings.useDebug = defines.Contains("IMSTK_DEBUG");

            AssetDatabase.SaveAssets();
#else
            var settings = CreateInstance<ImstkSettings>();
#endif
            // Temporary
            return settings;
        }
    }
}
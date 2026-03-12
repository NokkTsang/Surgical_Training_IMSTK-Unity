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

using System.IO;
using UnityEditor;
using UnityEngine;
using ImstkUnity;

namespace ImstkEditor
{
    public static class EditorUtils
    {
        public static GeometryFilter GeomFilterField(string fieldName, GeometryFilter filter)
        {
            return EditorGUILayout.ObjectField(fieldName, filter, typeof(GeometryFilter), true) as GeometryFilter;
        }

        /// <summary>
        /// Clears all plugins and iMSTKSharp directory ".cs" files. Reinstalls
        /// from installSourcePath
        /// </summary>
        /// <param name="installSourcePath"></param>
        public static void InstallImstk(string installSourcePath)
        {
            // Get the install directory
            if (installSourcePath.Length != 0)
            {
                // First check the directory exists
                if (!Directory.Exists(installSourcePath))
                {
                    Debug.LogError("Failed to install imstk, source location does not exist: " + installSourcePath);
                }
                // Next check that a file exists to lightly verify we have the right directory
                if (!Directory.Exists(installSourcePath + "/lib/cmake/iMSTK-5.0") ||
                    !File.Exists(installSourcePath + "/lib/cmake/iMSTK-5.0/iMSTKConfig.cmake"))
                {
                    Debug.LogError("Could not find relevant files, check the imstk install directory is specified properly: " + installSourcePath);
                }

                // Determine the directory what this script resides in and therefore the location of 
                // the Plugins directory

                string[] res = System.IO.Directory.GetFiles(Application.dataPath, "EditorUtils.cs", SearchOption.AllDirectories);
                if (res.Length == 0)
                {
                    Debug.LogError("Could not find target directory");
                    return;
                }
                string dataPath = res[0].Replace("EditorUtils.cs", "").Replace("\\", "/") + "../../";

                // Clear plugins directory and copy all files from bin to plugins
                ClearFiles(dataPath + "/Plugins/");
                CopyFiles(installSourcePath + "/bin/", dataPath + "/Plugins/", new string[] { ".dll", ".so" });

                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// Clear all files at path
        /// </summary>
        public static void ClearFiles(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            else
            {
                string[] files = Directory.GetFiles(path);

                for (int i = 0; i < files.Length; i++)
                {
                    File.Delete(files[i]);
                }

                Directory.Delete(path);
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// Copy all files from srcPath to destPath directory
        /// </summary>
        public static void CopyFiles(string srcPath, string destPath, string[] extFilter)
        {
            if (!Directory.Exists(srcPath))
            {
                return;
            }
            if (!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
            }

            string[] hi = Directory.GetFiles(srcPath);
            for (int i = 0; i < hi.Length; i++)
            {
                string ext = Path.GetExtension(hi[i]);
                bool validExt = false;
                for (int j = 0; j < extFilter.Length; j++)
                {
                    if (ext == extFilter[j])
                    {
                        validExt = true;
                    }
                }
                if (validExt)
                {
                    File.Copy(hi[i], destPath + Path.GetFileName(hi[i]));
                }
            }
        }
    }
}

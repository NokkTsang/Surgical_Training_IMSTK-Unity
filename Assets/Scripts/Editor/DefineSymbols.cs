using System.Collections.Generic;
using System.Linq;
using UnityEditor;


namespace ImstkEditor
{

    /// <summary>
    /// Adds the given define symbols to PlayerSettings define symbols.
    /// Just add your own define symbols to the Symbols property at the below.
    /// see https://forum.unity.com/threads/scripting-define-symbols-access-in-code.174390/
    /// </summary>
    [InitializeOnLoad]
    public class DefineSymbols : Editor
     {

        /// <summary>
        /// Symbols that will be added to the editor
        /// </summary>
        public static readonly string[] StaticSymbols = new string[] { };
        public static bool runonce = false;

        /// <summary>
        /// Add define symbols as soon as Unity gets done compiling.
        /// </summary>
        static DefineSymbols()
        {
            string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            List<string> allDefines = definesString.Split(';').ToList();
            List<string> originalDefines = new List<string>(allDefines);
            allDefines.AddRange(StaticSymbols.Except(allDefines));
            allDefines.AddRange(GetDeviceSymbols().Except(allDefines));
            if (Enumerable.SequenceEqual(allDefines, originalDefines)) return;
            PlayerSettings.SetScriptingDefineSymbolsForGroup(
                EditorUserBuildSettings.selectedBuildTargetGroup,
                string.Join(";", allDefines.ToArray()));
        }

        /// <summary>
        /// Check through the known device names and see if it exists in the factory, 
        /// if yes, set the appropriate symbol so that the device class will be compiled
        /// </summary>
        private static List<string> GetDeviceSymbols()
        {
            var factory = new Imstk.DeviceManagerFactory();
            var names = new string[] { "OpenHapticDeviceManager", "IMSTK_USE_OPENHAPTICS",
                                    "HaplyDeviceManager", "IMSTK_USE_HAPLY",
                                    "VRPNDeviceManager", "IMSTK_USE_VRPN" };
            var symbols = new List<string>();
            for (int i = 0; i < names.Length; i += 2)
            {
                if (Imstk.DeviceManagerFactory.contains(names[i]))
                {
                    symbols.Add(names[i + 1]);
                }
            }
            return symbols;
        }
    }
}

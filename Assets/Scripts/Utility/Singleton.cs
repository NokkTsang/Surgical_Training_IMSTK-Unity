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

using UnityEngine;

namespace ImstkUnity
{
    // Simple singleton class as per https://blog.yarsalabs.com/using-singletons-in-unity/
    // More powerful version is https://gist.github.com/rickyah/271e3aa31ff8079365bc
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        public static T Instance()
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<T>(true);
                
                if (_instance == null)
                {
                    _instance = new GameObject(typeof(T).Name).AddComponent<T>();
                }
                if (! _instance.enabled)
                {
                    Debug.LogWarning(System.String.Format("Singleton {} available but inactive", typeof(T).Name));
                }
            }
            else 
            {
                var instances = FindObjectsOfType<T>();
                foreach (var instance in instances)
                {
                    if (instance != _instance)
                    {
                        Destroy(instance);
                    }
                }
            }
            // Need to check if this is necessary
            // DontDestroyOnLoad(_instance.gameObject);
            return _instance;
        }
    }
}
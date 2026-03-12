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
    /// <summary>
    /// This is the base class of Imstk scripts. It exists to provide
    /// different init functions for Imstk classes. This is so that we
    /// can control initialization order in the SimulationManager
    /// </summary>
    public abstract class ImstkBehaviour : MonoBehaviour
    {
        public string GetFullName()
        {
            var stack = new System.Collections.Generic.Stack<string>();
            stack.Push(this.GetType().Name);
            stack.Push(gameObject.name);
            Transform parent = gameObject.transform.parent;
            while (parent != null)
            {
                stack.Push(parent.gameObject.name);
                parent = parent.transform.parent;
            }
            var newName = stack.Pop();
            while (stack.Count != 0)
            {
                newName = newName + "/" + stack.Pop();
            }
            return newName;
        }
        public void OnEnable()
        {

        }
        public void ImstkDestroy()
        {
            OnImstkDestroy();
        }

        /// <summary>
        /// Initialize the Imstk object, this may be called multiple times as 
        /// the ordering of components is not guaranteed, so while this is triggered
        /// by the simulation manager, other components may call it as well. 
        /// You should only ever need to implement ImstkInit or ImstkComponentInit
        /// </summary>
        public void ImstkInit() { OnImstkInit(); }

        /// <summary>
        /// Initialize an Imstk Component, ImstkInit will be called first
        /// You should only ever need to implement ImstkInit or ImstkComponentInit
        /// </summary>
        public void ImstkComponentInit() { OnImstkComponentInit(); }

        public void ImstkStart() { OnImstkStart(); }

        /// <summary>
        /// Called before initializing the scene
        /// </summary>
        protected virtual void OnImstkInit() { }

        /// <summary>
        /// Called before initializing the scene but after ImstkInit
        /// This is a workaround for the fact that we are using scene objects as entities
        /// and a allows a component to be added to a imstk SceneObject before
        /// </summary>
        protected virtual void OnImstkComponentInit() { }

        /// <summary>
        /// Called after scene has been initialized
        /// </summary>
        protected virtual void OnImstkStart() { }


        /// <summary>
        /// Called when done
        /// </summary>
        protected virtual void OnImstkDestroy() { }


    }
}

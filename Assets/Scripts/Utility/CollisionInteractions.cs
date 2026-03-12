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
using System.Collections.Generic;
using UnityEngine;

namespace ImstkUnity
{
    /// <summary>
    /// Class to handle a series of collision pairs and their data
    /// </summary>
    /// This is used in the SimulationManager to hold all collisionpairs as set through
    /// the editor
    [Serializable]
    public class CollisionInteractions
    {
        [Serializable]
        public class CollisionInteractionData
        {
            public DynamicalModel model1;
            public DynamicalModel model2;

            public double friction = 0.0;
            public double restitution = 0.0;

            public double deformableStiffness1 = 0.2;
            public double deformableStiffness2 = 0.2;

            public double rigidBodyCompliance = 0.0025;

            public string collisionTypeName = "Auto";
        }

        [SerializeField]
        public List<CollisionInteractionData> _collisions = new List<CollisionInteractionData>();

        public CollisionInteractions() {}
        public CollisionInteractions(CollisionInteractions other)
        {
            _collisions = new List<CollisionInteractionData>(other._collisions);
        }

        public CollisionInteractionData GetData(DynamicalModel a, DynamicalModel b)
        {
            var index = Index(a, b);
            if (index >= 0) return _collisions[index];
            else
            {
                throw new System.Exception("Could not find collision pair");
            }
        }

        public CollisionInteractionData Add(DynamicalModel a, DynamicalModel b)
        {
            var index = Index(a, b);
            if (index < 0)
            {

                var data = new CollisionInteractionData();
                data.model1 = a;
                data.model2 = b;
                _collisions.Add(data);
                return data;
            }
            else
            {
                throw new System.Exception("Add Called with pair that already exists");
            }
        }

        public void SetData(CollisionInteractionData d)
        {
            var index = Index(d.model1, d.model2);
            if (index >= 0)
            {
                _collisions[index] = d;
            }
            else
            {
                _collisions.Add(d);
            }
        }

        public void Remove(CollisionInteractionData d)
        {
            var index = Index(d.model1, d.model2);
            if (index > 0)
            {
                _collisions.RemoveAt(index);
            }
        }

        public void Remove(DynamicalModel a, DynamicalModel b)
        {
            var index = Index(a, b);
            if (index >= 0)
            {
                _collisions.RemoveAt(index);
            }
        }

        public void Remove(DynamicalModel a)
        {
            _collisions.RemoveAll(x => x.model1 == a || x.model2 == a);
        }

        public void RemoveAllNull()
        {
            _collisions.RemoveAll(x => x.model1 == null || x.model2 == null || 
            x.model1.GetCollidingGeometry() == null || x.model2.GetCollidingGeometry() == null);
        }

        public bool IsEnabled(DynamicalModel a, DynamicalModel b)
        {
            return Index(a, b) != -1;
        }


        private int Index(DynamicalModel a, DynamicalModel b)
        {
            return _collisions.FindIndex(x =>
            {
                return (a == x.model1 && b == x.model2) ||
                       (a == x.model2 && b == x.model1);
            });
        }

        public void UpdateFrom(CollisionInteractions other)
        {
            _collisions.Clear();
            _collisions.AddRange(other._collisions);
        }
    }
}
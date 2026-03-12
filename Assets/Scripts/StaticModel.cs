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
    /// Use this to represent unmovable rigid objects like the ground plane or other obstacles.
    /// </summary>
    [AddComponentMenu("iMSTK/StaticModel")]
    public class StaticModel : DynamicalModel
    {
        // These filters can accept either imstk or unity geometry input
        // and output imstk geometry
        public GeometryFilter visualGeomFilter = null;
        public GeometryFilter physicsGeomFilter = null;
        public GeometryFilter collisionGeomFilter = null;


        protected override void OnImstkInit()
        {
            imstkObject = InitObject();
            SimulationManager.sceneManager.getActiveScene().addSceneObject(imstkObject);
            InitGeometry();
            Configure();
        }

        protected override void Configure()
        {
            //throw new NotImplementedException();
        }

        protected override void InitGeometry()
        {
            // Setup the collision geometry in its already post transform configuration
            // as it is a static geometry and will not be transformed
            // CAVEAT DON'T "REUSE" GeometryFilters in two separate object
            if (collisionGeomFilter != null)
            {
                Imstk.Geometry colGeom = GetCollidingGeometry();
                if (colGeom != null)
                {
                    Imstk.Mat4d m = transform.localToWorldMatrix.ToMat4d();
                    colGeom.transform(m, Imstk.Geometry.TransformType.ApplyToData);
                    imstkObject.setCollidingGeometry(colGeom);
                }
                else
                {
                    Debug.LogError($"Failed to get collision geometry from GeometryFilter on object {gameObject.name}. Check that the GeometryFilter has valid input geometry assigned.");
                }
            }
            else
            {
                Debug.LogError("No collision geometry provided to DynamicalModel on object " + gameObject.name);
            }
        }

        protected override Imstk.CollidingObject InitObject()
        {
            return new Imstk.CollidingObject(GetFullName());
        }

        public override Imstk.Geometry GetVisualGeometry()
        {
            return visualGeomFilter.GetOutputGeometry();
        }
        public override Imstk.Geometry GetPhysicsGeometry()
        {
            return physicsGeomFilter.GetOutputGeometry();
        }
        public override Imstk.Geometry GetCollidingGeometry()
        {
            return collisionGeomFilter.GetOutputGeometry();
        }

        public override ImstkUnity.Geometry GetUnityColisionGeometry()
        {
            return collisionGeomFilter != null ? collisionGeomFilter.inputImstkGeom : null;
        }
    }
}

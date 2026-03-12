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

namespace ImstkUnity
{
    /// <summary>
    /// Allows the use of separate meshes for the deformable, visual and collision representation.
    /// </summary>
    /// Will move the vertices of the target mesh according to matching points on the source mesh.
    /// The points do not have to completely coincide. In almost all cases you will need to map
    /// FROM the physics mesh TO the visual mesh, and FROM the physics mesh TO the collision mesh.
    public abstract class GeometryMap : ImstkBehaviour
    {
        private Imstk.GeometryMap map = null;
        public GeometryFilter parentGeom = null;
        public GeometryFilter childGeom = null;
        public Imstk.GeometryMap GetMap()
        {
            if (map == null) map = MakeMap();
            return map;
        }

        protected abstract Imstk.GeometryMap MakeMap();
    }
}
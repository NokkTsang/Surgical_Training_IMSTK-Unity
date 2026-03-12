using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ImstkUnity
{
    public class CompoundGeometry : Geometry
    {
        public List<GeometryFilter> geometryFilters = new List<GeometryFilter>();
        public GameObject parent; // The GO this Geometry is on

        public CompoundGeometry() {
            geomType = GeometryType.CompoundGeometry;
        }

        void Add(GeometryFilter filter)
        {
            geometryFilters.Add(filter);
        }

        public void Remove(GeometryFilter filter)
        {
            geometryFilters.Remove(filter);
        }

        public void Clear()
        {
            geometryFilters.Clear();
        }

    }
}

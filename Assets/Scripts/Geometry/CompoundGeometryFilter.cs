using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace ImstkUnity
{
    public class CompoundGeometryFilter : GeometryFilter
    {

        public List<GeometryFilter> filters = new List<GeometryFilter>();
        public List<bool> oldDrawWidget = new List<bool>();

        CompoundGeometryFilter() {
            type = GeometryType.CompoundGeometry;
        }

        public void RefreshGeometry()
        {
            var geometry  = ScriptableObject.CreateInstance<CompoundGeometry>();
            geometry.geometryFilters = filters;
            geometry.parent = gameObject;
            inputImstkGeom = geometry;
        }

        override public Imstk.Geometry GetOutputGeometry()
        {
            type = GeometryType.CompoundGeometry;
            RefreshGeometry();
            return base.GetOutputGeometry();
        }

        public void SetSubShowHandles(bool showHandles)
        {
            foreach (var filter in filters)
            {
                filter.showHandles = showHandles;
            }
        }
    }
}

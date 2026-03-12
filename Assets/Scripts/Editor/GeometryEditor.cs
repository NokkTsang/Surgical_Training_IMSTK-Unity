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
using UnityEditor;

namespace ImstkUnity
{
    /// <summary>
    /// Editor for the asset, most commonly shown when selecting the asset in
    /// the project explorer
    /// </summary>
    [CustomEditor(typeof(Geometry), true)]
    class GeometryEditor : Editor
    {
        private PreviewRenderUtility m_PreviewUtility = null;
        private Material previewMaterial = null;
        private Mesh previewMesh = null;

        public override void OnInspectorGUI()
        {
            // Check the config file toggle for change
            Geometry asset = target as Geometry;
            if (asset.IsMesh)
            {
                ImstkMesh mesh = asset as ImstkMesh;
                GUILayout.Label("Vertex Count: " + mesh.vertices.Length);
                GUILayout.Label("Index Count: " + mesh.indices.Length);
                GUILayout.Label("Cell Count: " + mesh.indices.Length / ImstkMesh.typeToNumPts[mesh.geomType]);
            }
            GUILayout.Label("Geom Type: " + asset.geomType.ToString());
        }

        public override bool HasPreviewGUI() { return true; }

        public override GUIContent GetPreviewTitle() { return new GUIContent(target.name); }

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            Geometry asset = target as Geometry;
            // Preview only works with meshes
            if (!asset.IsMesh)
                return;
            ImstkMesh imstkMesh = asset as ImstkMesh;

            if (m_PreviewUtility == null)
            {
                m_PreviewUtility = new PreviewRenderUtility();
                previewMaterial = new Material(Shader.Find("Diffuse"));

                // If a tet or hex mesh extract surface for display (we could use some other method to better indicate
                // that this is a tet mesh)
                if (asset.IsVolume)
                {
                    // Extract the surface mesh for display
                    Imstk.TetrahedralMesh tetMesh = Imstk.Utils.CastTo<Imstk.TetrahedralMesh>(imstkMesh.ToPointSet());
                    Imstk.SurfaceMesh surfMesh = tetMesh.extractSurfaceMesh();
                    previewMesh = surfMesh.ToMesh();
                }
            }

            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            Camera camera = m_PreviewUtility.camera;
            float size = previewMesh.bounds.size.magnitude;
            Vector3 center = previewMesh.bounds.center;
            camera.transform.position = center + new Vector3(0.0f, size * 2.0f, -size * 2.0f);
            camera.transform.LookAt(center);
            camera.farClipPlane = 300.0f;

            m_PreviewUtility.BeginPreview(r, background);
            m_PreviewUtility.DrawMesh(previewMesh, Matrix4x4.identity, previewMaterial, 0);

            bool fog = RenderSettings.fog;
            Unsupported.SetRenderSettingsUseFogNoDirty(false);
            m_PreviewUtility.camera.Render();
            Unsupported.SetRenderSettingsUseFogNoDirty(fog);

            Texture texture = m_PreviewUtility.EndPreview();
            GUI.DrawTexture(r, texture, ScaleMode.StretchToFill, false);
        }

        void OnDisable()
        {
            previewMaterial = null;
            previewMesh = null;

            if (m_PreviewUtility != null)
            {
                m_PreviewUtility.Cleanup();
                m_PreviewUtility = null;
            }
        }
    }
}
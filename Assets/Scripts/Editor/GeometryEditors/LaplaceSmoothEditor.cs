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

using ImstkUnity;
using UnityEngine;
using UnityEditor;

namespace ImstkEditor
{
    /// <summary>
    /// Laplacian smoothens an input unity Mesh providing
    /// an output one
    /// </summary>
    public class LaplaceSmoothEditor : EditorWindow
    {
        public int numIterations = 20;
        public double relaxationFactor = 0.01;
        public double convergence = 0.0;
        public double featureAngle = 45.0;
        public double edgeAngle = 15.0;
        public bool featureEdgeSmoothing = false;
        public bool boundarySmoothing = true;
        public Mesh inputMesh = null;
        public Mesh outputMesh = null;

        public static void Init(Mesh inputMesh, Mesh outputMesh)
        {
            LaplaceSmoothEditor window = GetWindow(typeof(LaplaceSmoothEditor)) as LaplaceSmoothEditor;
            window.inputMesh = inputMesh;
            window.outputMesh = outputMesh;
            window.UpdateEditorResults();
            window.Show();
        }

        void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            inputMesh = EditorGUILayout.ObjectField("Input Mesh: ", inputMesh, typeof(Mesh), true) as Mesh;
            int tNumIterations = EditorGUILayout.IntField("Number Of Smoothing Iterations: ", numIterations);
            double tRelaxationFactor = EditorGUILayout.DoubleField("Relaxation Factor: ", relaxationFactor);
            double tConvergence = EditorGUILayout.DoubleField("Convergence: ", convergence);
            double tFeatureAngle = EditorGUILayout.DoubleField("Feature Angle: ", featureAngle);
            double tEdgeAngle = EditorGUILayout.DoubleField("Edge Angle: ", edgeAngle);
            bool tFeatureEdgeSmoothing = EditorGUILayout.Toggle("Use Feature Edge Smoothing: ", featureEdgeSmoothing);
            bool tBoundarySmoothing = EditorGUILayout.Toggle("Use Boundary Smoothing: ", boundarySmoothing);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RegisterCompleteObjectUndo(this, "Change of Parameters");
                numIterations = MathUtil.Max(tNumIterations, 1);
                relaxationFactor = MathUtil.Max(tRelaxationFactor, 0.0);
                convergence = MathUtil.Max(tConvergence, 0.0);
                featureAngle = MathUtil.Max(tFeatureAngle, 0.0);
                edgeAngle = MathUtil.Max(tEdgeAngle, 0.0);
                featureEdgeSmoothing = tFeatureEdgeSmoothing;
                boundarySmoothing = tBoundarySmoothing;

                UpdateEditorResults();
            }
        }

        private void UpdateEditorResults()
        {
            Imstk.SurfaceMesh surfMesh = inputMesh.ToImstkGeometry() as Imstk.SurfaceMesh;

            Imstk.SurfaceMeshSmoothen smoothen = new Imstk.SurfaceMeshSmoothen();
            smoothen.setInputMesh(surfMesh);
            smoothen.setNumberOfIterations(numIterations);
            smoothen.setRelaxationFactor(relaxationFactor);
            smoothen.setConvergence(convergence);
            smoothen.setFeatureAngle(featureAngle);
            smoothen.setEdgeAngle(edgeAngle);
            smoothen.setFeatureEdgeSmoothing(featureEdgeSmoothing);
            smoothen.setBoundarySmoothing(boundarySmoothing);
            smoothen.update();

            Imstk.SurfaceMesh outputSurfMesh = Imstk.Utils.CastTo<Imstk.SurfaceMesh>(smoothen.getOutput());
            GeomUtil.CopyMesh(outputSurfMesh.ToMesh(), outputMesh);
        }
    }
}
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
    /// High-level suturing interaction: needle + thread + tissue.
    ///
    /// Behaviour summary (all driven by iMSTK C++ NeedlePbdCH):
    ///   - Needle body touching tissue → PBD collision → tissue deforms elastically.
    ///   - Only the needle TIP (last vertex = endIndex) can trigger a puncture,
    ///     AND only when the dot product of needle direction · surface normal
    ///     exceeds punctureDotThreshold (iMSTK built-in).
    ///   - After puncture the thread is pulled through tissue via insertion constraints.
    ///   - Trigger / key press → stitch() contracts 4+ puncture points (wound closure).
    ///
    /// IMPORTANT — In SimulationManager "Colliding Objects":
    ///   UNTICK Needle↔Tissue — NeedleInteraction already creates this collision pair
    ///   with NeedlePbdCH. A duplicate standard PbdObjectCollision pushes the needle
    ///   back OUT of tissue after puncture, preventing insertion/thread constraints.
    ///   UNTICK Thread↔Needle — duplicate pair causes "Body vs Edge CCD:1" errors.
    ///   Tissue↔Thread is optional (prevents thread penetrating tissue at non-puncture spots).
    ///
    /// Debug logging: set enableDebugLog = true to see init info, puncture events,
    /// stitch attempts, collision contact counts, and per-frame state in the Console.
    /// </summary>
    public class Suturing : ImstkBehaviour
    {
        [Header("References")]
        public Rigid needle;
        public Deformable thread;
        public Deformable tissue;
        public Quest3InputHandler inputHandler;

        [Header("Stiffness / Threshold")]
        [Tooltip("Tissue-follows-needle strength during puncture insertion (iMSTK default: 0.2)")]
        public float needleSurfaceStiffness = 0.2f;
        [Tooltip("How much tissue resists needle (resistance to insertion)")]
        public float surfaceToNeedleStiffness = 0.2f;
        [Tooltip("Tissue-follows-thread strength after thread passes through")]
        public float threadSurfaceStiffness = 0.2f;
        [Tooltip("How much thread follows tissue (thread flexibility at puncture)")]
        public float threadToSurfaceStiffness = 0.2f;
        [Tooltip("Dot(needleDir, surfNormal) threshold for puncture (lower = easier pierce)")]
        public float punctureDotThreshold = 0.5f;

        [Header("Thread-Needle Attachment")]
        // Local-space offset from needle.transform origin to the tail (eye/thread hole).
        // Adjust in the Inspector so the thread attaches at the correct end of the needle.
        public Vector3 needleTailLocalOffset = new Vector3(0f, 0f, 0.0177f);

        [Header("Collision Detection Override")]
        [Tooltip("Leave empty to use auto-detected CD (ClosedSurfaceMeshToMeshCD). " +
                 "Only override if you know the exact CD class name in iMSTK.")]
        public string overrideCDType = "";

        [Header("Input")]
        public string activationKey = "s";

        [Header("Debug")]
        public bool enableDebugLog = true;

        // ── Internal state ───────────────────────────────────────────
        Imstk.NeedleInteraction _needleInteraction;
        private Imstk.PbdObject _threadPbd;
        private Imstk.PbdBody _needleBody;
        private bool _pbdThreadReady = false;
        private bool _triggerWasPressed = false;

        // Puncture-event tracking (for debug logging)
        private int _prevNeedlePunctureCount = 0;
        private int _prevThreadPunctureCount = 0;
        private int _prevStitchCount = 0;
        private bool _wasNeedlePunctured = false;
        private bool _wasThreadPunctured = false;

        // Contact monitoring (throttled logging)
        private float _lastContactLogTime = 0f;
        private int _prevContactCountA = -1;

        // ── Initialization ───────────────────────────────────────────
        protected override void OnImstkInit()
        {
            // --- Validate references ---
            if (needle == null) { Debug.LogError("[Suturing] Needle reference is null"); return; }
            if (thread == null) { Debug.LogError("[Suturing] Thread reference is null"); return; }
            if (tissue == null) { Debug.LogError("[Suturing] Tissue reference is null"); return; }

            if (!needle.isActiveAndEnabled || !thread.isActiveAndEnabled || !tissue.isActiveAndEnabled)
            {
                Debug.LogWarning("[Suturing] One or more objects inactive — disabling Suturing.");
                enabled = false;
                return;
            }

            // Mark thread vertex 0 as kinematic BEFORE ImstkInit so that
            // Deformable.Configure() includes it in fixedNodeIds → invMasses[0] = 0.
            thread.fixedIndices.Add(0);

            // Ensure all three PBD objects are initialized
            needle.ImstkInit();
            thread.ImstkInit();
            tissue.ImstkInit();

            var needlePbd = Imstk.Utils.CastTo<Imstk.PbdObject>(needle.GetDynamicObject());
            var threadPbd = Imstk.Utils.CastTo<Imstk.PbdObject>(thread.GetDynamicObject());
            var tissuePbd = Imstk.Utils.CastTo<Imstk.PbdObject>(tissue.GetDynamicObject());

            if (needlePbd == null || threadPbd == null || tissuePbd == null)
            {
                Debug.LogError("[Suturing] Failed to obtain PbdObject for needle/thread/tissue.");
                enabled = false;
                return;
            }

            // --- CRITICAL: Fix winding order for correct collision response ---
            // Unity = left-handed (clockwise winding = front face).
            // iMSTK = right-handed (counter-clockwise winding = front face).
            // The Unity→iMSTK mesh conversion passes triangle indices verbatim,
            // so tissue normals point INWARD in iMSTK. This causes:
            //   1) ClosedSurfaceMeshToMeshCD detects false "inside" for vertices OUTSIDE
            //   2) PbdPointTriangleConstraint pushes tissue AWAY from the needle
            // Fix: flip all triangle winding on the collision SurfaceMesh.
            var tissueColGeom = tissuePbd.getCollidingGeometry();
            var tissueColSurf = Imstk.Utils.CastTo<Imstk.SurfaceMesh>(tissueColGeom);
            if (tissueColSurf != null)
            {
                tissueColSurf.flipNormals();
                if (enableDebugLog)
                    Debug.Log("[Suturing] Flipped tissue collision mesh normals (Unity LH → iMSTK RH)");
            }
            else
            {
                Debug.LogWarning("[Suturing] Tissue collision geometry is not SurfaceMesh — cannot flip normals. " +
                                 "Collision response will be inverted (tissue will fly).");
            }

            // Also flip visual geometry if it's a separate SurfaceMesh
            var tissueVisGeom = tissuePbd.getVisualGeometry();
            var tissueVisSurf = Imstk.Utils.CastTo<Imstk.SurfaceMesh>(tissueVisGeom);
            if (tissueVisSurf != null && tissueVisGeom != tissueColGeom)
            {
                tissueVisSurf.flipNormals();
                if (enableDebugLog)
                    Debug.Log("[Suturing] Flipped tissue visual mesh normals too");
            }

            // --- Create NeedleInteraction (tissue ↔ needle collision + NeedlePbdCH) ---
            _needleInteraction = new Imstk.NeedleInteraction(tissuePbd, needlePbd, threadPbd);

            // WARNING: Do NOT call setDeformableStiffnessA/B or setRigidBodyCompliance here.
            // These reliably cause tissue/thread collapse to (0,0,0) on contact.
            // Pre-puncture collision uses PbdCollisionHandling defaults (stiffness=0.3, compliance=0.000001).

            // Log the auto-detected collision detection type
            var cd = _needleInteraction.getCollisionDetection();
            string cdTypeName = cd != null ? cd.getTypeName() : "NULL";
            if (enableDebugLog)
                Debug.Log($"[Suturing] Auto-detected CD type: {cdTypeName}");

            // --- Override CD algorithm if specified ---
            // The auto type for LineMesh vs SurfaceMesh is ClosedSurfaceMeshToMeshCD,
            // which requires a closed/watertight tissue surface. If your tissue mesh
            // is open (flat patch, wound, etc.), override to PointSetToSurfaceMeshCD.
            if (!string.IsNullOrEmpty(overrideCDType))
            {
                try
                {
                    var newCD = Imstk.CDObjectFactory.makeCollisionDetection(overrideCDType);
                    if (newCD != null)
                    {
                        // After CD order fix: A = needle (PointSet), B = tissue (SurfaceMesh)
                        newCD.setInputGeometryA(needlePbd.getCollidingGeometry());
                        newCD.setInputGeometryB(tissuePbd.getCollidingGeometry());

                        _needleInteraction.setCollisionDetection(newCD);

                        // Update collision handler to use the new CD's collision data
                        var chAB = _needleInteraction.getCollisionHandlingAB();
                        if (chAB != null)
                            chAB.setInputCollisionData(newCD.getCollisionData());

                        if (enableDebugLog)
                            Debug.Log($"[Suturing] CD overridden to: {overrideCDType}");
                    }
                    else
                    {
                        Debug.LogError($"[Suturing] Failed to create CD type: {overrideCDType}");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[Suturing] CD override failed: {e.Message}");
                }
            }

            var ch = Imstk.Utils.CastTo<Imstk.NeedlePbdCH>(_needleInteraction.getCollisionHandlingAB());
            if (ch == null)
            {
                Debug.LogError("[Suturing] NeedleInteraction has no NeedlePbdCH — aborting.");
                enabled = false;
                return;
            }

            // Configure collision handler stiffnesses and puncture threshold
            ch.init(threadPbd);

            ch.setNeedleToSurfaceStiffness(needleSurfaceStiffness);
            ch.setSurfaceToNeedleStiffness(surfaceToNeedleStiffness);
            ch.setThreadToSurfaceStiffness(threadSurfaceStiffness);
            ch.setSurfaceToThreadStiffness(threadToSurfaceStiffness);
            ch.setPunctureDotThreshold(punctureDotThreshold);

            // Register interaction with the iMSTK scene
            SimulationManager.sceneManager.getActiveScene().addInteraction(_needleInteraction);

            _threadPbd = threadPbd;
            _needleBody = needlePbd.getPbdBody();
            var threadBody = threadPbd.getPbdBody();
            if (threadBody != null)
            {
                var fixedIds = threadBody.fixedNodeIds;
                if (fixedIds != null && !fixedIds.Contains(0))
                    fixedIds.Add(0);
            }

            _pbdThreadReady = true;

            // --- Debug: log initialization summary ---
            if (enableDebugLog)
            {
                Debug.Log("[Suturing] ===== Initialization Complete =====");
                Debug.Log($"[Suturing]   Needle: {needle.gameObject.name}");
                Debug.Log($"[Suturing]   Thread: {thread.gameObject.name}  (vertex 0 fixed/kinematic)");
                Debug.Log($"[Suturing]   Tissue: {tissue.gameObject.name}");
                Debug.Log($"[Suturing]   needleTailLocalOffset: {needleTailLocalOffset}");
                Debug.Log($"[Suturing]   needleSurfaceStiffness: {needleSurfaceStiffness} (tissue follows needle)");
                Debug.Log($"[Suturing]   surfaceToNeedleStiffness: {surfaceToNeedleStiffness} (needle resists tissue)");
                Debug.Log($"[Suturing]   threadSurfaceStiffness: {threadSurfaceStiffness} (tissue follows thread)");
                Debug.Log($"[Suturing]   threadToSurfaceStiffness: {threadToSurfaceStiffness} (thread follows tissue)");
                Debug.Log($"[Suturing]   punctureDotThreshold: {punctureDotThreshold}");
                Debug.Log($"[Suturing]   CD type: {(string.IsNullOrEmpty(overrideCDType) ? cdTypeName + " (auto)" : overrideCDType + " (override)")}");
                Debug.Log($"[Suturing]   activationKey: '{activationKey}'");
                Debug.Log($"[Suturing]   inputHandler: {(inputHandler != null ? inputHandler.gameObject.name : "NONE")}");
                Debug.Log("[Suturing] NOTE: UNTICK both Needle↔Tissue AND Thread↔Needle in Colliding Objects!");
                Debug.Log("[Suturing]   Needle↔Tissue: NeedleInteraction handles this. Duplicate pair fights insertion.");
                Debug.Log("[Suturing]   Thread↔Needle: Duplicate pair causes 'Body vs Edge CCD:1' errors.");

                // Log collision geometry bounding info to diagnose overlap issues
                LogCollisionGeomBounds("Tissue", tissuePbd);
                LogCollisionGeomBounds("Needle", needlePbd);

                Debug.Log("[Suturing] ====================================");
            }
        }

        private void LogCollisionGeomBounds(string label, Imstk.PbdObject obj)
        {
            try
            {
                var colGeom = obj.getCollidingGeometry();
                if (colGeom == null) { Debug.Log($"[Suturing]   {label} colliding geometry: NULL"); return; }
                var ps = Imstk.Utils.CastTo<Imstk.PointSet>(colGeom);
                if (ps == null) { Debug.Log($"[Suturing]   {label} colliding geometry: {colGeom.getTypeName()} (not PointSet)"); return; }
                int n = (int)ps.getNumVertices();
                if (n == 0) { Debug.Log($"[Suturing]   {label} colliding geometry: 0 vertices!"); return; }
                var verts = ps.getVertexPositions();
                double minX = double.MaxValue, minY = double.MaxValue, minZ = double.MaxValue;
                double maxX = double.MinValue, maxY = double.MinValue, maxZ = double.MinValue;
                for (uint i = 0; i < (uint)n; i++)
                {
                    var v = verts[i];
                    double vx = v[0], vy = v[1], vz = v[2];
                    if (vx < minX) minX = vx; if (vx > maxX) maxX = vx;
                    if (vy < minY) minY = vy; if (vy > maxY) maxY = vy;
                    if (vz < minZ) minZ = vz; if (vz > maxZ) maxZ = vz;
                }
                Debug.Log($"[Suturing]   {label} colGeom: {colGeom.getTypeName()}, {n} verts, " +
                          $"bounds=({minX:F4},{minY:F4},{minZ:F4}) to ({maxX:F4},{maxY:F4},{maxZ:F4})");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[Suturing]   {label} colGeom bounds failed: {e.Message}");
            }
        }

        // ── Stitch / Pull ────────────────────────────────────────────
        public void Pull()
        {
            if (_needleInteraction == null) return;

            if (enableDebugLog)
            {
                var pData = _needleInteraction.getPunctureData();
                int threadPts = (pData != null && pData.thread != null) ? (int)pData.thread.Count : 0;
                int stitchSets = (pData != null && pData.stitch != null) ? (int)pData.stitch.Count : 0;
                Debug.Log($"[Suturing] Pull/Stitch requested — thread puncture points: {threadPts}, existing stitch sets: {stitchSets}");
                if (threadPts < 4)
                    Debug.LogWarning($"[Suturing] stitch() requires >= 4 thread puncture points (currently {threadPts}), will be rejected by iMSTK.");
            }

            _needleInteraction.stitch();

            if (enableDebugLog)
            {
                var pDataAfter = _needleInteraction.getPunctureData();
                int stitchAfter = (pDataAfter != null && pDataAfter.stitch != null) ? (int)pDataAfter.stitch.Count : 0;
                if (stitchAfter > _prevStitchCount)
                {
                    Debug.Log($"[Suturing] >>> STITCH CREATED! Total stitch sets: {stitchAfter} <<<");
                    _prevStitchCount = stitchAfter;
                }
            }
        }

        // ── Thread pinning ───────────────────────────────────────────
        private void PinThreadVertex0ToNeedle()
        {
            if (!_pbdThreadReady || _threadPbd == null || needle == null)
                return;

            var pbdBody = _threadPbd.getPbdBody();
            if (pbdBody == null) return;

            var vertices = pbdBody.vertices;
            if (vertices == null || vertices.size() == 0) return;

            Vector3 tailWorld = needle.transform.TransformPoint(needleTailLocalOffset);
            var pinPos = new Imstk.Vec3d(tailWorld.x, tailWorld.y, tailWorld.z);
            var zero   = new Imstk.Vec3d(0.0, 0.0, 0.0);

            vertices[0] = pinPos;

            var prevVerts = pbdBody.prevVertices;
            if (prevVerts != null && prevVerts.size() > 0)
                prevVerts[0] = pinPos;

            var velocities = pbdBody.velocities;
            if (velocities != null && velocities.size() > 0)
                velocities[0] = zero;
        }

        // ── Per-frame collision contact monitoring (debug) ───────────
        private void MonitorCollisionContacts()
        {
            if (!enableDebugLog || _needleInteraction == null) return;

            // Throttle: log at most every 2 seconds
            if (Time.time - _lastContactLogTime < 2f) return;
            _lastContactLogTime = Time.time;

            try
            {
                var cd = _needleInteraction.getCollisionDetection();
                if (cd == null) return;

                var data = cd.getCollisionData();
                if (data == null) return;

                var elemsA = data.elementsA;
                var elemsB = data.elementsB;
                int countA = (elemsA != null) ? elemsA.Count : 0;
                int countB = (elemsB != null) ? elemsB.Count : 0;

                // Only log when count changes to avoid spam
                if (countA != _prevContactCountA)
                {
                    Debug.Log($"[Suturing] CD contacts: tissue={countA}, needle={countB}  (CD type: {cd.getTypeName()})");
                    _prevContactCountA = countA;

                    if (countA == 0 && countB == 0)
                        Debug.Log("[Suturing] No collision contacts detected. Verify needle is close to tissue. " +
                                  "If tissue mesh is not closed/watertight, set overrideCDType to 'PointSetToSurfaceMeshCD'.");
                }
            }
            catch (System.Exception) { /* CD data may be accessed from wrong thread — ignore */ }
        }

        // ── Per-frame puncture monitoring (debug) ────────────────────
        private void MonitorPunctureState()
        {
            if (!enableDebugLog || _needleInteraction == null) return;

            var pData = _needleInteraction.getPunctureData();
            if (pData == null) return;

            int needlePts = (pData.needle != null) ? (int)pData.needle.Count : 0;
            int threadPts = (pData.thread != null) ? (int)pData.thread.Count : 0;
            int stitchSets = (pData.stitch != null) ? (int)pData.stitch.Count : 0;

            // Detect needle puncture event
            bool isNeedlePunctured = needlePts > 0;
            if (isNeedlePunctured && !_wasNeedlePunctured)
            {
                Debug.Log($"[Suturing] >>> NEEDLE PUNCTURED tissue! Puncture points: {needlePts} <<<");
            }
            else if (!isNeedlePunctured && _wasNeedlePunctured)
            {
                Debug.Log("[Suturing] Needle puncture ended (all needle puncture points removed/transitioned).");
            }
            _wasNeedlePunctured = isNeedlePunctured;

            // Detect new needle puncture points
            if (needlePts > _prevNeedlePunctureCount)
            {
                Debug.Log($"[Suturing] Needle puncture count: {_prevNeedlePunctureCount} -> {needlePts} (+{needlePts - _prevNeedlePunctureCount})");
            }
            _prevNeedlePunctureCount = needlePts;

            // Detect thread puncture transition
            bool isThreadPunctured = threadPts > 0;
            if (isThreadPunctured && !_wasThreadPunctured)
            {
                Debug.Log($"[Suturing] >>> THREAD entered tissue! Thread puncture points: {threadPts} <<<");
            }
            _wasThreadPunctured = isThreadPunctured;

            // Detect new thread puncture points
            if (threadPts > _prevThreadPunctureCount)
            {
                Debug.Log($"[Suturing] Thread puncture count: {_prevThreadPunctureCount} -> {threadPts} (+{threadPts - _prevThreadPunctureCount})");
            }
            _prevThreadPunctureCount = threadPts;

            // Detect new stitches
            if (stitchSets > _prevStitchCount)
            {
                Debug.Log($"[Suturing] Stitch sets: {_prevStitchCount} -> {stitchSets}");
                _prevStitchCount = stitchSets;
            }
        }

        // ── Update ───────────────────────────────────────────────────
        public void Update()
        {
            // Pin thread to needle every frame
            PinThreadVertex0ToNeedle();

            // Monitor collision contacts (debug — throttled)
            MonitorCollisionContacts();

            // Monitor puncture events (debug)
            MonitorPunctureState();

            // Keyboard stitch trigger
            if (Input.GetKeyDown(activationKey))
            {
                if (enableDebugLog) Debug.Log($"[Suturing] Key '{activationKey}' pressed → Pull()");
                Pull();
            }

            // VR controller trigger (fire once per press) — grip is now used for clutch control
            if (inputHandler != null)
            {
                bool triggerNow = inputHandler.IsTriggerPressed();
                if (triggerNow && !_triggerWasPressed)
                {
                    if (enableDebugLog) Debug.Log("[Suturing] VR trigger pressed → Pull()");
                    Pull();
                }
                _triggerWasPressed = triggerNow;
            }
        }

        // ── Public API ───────────────────────────────────────────────
        public Imstk.NeedlePbdCH.PunctureData GetPunctureData()
        {
            return _needleInteraction?.getPunctureData();
        }
    }
}

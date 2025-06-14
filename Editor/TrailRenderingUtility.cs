using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Combat;
using DivineDragon.EngageAnimationEvents;

namespace DivineDragon
{
    public static class TrailRenderingUtility
    {
        public static void DrawQuadBetweenCurvesAtTime(
            AnimationCurve rootX, AnimationCurve rootY, AnimationCurve rootZ,
            AnimationCurve tipX, AnimationCurve tipY, AnimationCurve tipZ,
            float currentTime, Color color, int steps = 1, int prevFrames = 1)
        {
            if (rootX == null || rootY == null || rootZ == null || tipX == null || tipY == null || tipZ == null) return;
            
            int currentIndex = -1;
            for (int i = 0; i < rootX.length; i++)
            {
                if (rootX.keys[i].time <= currentTime)
                    currentIndex = i;
                else
                    break;
            }
    
            // Check if we have enough frames to look back
            if (currentIndex < prevFrames) return;
    
            // Get the frame that's prevFrames back from current
            int frameBeforeIndex = currentIndex - prevFrames;
            float timeA = rootX.keys[frameBeforeIndex].time;
            float timeB = currentTime;
            float stepSize = (timeB - timeA) / Mathf.Max(1, steps);
    
            // Draw quads from newest to oldest
            for (int i = 0; i < steps; i++)
            {
                float segEnd = timeB - i * stepSize;
                float segStart = segEnd - stepSize;

                // Create fade effect (more recent = more opaque)
                float alpha = 0.75f * (steps - i) / steps;
                Color stepColor = new Color(color.r, color.g, color.b, alpha);

                // Calculate corner positions
                Vector3 rootA = new Vector3(rootX.Evaluate(segStart), rootY.Evaluate(segStart), rootZ.Evaluate(segStart));
                Vector3 tipA = new Vector3(tipX.Evaluate(segStart), tipY.Evaluate(segStart), tipZ.Evaluate(segStart));
                Vector3 rootB = new Vector3(rootX.Evaluate(segEnd), rootY.Evaluate(segEnd), rootZ.Evaluate(segEnd));
                Vector3 tipB = new Vector3(tipX.Evaluate(segEnd), tipY.Evaluate(segEnd), tipZ.Evaluate(segEnd));

                // Draw outline
                Handles.color = color;
                Handles.DrawLine(rootA, tipA);
                Handles.DrawLine(tipA, tipB);
                Handles.DrawLine(tipB, rootB);
                Handles.DrawLine(rootB, rootA);

                // Draw filled quad
                DrawFilledQuad(rootA, tipA, tipB, rootB, stepColor);
            }
        }
        
        private static void DrawFilledQuad(Vector3 rootA, Vector3 tipA, Vector3 tipB, Vector3 rootB, Color color)
        {
            Mesh quadMesh = new Mesh
            {
                vertices = new[] { rootA, tipA, tipB, rootB },
                triangles = new[] { 0, 1, 2, 2, 3, 0 }
            };

            Material mat = new Material(Shader.Find("Hidden/Internal-Colored"));
            mat.SetColor("_Color", new Color(color.r, color.g, color.b, color.a));
            mat.SetPass(0);
            
            Graphics.DrawMeshNow(quadMesh, Matrix4x4.identity);
        }
        
        public static void RenderTrailBetweenEvents<TBeginEvent, TEndEvent>(
            ParsedEngageAnimationEvent beginEvent,
            AnimationEditor go,
            List<ParsedEngageAnimationEvent> events,
            TrailTrack trailTrack,
            Color color,
            string endEventName)
            where TBeginEvent : ParsedEngageAnimationEvent
            where TEndEvent : ParsedEngageAnimationEvent
        {
            // Get the trail end event
            var endEvent = events.Find(e => e is TEndEvent && e.backingAnimationEvent.time > beginEvent.backingAnimationEvent.time) as TEndEvent;
            if (endEvent == null)
            {
                Debug.Log($"Unable to find {endEventName} event after the {beginEvent.displayName} event at time {beginEvent.backingAnimationEvent.time}");
                return;
            }
            
            var currentTime = go.currentTime;
            
            // If we aren't between the trail begin and end, return
            if (currentTime < beginEvent.backingAnimationEvent.time || currentTime > endEvent.backingAnimationEvent.time)
                return;
            
            // Find the prefetched curve for the weapon trail
            var genericObject = events.Find(e => e is GenericObject genericObject && genericObject.backingAnimationEvent.stringParameter == "PC") as GenericObject;
            if (genericObject == null)
            {
                return;
            }

            var prefetchedCurve = (PrefetchedCurve_Bridge) genericObject.backingAnimationEvent.objectReferenceParameter;
            
            if (prefetchedCurve != null && trailTrack != null)
            {
                DrawQuadBetweenCurvesAtTime(
                    trailTrack.RootX,
                    trailTrack.RootY,
                    trailTrack.RootZ,
                    trailTrack.TipX,
                    trailTrack.TipY,
                    trailTrack.TipZ,
                    currentTime,
                    color,
                    6,
                    1
                );
            }
        }
    }
}
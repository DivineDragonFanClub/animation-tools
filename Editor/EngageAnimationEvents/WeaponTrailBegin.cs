using System.Collections.Generic;
using Combat;
using UnityEditor;
using UnityEngine;

namespace DivineDragon.EngageAnimationEvents
{
    public class WeaponTrailBegin : ParsedEngageAnimationEvent
    {
        public override string displayName => "Weapon Trail Begin";

        public override EventCategory category => EventCategory.WeaponControl;

        public override string Summary => $"Float: {backingAnimationEvent.floatParameter}, Int: {backingAnimationEvent.intParameter}";

        public override string Explanation { get; } = "A Generic Object PrefetchedCurve_Bridge is expected to be present in the event list with the string parameter 'PC'. The float and int parameters have unknown purposes.";

        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>
        {
            ExposedPropertyType.Float,
            ExposedPropertyType.Int
        };
        
        public override void OnScrubbedTo(AnimationEditor go, List<ParsedEngageAnimationEvent> events)
        {
            // Find the child object named c_neck_jnt
            Transform c_neck_jnt = go.transform.GetChild(0).GetChild(0).Find("c_spine1_jnt/c_spine2_jnt/c_neck_jnt");
            // Display a little text label at the position of the c_neck_jnt object in the editor UI
            if (c_neck_jnt != null)
            {
                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.yellow;
                style.fontSize = 20;
                string labelText = $"Weapon Trail Begin: {backingAnimationEvent.floatParameter}";
                Handles.Label(c_neck_jnt.position, labelText, style);
            }
        }

        public override void AlwaysRender(AnimationEditor go, List<ParsedEngageAnimationEvent> events)
        {
            // get the weapon trail end event
            // first, check if we're between a weapon trail begin and end. there can be more than one end event in the list
            // check for the first that comes after this backingAnimationEvent's time
            var weaponTrailEnd = events.Find(e => e is WeaponTrailEnd && e.backingAnimationEvent.time > backingAnimationEvent.time) as WeaponTrailEnd;
            if (weaponTrailEnd == null)
            {
                Debug.Log("Unable to find WeaponTrailEnd event after the WeaponTrailBegin event at time " + backingAnimationEvent.time);
                return;
            }
            
            // var weaponTrailEnd = events.Find(e => e is WeaponTrailEnd) as WeaponTrailEnd;
            var currentTime = go.currentTime;
            
            // if we aren't between the weapon trail begin and end, return
            if (currentTime < backingAnimationEvent.time || currentTime > weaponTrailEnd.backingAnimationEvent.time)
                return;
            
            // find the prefetched curve for the weapon trail
            var genericObject = events.Find(e => e is GenericObject genericObject && genericObject.backingAnimationEvent.stringParameter == "PC") as GenericObject;
            if (genericObject == null)
            {
                return;
                
            }

            var prefetchedCurve = (PrefetchedCurve_Bridge) genericObject.backingAnimationEvent.objectReferenceParameter;
            
            
            if (prefetchedCurve != null)
            {
                DrawQuadBetweenCurvesAtTime(
                    prefetchedCurve.RightHand.RootX,
                    prefetchedCurve.RightHand.RootY,
                    prefetchedCurve.RightHand.RootZ,
                    prefetchedCurve.RightHand.TipX,
                    prefetchedCurve.RightHand.TipY,
                    prefetchedCurve.RightHand.TipZ,
                    currentTime,
                    Color.blue,
                    6,
                    1
                );
                DrawQuadBetweenCurvesAtTime(
                    prefetchedCurve.LeftHand.RootX,
                    prefetchedCurve.LeftHand.RootY,
                    prefetchedCurve.LeftHand.RootZ,
                    prefetchedCurve.LeftHand.TipX,
                    prefetchedCurve.LeftHand.TipY,
                    prefetchedCurve.LeftHand.TipZ,
                    currentTime,
                    Color.red,
                    6,
                    1
                );
            }
        }
        
        
        private void DrawQuadBetweenCurvesAtTime(
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
        private void DrawFilledQuad(Vector3 rootA, Vector3 tipA, Vector3 tipB, Vector3 rootB, Color color)
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
    }


    public class WeaponTrailBeginParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameMatchRule("武器軌跡始")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            WeaponTrailBegin weaponTrailBegin = new WeaponTrailBegin
            {
                backingAnimationEvent = animEvent
            };
            return weaponTrailBegin;
        }
    }
}
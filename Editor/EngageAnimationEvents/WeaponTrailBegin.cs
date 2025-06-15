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

        public override string Summary => $"Begin right hand weapon trail. Float: {backingAnimationEvent.floatParameter}, Int: {backingAnimationEvent.intParameter}";

        public override string Explanation { get; } = "Marks the start of the right hand weapon trail rendering. A Generic Object PrefetchedCurve_Bridge is expected to be present in the event list with the string parameter 'PC'. The float and int parameters have unknown purposes.";

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
            // Find the prefetched curve first to get the trail track
            var genericObject = events.Find(e => e is GenericObject genericObject && genericObject.backingAnimationEvent.stringParameter == "PC") as GenericObject;
            if (genericObject == null)
            {
                return;
            }

            var prefetchedCurve = (PrefetchedCurve_Bridge) genericObject.backingAnimationEvent.objectReferenceParameter;
            if (prefetchedCurve != null)
            {
                TrailRenderingUtility.RenderTrailBetweenEvents<WeaponTrailBegin, WeaponTrailEnd>(
                    this,
                    go,
                    events,
                    prefetchedCurve.RightHand,
                    Color.blue,
                    "WeaponTrailEnd"
                );
            }
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
using System.Collections.Generic;
using Combat;
using UnityEditor;
using UnityEngine;

namespace DivineDragon.EngageAnimationEvents
{
    public class LeftHandTrailBegin : ParsedEngageAnimationEvent
    {
        public override string displayName => "Left Hand Trail Begin";

        public override EventCategory category => EventCategory.WeaponControl;

        public override string Summary { get; } = "Begin rendering the left hand weapon trail.";

        public override string Explanation { get; } = "Marks the start of the left hand weapon trail rendering. A Generic Object PrefetchedCurve_Bridge is expected to be present in the event list with the string parameter 'PC'. The float and int parameters have unknown purposes.";

        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>();

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
                TrailRenderingUtility.RenderTrailBetweenEvents<LeftHandTrailBegin, LeftHandTrailEnd>(
                    this,
                    go,
                    events,
                    prefetchedCurve.LeftHand,
                    Color.red,
                    "LeftHandTrailEnd"
                );
            }
        }
    }


    public class LeftHandTrailBeginParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameMatchRule("左手軌跡始")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            LeftHandTrailBegin leftHandTrailBegin = new LeftHandTrailBegin
            {
                backingAnimationEvent = animEvent
            };
            return leftHandTrailBegin;
        }
    }
}
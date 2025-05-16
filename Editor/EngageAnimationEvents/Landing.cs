using System.Collections.Generic;
using UnityEngine;

namespace Combat.EngageAnimationEvents
{
    public class Landing : ParsedEngageAnimationEvent
    {
        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>
        {
            ExposedPropertyType.Float
        };
        public override EventCategory category => EventCategory.MotionControl;
        
        
        public override string Explanation { get; } = "Landing event with an offset. Unclear how it relates to Jump.";

        public override string displayName => "Landing";
        
        public override string Summary => $"With offset: {backingAnimationEvent.floatParameter}";
        
        public override void OnScrubbedTo(AnimationEditor go, List<ParsedEngageAnimationEvent> events)
        { 
            float landingOffset = backingAnimationEvent.floatParameter;
            Debug.Log($"Landing with offset: {landingOffset}");
        }
    }

    public class LandingParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameMatchRule("着地")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            Landing landing = new Landing
            {
                backingAnimationEvent = animEvent
            };
            return landing;
        }
    }
}

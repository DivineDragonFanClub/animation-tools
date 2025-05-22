using System.Collections.Generic;
using Combat;
using UnityEngine;

namespace DivineDragon.EngageAnimationEvents
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

using System.Collections.Generic;
using UnityEngine;

namespace Combat.EngageAnimationEvents
{
    public class HitDirection : QuantizedEvent
    {
        public override string displayName => "Hit Direction";
        public override string Explanation { get; } = "Marks the hit direction of this attack. Not yet fully understood. (Direction relative to what?)";
        
        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>
        {
            ExposedPropertyType.Float,
            ExposedPropertyType.Int,
        };
        
        public override EventCategory category => EventCategory.AttackSpecifics;
        



        public override void OnScrubbedTo(AnimationEditor go, List<ParsedEngageAnimationEvent> events)
        {
            // Implement the logic for when the animation is scrubbed to this event
        }
    }
    
    public class HitDirectionParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameStringParameterMatchRule("Vec3", "命中方向")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            HitDirection hitDirection = new HitDirection
            {
                backingAnimationEvent = animEvent
            };
            return hitDirection;
        }
    }
}
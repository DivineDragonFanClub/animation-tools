using System.Collections.Generic;
using Combat;
using UnityEngine;

namespace DivineDragon.EngageAnimationEvents
{
    public class UnknownEvent: ParsedEngageAnimationEvent
    {
        public UnknownEvent(AnimationEvent animEvent)
        {
            backingAnimationEvent = animEvent;
        }

        public override string displayName => "Unknown Event";

        public override EventCategory category => EventCategory.Uncategorized;

        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>
        {
            ExposedPropertyType.Float,
            ExposedPropertyType.Int,
            ExposedPropertyType.String,
            ExposedPropertyType.ObjectReference,
            ExposedPropertyType.FunctionName,
        };
    }


    public class UnknownEventParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            // matches no one
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            UnknownEvent unknownEvent = new UnknownEvent(
                animEvent
            );
            return unknownEvent;
        }
    }
}
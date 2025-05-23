using System.Collections.Generic;
using Combat;
using UnityEngine;

namespace DivineDragon.EngageAnimationEvents
{
    public class LookAtRate : ParsedEngageAnimationEvent
    {
        public override string displayName => "Look At Rate";

        public override EventCategory category => EventCategory.Unknown;

        public override string Summary { get; } = "Not yet investigated.";

        public override string Explanation { get; } = "Not yet investigated. Likely related to the look at rate of a character or object.";

        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>
        {
            ExposedPropertyType.Float,
            ExposedPropertyType.Int
        };
    }


    public class LookAtRateParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameMatchRule("注目率")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            LookAtRate lookAtRate = new LookAtRate
            {
                backingAnimationEvent = animEvent
            };
            return lookAtRate;
        }
    }
}
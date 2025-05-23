using System.Collections.Generic;
using Combat;
using UnityEngine;

namespace DivineDragon.EngageAnimationEvents
{
    public class TransitionTime : ParsedEngageAnimationEvent
    {
        public override string displayName => "Transition Time";

        public override EventCategory category => EventCategory.Uncategorized;

        public override string Summary => $"{backingAnimationEvent.floatParameter}";

        public override string Explanation { get; } = "Not yet investigated";

        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>
        {
            ExposedPropertyType.Float
        };
    }


    public class TransitionTimeParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameMatchRule("遷移時間")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            TransitionTime transitionTime = new TransitionTime
            {
                backingAnimationEvent = animEvent
            };
            return transitionTime;
        }
    }
}
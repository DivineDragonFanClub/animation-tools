using System.Collections.Generic;
using Combat;
using UnityEngine;

namespace DivineDragon.EngageAnimationEvents
{
    public class AfterCancelEnd : ParsedEngageAnimationEvent
    {
        public override string displayName => "Backward Cancel End";

        public override EventCategory category => EventCategory.Cancels;

        public override string Summary { get; } = "Backward cancel ends.";

        public override string Explanation { get; } = "Signals the end of a backward cancel.";

        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>();
    }


    public class AfterCancelEndParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameMatchRule("後キャン終")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            AfterCancelEnd afterCancelEnd = new AfterCancelEnd
            {
                backingAnimationEvent = animEvent
            };
            return afterCancelEnd;
        }
    }
}
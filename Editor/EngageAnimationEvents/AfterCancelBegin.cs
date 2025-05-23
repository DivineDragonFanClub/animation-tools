using System.Collections.Generic;
using Combat;
using UnityEngine;

namespace DivineDragon.EngageAnimationEvents
{
    public class AfterCancelBegin: ParsedEngageAnimationEvent
    {
        public override string displayName => "Backward Cancel Begin";

        public override EventCategory category => EventCategory.Cancels;

        public override string Summary { get; } = "Backward cancel begins.";

        public override string Explanation { get; } = "Signals when the backward cancel begins.";

        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>();
    }


    public class AfterCancelBeginParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameMatchRule("後キャン始")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            AfterCancelBegin afterCancelBegin = new AfterCancelBegin
            {
                backingAnimationEvent = animEvent
            };
            return afterCancelBegin;
        }
    }
}
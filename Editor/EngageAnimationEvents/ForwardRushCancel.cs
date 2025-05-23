using System.Collections.Generic;
using Combat;
using UnityEngine;

namespace DivineDragon.EngageAnimationEvents
{
    public class ForwardRushCancel : ParsedEngageAnimationEvent
    {
        public override string displayName => "Forward Rush Cancel";

        public override EventCategory category => EventCategory.Cancels;

        public override string Summary { get; } = "Forward rush cancel.";

        public override string Explanation { get; } = "Signals when the forward rush cancel is. More investigations are needed to determine the exact purpose of this event.";

        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>();
    }


    public class ForwardRushCancelParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameMatchRule("突進前キャン")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            ForwardRushCancel forwardRushCancel = new ForwardRushCancel
            {
                backingAnimationEvent = animEvent
            };
            return forwardRushCancel;
        }
    }
}
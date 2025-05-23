using System.Collections.Generic;
using Combat;
using UnityEngine;

namespace DivineDragon.EngageAnimationEvents
{
    public class ForwardCancel : ParsedEngageAnimationEvent
    {
        public override string displayName => "Forward Cancel";

        public override EventCategory category => EventCategory.Cancels;

        public override string Summary { get; } = "Forward cancel.";

        public override string Explanation { get; } = "Signals when the forward cancel is. More investigations are needed to determine the exact purpose of this event.";

        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>();
    }


    public class ForwardCancelParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameMatchRule("前キャンセル")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            ForwardCancel forwardCancel = new ForwardCancel
            {
                backingAnimationEvent = animEvent
            };
            return forwardCancel;
        }
    }
}
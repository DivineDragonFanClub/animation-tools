using System.Collections.Generic;
using Combat;
using UnityEngine;

namespace DivineDragon.EngageAnimationEvents
{
    public class ForwardCancel : ParsedEngageAnimationEvent
    
    {
        public override EventCategory category => EventCategory.Cancels;

        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>();

        public override string displayName => "Forward Cancel";

        public override string Explanation => "Signals when the forward cancel is. More investigations are needed to determine the exact purpose of this event.";
        
        public override string Summary => "Forward cancel.";
        public override void OnScrubbedTo(AnimationEditor go, List<ParsedEngageAnimationEvent> events)
        {
        }
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

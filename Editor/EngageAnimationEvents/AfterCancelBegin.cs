using System.Collections.Generic;
using UnityEngine;

namespace Combat.EngageAnimationEvents
{
    public class AfterCancelBegin: ParsedEngageAnimationEvent
    {
        public override EventCategory category => EventCategory.Cancels;
        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>();
        public override string displayName => "Backward Cancel Begin";
        public override string Explanation => "Signals when the backward cancel begins.";
        
        public override string Summary => "Backward cancel begins.";
        public override void OnScrubbedTo(AnimationEditor go, List<ParsedEngageAnimationEvent> events)
        {
            // throw new System.NotImplementedException();
        }
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
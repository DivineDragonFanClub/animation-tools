using System.Collections.Generic;
using UnityEngine;

namespace Combat.EngageAnimationEvents
{
    public class AfterCancelEnd : ParsedEngageAnimationEvent
    {
        public override EventCategory category => EventCategory.Cancels;
        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>();
        public override string displayName => "Backward Cancel End";
        public override void OnScrubbedTo(AnimationEditor go, List<ParsedEngageAnimationEvent> events)
        {
            // throw new System.NotImplementedException();
        }
        
        public override string Explanation { get; } = "Signals the end of a backward cancel.";
        
        public override string Summary => "Backward cancel ends.";


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
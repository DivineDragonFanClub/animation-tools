using System.Collections.Generic;
using UnityEngine;

namespace Combat.EngageAnimationEvents
{
    public class OpponentDodge : ParsedEngageAnimationEvent
    {
        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>();

        public override string displayName => "Opponent Dodge";
        
        public override EventCategory category => EventCategory.Opponent;

        
        public override string Explanation { get; } = "Signals that the opponent should dodge the attack at this moment.";
        
        public override string Summary => "Opponent dodges the attack.";

        public override void OnScrubbedTo(AnimationEditor go, List<ParsedEngageAnimationEvent> events)
        {
            // Implement the logic for when the animation is scrubbed to this event
        }
    }

    public class OpponentDodgeParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameMatchRule("相手回避")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            OpponentDodge opponentDodge = new OpponentDodge
            {
                backingAnimationEvent = animEvent
            };
            return opponentDodge;
        }
    }
}

using System.Collections.Generic;
using Combat;
using UnityEngine;

namespace DivineDragon.EngageAnimationEvents
{
    public class OpponentDodge : ParsedEngageAnimationEvent
    {
        public override string displayName => "Opponent Dodge";

        public override EventCategory category => EventCategory.Opponent;

        public override string Summary { get; } = "Opponent dodges the attack.";

        public override string Explanation { get; } = "Signals that the opponent should dodge the attack at this moment.";

        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>();

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
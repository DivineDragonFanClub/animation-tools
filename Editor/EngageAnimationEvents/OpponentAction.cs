using System.Collections.Generic;
using Combat;
using UnityEngine;

namespace DivineDragon.EngageAnimationEvents
{
    public class OpponentAction : ParsedEngageAnimationEvent
    {
        public override string displayName => "Opponent Action";
        
        public override EventCategory category => EventCategory.Opponent;

        public override string Explanation { get; } = "Make opponent perform an action. The game only ever calls this function with a string parameter of Ready.";
        
        public override string Summary => $"Action: {backingAnimationEvent.stringParameter}";

        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>
        {
            ExposedPropertyType.String
        };

    }

    public class OpponentActionParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameMatchRule("相手動作")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            OpponentAction opponentAction = new OpponentAction
            {
                backingAnimationEvent = animEvent
            };
            return opponentAction;
        }
        
        public override AnimationEvent Create()
        {
            var evt = base.Create();
            evt.stringParameter = "Ready";
            return evt;
        }
    }
}
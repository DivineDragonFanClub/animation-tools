using System.Collections.Generic;
using UnityEngine;

namespace Combat.EngageAnimationEvents
{
    public class LocalShootPosition : ParsedEngageAnimationEvent
    {
        public override EventCategory category => EventCategory.AttackSpecifics;
        public override string displayName => "Local Shoot Position";
        
        public override string Explanation { get; } = "Not yet investigated. Likely related to the shoot position of a projectile or similar.";
        
        public override string Summary => $"Not yet investigated.";
        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>
        {
            ExposedPropertyType.Float,
            ExposedPropertyType.Int,
            ExposedPropertyType.String,
            ExposedPropertyType.ObjectReference
        };

        public override void OnScrubbedTo(AnimationEditor go, List<ParsedEngageAnimationEvent> events)
        {
            // throw new System.NotImplementedException();
        }
    }
    
    public class LocalShootPositionParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameStringParameterMatchRule("Vec3", "発射位置"),
            new FunctionNameStringParameterMatchRule("Vec3", "SP")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)        { 
            LocalShootPosition localShootPosition = new LocalShootPosition
            {
                backingAnimationEvent = animEvent
            };
            return localShootPosition;
        }
    }
}
using System.Collections.Generic;
using Combat;
using UnityEngine;

namespace DivineDragon.EngageAnimationEvents
{
    public class LocalShootPosition : ParsedEngageAnimationEvent
    {
        public override string displayName => "Local Shoot Position";

        public override EventCategory category => EventCategory.AttackSpecifics;

        public override string Summary { get; } = "Not yet investigated.";

        public override string Explanation { get; } = "Not yet investigated.";

        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>
        {
            ExposedPropertyType.Float,
            ExposedPropertyType.Int,
            ExposedPropertyType.String,
            ExposedPropertyType.ObjectReference
        };
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
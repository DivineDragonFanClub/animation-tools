using System.Collections.Generic;
using Combat;
using UnityEngine;

namespace DivineDragon.EngageAnimationEvents.Vec3Types
{
    public class LocalShootPosition : QuantizedEvent
    {
        public override string displayName => "Local Shoot Position";

        public override EventCategory category => EventCategory.AttackSpecifics;

        public override string Explanation { get; } = "Marks the local shoot position for projectile-based attacks. The position is relative to the character's root transform.";

        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>
        {
            ExposedPropertyType.Float,
            ExposedPropertyType.Int,
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
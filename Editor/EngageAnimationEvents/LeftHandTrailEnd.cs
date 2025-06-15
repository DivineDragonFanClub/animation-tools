using System.Collections.Generic;
using Combat;
using UnityEngine;

namespace DivineDragon.EngageAnimationEvents
{
    public class LeftHandTrailEnd : ParsedEngageAnimationEvent
    {
        public override string displayName => "Left Hand Trail End";

        public override EventCategory category => EventCategory.WeaponControl;

        public override string Summary { get; } = "End rendering the left hand weapon trail.";

        public override string Explanation { get; } = "Marks the end of the left hand weapon trail rendering. The trail rendering that started with Left Hand Trail Begin will stop at this point. Not yet verified.";

        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>();
    }


    public class LeftHandTrailEndParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameMatchRule("左手軌跡終")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            LeftHandTrailEnd leftHandTrailEnd = new LeftHandTrailEnd
            {
                backingAnimationEvent = animEvent
            };
            return leftHandTrailEnd;
        }
    }
}
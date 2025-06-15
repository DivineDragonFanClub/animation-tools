using System.Collections.Generic;
using Combat;
using UnityEngine;

namespace DivineDragon.EngageAnimationEvents
{
    public class WingFlap : ParsedEngageAnimationEvent
    {
        public override string displayName => "Wing Flap";

        public override EventCategory category => EventCategory.MotionControl;

        public override string Summary => $"Wing flap with float {backingAnimationEvent.floatParameter}.";

        public override string Explanation { get; } = "Triggers a wing flapping effect. The vast majority of flap calls in the game use 1.0, though there seems to be some use of 0.5. The difference (if any) is yet to be investigated.";

        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>
        {
            ExposedPropertyType.Float
        };
    }


    public class WingFlapParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameMatchRule("羽ばたき")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            WingFlap wingFlap = new WingFlap
            {
                backingAnimationEvent = animEvent
            };
            return wingFlap;
        }

        public override AnimationEvent Create()
        {
            AnimationEvent evt = base.Create();
            evt.floatParameter = 1.0f;
            return evt;
        }
    }
}
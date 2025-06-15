using System.Collections.Generic;
using Combat;
using UnityEngine;

namespace DivineDragon.EngageAnimationEvents
{
    public class ScreenShake : ParsedEngageAnimationEvent
    {
        public override string displayName => "Screen Shake";

        public override EventCategory category => EventCategory.CameraEffects;

        public override string Summary => $"Screen shake intensity {backingAnimationEvent.floatParameter:F1}.";

        public override string Explanation { get; } = "Shakes the camera/screen. The float parameter controls shake intensity (0.1 to 1.0).";

        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>
        {
            ExposedPropertyType.Float
        };
    }


    public class ScreenShakeParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameMatchRule("画面揺れ")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            ScreenShake screenShake = new ScreenShake
            {
                backingAnimationEvent = animEvent
            };
            return screenShake;
        }

        public override AnimationEvent Create()
        {
            AnimationEvent evt = base.Create();
            evt.floatParameter = 0.5f;
            return evt;
        }
    }
}
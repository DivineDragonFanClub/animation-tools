using System.Collections.Generic;
using Combat;
using UnityEngine;

namespace DivineDragon.EngageAnimationEvents
{
    public class RadialBlur : ParsedEngageAnimationEvent
    {
        public override string displayName => "Radial Blur";

        public override EventCategory category => EventCategory.CameraEffects;

        public override string Summary => $"Radial blur intensity {backingAnimationEvent.floatParameter:F1}.";

        public override string Explanation { get; } = "Applies a radial blur post-processing effect. The float parameter controls blur intensity (0.0 to 1.0).";

        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>
        {
            ExposedPropertyType.Float
        };
    }


    public class RadialBlurParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameMatchRule("ラジアルブラー")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            RadialBlur radialBlur = new RadialBlur
            {
                backingAnimationEvent = animEvent
            };
            return radialBlur;
        }
    }
}
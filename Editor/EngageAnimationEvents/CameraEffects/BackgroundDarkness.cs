using System.Collections.Generic;
using Combat;
using UnityEngine;

namespace DivineDragon.EngageAnimationEvents
{
    public class BackgroundDarkness : ParsedEngageAnimationEvent
    {
        public override string displayName => "Background Darkness";

        public override EventCategory category => EventCategory.CameraEffects;

        public override string Summary => $"Background darkness {(backingAnimationEvent.floatParameter > 0 ? "on" : "off")} ({backingAnimationEvent.floatParameter:F1}).";

        public override string Explanation { get; } = "Controls background darkness/dimming. Float parameter is 0.0 (normal) or 1.0 (darkened).";

        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>
        {
            ExposedPropertyType.Float
        };
    }


    public class BackgroundDarknessParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameMatchRule("背景暗さ")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            BackgroundDarkness backgroundDarkness = new BackgroundDarkness
            {
                backingAnimationEvent = animEvent
            };
            return backgroundDarkness;
        }
    }
}
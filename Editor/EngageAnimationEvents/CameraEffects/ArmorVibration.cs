using System.Collections.Generic;
using Combat;
using UnityEngine;

namespace DivineDragon.EngageAnimationEvents
{
    public class ArmorVibration : ParsedEngageAnimationEvent
    {
        public override string displayName => "Armor Vibration";

        public override EventCategory category => EventCategory.CameraEffects;

        public override string Summary => $"Armor vibration {backingAnimationEvent.floatParameter:F1}.";

        public override string Explanation { get; } = "Vibration effect for armored units. The float parameter's purpose is unknown, but the game uses values of 0.0 and 1.0.";

        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>
        {
            ExposedPropertyType.Float
        };
    }


    public class ArmorVibrationParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameMatchRule("アーマー振動")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            ArmorVibration armorVibration = new ArmorVibration
            {
                backingAnimationEvent = animEvent
            };
            return armorVibration;
        }
    }
}
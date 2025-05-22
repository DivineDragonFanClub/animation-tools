using System.Collections.Generic;
using Combat;
using UnityEngine;

namespace DivineDragon.EngageAnimationEvents
{
    public class GenericSound : ParsedEngageAnimationEvent
    {
        public override EventCategory category => EventCategory.Sound;
        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>
        {
            ExposedPropertyType.String,
            ExposedPropertyType.Int
        };

        public override string displayName => "Generic Sound";

        public override string Explanation { get; } = "Play a generic sound effect. Refer to the game's files for possible sound effect names.";
        public override string Summary => $"Play sound: {backingAnimationEvent.stringParameter}";

        
    }

    public class GenericSoundParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameMatchRule("音汎用")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            GenericSound genericSound = new GenericSound
            {
                backingAnimationEvent = animEvent
            };
            return genericSound;
        }
    }
}
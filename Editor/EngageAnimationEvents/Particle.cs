using System.Collections.Generic;
using Combat;
using UnityEngine;

namespace DivineDragon.EngageAnimationEvents
{
    public class Particle : ParsedEngageAnimationEvent
    {
        public override string displayName => "Particle";

        public override EventCategory category => EventCategory.Particle;

        public override string Summary => $"Spawn particle {backingAnimationEvent.objectReferenceParameter?.name ?? "Unknown"}" + 
                                         $"{(!string.IsNullOrEmpty(backingAnimationEvent.stringParameter) ? $" at {backingAnimationEvent.stringParameter}" : "")}" +
                                         $"{(backingAnimationEvent.intParameter != 0 ? $" with setting {backingAnimationEvent.intParameter}" : "")}";

        public override string Explanation { get; } = "Spawn a particle. Remember to make the particle object an addressable. See game's files for usage examples.";

        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>
        {
            ExposedPropertyType.String,
            ExposedPropertyType.Int,
            ExposedPropertyType.ObjectReference
        };

    }


    public class ParticleParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameMatchRule("パーティクル")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            Particle particle = new Particle
            {
                backingAnimationEvent = animEvent
            };
            return particle;
        }
    }
}
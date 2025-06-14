using System.Collections.Generic;
using UnityEngine;

namespace DivineDragon.EngageAnimationEvents
{
    public class HitParticle : ParsedEngageAnimationEvent
    {
        public override string displayName => "Hit Particle";

        public override EventCategory category => EventCategory.Particle;

        public override string Summary => $"Spawn hit particle {backingAnimationEvent.objectReferenceParameter?.name ?? "Unknown"}" + 
                                         $"{(!string.IsNullOrEmpty(backingAnimationEvent.stringParameter) ? $" at {backingAnimationEvent.stringParameter}" : "")}" +
                                         $"{(backingAnimationEvent.intParameter != 0 ? $" with setting {backingAnimationEvent.intParameter}" : "")}";

        public override string Explanation { get; } = "Probably spawns a hit particle effect at the specified location - needs further investigation. See uAnim_Ent0AT-Ft1_c000_Attack2 animation for usage example.";

        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>
        {
            ExposedPropertyType.String,
            ExposedPropertyType.Int,
            ExposedPropertyType.ObjectReference
        };

    }


    public class HitParticleParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameMatchRule("命中パーティクル")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            HitParticle hitParticle = new HitParticle
            {
                backingAnimationEvent = animEvent
            };
            return hitParticle;
        }
    }
}
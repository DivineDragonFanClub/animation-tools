using System.Collections.Generic;
using Combat;
using UnityEngine;

namespace DivineDragon.EngageAnimationEvents
{
    // see Combat.CharacterSignalObserver$$GroundParticle for how it's used
    public class GroundSurfaceParticle : ParsedEngageAnimationEvent
    {
        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>
        {
            ExposedPropertyType.Int,
            ExposedPropertyType.String,
        };        
        
        public override string displayName => "Ground Surface Particle";
        public override EventCategory category => EventCategory.Particle;

        public override string Explanation { get; } = "Play a ground surface particle effect. See the game's files for usage examples.";
        
        public override string Summary => $"Particle at location {backingAnimationEvent.stringParameter} with option: {backingAnimationEvent.intParameter}";


        public override void OnScrubbedTo(AnimationEditor go, List<ParsedEngageAnimationEvent> events)
        {
            // Implement the logic for when the animation is scrubbed to this event
        }
    }

    public class GroundSurfaceParticleParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameMatchRule("地面パーティクル")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            GroundSurfaceParticle groundSurfaceParticle = new GroundSurfaceParticle
            {
                backingAnimationEvent = animEvent
            };
            return groundSurfaceParticle;
        }
    }
}
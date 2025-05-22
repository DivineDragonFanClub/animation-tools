using System.Collections.Generic;
using Combat;
using UnityEngine;

namespace DivineDragon.EngageAnimationEvents
{
    public class DeleteParticle : ParsedEngageAnimationEvent
    {
        
        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>
        {
            ExposedPropertyType.String,
            ExposedPropertyType.ObjectReference
        };
        
        public override string Explanation { get; } = "Delete a particle - takes a string and an object reference. The string is believed to be the name of the particle to delete, and the object reference is the particle system to delete it from.";
        
        // try to fetch the name of the particle from the object reference
        public override string Summary => $"Delete particle: {backingAnimationEvent.objectReferenceParameter?.name} from {backingAnimationEvent.stringParameter}";

        public override string displayName => "Delete Particle";
        public override EventCategory category => EventCategory.Particle;




    }

    public class DeleteParticleParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameMatchRule("パーティクル削除")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            DeleteParticle deleteParticle = new DeleteParticle
            {
                backingAnimationEvent = animEvent
            };
            return deleteParticle;
        }
    }
}
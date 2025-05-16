using System.Collections.Generic;
using UnityEngine;

namespace Combat.EngageAnimationEvents
{
    // probably handled by      Combat.CharacterTimespace$$<>b__30_0
    //           (Combat_CharacterTimespace_o *__this,Combat_Character_o *chr,MethodInfo *method)
    public class WorldTime : ParsedEngageAnimationEvent
    {
        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>
        {
            ExposedPropertyType.Float, // a float that should be anything less than 1? 
            // 1 is the end of the world
            ExposedPropertyType.Int // might be for Combat.SignalToWhom$$IsForMe, which means in general it will be 3
        };
        
        public override EventCategory category => EventCategory.Camera;

        public override string displayName => "World Time";
        
        public override string Explanation { get; } = "Slows down the world time. 1 is normal speed, and smaller values slow down the time. One potential confusing point is, even if the game time is slowed down, some events (such as another WorldTime) still proceed in realtime and are not slowed down by other WorldTime events.";
        
        public override string Summary => $"World time scale set to {backingAnimationEvent.floatParameter}.";

        public override void OnScrubbedTo(AnimationEditor go, List<ParsedEngageAnimationEvent> events)
        {
            // Implement the logic for when the animation is scrubbed to this event
        }
    }

    public class WorldTimeParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameMatchRule("ワールド時間")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            WorldTime worldTime = new WorldTime
            {
                backingAnimationEvent = animEvent
            };
            return worldTime;
        }
    }
}
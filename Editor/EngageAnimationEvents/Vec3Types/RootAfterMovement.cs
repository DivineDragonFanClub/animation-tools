using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Combat.EngageAnimationEvents
{
    public class RootAfterMovement : QuantizedEvent
    {
        public override string displayName => "Root After Movement";

        public override string Explanation { get; } = "Marks the location of the root at the end of the clip. Not yet fully understood.";

        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>
        {
            ExposedPropertyType.Float,
            ExposedPropertyType.Int,
        };
        public override EventCategory category => EventCategory.MotionControl;
        
        public override void OnScrubbedTo(AnimationEditor go, List<ParsedEngageAnimationEvent> events)
        {
        }
    }
    
    public class RootAfterMovementParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameStringParameterMatchRule("Vec3", "ルート移動後")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            RootAfterMovement rootAfterMovement = new RootAfterMovement
            {
                backingAnimationEvent = animEvent
            };
            return rootAfterMovement;
        }
    }
}
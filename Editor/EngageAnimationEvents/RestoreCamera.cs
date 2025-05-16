using System.Collections.Generic;
using Combat;
using UnityEngine;

namespace DivineDragon.EngageAnimationEvents
{
    public class RestoreCamera : ParsedEngageAnimationEvent
    {
        public override EventCategory category => EventCategory.Camera;

        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>
        {

        };
        
        public override string Explanation { get; } = "Restores the camera to its original position, ending any Camera event.";
        public override string Summary => "Restore camera.";

        public override string displayName => "Restore Camera";
        public override void OnScrubbedTo(AnimationEditor go, List<ParsedEngageAnimationEvent> events)
        {
            // Implement the logic for when the animation is scrubbed to this event
        }
    }

    public class RestoreCameraAnimationParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameMatchRule("カメラ戻す")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            RestoreCamera restoreCamera = new RestoreCamera
            {
                backingAnimationEvent = animEvent
            };
            return restoreCamera;
        }
    }
}
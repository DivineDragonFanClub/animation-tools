using System.Collections.Generic;
using Combat;
using UnityEngine;

namespace DivineDragon.EngageAnimationEvents
{
    public class RestoreCamera : ParsedEngageAnimationEvent
    {
        public override string displayName => "Restore Camera";

        public override EventCategory category => EventCategory.Camera;

        public override string Summary { get; } = "Restore camera.";

        public override string Explanation { get; } = "Restores the camera to its original position, ending any Camera event.";

        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>
        {

        };
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
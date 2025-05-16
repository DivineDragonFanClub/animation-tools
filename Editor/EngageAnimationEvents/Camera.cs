using System.Collections.Generic;
using UnityEngine;

namespace Combat.EngageAnimationEvents
{
    public class Camera : ParsedEngageAnimationEvent
    {
        public override EventCategory category => EventCategory.Camera;

        // see the following for potentially where this is being used
        // Combat.CameraManager$$UpdateSituation	71025b9170	void Combat.CameraManager$$UpdateSituation(Combat_CameraManager_o * __this, UnityEngine_AnimationEvent_o * ev, int32_t callSide, MethodInfo * method)	312
        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>
        {
            ExposedPropertyType.String, // just the string name of the camera in the combatcameras bundle?
            // probably not all of them will work properly, need to study animations clips to see which ones are used
            ExposedPropertyType.Int // used for Combat.SignalToWhom$$IsForMe, which means in general it will be 3
            //
            // However for criticals it can be different.
            //
            // See the Sniper critical attack for how this is used in practice.
            
            // Two camera events are used at the exact same time, but only one of them will actually be at runtime.
            // For the player, this is called with VCam_Selfie, int param 1, which means that the camera will be focused on the player character.
            // For the enemy, this is called with VCam_CriticalPlayer1, int param 10, which means SetInverse will be called on VCam_CriticalPlayer1
            
            // also, it might be ANDed with 8, which determines if the camera gets SetInverse called on it
            // see code around 71025b924c
        };

        public override string displayName => "Camera";

        public override string Explanation { get; } = "Camera control - needs more investigation.";
        
        public override string Summary => $"Camera Name: {backingAnimationEvent.stringParameter}\n" +
                                            $"Camera Int: {backingAnimationEvent.intParameter}";


        public override void OnScrubbedTo(AnimationEditor go, List<ParsedEngageAnimationEvent> events)
        {
            // Implement the logic for when the animation is scrubbed to this event
        }
    }

    public class CameraAnimationParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameMatchRule("カメラ")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            Camera camera = new Camera
            {
                backingAnimationEvent = animEvent
            };
            return camera;
        }
    }
}
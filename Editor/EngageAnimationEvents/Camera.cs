using System;
using System.Collections.Generic;
using Combat;
using DivineDragon.EngageAnimations;
using UnityEngine;
using UnityEngine.UIElements;

namespace DivineDragon.EngageAnimationEvents
{
    public class Camera : ParsedEngageAnimationEvent
    {
        public override string displayName => "Camera";

        public override EventCategory category => EventCategory.Camera;

        public override string Summary => $"Camera Name: {backingAnimationEvent.stringParameter}, " +
                                          $"For Self: {IsForSelf()}, " +
                                          $"For Enemy: {IsForEnemy()}, " +
                                          $"Inverse: {IsInverse()}";

        public override string Explanation { get; } = "The string parameter is the name of the camera, and the int parameter is used for determining if it's a camera that's used for self, the opponent, or both. Additionally, it determines if the camera should be inverted. More investigation is needed to determine what all this really means in practice..";

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

        private bool IsForSelf()
        {
            return Bit.Get(backingAnimationEvent.intParameter, 1, 0) != 0;
        }

        private bool IsForEnemy()
        {
            return Bit.Get(backingAnimationEvent.intParameter, 1, 1) != 0;
        }

        private bool IsInverse()
        {
            return Bit.Get(backingAnimationEvent.intParameter, 1, 3) != 0;
        }

        private AnimationEvent SetForSelf(bool value)
        {
            int param = backingAnimationEvent.intParameter;
            int result = Bit.Combine(param, value ? 1 : 0, 1, 0);
            var clone = backingAnimationEvent.Clone();
            clone.intParameter = result;
            return clone;
        }

        private AnimationEvent SetForEnemy(bool value)
        {
            int param = backingAnimationEvent.intParameter;
            int result = Bit.Combine(param, value ? 1 : 0, 1, 1);
            var clone = backingAnimationEvent.Clone();
            clone.intParameter = result;
            return clone;
        }

        private AnimationEvent SetInverse(bool value)
        {
            int param = backingAnimationEvent.intParameter;
            int result = Bit.Combine(param, value ? 1 : 0, 1, 3);
            var clone = backingAnimationEvent.Clone();
            clone.intParameter = result;
            return clone;
        }

        public override VisualElement MakeSpecialEditor(Action<ParsedEngageAnimationEvent, AnimationEvent> onSave,
            List<ParsedEngageAnimationEvent> events)
        {
            VisualElement container = new VisualElement();

            // Camera name field
            var cameraName = new TextField("Camera Name")
            {
                value = backingAnimationEvent.stringParameter,
                isDelayed = true
            };
            cameraName.RegisterValueChangedCallback(evt => 
            {
                var clone = backingAnimationEvent.Clone();
                clone.stringParameter = evt.newValue;
                onSave(this, clone);
            });
            container.Add(cameraName);

            // For Self checkbox
            var forSelf = new Toggle("For Self")
            {
                value = IsForSelf()
            };
            forSelf.RegisterValueChangedCallback(evt => 
            {
                onSave(this, SetForSelf(evt.newValue));
            });
            container.Add(forSelf);

            // For Enemy checkbox
            var forEnemy = new Toggle("For Enemy")
            {
                value = IsForEnemy()
            };
            forEnemy.RegisterValueChangedCallback(evt => 
            {
                onSave(this, SetForEnemy(evt.newValue));
            });
            container.Add(forEnemy);

            // Inverse checkbox
            var inverse = new Toggle("Inverse")
            {
                value = IsInverse()
            };
            inverse.RegisterValueChangedCallback(evt => 
            {
                onSave(this, SetInverse(evt.newValue));
            });
            container.Add(inverse);

            return container;
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
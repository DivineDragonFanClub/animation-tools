using System;
using System.Collections.Generic;
using Combat;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace DivineDragon.EngageAnimationEvents
{
    public class FootIK : ParsedEngageAnimationEvent
    {
        public override string displayName => "Foot IK";

        public override EventCategory category => EventCategory.Uncategorized;

        public override string Summary => backingAnimationEvent.intParameter == 1 ? "Enable Foot IK" : "Disable Foot IK";

        public override string Explanation { get; } = "Controls Inverse Kinematics for foot positioning during combat animations. Used exclusively in Emblem AttackC animations.";

        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>
        {
            ExposedPropertyType.Int,
        };

        public override VisualElement MakeSpecialEditor(Action<ParsedEngageAnimationEvent, AnimationEvent> onSave,
            List<ParsedEngageAnimationEvent> events)
        {
            VisualElement container = new VisualElement();

            // Render a checkbox for the Foot IK property
            var ikToggle = new Toggle("Enable Foot IK")
            {
                value = backingAnimationEvent.intParameter == 1
            };
            ikToggle.RegisterValueChangedCallback(evt => 
            { 
                var clone = backingAnimationEvent.Clone();
                clone.intParameter = evt.newValue ? 1 : 0;
                onSave(this, clone);
            });
            container.Add(ikToggle);

            return container;
        }
    }


    public class FootIKParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameMatchRule("FootIK")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            FootIK footIK = new FootIK
            {
                backingAnimationEvent = animEvent
            };
            return footIK;
        }

        public override AnimationEvent Create()
        {
            AnimationEvent animEvent = new AnimationEvent
            {
                functionName = "FootIK",
                intParameter = 1
            };
            
            return animEvent;
        }
    }
}
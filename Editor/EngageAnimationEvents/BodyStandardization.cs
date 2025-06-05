using System;
using System.Collections.Generic;
using Combat;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace DivineDragon.EngageAnimationEvents
{
    public class BodyStandardization : ParsedEngageAnimationEvent
    {
        public override string displayName => "Body Standardization";

        public override EventCategory category => EventCategory.Uncategorized;

        public override string Summary => "Unclear what this does.";

        public override string Explanation { get; } = "Unclear what this does.";

        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>
        {
            // No parameters - all are zero
        };
        
        public override void OnScrubbedTo(AnimationEditor go, List<ParsedEngageAnimationEvent> events)
        {
            // Find the character root transform to display standardization status
            Transform characterRoot = go.transform.GetChild(0).GetChild(0);
            if (characterRoot != null)
            {
                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.yellow;
                style.fontSize = 18;
                Handles.Label(characterRoot.position + Vector3.up * 2.0f, "Body Standardization", style);
            }
        }

        public override VisualElement MakeSpecialEditor(Action<ParsedEngageAnimationEvent, AnimationEvent> onSave,
            List<ParsedEngageAnimationEvent> events)
        {
            VisualElement container = new VisualElement();

            container.Add(new Label("Body Standardization Event"));
            container.Add(new Label("This event resets character scaling during transformation animations."));
            container.Add(new Label("All parameters are fixed at zero - no editing required."));

            // Show the fixed parameter values
            var parameterInfo = new VisualElement();
            parameterInfo.Add(new Label($"String Parameter: \"{backingAnimationEvent.stringParameter}\""));
            parameterInfo.Add(new Label($"Float Parameter: {backingAnimationEvent.floatParameter}"));
            parameterInfo.Add(new Label($"Int Parameter: {backingAnimationEvent.intParameter}"));
            container.Add(parameterInfo);

            return container;
        }
    }


    public class BodyStandardizationParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameMatchRule("体格標準化")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            BodyStandardization bodyStandardization = new BodyStandardization
            {
                backingAnimationEvent = animEvent
            };
            return bodyStandardization;
        }

        public override AnimationEvent Create()
        {
            AnimationEvent animEvent = new AnimationEvent
            {
                functionName = "体格標準化",
                stringParameter = "",
                floatParameter = 0.0f,
                intParameter = 0
            };
            
            return animEvent;
        }
    }
}
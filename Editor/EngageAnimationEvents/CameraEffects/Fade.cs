using System;
using System.Collections.Generic;
using Combat;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace DivineDragon.EngageAnimationEvents
{
    public class Fade : ParsedEngageAnimationEvent
    {
        // Define the fade types
        public static readonly Dictionary<string, int> FadeTypes = new Dictionary<string, int>
        {
            { "Fade to Black", 0 },
            { "Fade to White", 1 },
            { "Fade In", 2 }
        };

        public override string displayName => "Fade";

        public override EventCategory category => EventCategory.CameraEffects;

        public override string Summary => $"Fade {GetFadeType()} over {backingAnimationEvent.floatParameter:F1}s.";

        public override string Explanation { get; } = "Fades the screen. Float parameter is duration in seconds, int parameter is type (0 = fade to black, 1 = fade to white, 2 = fade in from current fade).";

        private string GetFadeType()
        {
            switch (backingAnimationEvent.intParameter)
            {
                case 0: return "to black";
                case 1: return "to white";
                case 2: return "in";
                default: return "unknown";
            }
        }

        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>
        {
            ExposedPropertyType.Float,
            ExposedPropertyType.Int
        };

        public override VisualElement MakeSpecialEditor(Action<ParsedEngageAnimationEvent, AnimationEvent> onSave,
            List<ParsedEngageAnimationEvent> events)
        {
            VisualElement container = new VisualElement();
            container.Add(new Label("Fade Type"));
    
            // Create a list of fade type names
            List<string> fadeTypeNames = new List<string>(FadeTypes.Keys);
    
            // Find the current fade type
            string currentFadeType = GetFadeTypeFromInt(backingAnimationEvent.intParameter);
            int currentIndex = fadeTypeNames.IndexOf(currentFadeType);
            if (currentIndex < 0) currentIndex = 0; // Default to first item if not found
    
            // Create the dropdown
            var fadeDropdown = new PopupField<string>(
                fadeTypeNames, 
                currentIndex,
                formatSelectedValueCallback: (s) => s,
                formatListItemCallback: (s) => s
            );
    
            fadeDropdown.RegisterValueChangedCallback(evt => {
                string selectedFadeType = evt.newValue;
                int selectedValue = FadeTypes[selectedFadeType];
        
                // Create a clone of the animation event
                var clone = backingAnimationEvent.Clone();
        
                // Update the int parameter
                clone.intParameter = selectedValue;
        
                // Save the changes
                onSave(this, clone);
            });
    
            container.Add(fadeDropdown);
            
            // Add duration field
            var durationField = new FloatField("Duration") { value = backingAnimationEvent.floatParameter };
            durationField.RegisterValueChangedCallback(evt => {
                var clone = backingAnimationEvent.Clone();
                clone.floatParameter = evt.newValue;
                onSave(this, clone);
            });
            container.Add(durationField);
            
            return container;
        }

        private string GetFadeTypeFromInt(int value)
        {
            foreach (var kvp in FadeTypes)
            {
                if (kvp.Value == value)
                    return kvp.Key;
            }
            return "Fade to Black"; // Default
        }
    }


    public class FadeParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameMatchRule("フェード")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            Fade fade = new Fade
            {
                backingAnimationEvent = animEvent
            };
            return fade;
        }

        public override AnimationEvent Create()
        {
            AnimationEvent evt = base.Create();
            evt.floatParameter = 0.5f;
            evt.intParameter = 1;
            return evt;
        }
    }
}
using System;
using System.Collections.Generic;
using Combat;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace DivineDragon.EngageAnimationEvents
{
    public class Reins : ParsedEngageAnimationEvent
    {
        // Define the reins actions
        public static readonly Dictionary<string, int> ReinsActions = new Dictionary<string, int>
        {
            { "つかむ", 1 },  // Grab
            { "離す", 0 }     // Release
        };

        public override string displayName => "Reins";

        public override EventCategory category => EventCategory.Riders;

        public override string Summary => $"Reins action: {backingAnimationEvent.stringParameter}.";

        public override string Explanation { get; } = "Controls the reins of a mount. It hasn't been investigated whether it's the string or the int that matters here.";

        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>
        {
            ExposedPropertyType.Int,
            ExposedPropertyType.String,
        };
        
        public override void OnScrubbedTo(AnimationEditor go, List<ParsedEngageAnimationEvent> events)
        {
            Transform transform = go.transform.GetChild(0).GetChild(0);
            if (transform != null)
            {
                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.yellow;
                style.fontSize = 20;
                string labelText = $"Reins: {backingAnimationEvent.stringParameter}";
                Handles.Label(transform.position, labelText, style);
            }
        }

        public override VisualElement MakeSpecialEditor(Action<ParsedEngageAnimationEvent, AnimationEvent> onSave,
            List<ParsedEngageAnimationEvent> events)
        {
            VisualElement container = new VisualElement();
            container.Add(new Label("Reins Action"));
    
            // Create a list of action names
            List<string> actionNames = new List<string>(ReinsActions.Keys);
    
            // Find the current action index
            string currentAction = backingAnimationEvent.stringParameter;
            int currentIndex = actionNames.IndexOf(currentAction);
            if (currentIndex < 0) currentIndex = 0; // Default to first item if not found
    
            // Create the dropdown
            var actionDropdown = new PopupField<string>(
                actionNames, 
                currentIndex,
                formatSelectedValueCallback: (s) => s,
                formatListItemCallback: (s) => s
            );
    
            actionDropdown.RegisterValueChangedCallback(evt => {
                string selectedAction = evt.newValue;
                int selectedValue = ReinsActions[selectedAction];
        
                // Create a clone of the animation event
                var clone = backingAnimationEvent.Clone();
        
                // Update both parameters
                clone.stringParameter = selectedAction;
                clone.intParameter = selectedValue;
        
                // Save the changes
                onSave(this, clone);
            });
    
            container.Add(actionDropdown);
            return container;
        }
    }


    public class ReinsParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameMatchRule("手綱")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            Reins reins = new Reins
            {
                backingAnimationEvent = animEvent
            };
            return reins;
        }
    }
}
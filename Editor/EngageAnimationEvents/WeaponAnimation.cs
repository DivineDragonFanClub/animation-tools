using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Combat.EngageAnimationEvents
{
    public class WeaponAnimation : ParsedEngageAnimationEvent
    {
        // Define the predefined weapon animations
        public static readonly Dictionary<string, int> WeaponAnimations = new Dictionary<string, int>
        {
            { "Close", 1 },
            { "Open", 0 },
            { "OpenHold", 3 },
            { "OpenLoop", 4 }
        };

        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>
        {
            ExposedPropertyType.Int,
            ExposedPropertyType.String,
        };

        public override string displayName => "Weapon Animation";
        public override string Summary => $"Play the {backingAnimationEvent.stringParameter} weapon animation.";
        
        public override EventCategory category => EventCategory.WeaponControl;
        public override string Explanation { get; } = "Play a weapon animation. Only the string parameter is actually used by the game.";
        
        public override void OnScrubbedTo(AnimationEditor go, List<ParsedEngageAnimationEvent> events)
        {
            Transform transform = go.transform.GetChild(0).GetChild(0);
            if (transform != null)
            {
                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.green;
                style.fontSize = 20;
                string labelText = $"Weapon Anim: {backingAnimationEvent.stringParameter}";
                Handles.Label(transform.position, labelText, style);
            }
        }

        public override VisualElement MakeSpecialEditor(Action<ParsedEngageAnimationEvent, AnimationEvent> onSave,
            List<ParsedEngageAnimationEvent> events)
        {
            VisualElement container = new VisualElement();
            container.Add(new Label("Weapon Animation"));
    
            // Create a list of animation names
            List<string> animNames = new List<string>(WeaponAnimations.Keys);
    
            // Find the current animation index
            string currentAnim = backingAnimationEvent.stringParameter;
            int currentIndex = animNames.IndexOf(currentAnim);
            if (currentIndex < 0) currentIndex = 0; // Default to first item if not found
    
            // Create the dropdown
            var animDropdown = new PopupField<string>(
                animNames, 
                currentIndex,
                formatSelectedValueCallback: (s) => s,
                formatListItemCallback: (s) => s
            );
    
            animDropdown.RegisterValueChangedCallback(evt => {
                string selectedAnim = evt.newValue;
                int selectedValue = WeaponAnimations[selectedAnim];
        
                // Create a clone of the animation event
                var clone = backingAnimationEvent.Clone();
        
                // Update both parameters
                clone.stringParameter = selectedAnim;
                clone.intParameter = selectedValue;
        
                // Save the changes
                onSave(this, clone);
            });
    
            container.Add(animDropdown);
            return container;
        }
    }

    public class WeaponAnimationParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameMatchRule("武器アニメ")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            WeaponAnimation weaponAnimation = new WeaponAnimation
            {
                backingAnimationEvent = animEvent
            };
            return weaponAnimation;
        }
    }
}
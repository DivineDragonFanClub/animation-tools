using System;
using System.Collections.Generic;
using Combat;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace DivineDragon.EngageAnimationEvents
{
    public class Expression: ParsedEngageAnimationEvent
    {
        // return a formated string explaining which expression is being used
        public override string Summary => "Show the " + backingAnimationEvent.stringParameter + " expression.";
        
        // return a string explaining what this event does
        public override string Explanation => "Shows an expression for the character. " +
                                              "The string is the name of the expression, and the int is the index of the expression in the list, though in actuality, only the string parameter is actually used.";
        // string array for the possible expressions
        public override string displayName => "Facial Expression";

        public override EventCategory category => EventCategory.AttackingCharacter;

        // string array for the possible expressions
        public static string[] Expressions = new[]
        {
            "Normal",
            "Angry",
            "Pain",
            "Sad",
            "Smile",
            "Strike",
            "Surprise",
            "Die",
            "Pain", // Yes, again
            "StandBy",
            "Status",
            "Relax",
            "Serious",
            "Shy",
        };
        
        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>
        {
            ExposedPropertyType.Int, // By inspection, the int field is never read by the game.
            ExposedPropertyType.String, // Only the string matters and actually gets read.
            // However, all the vanilla events always set both of them, and they are consistent.
            // Just a sample of what I've seen in actual game events.
            // Normal 0
            // Angry 1
            // Pain 2
            // Smile 4
            // Strike 5
            // Die 7
            // These seem to be based on the String name and the index of the clip in the 
            // AC_Face AnimatorController for uheads.
            
            // Pain has a special handling during combat, see Combat.CharacterSignalObserver.<>c$$<MyStart>b__9_6 for details
            
        };
        
        public override void OnScrubbedTo(AnimationEditor go, List<ParsedEngageAnimationEvent> events)
        {
            // Find the child object named c_neck_jnt
            Transform c_neck_jnt = go.transform.GetChild(0).GetChild(0).Find("c_spine1_jnt/c_spine2_jnt/c_neck_jnt");
            // Display a little text label at the position of the c_neck_jnt object in the editor UI
            if (c_neck_jnt != null)
            {
                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.blue;
                style.fontSize = 20;
                string labelText = $"Expression: {backingAnimationEvent.stringParameter}";
                Handles.Label(c_neck_jnt.position, labelText, style);
            }
        }

        public override VisualElement MakeSpecialEditor(Action<ParsedEngageAnimationEvent, AnimationEvent> onSave,
            List<ParsedEngageAnimationEvent> events)
        {
            VisualElement container = new VisualElement();
            container.Add(new Label("Expression"));
    
            // Create a list of unique expressions (skipping the duplicate Pain)
            List<string> uniqueExpressions = new List<string>();
            for (int i = 0; i < Expressions.Length; i++)
            {
                // Skip the second Pain entry at index 8
                if (i != 8)
                {
                    uniqueExpressions.Add(Expressions[i]);
                }
            }
    
            // Find the current expression index
            string currentExpression = backingAnimationEvent.stringParameter;
            int currentIndex = uniqueExpressions.IndexOf(currentExpression);
            if (currentIndex < 0) currentIndex = 0; // Default to first item if not found
    
            // Create the dropdown
            var expressionDropdown = new PopupField<string>(
                uniqueExpressions, 
                currentIndex,
                formatSelectedValueCallback: (s) => s,
                formatListItemCallback: (s) => s
            );
    
            expressionDropdown.RegisterValueChangedCallback(evt => {
                // Get the selected expression
                string selectedExpression = evt.newValue;
        
                // Find the index in the original array
                int expressionIndex = Array.IndexOf(Expressions, selectedExpression);
        
                // Create a clone of the animation event
                var clone = backingAnimationEvent.Clone();
        
                // Update both parameters
                clone.stringParameter = selectedExpression;
                clone.intParameter = expressionIndex;
        
                // Save the changes
                onSave(this, clone);
            });
    
            container.Add(expressionDropdown);
            return container;
        }

    }
    
    public class ExpressionParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameMatchRule("表情")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)        { 
            Expression expression = new Expression
            {
                backingAnimationEvent = animEvent
            };
            return expression;
        }
    }
}
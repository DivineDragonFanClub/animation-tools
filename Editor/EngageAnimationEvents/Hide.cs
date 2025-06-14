using System;
using System.Collections.Generic;
using Combat;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace DivineDragon.EngageAnimationEvents
{
    public class Hide : ParsedEngageAnimationEvent
    {
        public override string displayName => "Hide";

        public override EventCategory category => EventCategory.MotionControl;

        public override string Summary => backingAnimationEvent.intParameter == 1 ? "Hide model" : "Show model";

        public override string Explanation { get; } = "Controls character visibility. Used primarily in corrupted unit death animations (c703/c702) where characters disappear, and in certain attack animations for teleportation or special effects.";

        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>
        {
            ExposedPropertyType.Int,
        };
        
        public override void OnScrubbedTo(AnimationEditor go, List<ParsedEngageAnimationEvent> events)
        {
            // Find the child object named c_neck_jnt
            Transform c_neck_jnt = go.transform.GetChild(0).GetChild(0).Find("c_spine1_jnt/c_spine2_jnt/c_neck_jnt");
            // Display a little text label at the position of the c_neck_jnt object in the editor UI
            if (c_neck_jnt != null)
            {
                GUIStyle style = new GUIStyle();
                style.normal.textColor = backingAnimationEvent.intParameter == 1 ? Color.magenta : Color.green;
                style.fontSize = 20;
                string labelText = backingAnimationEvent.intParameter == 1 ? "Hide Model" : "Show Model";
                Handles.Label(c_neck_jnt.position, labelText, style);
            }
        }

        public override VisualElement MakeSpecialEditor(Action<ParsedEngageAnimationEvent, AnimationEvent> onSave,
            List<ParsedEngageAnimationEvent> events)
        {
            VisualElement container = new VisualElement();

            // Render a checkbox for the Hide property
            var hideToggle = new Toggle("Hide Character")
            {
                value = backingAnimationEvent.intParameter == 1
            };
            hideToggle.RegisterValueChangedCallback(evt => 
            { 
                var clone = backingAnimationEvent.Clone();
                clone.intParameter = evt.newValue ? 1 : 0;
                onSave(this, clone);
            });
            container.Add(hideToggle);

            return container;
        }
    }


    public class HideParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameMatchRule("姿を隠す")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            Hide hide = new Hide
            {
                backingAnimationEvent = animEvent
            };
            return hide;
        }

        public override AnimationEvent Create()
        {
            AnimationEvent animEvent = new AnimationEvent
            {
                functionName = "姿を隠す",
                intParameter = 1
            };
            
            return animEvent;
        }
    }
}
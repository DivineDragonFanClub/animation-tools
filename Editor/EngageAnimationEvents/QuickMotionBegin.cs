using System.Collections.Generic;
using Combat;
using UnityEditor;
using UnityEngine;

namespace DivineDragon.EngageAnimationEvents
{
    public class QuickMotionBegin : ParsedEngageAnimationEvent
    {
        public override string displayName => "Quick Motion Begin";

        public override EventCategory category => EventCategory.MotionControl;

        public override string Summary { get; } = "The beginning of a quick motion.";

        public override string Explanation { get; } = "Signals the beginning of a quick motion. Its effect is not yet fully understood.";

        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>();

        public override void OnScrubbedTo(AnimationEditor go, List<ParsedEngageAnimationEvent> events)
        {
            // Find the child object named c_neck_jnt
            Transform c_neck_jnt = go.transform.GetChild(0).GetChild(0).Find("c_spine1_jnt/c_spine2_jnt/c_neck_jnt");
            // Display a little text label at the position of the c_neck_jnt object in the editor UI
            if (c_neck_jnt != null)
            {
                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.yellow;
                style.fontSize = 20;
                string labelText = "Quick Motion Begin";
                Handles.Label(c_neck_jnt.position, labelText, style);
            }
        }
    }


    public class QuickMotionBeginParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameMatchRule("素早い動作始")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            QuickMotionBegin quickMotionBegin = new QuickMotionBegin
            {
                backingAnimationEvent = animEvent
            };
            return quickMotionBegin;
        }
    }
}
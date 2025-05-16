using System.Collections.Generic;
using Combat;
using UnityEditor;
using UnityEngine;

namespace DivineDragon.EngageAnimationEvents
{
    public class HeavyMotionBegin : ParsedEngageAnimationEvent
    {
        public override string displayName => "Heavy Motion Begin";

        public override string Explanation { get; } = "Signals the beginning of a heavy motion. Its effect is not yet fully understood.";
        
        public override string Summary { get; } = "The beginning of a heavy motion.";
        
        public override EventCategory category => EventCategory.MotionControl;

        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>();

        public override void OnScrubbedTo(AnimationEditor go, List<ParsedEngageAnimationEvent> events)
        {
            // Find the child object named c_neck_jnt
            Transform c_neck_jnt = go.transform.GetChild(0).GetChild(0).Find("c_spine1_jnt/c_spine2_jnt/c_neck_jnt");
            // Display a little text label at the position of the c_neck_jnt object in the editor UI
            if (c_neck_jnt != null)
            {
                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.cyan;
                style.fontSize = 20;
                string labelText = "Heavy Motion Begin";
                Handles.Label(c_neck_jnt.position, labelText, style);
            }
        }
    }

    public class HeavyMotionBeginParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameMatchRule("重い動作始")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            HeavyMotionBegin heavyMotionBegin = new HeavyMotionBegin
            {
                backingAnimationEvent = animEvent
            };
            return heavyMotionBegin;
        }
    }
}
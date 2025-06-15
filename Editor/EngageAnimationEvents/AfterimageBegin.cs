using System.Collections.Generic;
using Combat;
using UnityEditor;
using UnityEngine;

namespace DivineDragon.EngageAnimationEvents
{
    public class AfterimageBegin : ParsedEngageAnimationEvent
    {
        public override string displayName => "Afterimage Begin";

        public override EventCategory category => EventCategory.MotionControl;

        public override string Summary { get; } = "Begin afterimage effect.";

        public override string Explanation { get; } = "Marks the start of the afterimage effect.";

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
                string labelText = "Afterimage Begin";
                Handles.Label(c_neck_jnt.position, labelText, style);
            }
        }
    }


    public class AfterimageBeginParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameMatchRule("残像始")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            AfterimageBegin afterimageBegin = new AfterimageBegin
            {
                backingAnimationEvent = animEvent
            };
            return afterimageBegin;
        }
    }
}
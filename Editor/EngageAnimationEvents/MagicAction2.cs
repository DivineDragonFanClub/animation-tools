using System.Collections.Generic;
using Combat;
using UnityEditor;
using UnityEngine;

namespace DivineDragon.EngageAnimationEvents
{
    public class MagicAction2 : ParsedEngageAnimationEvent
    {
        public override string displayName => "Magic Action 2";

        public override EventCategory category => EventCategory.MotionControl;

        public override string Summary { get; } = "Trigger magic action 2.";

        public override string Explanation { get; } = "Works in conjunction with the Magic objects (which are not yet supported in this editor).";

        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>();

        public override void OnScrubbedTo(AnimationEditor go, List<ParsedEngageAnimationEvent> events)
        {
            // Find the child object named c_neck_jnt
            Transform c_neck_jnt = go.transform.GetChild(0).GetChild(0).Find("c_spine1_jnt/c_spine2_jnt/c_neck_jnt");
            // Display a little text label at the position of the c_neck_jnt object in the editor UI
            if (c_neck_jnt != null)
            {
                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.magenta;
                style.fontSize = 20;
                string labelText = "Magic Action 2";
                Handles.Label(c_neck_jnt.position, labelText, style);
            }
        }
    }


    public class MagicAction2Parser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameMatchRule("魔法動作2")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            MagicAction2 magicAction2 = new MagicAction2
            {
                backingAnimationEvent = animEvent
            };
            return magicAction2;
        }
    }
}
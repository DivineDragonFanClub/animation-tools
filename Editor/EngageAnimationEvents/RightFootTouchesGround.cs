using System.Collections.Generic;
using Combat;
using UnityEditor;
using UnityEngine;

namespace DivineDragon.EngageAnimationEvents
{
    public class RightFootTouchesGround : ParsedEngageAnimationEvent
    {
        public override string displayName => "Right Foot Down";

        public override EventCategory category => EventCategory.FootRight;

        public override string Summary { get; } = "Right foot is down.";

        public override string Explanation { get; } = "Signals that the right foot has landed on the ground.";

        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>
        {
            ExposedPropertyType.Float,
        };

        public override void OnScrubbedTo(AnimationEditor go, List<ParsedEngageAnimationEvent> events)
        {
            // Find the child object named c_r_leg4_jnt
            Transform c_r_leg4_jnt = go.transform.GetChild(0).GetChild(0).Find("r_leg1_jnt/r_leg2_jnt/r_leg3_jnt/r_leg4_jnt");
            // Display a little text label at the position of the c_r_leg4_jnt object in the editor UI
            if (c_r_leg4_jnt != null)
            {
                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.blue;
                style.fontSize = 20;
                string labelText = $"Right Foot Down ⬇";
                Handles.Label(c_r_leg4_jnt.position, labelText, style);
            }
        }
    }


    public class RightFootTouchesGroundParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameMatchRule("右足接地")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            RightFootTouchesGround rightFootTouchesGround = new RightFootTouchesGround
            {
                backingAnimationEvent = animEvent
            };
            return rightFootTouchesGround;
        }

        public override AnimationEvent Create()
        {
            AnimationEvent evt = base.Create();
            evt.floatParameter = 0.5f;
            return evt;
        }
    }
}
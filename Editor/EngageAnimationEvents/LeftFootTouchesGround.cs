using System.Collections.Generic;
using Combat;
using UnityEditor;
using UnityEngine;

namespace DivineDragon.EngageAnimationEvents
{
    public class LeftFootTouchesGround : ParsedEngageAnimationEvent
    {
        public override string displayName => "Left Foot Down";
        
        public override string Explanation { get; } = "Signals that the left foot has landed on the ground.";
        
        public override string Summary => "Left foot is down.";


        public override EventCategory category => EventCategory.FootLeft;

        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>
        {
            ExposedPropertyType.Float,
        };

        public override void OnScrubbedTo(AnimationEditor go, List<ParsedEngageAnimationEvent> events)
        {
            // Find the child object named c_l_leg4_jnt
            Transform c_l_leg4_jnt = go.transform.GetChild(0).GetChild(0).Find("l_leg1_jnt/l_leg2_jnt/l_leg3_jnt/l_leg4_jnt");
            // Display a little text label at the position of the c_l_leg4_jnt object in the editor UI
            if (c_l_leg4_jnt != null)
            {
                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.blue;
                style.fontSize = 20;
                string labelText = $"Left Foot Down ⬇";
                Handles.Label(c_l_leg4_jnt.position, labelText, style);
            }
        }
    }
    
    public class LeftFootTouchesGroundParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameMatchRule("左足接地")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            LeftFootTouchesGround leftFootTouchesGround = new LeftFootTouchesGround
            {
                backingAnimationEvent = animEvent
            };
            return leftFootTouchesGround;
        }

        public override AnimationEvent Create()
        {
            AnimationEvent evt = base.Create();
            evt.floatParameter = 0.5f;
            return evt;
        }
    }
}
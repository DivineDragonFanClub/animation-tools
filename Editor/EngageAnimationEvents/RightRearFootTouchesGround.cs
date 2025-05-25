using System.Collections.Generic;
using Combat;
using UnityEditor;
using UnityEngine;

namespace DivineDragon.EngageAnimationEvents
{
    public class RightRearFootTouchesGround : ParsedEngageAnimationEvent
    {
        public override string displayName => "Right Rear Foot Down";

        public override EventCategory category => EventCategory.Riders;

        public override string Summary { get; } = "Right rear foot touches the ground.";

        public override string Explanation { get; } = "Signals that the right rear foot has touched the ground.";

        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>
        {
            ExposedPropertyType.Float,
        };

        public override void OnScrubbedTo(AnimationEditor go, List<ParsedEngageAnimationEvent> events)
        {
            Transform root = go.transform.GetChild(0).GetChild(0);
            if (root != null)
            {
                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.magenta;
                style.fontSize = 20;
                string labelText = "Right Rear Foot Down ⬇";
                Handles.Label(root.position, labelText, style);
            }
        }
    }


    public class RightRearFootTouchesGroundParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameMatchRule("右後足接地")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            RightRearFootTouchesGround rightRearFootTouchesGround = new RightRearFootTouchesGround
            {
                backingAnimationEvent = animEvent
            };
            return rightRearFootTouchesGround;
        }

        public override AnimationEvent Create()
        {
            AnimationEvent evt = base.Create();
            evt.floatParameter = 0.5f;
            return evt;
        }
    }
}
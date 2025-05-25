using System.Collections.Generic;
using Combat;
using UnityEditor;
using UnityEngine;

namespace DivineDragon.EngageAnimationEvents
{
    public class LeftRearFootTouchesGround : ParsedEngageAnimationEvent
    {
        public override string displayName => "Left Rear Foot Down";

        public override EventCategory category => EventCategory.Riders;

        public override string Summary { get; } = "Left rear foot touches the ground.";

        public override string Explanation { get; } = "Signals that the left rear foot has touched the ground.";

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
                style.normal.textColor = Color.cyan;
                style.fontSize = 20;
                string labelText = "Left Rear Foot Down ⬇";
                Handles.Label(root.position, labelText, style);
            }
        }
    }


    public class LeftRearFootTouchesGroundParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameMatchRule("左後足接地")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            LeftRearFootTouchesGround leftRearFootTouchesGround = new LeftRearFootTouchesGround
            {
                backingAnimationEvent = animEvent
            };
            return leftRearFootTouchesGround;
        }

        public override AnimationEvent Create()
        {
            AnimationEvent evt = base.Create();
            evt.floatParameter = 0.5f;
            return evt;
        }
    }
}
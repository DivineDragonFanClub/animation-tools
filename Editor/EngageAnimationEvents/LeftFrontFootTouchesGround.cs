using System.Collections.Generic;
using Combat;
using UnityEditor;
using UnityEngine;

namespace DivineDragon.EngageAnimationEvents
{
    public class LeftFrontFootTouchesGround : ParsedEngageAnimationEvent
    {
        public override string displayName => "Left Front Foot Down";

        public override EventCategory category => EventCategory.Riders;

        public override string Summary { get; } = "Left front foot touches the ground.";

        public override string Explanation { get; } = "Signals that the left front foot has touched the ground.";

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
                style.normal.textColor = Color.blue;
                style.fontSize = 20;
                string labelText = "Left Front Foot Down ⬇";
                Handles.Label(root.position, labelText, style);
            }
        }
    }


    public class LeftFrontFootTouchesGroundParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameMatchRule("左前足接地")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            LeftFrontFootTouchesGround leftFrontFootTouchesGround = new LeftFrontFootTouchesGround
            {
                backingAnimationEvent = animEvent
            };
            return leftFrontFootTouchesGround;
        }

        public override AnimationEvent Create()
        {
            AnimationEvent evt = base.Create();
            evt.floatParameter = 0.5f;
            return evt;
        }
    }
}
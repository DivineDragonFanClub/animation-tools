using System.Collections.Generic;
using Combat;
using UnityEditor;
using UnityEngine;

namespace DivineDragon.EngageAnimationEvents
{
    public class LeftFrontFootAscending : ParsedEngageAnimationEvent
    {
        public override string displayName => "Left Front Foot Up";

        public override EventCategory category => EventCategory.Riders;

        public override string Summary { get; } = "Left front foot is up.";

        public override string Explanation { get; } = "Signals that the left front foot has left the ground.";

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
                string labelText = "Left Front Foot Up ⬆";
                Handles.Label(root.position, labelText, style);
            }
        }
    }


    public class LeftFrontFootAscendingParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameMatchRule("左前足上昇")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            LeftFrontFootAscending leftFrontFootAscending = new LeftFrontFootAscending
            {
                backingAnimationEvent = animEvent
            };
            return leftFrontFootAscending;
        }

        public override AnimationEvent Create()
        {
            AnimationEvent evt = base.Create();
            evt.floatParameter = 0.5f;
            return evt;
        }
    }
}
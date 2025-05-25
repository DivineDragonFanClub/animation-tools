using System.Collections.Generic;
using Combat;
using UnityEditor;
using UnityEngine;

namespace DivineDragon.EngageAnimationEvents
{
    public class RightFrontFootAscending : ParsedEngageAnimationEvent
    {
        public override string displayName => "Right Front Foot Up";

        public override EventCategory category => EventCategory.Riders;

        public override string Summary { get; } = "Right front foot is up.";

        public override string Explanation { get; } = "Signals that the right front foot has left the ground.";

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
                style.normal.textColor = Color.red;
                style.fontSize = 20;
                string labelText = "Right Front Foot Up ⬆";
                Handles.Label(root.position, labelText, style);
            }
        }
    }


    public class RightFrontFootAscendingParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameMatchRule("右前足上昇")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            RightFrontFootAscending rightFrontFootAscending = new RightFrontFootAscending
            {
                backingAnimationEvent = animEvent
            };
            return rightFrontFootAscending;
        }

        public override AnimationEvent Create()
        {
            AnimationEvent evt = base.Create();
            evt.floatParameter = 0.5f;
            return evt;
        }
    }
}
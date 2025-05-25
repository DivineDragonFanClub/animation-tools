using System.Collections.Generic;
using Combat;
using UnityEditor;
using UnityEngine;

namespace DivineDragon.EngageAnimationEvents
{
    public class RightRearFootAscending : ParsedEngageAnimationEvent
    {
        public override string displayName => "Right Rear Foot Up";

        public override EventCategory category => EventCategory.Riders;

        public override string Summary { get; } = "Right rear foot is up.";

        public override string Explanation { get; } = "Signals that the right rear foot has left the ground.";

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
                string labelText = "Right Rear Foot Up ⬆";
                Handles.Label(root.position, labelText, style);
            }
        }
    }


    public class RightRearFootAscendingParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameMatchRule("右後足上昇")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            RightRearFootAscending rightRearFootAscending = new RightRearFootAscending
            {
                backingAnimationEvent = animEvent
            };
            return rightRearFootAscending;
        }

        public override AnimationEvent Create()
        {
            AnimationEvent evt = base.Create();
            evt.floatParameter = 0.5f;
            return evt;
        }
    }
}
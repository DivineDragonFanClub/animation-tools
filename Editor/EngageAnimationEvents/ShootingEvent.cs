using System.Collections.Generic;
using Combat;
using UnityEditor;
using UnityEngine;

namespace DivineDragon.EngageAnimationEvents
{
    public class ShootingEvent : ParsedEngageAnimationEvent
    {
        public override EventCategory category => EventCategory.AttackSpecifics;
        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>();
        public override string displayName => "Shooting Event";
        
        public override string Explanation { get; } = "Not yet investigated. Likely related to the moment a ranged attack is made.";
        
        public override string Summary => "Shooting event.";

        public override void OnScrubbedTo(AnimationEditor go, List<ParsedEngageAnimationEvent> events)
        {
            Transform c_trans = go.transform.GetChild(0).GetChild(0);
            if (c_trans != null)
            {
                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.red;
                style.fontSize = 20;
                string labelText = $"Shooting Event";
                Handles.Label(c_trans.position, labelText, style);
            }
        }
    }

    public class ShootingEventParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameMatchRule("発射")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            ShootingEvent shootingEvent = new ShootingEvent
            {
                backingAnimationEvent = animEvent
            };
            return shootingEvent;
        }
    }
}
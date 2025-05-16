using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Combat.EngageAnimationEvents
{
    public class AttackVoice : ParsedEngageAnimationEvent
    {
        public override string displayName => "Attack Voice";
        public override EventCategory category => EventCategory.AttackingCharacter;

        public override string Summary => $"Play the '{backingAnimationEvent.stringParameter}' attack voice clip.";
        
        public override string Explanation { get; } = "Plays the specified attack voice clip. The clip name is passed as a string parameter.";
        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>
        {
            ExposedPropertyType.String,
        };
        
        public override void OnScrubbedTo(AnimationEditor go, List<ParsedEngageAnimationEvent> events)
        {
            // Find the child object named c_neck_jnt
            Transform c_neck_jnt = go.transform.GetChild(0).GetChild(0).Find("c_spine1_jnt/c_spine2_jnt/c_neck_jnt");
            // Display a little text label at the position of the c_neck_jnt object in the editor UI
            if (c_neck_jnt != null)
            {
                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.blue;
                style.fontSize = 20;
                string labelText = $"Critical Voice: {backingAnimationEvent.intParameter}";
                Handles.Label(c_neck_jnt.position, labelText, style);
            }
        }
    }

    public class AttackVoiceParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameMatchRule("音攻撃ボイス")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            AttackVoice attackVoice = new AttackVoice
            {
                backingAnimationEvent = animEvent
            };
            return attackVoice;
        }
    }
}
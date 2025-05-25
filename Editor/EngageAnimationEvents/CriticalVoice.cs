using System.Collections.Generic;
using Combat;
using UnityEditor;
using UnityEngine;

namespace DivineDragon.EngageAnimationEvents
{
    public class CriticalVoice : ParsedEngageAnimationEvent
    {
        public override string displayName => "Critical Voice";

        public override EventCategory category => EventCategory.AttackingCharacter;

        public override string Summary { get; } = "Play critical voice clip.";

        public override string Explanation { get; } = "Play a critical voice clip. No parameters are used, as the game will automatically select the appropriate voice clip.";

        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>
        {
            ExposedPropertyType.Int,
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
                string labelText = $"Critical Voice";
                Handles.Label(c_neck_jnt.position, labelText, style);
            }
        }
    }


    public class CriticalVoiceParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameMatchRule("音必殺ボイス")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            CriticalVoice criticalVoice = new CriticalVoice
            {
                backingAnimationEvent = animEvent
            };
            return criticalVoice;
        }
        
        public override AnimationEvent Create()
        {
            var evt = base.Create();
            evt.intParameter = 1;
            return evt;
        }
    }
}
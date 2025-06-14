using System.Collections.Generic;
using Combat;
using UnityEditor;
using UnityEngine;

namespace DivineDragon.EngageAnimationEvents
{
    public class VoiceSound : ParsedEngageAnimationEvent
    {
        public override string displayName => "Voice Sound";

        public override EventCategory category => EventCategory.Sound;

        public override string Summary => $"Play the '{backingAnimationEvent.stringParameter}' voice clip.";

        public override string Explanation { get; } = "Triggers voice lines during combat animations. Common voice IDs include V_Attack_L, V_Attack_H, V_Win, V_Engage_Attack, and character-specific lines like V_LUEUR_M_Critical_06.";

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
                style.normal.textColor = Color.green;
                style.fontSize = 20;
                string labelText = $"Voice: {backingAnimationEvent.stringParameter}";
                Handles.Label(c_neck_jnt.position, labelText, style);
            }
        }
    }


    public class VoiceSoundParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameMatchRule("音ボイス")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            VoiceSound voiceSound = new VoiceSound
            {
                backingAnimationEvent = animEvent
            };
            return voiceSound;
        }

        public override AnimationEvent Create()
        {
            AnimationEvent animEvent = new AnimationEvent
            {
                functionName = "音ボイス",
                floatParameter = 0.0f,
                intParameter = 1
            };
            
            return animEvent;
        }
    }
}
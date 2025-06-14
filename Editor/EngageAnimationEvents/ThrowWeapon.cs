using System.Collections.Generic;
using Combat;
using UnityEditor;
using UnityEngine;

namespace DivineDragon.EngageAnimationEvents
{
    public class ThrowWeapon : ParsedEngageAnimationEvent
    {
        public override string displayName => "Throw Weapon";

        public override EventCategory category => EventCategory.WeaponControl;

        public override string Summary => $"Throw weapon with force {backingAnimationEvent.floatParameter}";

        public override string Explanation { get; } = "Used primarily in death animations (99%) to release weapons when dying. The float parameter controls throw force: 8.0 is standard, 5.0 is used by Arm0 animations. One exception exists in the victory animation uAnim_Rod1AM-Ft1_c300_Win.";

        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>
        {
            ExposedPropertyType.Float,
        };
        
        public override void OnScrubbedTo(AnimationEditor go, List<ParsedEngageAnimationEvent> events)
        {
            // Find the child object named c_neck_jnt
            Transform c_neck_jnt = go.transform.GetChild(0).GetChild(0).Find("c_spine1_jnt/c_spine2_jnt/c_neck_jnt");
            // Display a little text label at the position of the c_neck_jnt object in the editor UI
            if (c_neck_jnt != null)
            {
                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.red;
                style.fontSize = 20;
                string labelText = $"Throw Weapon: {backingAnimationEvent.floatParameter}";
                Handles.Label(c_neck_jnt.position, labelText, style);
            }
        }
    }


    public class ThrowWeaponParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameMatchRule("武器放り投げる")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            ThrowWeapon throwWeapon = new ThrowWeapon
            {
                backingAnimationEvent = animEvent
            };
            return throwWeapon;
        }

        public override AnimationEvent Create()
        {
            AnimationEvent animEvent = new AnimationEvent
            {
                functionName = "武器放り投げる",
                floatParameter = 8.0f
            };
            
            return animEvent;
        }
    }
}
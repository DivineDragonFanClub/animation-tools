using System.Collections.Generic;
using Combat;
using UnityEditor;
using UnityEngine;

namespace DivineDragon.EngageAnimationEvents
{
    public class WeaponTrailEnd : ParsedEngageAnimationEvent
    {
        public override string displayName => "Weapon Trail End";

        public override EventCategory category => EventCategory.WeaponControl;

        public override string Summary { get; } = "End right hand weapon trail.";

        public override string Explanation { get; } = "Marks the end of the right hand weapon trail rendering that started with Weapon Trail Begin.";

        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>();

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
                string labelText = "Weapon Trail End";
                Handles.Label(c_neck_jnt.position, labelText, style);
            }
        }
    }


    public class WeaponTrailEndParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameMatchRule("武器軌跡終")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            WeaponTrailEnd weaponTrailEnd = new WeaponTrailEnd
            {
                backingAnimationEvent = animEvent
            };
            return weaponTrailEnd;
        }
    }
}
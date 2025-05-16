using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Combat.EngageAnimationEvents
{
    public class Jump : ParsedEngageAnimationEvent
    {
        public override string displayName => "Jump";
        public override EventCategory category => EventCategory.MotionControl;

        public override string Explanation { get; } = "Jump event for the character. More investigation needed to fully understand its parameters.";

        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>
        {
            ExposedPropertyType.Float,
            ExposedPropertyType.Int
        };

        public override string Summary => 
            $"{(GetJumpIsGrounding() ? "Grounding" : "Non-grounding")}, " +
            $"duration {GetLandingTimeAfter():F2}s, " +
            $"using {GetJumpCurveType()} curve (power: {GetJumpCurvePower()})" +
            $"landing at point {GetLandingPoint():F2}";

        public override void OnScrubbedTo(AnimationEditor go, List<ParsedEngageAnimationEvent> events)
        {
            // Find the child object named c_neck_jnt
            Transform c_neck_jnt = go.transform.GetChild(0).GetChild(0).Find("c_spine1_jnt/c_spine2_jnt/c_neck_jnt");
            // Display a little text label at the position of the c_neck_jnt object in the editor UI
            if (c_neck_jnt != null)
            {
                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.magenta;
                style.fontSize = 20;
                string labelText = $"Jump: {backingAnimationEvent.floatParameter}, {backingAnimationEvent.intParameter}";
                Handles.Label(c_neck_jnt.position, labelText, style);
            }
        }
        
        private float GetLandingPoint()
        {
            var bit = Bit.GetSigned(backingAnimationEvent.intParameter, 0x10, 0);
            return bit * 0.00390625f;
        }
        
        private AnimationEvent SetLandingPoint(float value)
        {
            float clampedValue = Mathf.Clamp(value, -127f, 127f);
            clampedValue *= 256f;
            float finalValue = float.IsInfinity(clampedValue) ? -clampedValue : clampedValue;

            int newParam = Combat.Bit.Combine(backingAnimationEvent.intParameter, (int)finalValue, 0x10, 0);
            var clone = backingAnimationEvent.Clone();
            clone.intParameter = newParam;
            return clone;
        }

        private float GetLandingTimeAfter()
        {
            return backingAnimationEvent.floatParameter;
        }
        
        private bool GetJumpIsGrounding()
        {
            int param = backingAnimationEvent.intParameter;
            int result = Bit.Get(param, 1, 0x1c);
            return result != 0;
        }
        
        private AnimationEvent SetJumpIsGrounding(bool value)
        {
            int param = backingAnimationEvent.intParameter;
            int result = Bit.Combine(param, value ? 1 : 0, 1, 0x1c);
            var clone = backingAnimationEvent.Clone();
            clone.intParameter = result;
            return clone;
        }

        private int GetJumpCurvePower()
        {
            int param = backingAnimationEvent.intParameter;
            int result = Combat.Bit.Get(param, 8, 0x10);
            return result;
        }
        
        private AnimationEvent SetJumpCurvePower(int value)
        {
            int param = backingAnimationEvent.intParameter;
            int result = Combat.Bit.Combine(param, value, 8, 0x10);
            var clone = backingAnimationEvent.Clone();
            clone.intParameter = result;
            return clone;
        }
        
        public enum CurveType
        {
            Linear = 0,
            Accel = 1,
            Decel = 2,
            AccelDecel = 3,
            DecelAccel = 4,
            LinearDecel = 5,
            LinearAccel = 6,
            DecelLinear = 7,
            AccelLinear = 8
        }
        
        private CurveType GetJumpCurveType()
        {
            int param = backingAnimationEvent.intParameter;
            int result = Combat.Bit.Get(param, 4, 24);
            return (CurveType) result;
        }
        
        private AnimationEvent SetJumpCurveType(CurveType value)
        {
            int param = backingAnimationEvent.intParameter;
            int result = Combat.Bit.Combine(param, (int) value, 4, 24);
            var clone = backingAnimationEvent.Clone();
            clone.intParameter = result;
            return clone;
        }

        public override VisualElement MakeSpecialEditor(Action<ParsedEngageAnimationEvent, AnimationEvent> onSave,
            List<ParsedEngageAnimationEvent> events)
        {
            VisualElement container = new VisualElement();
            // Render a checkbox for the IsGrounding property
            var isGrounding = new Toggle("Is Grounding");
            isGrounding.value = GetJumpIsGrounding();
            isGrounding.RegisterValueChangedCallback(evt =>
            {
                onSave(this, SetJumpIsGrounding(evt.newValue));
            });
            
            container.Add(isGrounding);
            
            // Render a float field for the LandingPoint property
            var landingPoint = new FloatField("Landing Point")
            {
                isDelayed = true,
                value = GetLandingPoint()
            };
            landingPoint.RegisterValueChangedCallback(evt =>
            {
                onSave(this, SetLandingPoint(evt.newValue));
            });
            
            container.Add(landingPoint);
            
            // Render a float field for the LandingTimeAfter property, which is just simply a float
            var landingTimeAfter = new FloatField("Landing Time After (Duration)")
            {
                isDelayed = true,
                value = GetLandingTimeAfter()
            };
            landingTimeAfter.RegisterValueChangedCallback(evt =>
            {
                var clone = backingAnimationEvent.Clone();
                clone.floatParameter = evt.newValue;
                onSave(this, clone);
            });
            
            container.Add(landingTimeAfter);
            
            // Render dropdown for the CurveType property
            var curveType = new EnumField("Curve Type", GetJumpCurveType());
            curveType.Init(GetJumpCurveType());
            curveType.RegisterValueChangedCallback(evt =>
            {
                onSave(this, SetJumpCurveType((CurveType) evt.newValue));
            });
            
            container.Add(curveType);
            
            // Render an int field for the CurvePower property
            var curvePower = new IntegerField("Curve Power")
            {
                isDelayed = true,
                value = GetJumpCurvePower()
            };
            curvePower.RegisterValueChangedCallback(evt =>
            {
                onSave(this, SetJumpCurvePower(evt.newValue));
            });
            
            container.Add(curvePower);
            
            return container;
        }
    }

    public class JumpParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameMatchRule("ジャンプ")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            Jump jump = new Jump
            {
                backingAnimationEvent = animEvent
            };
            return jump;
        }
    }
}
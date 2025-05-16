using System;
using System.Collections.Generic;
using Combat;
using UnityEngine;
using UnityEngine.UIElements;

namespace DivineDragon.EngageAnimationEvents
{
    // void Combat.ActionAttack$$EnterApproach(Combat_ActionAttack_o *__this,MethodInfo *method)
    // Some guesses from that - need to move close enough to attack, but not too close.
    // I think that's what this is for.
    public class AttackPossibleRange : ParsedEngageAnimationEvent
    {
        public override EventCategory category => EventCategory.AttackSpecifics;

        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>
        {
            ExposedPropertyType.Int,
            ExposedPropertyType.Float
        };

        public override string displayName => "Attack Possible Range";

        public override string Explanation { get; } = "Controls at what distances the game will try to use this animation.";
        
        public override string Summary => $"Near Range: {Math.Abs(backingAnimationEvent.intParameter / 10000.0f)}, " +
                                           $"Far Range: {Math.Abs(backingAnimationEvent.floatParameter)}";
        public override VisualElement MakeSpecialEditor(Action<ParsedEngageAnimationEvent, AnimationEvent> onSave,
            List<ParsedEngageAnimationEvent> events)
        {
            // _AttackNearRange_k__BackingField
            // _AttackFarRange_k__BackingField
            float attackNearRange = Math.Abs(backingAnimationEvent.intParameter / 10000.0f);
            float attackFarRange = Math.Abs(backingAnimationEvent.floatParameter);
            // No idea how these values are actually used.
            var container = new VisualElement();
            // Just display them for now
            container.Add(new Label($"Attack Far Range: {attackFarRange}"));
            container.Add(new Label($"Attack Near Range: {attackNearRange}"));
            return container;
        }

        public override void OnScrubbedTo(AnimationEditor go, List<ParsedEngageAnimationEvent> events)
        {
            
        }
    }

    public class AttackPossibleRangeParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameMatchRule("攻撃可能範囲")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            AttackPossibleRange attackPossibleRange = new AttackPossibleRange
            {
                backingAnimationEvent = animEvent
            };
            return attackPossibleRange;
        }
    }
}
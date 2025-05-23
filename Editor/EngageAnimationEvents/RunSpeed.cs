using System.Collections.Generic;
using Combat;
using UnityEngine;

namespace DivineDragon.EngageAnimationEvents
{
    public class RunSpeed : ParsedEngageAnimationEvent
    {
        public override string displayName => "Run Speed";

        public override EventCategory category => EventCategory.MotionControl;

        public override string Summary => $"Run speed set to {backingAnimationEvent.floatParameter}.";

        public override string Explanation { get; } = "Sets the character's run speed. Optional, and only used in RunLoops animations.";

        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>
        {
            ExposedPropertyType.Float
        };
    }


    public class RunSpeedParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameMatchRule("Run速度")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            RunSpeed runSpeed = new RunSpeed
            {
                backingAnimationEvent = animEvent
            };
            return runSpeed;
        }
    }
}

// todo: Combat.Character$$SetupForCombat
        //     pUVar8 = UnityEngine.AnimationClip$$get_events
        //                    ((a->fields)._Clip_k__BackingField,(MethodInfo *)0x0);
        // __this_02 = Combat.RuntimeAnimationEventUtility$$FindByName(pUVar8,Run速度,(MethodInfo *)0x0);
        // if (__this_02 != (UnityEngine_AnimationEvent_o *)0x0) {
        //   if ((DAT_710631b460 & 1) == 0) {
        //     thunk_FUN_710043f8a4(&Method$UnityEngine.Component.GetComponent<CharacterMove>());
        //     DAT_710631b460 = 1;
        //   }
        //   if ((__this->fields).cached_Move == false) {
        //     (__this->fields).cached_Move = true;
        //     pAVar9 = UnityEngine.Component$$GetComponent<>
        //                        ((UnityEngine_Component_o *)__this,
        //                         Method$UnityEngine.Component.GetComponent<CharacterMove>());
        //     (__this->fields)._Move = (Combat_CharacterMove_o *)pAVar9;
        //     thunk_FUN_7100471450(&(__this->fields)._Move);
        //   }
        //   __this_04 = (__this->fields)._Move;
        //   fVar20 = UnityEngine.AnimationEvent$$get_floatParameter(__this_02,(MethodInfo *)0x0);
        //   Combat.CharacterMove$$SetMaxRunSpeed(__this_04,fVar20,(MethodInfo *)0x0);

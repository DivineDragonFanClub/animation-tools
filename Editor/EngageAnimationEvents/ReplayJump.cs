using System.Collections.Generic;
using UnityEngine;
using Combat;

namespace DivineDragon.EngageAnimationEvents
{
    public class ReplayJump : ParsedEngageAnimationEvent
    {
        public override string displayName => "Replay Jump";

        public override EventCategory category => EventCategory.Uncategorized;

        public override string Summary => $"Replay Jump (int: {backingAnimationEvent.intParameter})";

        public override string Explanation { get; } = "Found exclusively in Marth's Special1 animations (uAnim_Mar1AM-Sw1_c000_Special1 and uAnim_Mar1AF-Sw1_c000_Special1). Appears to be related to animation looping control based on decompiled code analysis. The integer parameter's exact purpose is unknown - curious users can experiment or check the game code for more details.";

        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>
        {
            ExposedPropertyType.Int,
        };

        public override void OnScrubbedTo(AnimationEditor go, List<ParsedEngageAnimationEvent> events)
        {
            // no visualizations yet
        }
    }


    public class ReplayJumpParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameMatchRule("再生ジャンプ")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            ReplayJump replayJump = new ReplayJump
            {
                backingAnimationEvent = animEvent
            };
            return replayJump;
        }

        public override AnimationEvent Create()
        {
            AnimationEvent animEvent = new AnimationEvent
            {
                functionName = "再生ジャンプ",
                intParameter = 7
            };

            return animEvent;
        }
    }
}
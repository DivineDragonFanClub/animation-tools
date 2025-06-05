using System.Collections.Generic;
using UnityEngine;
using Combat;

namespace DivineDragon.EngageAnimationEvents
{
    public class ReplayLanding : ParsedEngageAnimationEvent
    {
        public override string displayName => "Replay Landing";

        public override EventCategory category => EventCategory.Uncategorized;

        public override string Summary => $"Replay Landing (int: {backingAnimationEvent.intParameter})";

        public override string Explanation { get; } = "Found exclusively in Marth's Special1 animations (uAnim_Mar1AM-Sw1_c000_Special1 and uAnim_Mar1AF-Sw1_c000_Special1). Based on decompiled code, appears to mark a position where the animation can jump back to. The exact mechanics and the purpose of the integer parameter are unclear.";

        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>
        {
            ExposedPropertyType.Int,
        };

        public override void OnScrubbedTo(AnimationEditor go, List<ParsedEngageAnimationEvent> events)
        {
            // no visualizations yet
        }
    }


    public class ReplayLandingParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameMatchRule("再生着地")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            ReplayLanding replayLanding = new ReplayLanding
            {
                backingAnimationEvent = animEvent
            };
            return replayLanding;
        }

        public override AnimationEvent Create()
        {
            AnimationEvent animEvent = new AnimationEvent
            {
                functionName = "再生着地",
                intParameter = 7
            };

            return animEvent;
        }
    }
}
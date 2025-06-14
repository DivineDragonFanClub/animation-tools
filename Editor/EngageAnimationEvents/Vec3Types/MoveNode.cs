using System.Collections.Generic;
using Combat;
using UnityEngine;

namespace DivineDragon.EngageAnimationEvents.Vec3Types
{
    public class MoveNode : QuantizedEvent
    {
        public override string displayName => "Move Node";

        public override EventCategory category => EventCategory.MotionControl;

        public override string Summary => $"{backingAnimationEvent.stringParameter}: (x: {quantizedPosition.x:F2}, y: {quantizedPosition.y:F2}, z: {quantizedPosition.z:F2})";

        public override string Explanation { get; } = "Moves a specific node by the specified Vec3 offset. The game uses 'magic1', 'magic3', 'magic4' and 'l_wpn1_loc'.";

        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>
        {
            ExposedPropertyType.Float,
            ExposedPropertyType.Int,
            ExposedPropertyType.String,
        };
    }


    public class MoveNodeParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameMatchRule("ノード移動")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            MoveNode moveNode = new MoveNode
            {
                backingAnimationEvent = animEvent
            };
            return moveNode;
        }
    }
}
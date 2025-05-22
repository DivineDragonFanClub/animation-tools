using System.Collections.Generic;
using Combat;
using DivineDragon.EngageAnimations;
using UnityEditor;
using UnityEngine;

namespace DivineDragon.EngageAnimationEvents.Vec3Types
{
    public class HitPosition : QuantizedEvent
    {
        public override string displayName => "Hit Position";
        public override string Explanation { get; } = "Marks the hit position of this attack. Not yet fully understood.";

        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>
        {
            ExposedPropertyType.Float,
            ExposedPropertyType.Int,
        };



        
        public override EventCategory category => EventCategory.AttackSpecifics;
        
        public override void AlwaysRender(AnimationEditor go, List<ParsedEngageAnimationEvent> events)
        {
            base.AlwaysRender(go, events);
            // Additional logic for rendering hit direction.
            // TBD where it should actually go.
            
            foreach (var parsedEvent in events)
            {
                if (parsedEvent is HitDirection hitDirection)
                {
                    var hitDirectionQuant = Quantizer.FItoVec3(hitDirection.backingAnimationEvent.floatParameter,
                        hitDirection.backingAnimationEvent.intParameter);
                    Handles.color = Color.blue;

                    // Draw the line
                    Handles.DrawLine(quantizedPosition, hitDirectionQuant);

                    // Draw arrow cap at the end
                    Vector3 direction = (hitDirectionQuant - quantizedPosition).normalized;
                    float size = HandleUtility.GetHandleSize(hitDirectionQuant) * 0.2f; // Arrow size proportional to handle size
                    Handles.ConeHandleCap(0, hitDirectionQuant - direction * size * 0.5f, 
                        Quaternion.LookRotation(direction), size, EventType.Repaint);

                    Handles.color = Color.white;
                    Handles.Label(hitDirectionQuant, "Hit Direction");
                }
            }
        }
    }
    
    public class HitPositionParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameStringParameterMatchRule("Vec3", "命中位置")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            HitPosition hitDirection = new HitPosition
            {
                backingAnimationEvent = animEvent
            };
            return hitDirection;
        }
    }
}
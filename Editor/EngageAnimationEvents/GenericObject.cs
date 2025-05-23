using System.Collections.Generic;
using Combat;
using UnityEngine;

namespace DivineDragon.EngageAnimationEvents
{
    public class GenericObject : ParsedEngageAnimationEvent
    {
        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>
        {
            ExposedPropertyType.String,
            ExposedPropertyType.ObjectReference
        };
        
        public override EventCategory category => EventCategory.WeaponControl;

        public override string displayName => "Generic Object";

        public override string Explanation =>
            "In the game's files, this is only ever called with the string PC as the name, and an object reference that is PrefetchedCurve_Bridge.";
        
        public override string Summary => $"Create object \"{backingAnimationEvent.stringParameter}\" from \"{backingAnimationEvent.objectReferenceParameter?.name}\".";

    }

    public class GenericObjectParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameMatchRule("汎用Object")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            GenericObject genericObject = new GenericObject
            {
                backingAnimationEvent = animEvent
            };
            return genericObject;
        }
        
        public override AnimationEvent Create()
        {
            var evt = base.Create();
            evt.stringParameter = "PC";
            return evt;
        }
    }
}

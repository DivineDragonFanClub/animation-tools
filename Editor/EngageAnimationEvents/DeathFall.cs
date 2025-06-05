using System;
using System.Collections.Generic;
using Combat;
using DivineDragon.EngageAnimations;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace DivineDragon.EngageAnimationEvents
{
    public class DeathFall : ParsedEngageAnimationEvent
    {
        private Vector3 _handlesPosition;
        private Quaternion _handlesRotation;
        private bool _initialized;
        private bool _showEditor;
        
        public override string displayName => "Death Fall";
        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>
        {
            ExposedPropertyType.Int, 
        };
        public override string Summary => $"Fall Direction (x: {deathFallDirection.x:F3}, z: {deathFallDirection.z:F3})";
        
        protected FXZ deathFallDirection => Quantizer.ItoFXZ(backingAnimationEvent.intParameter);
        
        public override void AlwaysRender(AnimationEditor go, List<ParsedEngageAnimationEvent> events)
        {
            var fallPosition = new Vector3(deathFallDirection.x, 0, deathFallDirection.z);
            
            if (!_showEditor)
            {
                Handles.color = Color.red;
                Handles.SphereHandleCap(0, fallPosition, Quaternion.identity, 0.1f, EventType.Repaint);
                Handles.color = Color.white;
                Handles.Label(fallPosition, "Death Fall");
                
                // Draw arrow showing fall direction
                Handles.color = Color.red;
                Handles.ArrowHandleCap(0, Vector3.zero, Quaternion.LookRotation(fallPosition), 
                    fallPosition.magnitude, EventType.Repaint);
            }
            else
            {
                // Show old position in transparent pink
                Handles.color = new Color(1, 0, 1, 0.5f);
                Handles.SphereHandleCap(0, fallPosition, Quaternion.identity, 0.1f, EventType.Repaint);
                Handles.color = Color.white;
                Handles.Label(fallPosition + new Vector3(0, 0.2f, 0), "Old Death Fall");
            }

            if (!_initialized)
            {
                _handlesPosition = fallPosition;
                _handlesRotation = Quaternion.identity;
                _initialized = true;
            }
            
            if (_showEditor) 
            { 
                Handles.TransformHandle(ref _handlesPosition, ref _handlesRotation);
                Handles.Label(_handlesPosition + new Vector3(0, -0.2f, 0), "New Death Fall");
                
                // Draw new position
                Handles.color = Color.green;
                Handles.SphereHandleCap(0, _handlesPosition, Quaternion.identity, 0.1f, EventType.Repaint);
                
                // Draw new arrow
                Handles.color = Color.green;
                Handles.ArrowHandleCap(0, Vector3.zero, Quaternion.LookRotation(_handlesPosition), 
                    _handlesPosition.magnitude, EventType.Repaint);
            }
        }
        
        public override VisualElement MakeSpecialEditor(Action<ParsedEngageAnimationEvent, AnimationEvent> onSave,
            List<ParsedEngageAnimationEvent> events)
        {
            var container = new VisualElement();
            
            container.Add(new Label($"Death Fall Direction: x={deathFallDirection.x:F2}, z={deathFallDirection.z:F2}"));
            container.Add(new Label($"Raw Integer: {backingAnimationEvent.intParameter}"));
            
            var saveButton = new Button(() =>
            {
                // Convert back to integer using FXZtoI
                var newFXZ = new FXZ(_handlesPosition.x, _handlesPosition.z);
                int newInt = Quantizer.FXZtoI(newFXZ);
                
                var clone = backingAnimationEvent.Clone();
                clone.intParameter = newInt;
                onSave(this, clone);
            });
            saveButton.text = "Save New Death Fall Direction";
            container.Add(saveButton);
            
            saveButton.style.display = _showEditor ? DisplayStyle.Flex : DisplayStyle.None;
            
            var toggleButton = new Button();
            toggleButton.clicked += () =>
            {
                _showEditor = !_showEditor;
                saveButton.style.display = _showEditor ? DisplayStyle.Flex : DisplayStyle.None;
                toggleButton.text = _showEditor ? "Cancel" : "Show Gizmo Editor in Scene";
                
                if (!_showEditor)
                {
                    _handlesPosition = new Vector3(deathFallDirection.x, 0, deathFallDirection.z);
                    _handlesRotation = Quaternion.identity;
                }
            };
            
            toggleButton.text = "Show Gizmo Editor in Scene";
            container.Add(toggleButton);
            
            return container;
        }
    }

    public class DeathFallParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameMatchRule("死亡落下")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            DeathFall deathFall = new DeathFall
            {
                backingAnimationEvent = animEvent
            };
            return deathFall;
        }

        public override AnimationEvent Create()
        {
            AnimationEvent animEvent = new AnimationEvent
            {
                functionName = "死亡落下",
            };
            
            return animEvent;
        }
    }
}
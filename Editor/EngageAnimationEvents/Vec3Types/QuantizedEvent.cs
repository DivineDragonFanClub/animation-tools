using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Combat.EngageAnimationEvents
{
    // Todo: generalize to FXZtoI 
    public abstract class QuantizedEvent: ParsedEngageAnimationEvent
    {
        private Vector3 _handlesPosition;
        private Quaternion _handlesRotation;
        private bool _initialized;
        private bool _showEditor;
        
        public override string Summary => $"{quantizedPosition}";
        
        // for these, it's better to use the function name + string parameter
        public override string originalName => $"{backingAnimationEvent.functionName}, {backingAnimationEvent.stringParameter}";

        protected virtual Vector3 quantizedPosition => Quantizer.FItoVec3(backingAnimationEvent.floatParameter,
            backingAnimationEvent.intParameter);
        
        public override void AlwaysRender(AnimationEditor go, List<ParsedEngageAnimationEvent> events)
        {
            // use the quantized position
            // to draw a transform widget at the position
            if (!_showEditor)
            {
                Handles.color = Color.red;
                Handles.SphereHandleCap(0, quantizedPosition, Quaternion.identity, 0.1f, EventType.Repaint);
                Handles.color = Color.white;
                Handles.Label(quantizedPosition, displayName);
            }
            else
            {
                // still draw the sphere, but make it transparent like a pink, and label it as the old position
                Handles.color = new Color(1, 0, 1, 0.5f);
                Handles.SphereHandleCap(0, quantizedPosition, Quaternion.identity, 0.1f, EventType.Repaint);
                Handles.color = Color.white;
                // move the label up a bit
                Handles.Label(quantizedPosition + new Vector3(0, 0.2f, 0), $"Old {displayName}");
            }

            if (!_initialized)
            {
                _handlesPosition = quantizedPosition;
                _handlesRotation = Quaternion.identity;
                _initialized = true;
            }
            if (_showEditor) { 
                Handles.TransformHandle(ref _handlesPosition, ref _handlesRotation);
                // move the label down a bit
                Handles.Label(_handlesPosition + new Vector3(0, -0.2f, 0), $"New {displayName}");
                // draw a nice dot at the new position
                Handles.color = Color.green;
                Handles.SphereHandleCap(0, _handlesPosition, Quaternion.identity, 0.1f, EventType.Repaint);
            }
        }
        
        public override VisualElement MakeSpecialEditor(Action<ParsedEngageAnimationEvent, AnimationEvent> onSave,
            List<ParsedEngageAnimationEvent> events)
        {
            var container = new VisualElement();
            // use quantizer to output the position
            container.Add(new Label($"{displayName}: {quantizedPosition}"));
            var saveButton = new Button(() =>
            {
                var quantResultBackToFi = Quantizer.Vec3toFI(_handlesPosition);
                var clone = backingAnimationEvent.Clone();
                clone.floatParameter = quantResultBackToFi.Item1;
                clone.intParameter = quantResultBackToFi.Item2;
                onSave(this, clone);
            });
            saveButton.text = $"Save New {displayName}";
            container.Add(saveButton);
            // show and hide this save button based on the showEditor bool, dynamically
            saveButton.style.display = _showEditor ? DisplayStyle.Flex : DisplayStyle.None;
            var toggleButton = new Button();
            toggleButton.clicked += () =>
            {
                _showEditor = !_showEditor;
                saveButton.style.display = _showEditor ? DisplayStyle.Flex : DisplayStyle.None;
                // say cancel if showEditor is true
                toggleButton.text = _showEditor ? "Cancel" : "Show Editor";
                // if canceling, reset the handlesPosition and handlesRotation
                if (!_showEditor)
                {
                    _handlesPosition = quantizedPosition;
                    _handlesRotation = Quaternion.identity;
                }
            };
            
            toggleButton.text = "Show Editor";
            container.Add(toggleButton);
            return container;
        }
    }
}
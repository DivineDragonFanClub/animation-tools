using System;
using System.Collections.Generic;
using Combat;
using DivineDragon.EngageAnimationEvents.Vec3Types;
using DivineDragon.EngageAnimations;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace DivineDragon.EngageAnimationEvents
{
    public class Hit : ParsedEngageAnimationEvent
    {
        public override EventCategory category => EventCategory.AttackSpecifics;

        public override HashSet<ExposedPropertyType> exposedProperties => new HashSet<ExposedPropertyType>
        {
            ExposedPropertyType.String,
            ExposedPropertyType.Float,
            ExposedPropertyType.Int
        };

        public override string displayName => "Hit";
        
        public override string Explanation { get; } = "The enemy is getting hit here. Check the game's files for usage examples.";
        
        // Write a short summary of the event
        public override string Summary => $"{(GetIsDummy() ? "Dummy " : "")}" + 
                                          $"{GetSlashType()} attack with hitstop {GetHitstopScale()}, " +
                                          $"hand type {GetHitHandType()}, " +
                                          $"direction {GetSlashDirection().normalized:F2}";

        public override void OnScrubbedTo(AnimationEditor go, List<ParsedEngageAnimationEvent> events)
        {
            foreach (var parsedEvent in events)
            {
                if (parsedEvent is HitPosition hitPosition)
                {
                    var hitPositionQuant = Quantizer.FItoVec3(hitPosition.backingAnimationEvent.floatParameter,
                        hitPosition.backingAnimationEvent.intParameter);
                    var offset = new Vector3(0.1f, 0.2f, 0); // Adjust the offset values as needed

                    GUIStyle style = new GUIStyle();
                    style.normal.textColor = Color.red;
                    style.fontSize = 20; // Adjust the font size as needed

                    Handles.Label(hitPositionQuant + offset, "Hit!", style);
                }
            }
        }

        // Namespace: Combat
        public enum HitstopScale
        {
            One = 0,
            Half = 1,
            Quarter = 2,
            Zero = 3
        }

        private HitstopScale GetHitstopScale()
        {
            return (HitstopScale)Bit.Get(backingAnimationEvent.intParameter, 2, 0x15);
        }

        public enum SlashType
        {
            Slash = 0,
            Sting = 1,
            Blow = 2,
            Undisplay = 3,
            Magic = 4
        }

        private SlashType GetSlashType()
        {
            return (SlashType)Bit.Get(backingAnimationEvent.intParameter, 2, 0x18);
        }

        private Vector3 GetSlashDirection()
        {
            return IntVec.Decode(backingAnimationEvent.intParameter);
        }

        private bool GetIsDummy()
        {
            return Bit.Get(backingAnimationEvent.intParameter, 1, 0x1c) != 0;
        }

        private int GetHitHandType()
        {
            return Bit.Get(backingAnimationEvent.intParameter, 2, 0x1a);
        }

        public override VisualElement MakeSpecialEditor(Action<ParsedEngageAnimationEvent, AnimationEvent> onSave,
            List<ParsedEngageAnimationEvent> events)
        {
            VisualElement container = new VisualElement();

            // Render a checkbox for the IsDummy property
            var isDummy = new Toggle("Is Dummy")
            {
                value = GetIsDummy()
            };
            isDummy.RegisterValueChangedCallback(evt => { onSave(this, SetIsDummy(evt.newValue)); });
            container.Add(isDummy);

            // Render an EnumField for the HitstopScale property
            var hitstopScale = new EnumField("Hitstop Scale", GetHitstopScale());
            hitstopScale.RegisterValueChangedCallback(evt =>
            {
                onSave(this, SetHitstopScale((HitstopScale)evt.newValue));
            });

            container.Add(hitstopScale);

            // Render input for hand hit type
            var hitHandType = new IntegerField("Hit Hand Type")
            {
                value = GetHitHandType(),
                isDelayed = true
            };
            hitHandType.RegisterValueChangedCallback(evt => { onSave(this, SetHitHandType(evt.newValue)); });
            container.Add(hitHandType);

            // render input for slash type
            var slashType = new EnumField("Slash Type", GetSlashType());
            slashType.RegisterValueChangedCallback(evt => { onSave(this, SetSlashType((SlashType)evt.newValue)); });
            container.Add(slashType);

            // Get the current slash direction vector
            Vector3 currentDirection = GetSlashDirection();
            // hide foldout
            var foldout = new Foldout
            {
                text = "Slash Direction (Normalized)",
                value = false
            };
            var xInput = new FloatField("X")
            {
                value = currentDirection.x,
                isDelayed = true
            };

            var yInput = new FloatField("Y")
            {
                value = currentDirection.y,
                isDelayed = true
            };

            var zInput = new FloatField("Z")
            {
                value = currentDirection.z,
                isDelayed = true
            };

            foldout.Add(xInput);
            foldout.Add(yInput);
            foldout.Add(zInput);

            void UpdateDirection()
            {
                Vector3 newDirection = new Vector3(xInput.value, yInput.value, zInput.value);
                SetSlashDirection(newDirection);
                onSave(this, SetSlashDirection(newDirection));
            }

            xInput.RegisterValueChangedCallback(evt => UpdateDirection());
            yInput.RegisterValueChangedCallback(evt => UpdateDirection());
            zInput.RegisterValueChangedCallback(evt => UpdateDirection());
            // Add a section for the slash direction

            // Add the 3D editor toggle
            var toggleButton = new Button();
            toggleButton.clicked += () =>
            {
                _showDirectionEditor = !_showDirectionEditor;
                toggleButton.text = _showDirectionEditor ? "Hide Direction Editor" : "Edit Direction in Scene";

                // Reset the handles when canceling
                if (!_showDirectionEditor)
                {
                    Vector3 slashDir = GetSlashDirection().normalized;

                    // Find hit position
                    Vector3 hitPos = Vector3.zero;
                    foreach (var evt in events)
                    {
                        if (evt is HitPosition hitPosition)
                        {
                            hitPos = Quantizer.FItoVec3(hitPosition.backingAnimationEvent.floatParameter,
                                hitPosition.backingAnimationEvent.intParameter);
                            break;
                        }
                    }

                    _handlesPosition = hitPos + slashDir;
                    _handlesRotation = Quaternion.LookRotation(slashDir);
                }
            };
            toggleButton.text = "Edit Direction in Scene";
            container.Add(toggleButton);

            // Add a save button for the scene handle edits
            var saveDirectionButton = new Button(() =>
            {
                // Find hit position
                Vector3 hitPos = Vector3.zero;
                foreach (var evt in events)
                {
                    if (evt is HitPosition hitPosition)
                    {
                        hitPos = Quantizer.FItoVec3(hitPosition.backingAnimationEvent.floatParameter,
                            hitPosition.backingAnimationEvent.intParameter);
                        break;
                    }
                }

                // Calculate the normalized direction
                Vector3 newDirection = (_handlesPosition - hitPos).normalized;
                onSave(this, SetSlashDirection(newDirection));
            });
            saveDirectionButton.text = "Save Direction";
            saveDirectionButton.style.display = DisplayStyle.None;
            container.Add(saveDirectionButton);
            
            container.Add(foldout);

            // Show the save button only when the editor is active
            toggleButton.clicked += () =>
            {
                saveDirectionButton.style.display = _showDirectionEditor ? DisplayStyle.Flex : DisplayStyle.None;
            };

            return container;
        }

        private AnimationEvent SetSlashDirection(Vector3 evtNewValue)
        {
            int encodedDirection = IntVec.Encode(evtNewValue);

            int param = backingAnimationEvent.intParameter;

            int result = Bit.Combine(param, encodedDirection, 0x15, 0);

            var clone = backingAnimationEvent.Clone();
            clone.intParameter = result;
            return clone;
        }

        private AnimationEvent SetSlashType(SlashType evtNewValue)
        {
            int param = backingAnimationEvent.intParameter;
            int result = Bit.Combine(param, (int)evtNewValue, 2, 0x18);
            var clone = backingAnimationEvent.Clone();
            clone.intParameter = result;
            return clone;
        }

        private AnimationEvent SetHitHandType(int evtNewValue)
        {
            int param = backingAnimationEvent.intParameter;
            int result = Bit.Combine(param, evtNewValue, 2, 0x1a);
            var clone = backingAnimationEvent.Clone();
            clone.intParameter = result;
            return clone;
        }

        private AnimationEvent SetHitstopScale(HitstopScale evtNewValue)
        {
            int param = backingAnimationEvent.intParameter;
            int result = Bit.Combine(param, (int)evtNewValue, 2, 0x15);
            var clone = backingAnimationEvent.Clone();
            clone.intParameter = result;
            return clone;
        }

        private AnimationEvent SetIsDummy(bool evtNewValue)
        {
            int param = backingAnimationEvent.intParameter;
            int result = Bit.Combine(param, evtNewValue ? 1 : 0, 1, 0x1c);
            var clone = backingAnimationEvent.Clone();
            clone.intParameter = result;
            return clone;
        }

        private Vector3 _handlesPosition;
        private Quaternion _handlesRotation;
        private bool _showDirectionEditor;
        private bool _handlesInitialized;

        public override void AlwaysRender(AnimationEditor go, List<ParsedEngageAnimationEvent> events)
        {
            // Find the hit position from events list
            Vector3 hitPos = Vector3.zero;
            foreach (var evt in events)
            {
                if (evt is HitPosition hitPosition)
                {
                    hitPos = Quantizer.FItoVec3(hitPosition.backingAnimationEvent.floatParameter,
                        hitPosition.backingAnimationEvent.intParameter);
                    break;
                }
            }

            // Draw the current slash direction
            Vector3 slashDir = GetSlashDirection().normalized;

            // Draw a line showing the current slash direction
            Handles.color = Color.red;
            Handles.DrawLine(hitPos, hitPos + slashDir, 2f);
            Handles.Label(hitPos + slashDir, "Hit Direction (For Hit Event at " + backingAnimationEvent.time + ")");

            // Draw arrowhead
            Handles.ConeHandleCap(0, hitPos + slashDir,
                Quaternion.LookRotation(slashDir), 0.1f, EventType.Repaint);


            // Initialize handle position if needed
            if (!_handlesInitialized)
            {
                _handlesPosition = hitPos + slashDir;
                _handlesRotation = Quaternion.identity;
                _handlesInitialized = true;
            }

            if (_showDirectionEditor)
            {
                // Draw the editable handle
                Handles.color = Color.green;

                // Show the position handle
                _handlesPosition = Handles.PositionHandle(_handlesPosition, _handlesRotation);
                Vector3 newDirection = (_handlesPosition - hitPos).normalized;

                _handlesRotation = Quaternion.LookRotation(newDirection);
                // Draw the proposed new direction
                Handles.color = Color.green;
                Handles.DrawLine(hitPos, _handlesPosition, 2f);
                Handles.ConeHandleCap(0, _handlesPosition, _handlesRotation, 0.1f, EventType.Repaint);
                Handles.Label(_handlesPosition, "New Direction");
            }
        }
    }

    public class HitParser : EngageAnimationEventParser<ParsedEngageAnimationEvent>
    {
        public override MatchRule[] matchRules => new MatchRule[]
        {
            new FunctionNameMatchRule("命中")
        };

        public override ParsedEngageAnimationEvent ParseFrom(AnimationEvent animEvent)
        {
            Hit hit = new Hit
            {
                backingAnimationEvent = animEvent
            };
            return hit;
        }
    }
}
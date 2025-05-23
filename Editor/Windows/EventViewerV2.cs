using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace DivineDragon.Windows
{
    public class EventViewerV2 : AnimationEditorInspectorHelper
    {
        private const string ScrollInTandemKey = "EventViewerV2_ScrollInTandem";
        private const string ScrubToEventKey = "EventViewerV2_ScrubToEvent";
        private const string ShowDifferentialTimestampKey = "EventViewerV2_ShowDifferentialTimestamp";
        private const string DeveloperModeKey = "EventViewerV2_DeveloperMode";

        [MenuItem("Divine Dragon/Animation Tools/Event Viewer")]
        public static void ShowExample()
        {
            EventViewerV2 wnd = GetWindow<EventViewerV2>();
            wnd.titleContent = new GUIContent("Event Viewer");
        }

        public void scrollToEventCurrentTime()
        {
            scrollToTime(GetAnimationWindow().time);
        }

        public void scrollToTime(float time)
        {
            // find the event in eventItems that is closest to the current time
            float closestTime = float.MaxValue;
            // loop through all the events
            int closestIndex = -1;
            foreach (var item in listView.itemsSource)
            {
                if (item is ParsedEngageAnimationEvent parsedEvent)
                {
                    float deltaTime = Mathf.Abs(parsedEvent.backingAnimationEvent.time - time);
                    if (deltaTime < closestTime)
                    {
                        closestTime = deltaTime;
                        closestIndex = listView.itemsSource.IndexOf(item);
                    }
                }
            }

            // scroll to the closest event
            if (closestIndex != -1)
            {
                listView.ScrollToItem(closestIndex);
            }
        }

        public void scrollToUuid(string uuid)
        {
            int foundIndex = -1;
            foreach (var item in listView.itemsSource)
            {
                if (item is ParsedEngageAnimationEvent parsedEvent)
                {
                    if (parsedEvent.Uuid == uuid)
                    {
                        foundIndex = listView.itemsSource.IndexOf(item);
                        break;
                    }
                }
            }

            // scroll to the event
            if (foundIndex != -1)
            {
                listView.ScrollToItem(foundIndex);
            }
        }

        private bool scrollInTandem = false;
        private bool scrubToEvent = true;
        private bool showDifferentialTimestamp = false;
        private bool developerMode = false;

        private void UpdateButtonToggleState(Button button, bool isActive, string textPrefix)
        {
            button.text = isActive ? $"{textPrefix}: ON" : $"{textPrefix}: OFF";
            // Optional: Change color to more clearly indicate state
            button.style.backgroundColor = isActive ? new StyleColor(new Color(0.4f, 0.6f, 0.4f, 0.5f)) : new StyleColor(StyleKeyword.Initial);
        }

        private void LoadPreferences()
        {
            scrollInTandem = EditorPrefs.GetBool(ScrollInTandemKey, false);
            scrubToEvent = EditorPrefs.GetBool(ScrubToEventKey, true);
            showDifferentialTimestamp = EditorPrefs.GetBool(ShowDifferentialTimestampKey, false);
            developerMode = EditorPrefs.GetBool(DeveloperModeKey, false);
        }

        private void SavePreferences()
        {
            EditorPrefs.SetBool(ScrollInTandemKey, scrollInTandem);
            EditorPrefs.SetBool(ScrubToEventKey, scrubToEvent);
            EditorPrefs.SetBool(ShowDifferentialTimestampKey, showDifferentialTimestamp);
            EditorPrefs.SetBool(DeveloperModeKey, developerMode);
        }

        protected override void OnUnderlyingAnimationClipChanged()
        {
            // Dirty hack to re-get the animation editor
            base.OnUnderlyingAnimationClipChanged();
            if (root == null)
            {
                return;
            }

            root.Clear();
            var forceRefreshButton = new Button(() => { UpdateInspector(scrollView); })
            {
                text = "Force Refresh",
                tooltip = "Force a refresh of the event list in case it bugs out"
            };
            root.Add(forceRefreshButton);

            scrollView = new VisualElement();

            root.Add(scrollView);

            AnimationClipWatcher.OnClipEventsChanged += OnClipChanged;
            Undo.undoRedoPerformed += UpdateInspectorCall;
            UpdateInspector(scrollView);
        }

        private void OnClipChanged(AnimationClip changedClip, HashSet<string> addedEventUuids)
        {
            if (changedClip == getAttachedClip())
            {
                var latestParsedEvents = AnimationClipWatcher.GetParsedEvents(changedClip);
                listView.itemsSource = latestParsedEvents;

                // If exactly one new event was added, select and scroll to it
                if (addedEventUuids.Count == 1)
                {
                    var newUuid = addedEventUuids.First();
                    selectedEvents = new List<string> { newUuid };
                    int newIndex = latestParsedEvents.FindIndex(item => item.Uuid == newUuid);
                    listView.selectedIndex = newIndex;
                    if (newIndex != -1)
                    {
                        listView.ScrollToItem(newIndex);
                    }
                }
                else if (selectedEvents.Count != 0)
                {
                    // Try to preserve selection if possible
                    listView.selectedIndex = latestParsedEvents.FindIndex(item => item.Uuid == selectedEvents[0]);
                }

                listView.Refresh();
                UpdateOperationsPanel(operationsPanel, selectedEvents, changedClip, GetAnimationWindow());
            }
        }

        private ListView listView;

        public void UpdateInspectorCall()
        {
            UpdateInspector(scrollView);
        }

        private VisualElement scrollView;

        public void handleScrollInTandem()
        {
            if (scrollInTandem)
            {
                // keep the scroll view in sync with the animation window
                scrollToEventCurrentTime();
            }
        }

        public void OnDestroy()
        {
            Debug.Log("Destroying EventViewer");
            EditorApplication.update -= handleScrollInTandem;
            EditorApplication.update -= HandleRefreshTick;
            Undo.undoRedoPerformed -= UpdateInspectorCall;
            AnimationClipWatcher.OnClipEventsChanged -= OnClipChanged;
            // Save preferences when the window is destroyed
            SavePreferences();
        }

        List<string> selectedEvents = new List<string>();

        public List<string> GetSelectedEvents()
        {
            return selectedEvents;
        }
        // previous time
        private float previousTime = 0f;

        public void HandleRefreshTick()
        {
            if (previousTime != CurrentTime)
            {
                listView.Refresh();
                previousTime = CurrentTime;
            }
        }

        public void UpdateInspector(VisualElement myInspector)
        {
            LoadPreferences(); // Load preferences at the beginning of UI rebuild
            myInspector.Clear();
            EditorApplication.update += handleScrollInTandem;
            EditorApplication.update += HandleRefreshTick;

            var editor = GetAnimationWindow();
            AnimationClip currentClip = getAttachedClip();

            // Create root container with vertical layout
            var rootContainer = new VisualElement();
            rootContainer.style.flexGrow = 1;

            // Top section for controls
            var topControls = new VisualElement();
            topControls.style.flexDirection = FlexDirection.Row;
            topControls.style.marginBottom = 5;

            // Click to Scrub to Event Button
            var scrubToEventButton = new Button();
            scrubToEventButton.tooltip = "Clicking on an event will scrub the timeline to that event";
            UpdateButtonToggleState(scrubToEventButton, scrubToEvent, "Click to Scrub");
            scrubToEventButton.clicked += () =>
            {
                scrubToEvent = !scrubToEvent;
                SavePreferences();
                UpdateButtonToggleState(scrubToEventButton, scrubToEvent, "Click to Scrub");
            };
            topControls.Add(scrubToEventButton);

            // Synced Scroll Button
            var scrollInTandemButton = new Button();
            scrollInTandemButton.tooltip = "Scroll the event list when scrubbing the timeline in the animation window";
            UpdateButtonToggleState(scrollInTandemButton, scrollInTandem, "Synced Scroll");
            scrollInTandemButton.clicked += () =>
            {
                scrollInTandem = !scrollInTandem;
                SavePreferences();
                UpdateButtonToggleState(scrollInTandemButton, scrollInTandem, "Synced Scroll");
            };
            topControls.Add(scrollInTandemButton);

            // Show Differential Timestamp Button
            var showDifferentialTimestampButton = new Button();
            showDifferentialTimestampButton.tooltip = "Show the time difference between the event and the current playhead time in the list";
            UpdateButtonToggleState(showDifferentialTimestampButton, showDifferentialTimestamp, "Diff. Timestamp");
            showDifferentialTimestampButton.clicked += () =>
            {
                showDifferentialTimestamp = !showDifferentialTimestamp;
                SavePreferences();
                UpdateButtonToggleState(showDifferentialTimestampButton, showDifferentialTimestamp, "Diff. Timestamp");
                if (listView != null)
                {
                    listView.Refresh();
                }
            };
            topControls.Add(showDifferentialTimestampButton);

            // Developer Mode Button
            var developerModeButton = new Button();
            developerModeButton.tooltip = "Show additional debug information for developers";
            UpdateButtonToggleState(developerModeButton, developerMode, "Dev Mode");
            developerModeButton.clicked += () =>
            {
                developerMode = !developerMode;
                SavePreferences();
                UpdateButtonToggleState(developerModeButton, developerMode, "Dev Mode");
                if (listView != null)
                {
                    listView.Refresh();
                }
            };
            topControls.Add(developerModeButton);

            // Get events and filter by display name
            var events = AnimationClipWatcher.GetParsedEvents(currentClip);

            // Create ListView
            listView = new ListView(events, 45, MakeEventItem, BindEventItem);
            listView.selectionType = SelectionType.Single;
            listView.style.flexGrow = 1;

            operationsPanel = new VisualElement();
            operationsPanel.style.flexDirection = FlexDirection.Column;

            // Handle selection changes
            listView.onSelectionChange += objects =>
            {
                selectedEvents.Clear();
                foreach (var obj in objects)
                {
                    if (obj is ParsedEngageAnimationEvent evt)
                    {
                        selectedEvents.Add(evt.Uuid);
                        if (scrubToEvent)
                        {
                            FocusAnimationWindow();
                            // Also move the playhead to the event time
                            editor.time = evt.backingAnimationEvent.time;
                            // force refresh the animation window
                            editor.Repaint();
                        }
                    }
                }

                UpdateOperationsPanel(operationsPanel, selectedEvents, currentClip, editor);
            };

            // initialize selected events
            if (selectedEvents.Count != 0)
            {
                // find the index of the selected event in the filtered events
                listView.selectedIndex = events.FindIndex(item => item.Uuid == selectedEvents[0]);
            }


            // Operations panel at the bottom
            operationsPanel.style.borderTopWidth = 1;
            operationsPanel.style.borderTopColor = new StyleColor(Color.gray);
            operationsPanel.style.paddingTop = 10;
            operationsPanel.style.paddingBottom = 10;

            // Initialize operations panel (disabled by default)
            UpdateOperationsPanel(operationsPanel, selectedEvents, currentClip, editor);

            // Add all sections to root container
            rootContainer.Add(topControls);
            twoPlaneSplitView = new TwoPaneSplitView(1, 250, TwoPaneSplitViewOrientation.Vertical);
            // take up maximum height without flexGrow
            // Set explicit height to fill available space
            twoPlaneSplitView.style.height = 1000;

            twoPlaneSplitView.Add(listView);
            twoPlaneSplitView.Add(operationsPanel);

            rootContainer.Add(twoPlaneSplitView);

            myInspector.Add(rootContainer);
        }

        public TwoPaneSplitView twoPlaneSplitView { get; set; }

        public VisualElement operationsPanel { get; set; }

        private void UpdateOperationsPanel(VisualElement panel, List<string> selectedEvents,
            AnimationClip currentClip, AnimationWindow editor)
        {
            panel.Clear();

            bool hasSelection = selectedEvents.Count > 0;

            var events = AnimationClipWatcher.GetParsedEvents(currentClip);
            var selectedEventItem = selectedEvents
                .Select(uuid => events
                    .FirstOrDefault(item => item.Uuid == uuid)).FirstOrDefault();

            if (selectedEventItem == null)
            {
                panel.Add(new HelpBox("No event selected", HelpBoxMessageType.None));
                return;
            }
            var panelTitleContainer = new VisualElement();
            panelTitleContainer.style.flexDirection = FlexDirection.Row;

            // label name of event for confirmation
            var label = new Label();
            label.text = hasSelection
                ? $"{selectedEventItem.displayName} ({selectedEventItem.originalName})"
                : "No Event Selected";

            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.color = new StyleColor(Color.white);
            label.style.fontSize = 20;
            label.style.marginBottom = 5;
            label.style.paddingLeft = 5;
            label.style.paddingRight = 5;
            panelTitleContainer.Add(label);
            // offer a right click menu to copy the event name
            panelTitleContainer.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                evt.menu.AppendAction("Copy Event Name", (a) =>
                {
                    EditorGUIUtility.systemCopyBuffer = $"{selectedEventItem.displayName} ({selectedEventItem.originalName})";
                });
            }));

            // timestamp of event
            // var timeLabel = new Label();
            // timeLabel.text = hasSelection
            //     ? $"at {selectedEventItem?.backingAnimationEvent.time:F3}s"
            //     : "";
            // timeLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            // timeLabel.style.color = new StyleColor(Color.white);
            // panelTitleContainer.Add(timeLabel);

            panel.Add(panelTitleContainer);

            var timeField = new FloatField() { isDelayed = true, tooltip = "Time of the event" };
            BindTimeField(timeField, selectedEventItem);
            panelTitleContainer.Add(timeField);

            // upper bar for quick controls
            var quickControls = new VisualElement();

            quickControls.style.flexDirection = FlexDirection.Row;

            // Jump to event button
            var jumpToEventButton = new Button(() =>
            {
                if (selectedEvents.Count > 0)
                {
                    FocusAnimationWindow();
                    editor.time = selectedEventItem.backingAnimationEvent.time;
                    editor.Repaint();
                }
            })
            {
                text = "Scrub to Event",
                tooltip = hasSelection
                    ? $"Scrub to this event in the animation window"
                    : "Select an event first"
            };
            jumpToEventButton.SetEnabled(hasSelection);

            // Move to playhead button
            var moveEventButton = new Button(() =>
            {
                var clone = selectedEventItem.backingAnimationEvent.Clone();
                clone.time = editor.time;
                AnimationClipWatcher.ReplaceEventProgrammatically(currentClip, selectedEventItem, clone, $"Moving Event to Playhead");
            })
            {
                text = "Move Event to Playhead",
                tooltip = hasSelection ? "Move selected event to where the playhead is in the animation window" : "Select an event first"
            };
            moveEventButton.SetEnabled(hasSelection);

            // Nudge buttons
            var nudgeOneFrameTime = 1f / 60f;

            var nudgeBackButton = new Button(() =>
            {
                var clone = selectedEventItem.backingAnimationEvent.Clone();
                clone.time -= nudgeOneFrameTime;
                AnimationClipWatcher.ReplaceEventProgrammatically(currentClip, selectedEventItem, clone, $"Nudging Event Back");
            })
            {
                text = "◄ Nudge Back",
                tooltip = hasSelection ? "Nudge selected event back by one frame" : "Select an event first"
            };
            nudgeBackButton.SetEnabled(hasSelection);

            var nudgeForwardButton = new Button(() =>
            {
                var clone = selectedEventItem.backingAnimationEvent.Clone();
                clone.time += nudgeOneFrameTime;
                AnimationClipWatcher.ReplaceEventProgrammatically(currentClip, selectedEventItem, clone, $"Nudging Event Forward");
            })
            {
                text = "Nudge Forward ►",
                tooltip = hasSelection ? "Nudge selected event forward by one frame" : "Select an event first"
            };
            nudgeForwardButton.SetEnabled(hasSelection);

            // Delete button
            var deleteButton = new Button(() => { DeleteAnimationEvent(currentClip, selectedEventItem); })
            {
                text = "Delete",
                tooltip = hasSelection ? "Delete selected event" : "Select an event first"
            };

            deleteButton.style.backgroundColor = new StyleColor(new Color(0.7f, 0.3f, 0.3f));
            deleteButton.SetEnabled(hasSelection);

            // Add all buttons to panel
            quickControls.Add(jumpToEventButton);
            quickControls.Add(moveEventButton);
            quickControls.Add(nudgeBackButton);
            quickControls.Add(nudgeForwardButton);
            quickControls.Add(deleteButton);

            panel.Add(quickControls);

            // Add spacing between buttons
            foreach (var child in quickControls.Children())
            {
                child.style.marginLeft = 2;
                child.style.marginRight = 2;
            }

            // justification left
            quickControls.style.justifyContent = Justify.FlexStart;
            quickControls.style.marginLeft = 2;

            var parameterEditors = new Box();
            parameterEditors.style.flexDirection = FlexDirection.Row;

            var specialEditor = selectedEventItem.MakeSpecialEditor((parsedEvent, animEvent) =>
            {
                AnimationClipWatcher.ReplaceEventProgrammatically(currentClip, parsedEvent, animEvent, $"Saving ${parsedEvent.displayName} Event");
            }, events);

            if (specialEditor != null)
            {
                var specialEditorContainer = new Box();
                specialEditorContainer.style.flexDirection = FlexDirection.Column;
                specialEditorContainer.style.marginLeft = 2;
                var specialEditorLabel = new Label("Special Editor");
                specialEditorLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                specialEditorLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                specialEditorLabel.style.marginBottom = 5;

                specialEditorLabel.style.color = new StyleColor(Color.white);
                specialEditorContainer.Add(specialEditorLabel);
                specialEditorContainer.Add(specialEditor);
                specialEditorContainer.style.flexGrow = 1;
                parameterEditors.Add(specialEditorContainer);
            }


            // Add backing parameters
            var containerToPutActualBackingParameters = new VisualElement();
            // grow
            containerToPutActualBackingParameters.style.flexGrow = 1;

            var labelBackingParameters = new Label("Backing Parameters");
            labelBackingParameters.style.unityFontStyleAndWeight = FontStyle.Bold;
            labelBackingParameters.style.color = new StyleColor(Color.white);
            labelBackingParameters.style.marginBottom = 5;
            containerToPutActualBackingParameters.Add(labelBackingParameters);

            if (selectedEventItem.exposedProperties.Contains(ParsedEngageAnimationEvent.ExposedPropertyType.FunctionName))
            {
                var functionNameParam = new TextField("Function Name")
                {
                    isDelayed = true
                };
                BindFunctionNameField(functionNameParam, selectedEventItem);
                containerToPutActualBackingParameters.Add(functionNameParam);
            }

            if (selectedEventItem.exposedProperties.Contains(ParsedEngageAnimationEvent.ExposedPropertyType.String))
            {
                var stringParam = new TextField("String")
                {
                    isDelayed = true
                };
                BindStringField(stringParam, selectedEventItem);
                containerToPutActualBackingParameters.Add(stringParam);
            }

            if (selectedEventItem.exposedProperties.Contains(ParsedEngageAnimationEvent.ExposedPropertyType.Float))
            {
                var floatField = new FloatField("Float Parameter")
                {
                    isDelayed = true
                };
                BindFloatField(floatField, selectedEventItem);
                containerToPutActualBackingParameters.Add(floatField);
            }

            if (selectedEventItem.exposedProperties.Contains(ParsedEngageAnimationEvent.ExposedPropertyType.Int))
            {
                var intField = new IntegerField("Int Parameter")
                {
                    isDelayed = true
                };
                BindIntField(intField, selectedEventItem);
                containerToPutActualBackingParameters.Add(intField);
            }

            if (selectedEventItem.exposedProperties.Contains(ParsedEngageAnimationEvent.ExposedPropertyType.ObjectReference))
            {
                var objectField = new ObjectField("Object Reference")
                {
                    objectType = typeof(Object)
                };
                BindObjectField(objectField, selectedEventItem);
                containerToPutActualBackingParameters.Add(objectField);
            }

            if (selectedEventItem.exposedProperties.Count == 0)
            {
                var labelNoBackingParameters = new Label("This event has no backing parameters - just its existence is sufficient.");
                labelNoBackingParameters.style.color = new StyleColor(Color.white);
                containerToPutActualBackingParameters.Add(labelNoBackingParameters);
            }

            // Add backing parameters to panel
            parameterEditors.Add(containerToPutActualBackingParameters);
            containerToPutActualBackingParameters.style.flexDirection = FlexDirection.Column;

            // margin for parameter editors
            parameterEditors.style.marginLeft = 2;
            parameterEditors.style.marginRight = 2;
            parameterEditors.style.marginTop = 5;
            parameterEditors.style.marginBottom = 5;

            panel.Add(parameterEditors);
            // border for parameter editors


            // space between parameter editors
            foreach (var child in parameterEditors.Children())
            {
                child.style.marginLeft = 5;
                child.style.marginRight = 5;
            }

            var helpBox = new HelpBox(selectedEventItem.Explanation, HelpBoxMessageType.Info);
            panel.Add(helpBox);

            // create a piece of user selectable text

        }

        private VisualElement MakeEventItem()
        {
            var itemContainer = new VisualElement();

            // Border
            itemContainer.style.borderLeftWidth = 1;
            itemContainer.style.borderBottomWidth = 1;
            itemContainer.style.borderBottomColor = new StyleColor(new Color(0.5f, 0.5f, 0.5f, 0.2f));

            // Padding
            itemContainer.style.paddingBottom = 7;
            itemContainer.style.paddingLeft = 7;
            itemContainer.style.paddingRight = 5;

            // Header container
            var headerContainer = new VisualElement();
            headerContainer.name = "header-container";
            headerContainer.style.paddingTop = 7;
            headerContainer.style.paddingBottom = 7;
            headerContainer.style.flexDirection = FlexDirection.Row;

            // Labels
            var nameLabel = new Label();
            nameLabel.name = "name-label";
            nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            nameLabel.style.color = new StyleColor(Color.white);

            var backingNameLabel = new Label();
            backingNameLabel.name = "backing-name-label";
            backingNameLabel.style.unityFont = EditorStyles.miniFont;
            // hide this label by default from normal users
            backingNameLabel.style.display = DisplayStyle.None;

            var uuidLabel = new Label();
            uuidLabel.name = "uuid-label";
            uuidLabel.style.unityFont = EditorStyles.miniFont;
            uuidLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            uuidLabel.style.color = new StyleColor(Color.white);
            // hide uuid label by default from normal users
            uuidLabel.style.display = DisplayStyle.None;

            var timeLabel = new Label();
            timeLabel.name = "time-label";

            // Add labels to header
            headerContainer.Add(nameLabel);
            headerContainer.Add(backingNameLabel);
            headerContainer.Add(timeLabel);
            var timeIndicator = new Label();
            timeIndicator.name = "time-indicator";
            headerContainer.Add(timeIndicator);

            headerContainer.Add(uuidLabel);

            itemContainer.Add(headerContainer);

            var summaryContainer = new Label();
            summaryContainer.name = "summary-container";
            summaryContainer.style.unityFont = EditorStyles.miniFont;
            summaryContainer.style.paddingTop = 5;
            itemContainer.Add(summaryContainer);

            itemContainer.AddManipulator(new ContextualMenuManipulator(BuildContextMenu));

            return itemContainer;
        }


        private void BuildContextMenu(ContextualMenuPopulateEvent evt)
        {
            // Get the event data from the target element
            var element = evt.target as VisualElement;
            if (element == null) return;

            // Find the event in the list by traversing up the visual hierarchy if needed
            var container = element;
            while (container != null && container.userData == null)
            {
                container = container.parent;
            }

            if (container?.userData is ParsedEngageAnimationEvent sourceEvent)
            {
                // Add copy option
                evt.menu.AppendAction("Copy Event", (a) => CopyEventToClipboard(sourceEvent));
                evt.menu.AppendAction("Paste Over Event", (a) => PasteEventFromClipboard(sourceEvent),
                    (a) => CanPasteEvent() ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
                // Add paste as new option
                evt.menu.AppendAction("Paste As New Event", (a) => PasteEventFromClipboardAsNew(sourceEvent),
                    (a) => CanPasteEvent() ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
                // Add duplicate option
                evt.menu.AppendAction("Duplicate Event", (a) => DuplicateEvent(sourceEvent));
                // Add delete option
                evt.menu.AppendAction("Delete Event", (a) => DeleteAnimationEvent(getAttachedClip(), sourceEvent));
            }
        }

        private void CopyEventToClipboard(ParsedEngageAnimationEvent evt)
        {
            // Serialize event data to JSON or another format
            var data = AnimationEventToJson(evt.backingAnimationEvent);
            EditorGUIUtility.systemCopyBuffer = data;
            Debug.Log($"Copied event: {evt.displayName}");
        }

        private string AnimationEventToJson(AnimationEvent animationEvent)
        {
            if (animationEvent == null)
                return "{}";

            var serializable = SerializableAnimationEvent.FromAnimationEvent(animationEvent);
            return JsonUtility.ToJson(serializable, true);
        }


        [Serializable]
        private class SerializableAnimationEvent
        {
            public float time;
            public string functionName;
            public float floatParameter;
            public int intParameter;
            public string stringParameter;
            public string objectReferencePath;
            public string objectReferenceType;

            public static SerializableAnimationEvent FromAnimationEvent(AnimationEvent evt)
            {
                var result = new SerializableAnimationEvent
                {
                    time = evt.time,
                    functionName = evt.functionName,
                    floatParameter = evt.floatParameter,
                    intParameter = evt.intParameter,
                    stringParameter = evt.stringParameter
                };

                if (evt.objectReferenceParameter != null)
                {
                    result.objectReferencePath = AssetDatabase.GetAssetPath(evt.objectReferenceParameter);
                    result.objectReferenceType = evt.objectReferenceParameter.GetType().AssemblyQualifiedName;
                }

                return result;
            }

            public AnimationEvent ToAnimationEvent()
            {
                var evt = new AnimationEvent
                {
                    time = this.time,
                    functionName = this.functionName,
                    floatParameter = this.floatParameter,
                    intParameter = this.intParameter,
                    stringParameter = this.stringParameter
                };

                if (!string.IsNullOrEmpty(objectReferencePath))
                {
                    evt.objectReferenceParameter = AssetDatabase.LoadAssetAtPath(
                        objectReferencePath,
                        Type.GetType(objectReferenceType));
                }

                return evt;
            }
        }

        private bool CanPasteEvent()
        {
            // Validate if clipboard has valid event data
            try
            {
                var clipData = EditorGUIUtility.systemCopyBuffer;
                return !string.IsNullOrEmpty(clipData) && clipData.Contains("functionName");
            }
            catch
            {
                return false;
            }
        }

        private void PasteEventFromClipboard(ParsedEngageAnimationEvent targetEvent)
        {
            try
            {
                var clipData = EditorGUIUtility.systemCopyBuffer;
                var serializedEvent = JsonUtility.FromJson<SerializableAnimationEvent>(clipData);
                var sourceEvent = serializedEvent.ToAnimationEvent();

                if (sourceEvent != null)
                {
                    // Create a clone to preserve the time of the target event
                    var clone = sourceEvent.Clone();
                    clone.time = targetEvent.backingAnimationEvent.time;

                    // Update the event
                    AnimationClipWatcher.ReplaceEventProgrammatically(getAttachedClip(), targetEvent, clone, "Paste Event Data");
                    Debug.Log("Pasted event data successfully");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to paste event data: {ex.Message}");
            }
        }

        // paste it at the same time as the target event, but don't overwrite it
        private void PasteEventFromClipboardAsNew(ParsedEngageAnimationEvent targetEvent)
        {
            try
            {
                var clipData = EditorGUIUtility.systemCopyBuffer;
                var serializedEvent = JsonUtility.FromJson<SerializableAnimationEvent>(clipData);
                var sourceEvent = serializedEvent.ToAnimationEvent();

                if (sourceEvent != null)
                {
                    // Create a clone to preserve the time of the target event
                    var clone = sourceEvent.Clone();
                    clone.time = targetEvent.backingAnimationEvent.time;

                    // Add the new event as a duplicate
                    AnimationClipWatcher.AddEventProgrammatically(getAttachedClip(), clone, "Paste Event Data as New");
                    Debug.Log("Pasted event data successfully as new event");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to paste event data: {ex.Message}");
            }
        }

        private void DuplicateEvent(ParsedEngageAnimationEvent sourceEvent)
        {
            // Clone the event and offset time slightly
            var clone = sourceEvent.backingAnimationEvent.Clone();
            clone.time += 1f / 60f; // Offset by one frame

            AnimationClipWatcher.AddEventProgrammatically(getAttachedClip(), clone, "Duplicate Event");
        }

        private void BindEventItem(VisualElement element, int index)
        {
            var editor = GetAnimationWindow();
            float currentTime = editor.time;
            AnimationClip currentClip = getAttachedClip();

            // Get filtered events
            var events = AnimationClipWatcher.GetParsedEvents(currentClip);

            // filter not yet implemented.
            var filteredEvents = events;

            if (index >= filteredEvents.Count) return;

            var item = filteredEvents[index];

            element.userData = item;

            // Bind data to UI elements
            var headerContainer = element.Q("header-container");
            headerContainer.Q<Label>("name-label").text = item.displayName;
            var backingNameLabel = headerContainer.Q<Label>("backing-name-label");
            backingNameLabel.text = item.originalName;
            backingNameLabel.style.display = developerMode ? DisplayStyle.Flex : DisplayStyle.None;
            
            var uuidLabel = headerContainer.Q<Label>("uuid-label");
            uuidLabel.text = item.Uuid;
            uuidLabel.style.display = developerMode ? DisplayStyle.Flex : DisplayStyle.None;

            headerContainer.Q<Label>("time-label").text = $"({item.backingAnimationEvent.time:F3})";

            float deltaTime = item.backingAnimationEvent.time - currentTime;

            // Update time indicator
            var timeIndicator = element.Q<Label>("time-indicator");
            timeIndicator.text = $"({deltaTime:+0.000;-0.000;0.000}s)";
            timeIndicator.style.display = showDifferentialTimestamp ? DisplayStyle.Flex : DisplayStyle.None;

            StyleColor borderColor;
            float width;

            if (Mathf.Abs(deltaTime) > 0.5f)
            {
                borderColor = new StyleColor(new Color(0.5f, 0.5f, 0.5f, 0.2f));
                width = 0.5f;
            }
            else if (Mathf.Abs(deltaTime) < 0.03f)
            {
                borderColor = new StyleColor(Color.green);
                width = 2f;
            }
            else
            {
                float t = (0.5f - Mathf.Abs(deltaTime)) / 0.44f;
                borderColor = new StyleColor(Color.Lerp(new Color(0.5f, 0.5f, 0.5f, 0.2f), Color.white, t));
                width = Mathf.Lerp(0.5f, 2, t);
            }

            element.style.borderLeftColor = borderColor;
            element.style.borderLeftWidth = width;

            // left padding + left border always 5
            element.style.paddingLeft = 7 - width;

            // summary
            var summaryContainer = element.Q<Label>("summary-container");
            summaryContainer.text = item.Summary;
        }

        private void DeleteAnimationEvent(AnimationClip currentClip, ParsedEngageAnimationEvent item)
        {
            if (!EditorUtility.DisplayDialog(
                "Delete Animation Event",
                $"Are you sure you want to delete the event '{item.displayName}' at {item.backingAnimationEvent.time:F3}s?",
                "Delete",
                "Cancel"))
            {
                return;
            }
            AnimationClipWatcher.DeleteEventProgrammatically(currentClip, item, $"Deleting ${item.displayName} Event");
        }

        private void BindFloatField(FloatField floatField, ParsedEngageAnimationEvent parsedEvent)
        {
            floatField.value = parsedEvent.backingAnimationEvent.floatParameter;
            floatField.RegisterValueChangedCallback(evt =>
            {
                var clone = parsedEvent.backingAnimationEvent.Clone();
                clone.floatParameter = evt.newValue;
                AnimationClipWatcher.ReplaceEventProgrammatically(getAttachedClip(), parsedEvent, clone,
                    $"Changing float value for ${parsedEvent.displayName} Event to {evt.newValue}");
            });
        }

        private void BindFunctionNameField(TextField stringField, ParsedEngageAnimationEvent parsedEvent)
        {
            stringField.value = parsedEvent.backingAnimationEvent.functionName;
            stringField.RegisterValueChangedCallback(evt =>
            {
                var clone = parsedEvent.backingAnimationEvent.Clone();
                clone.functionName = evt.newValue;
                AnimationClipWatcher.ReplaceEventProgrammatically(getAttachedClip(), parsedEvent, clone,
                    $"Changing function name value for ${parsedEvent.displayName} Event to {evt.newValue}");
            });
        }

        private void BindStringField(TextField stringField, ParsedEngageAnimationEvent parsedEvent)
        {
            stringField.value = parsedEvent.backingAnimationEvent.stringParameter;
            stringField.RegisterValueChangedCallback(evt =>
            {
                var clone = parsedEvent.backingAnimationEvent.Clone();
                clone.stringParameter = evt.newValue;
                AnimationClipWatcher.ReplaceEventProgrammatically(getAttachedClip(), parsedEvent, clone,
                    $"Changing string value for ${parsedEvent.displayName} Event to {evt.newValue}");
            });
        }

        private void BindIntField(IntegerField intField, ParsedEngageAnimationEvent parsedEvent)
        {
            intField.value = parsedEvent.backingAnimationEvent.intParameter;
            intField.RegisterValueChangedCallback(evt =>
            {
                var clone = parsedEvent.backingAnimationEvent.Clone();
                clone.intParameter = evt.newValue;
                AnimationClipWatcher.ReplaceEventProgrammatically(getAttachedClip(), parsedEvent, clone,
                    $"Changing int value for ${parsedEvent.displayName} Event to {evt.newValue}");
            });
        }

        private void BindObjectField(ObjectField objectField, ParsedEngageAnimationEvent parsedEvent)
        {
            objectField.value = parsedEvent.backingAnimationEvent.objectReferenceParameter;
            objectField.RegisterValueChangedCallback(evt =>
            {
                var clone = parsedEvent.backingAnimationEvent.Clone();
                clone.objectReferenceParameter = evt.newValue;
                AnimationClipWatcher.ReplaceEventProgrammatically(getAttachedClip(), parsedEvent, clone,
                    $"Changing object reference for ${parsedEvent.displayName} Event to {evt.newValue}");
            });
        }
        
        private void BindTimeField(FloatField timeField, ParsedEngageAnimationEvent parsedEvent)
        {
            timeField.value = parsedEvent.backingAnimationEvent.time;
            timeField.RegisterValueChangedCallback(evt =>
            {
                var clone = parsedEvent.backingAnimationEvent.Clone();
                clone.time = evt.newValue;
                AnimationClipWatcher.ReplaceEventProgrammatically(getAttachedClip(), parsedEvent, clone,
                    $"Changing time for ${parsedEvent.displayName} Event to {evt.newValue}");
            });
        }

        private void FocusAnimationWindow()
        {
            if (this.animationEditor != null)
            {
                Selection.activeGameObject = this.animationEditor.gameObject;
            }
            Type animationWindowType = Type.GetType("UnityEditor.AnimationWindow,UnityEditor");
            if (animationWindowType != null)
            {
                FocusWindowIfItsOpen(animationWindowType);
            }
        }
    }
}
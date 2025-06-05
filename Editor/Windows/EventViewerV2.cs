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
        private const string SearchModeKey = "EventViewerV2_SearchMode";
        
        private enum SearchMode
        {
            All,
            [System.ComponentModel.Description("Name Only")]
            NameOnly
        }

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
        private SearchMode searchMode = SearchMode.All;

        private void UpdateButtonToggleState(Button button, bool isActive, string textPrefix)
        {
            button.text = isActive ? $"{textPrefix}: ON" : $"{textPrefix}: OFF";
            button.style.backgroundColor = isActive ? new StyleColor(new Color(0.4f, 0.6f, 0.4f, 0.5f)) : new StyleColor(StyleKeyword.Initial);
        }

        private void LoadPreferences()
        {
            scrollInTandem = EditorPrefs.GetBool(ScrollInTandemKey, false);
            scrubToEvent = EditorPrefs.GetBool(ScrubToEventKey, true);
            showDifferentialTimestamp = EditorPrefs.GetBool(ShowDifferentialTimestampKey, false);
            developerMode = EditorPrefs.GetBool(DeveloperModeKey, false);
            searchMode = (SearchMode)EditorPrefs.GetInt(SearchModeKey, (int)SearchMode.All);
        }

        private void SavePreferences()
        {
            EditorPrefs.SetBool(ScrollInTandemKey, scrollInTandem);
            EditorPrefs.SetBool(ScrubToEventKey, scrubToEvent);
            EditorPrefs.SetBool(ShowDifferentialTimestampKey, showDifferentialTimestamp);
            EditorPrefs.SetBool(DeveloperModeKey, developerMode);
            EditorPrefs.SetInt(SearchModeKey, (int)searchMode);
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
                tooltip = "Force a refresh of the event list in case it bugs out",
                style =
                {
                    marginBottom = 5,
                    marginLeft = 5,
                    marginRight = 5,
                }
            };
            root.Add(forceRefreshButton);

            scrollView = new VisualElement();

            root.Add(scrollView);

            AnimationClipWatcher.OnClipEventsChanged += OnClipChanged;
            Undo.undoRedoPerformed += UpdateInspectorCall;
            UpdateInspector(scrollView);
        }

        private string currentSearchTerm = "";

        private HelpBox noEventsHelpBox;
        private VisualElement emptyStateContainer;
        private Button emptyStatePasteButton;
        private string lastClipboardContent = "";
        
        private void FilterEventsList(string searchTerm)
        {
            currentSearchTerm = searchTerm;
            
            var allEvents = AnimationClipWatcher.GetParsedEvents(getAttachedClip());
            List<ParsedEngageAnimationEvent> eventsToShow;
            
            if (string.IsNullOrEmpty(searchTerm))
            {
                eventsToShow = allEvents;
            }
            else
            {
                var lowerSearchTerm = searchTerm.ToLower();
                eventsToShow = allEvents.Where(evt =>
                {
                    if (searchMode == SearchMode.NameOnly)
                    {
                        // Only search in display name and original name (Japanese name)
                        return evt.displayName.ToLower().Contains(lowerSearchTerm) ||
                               evt.originalName.ToLower().Contains(lowerSearchTerm);
                    }
                    else // SearchMode.All
                    {
                        // Check display name
                        if (evt.displayName.ToLower().Contains(lowerSearchTerm))
                            return true;

                        // Check original name (backing function name)
                        if (evt.originalName.ToLower().Contains(lowerSearchTerm))
                            return true;

                        // Check function name parameter
                        if (evt.backingAnimationEvent.functionName.ToLower().Contains(lowerSearchTerm))
                            return true;

                        // Check string parameter
                        if (!string.IsNullOrEmpty(evt.backingAnimationEvent.stringParameter) &&
                            evt.backingAnimationEvent.stringParameter.ToLower().Contains(lowerSearchTerm))
                            return true;

                        // Check summary
                        if (!string.IsNullOrEmpty(evt.Summary) &&
                            evt.Summary.ToLower().Contains(lowerSearchTerm))
                            return true;

                        // Check time as string
                        if (evt.backingAnimationEvent.time.ToString("F3").Contains(searchTerm))
                            return true;

                        return false;
                    }
                }).ToList();
            }
            
            listView.itemsSource = eventsToShow;
            listView.Refresh();
            
            // Update empty state visibility and paste button
            if (emptyStateContainer != null)
            {
                // Only show empty state container if there are actually no events in the clip
                bool clipIsActuallyEmpty = allEvents.Count == 0;
                emptyStateContainer.style.display = eventsToShow.Count == 0 ? DisplayStyle.Flex : DisplayStyle.None;
                
                // Hide paste button if we're just filtering
                if (emptyStatePasteButton != null)
                {
                    emptyStatePasteButton.style.display = clipIsActuallyEmpty ? DisplayStyle.Flex : DisplayStyle.None;
                }
            }
            
            // Update help box message
            if (noEventsHelpBox != null)
            {
                // Update message based on whether we're filtering
                if (eventsToShow.Count == 0 && !string.IsNullOrEmpty(searchTerm))
                {
                    noEventsHelpBox.text = "No events match your search.";
                }
                else if (eventsToShow.Count == 0)
                {
                    noEventsHelpBox.text = "No events in this clip.";
                }
            }
        }

        private void OnClipChanged(AnimationClip changedClip, HashSet<string> addedEventUuids)
        {
            if (changedClip == getAttachedClip())
            {
                var latestParsedEvents = AnimationClipWatcher.GetParsedEvents(changedClip);
                
                // Apply current search filter if any
                var filteredEvents = latestParsedEvents;
                if (!string.IsNullOrEmpty(currentSearchTerm))
                {
                    var lowerSearchTerm = currentSearchTerm.ToLower();
                    filteredEvents = latestParsedEvents.Where(evt =>
                    {
                        if (searchMode == SearchMode.NameOnly)
                        {
                            // Only search in display name and original name (Japanese name)
                            return evt.displayName.ToLower().Contains(lowerSearchTerm) ||
                                   evt.originalName.ToLower().Contains(lowerSearchTerm);
                        }
                        else // SearchMode.All
                        {
                            // Check display name
                            if (evt.displayName.ToLower().Contains(lowerSearchTerm))
                                return true;

                            // Check original name (backing function name)
                            if (evt.originalName.ToLower().Contains(lowerSearchTerm))
                                return true;

                            // Check function name parameter
                            if (evt.backingAnimationEvent.functionName.ToLower().Contains(lowerSearchTerm))
                                return true;

                            // Check string parameter
                            if (!string.IsNullOrEmpty(evt.backingAnimationEvent.stringParameter) &&
                                evt.backingAnimationEvent.stringParameter.ToLower().Contains(lowerSearchTerm))
                                return true;

                            // Check summary
                            if (!string.IsNullOrEmpty(evt.Summary) &&
                                evt.Summary.ToLower().Contains(lowerSearchTerm))
                                return true;

                            // Check time as string
                            if (evt.backingAnimationEvent.time.ToString("F3").Contains(currentSearchTerm))
                                return true;

                            return false;
                        }
                    }).ToList();
                }
                
                listView.itemsSource = filteredEvents;

                // If exactly one new event was added, select and scroll to it
                if (addedEventUuids.Count == 1)
                {
                    var newUuid = addedEventUuids.First();
                    selectedEvents = new List<string> { newUuid };
                    int newIndex = filteredEvents.FindIndex(item => item.Uuid == newUuid);
                    listView.selectedIndex = newIndex;
                    if (newIndex != -1)
                    {
                        listView.ScrollToItem(newIndex);
                    }
                }
                else if (selectedEvents.Count != 0)
                {
                    // Try to preserve selection if possible
                    listView.selectedIndex = filteredEvents.FindIndex(item => item.Uuid == selectedEvents[0]);
                }

                listView.Refresh();
                UpdateOperationsPanel(operationsPanel, selectedEvents, changedClip, GetAnimationWindow());
                
                // Update empty state visibility and paste button
                if (emptyStateContainer != null)
                {
                    emptyStateContainer.style.display = filteredEvents.Count == 0 ? DisplayStyle.Flex : DisplayStyle.None;
                    
                    // Only show paste button if clip is actually empty
                    if (emptyStatePasteButton != null)
                    {
                        bool clipIsActuallyEmpty = latestParsedEvents.Count == 0;
                        emptyStatePasteButton.style.display = clipIsActuallyEmpty ? DisplayStyle.Flex : DisplayStyle.None;
                    }
                }
                
                // Update help box message
                if (noEventsHelpBox != null)
                {
                    // Update message based on whether we're filtering
                    if (filteredEvents.Count == 0 && !string.IsNullOrEmpty(currentSearchTerm))
                    {
                        noEventsHelpBox.text = "No events match your search.";
                    }
                    else if (filteredEvents.Count == 0)
                    {
                        noEventsHelpBox.text = "No events in this clip.";
                    }
                }
            }
        }

        private ListView listView;
        private TextField searchField;

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
            
            // Check clipboard changes for paste button
            CheckClipboardForPasteButton();
        }
        
        private void CheckClipboardForPasteButton()
        {
            if (emptyStatePasteButton == null) return;
            
            try
            {
                var currentClipboard = EditorGUIUtility.systemCopyBuffer;
                if (currentClipboard != lastClipboardContent)
                {
                    lastClipboardContent = currentClipboard;
                    // Update paste button state
                    bool canPaste = CanPasteEvent();
                    emptyStatePasteButton.SetEnabled(canPaste);
                }
            }
            catch
            {
                // Ignore clipboard access errors
            }
        }

        public void UpdateInspector(VisualElement myInspector)
        {
            LoadPreferences(); // Load preferences at the beginning of UI rebuild
            myInspector.Clear();
            EditorApplication.update += handleScrollInTandem;
            EditorApplication.update += HandleRefreshTick;
            
            // Load custom stylesheet
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.divinedragon.animation_tools/Editor/AnimationTools.uss");
            if (styleSheet != null)
            {
                myInspector.styleSheets.Add(styleSheet);
            }

            var editor = GetAnimationWindow();
            AnimationClip currentClip = getAttachedClip();

            // Create root container with vertical layout
            var rootContainer = new VisualElement();
            rootContainer.style.flexGrow = 1;

            // Create container for search field
            var searchControls = new VisualElement();
            searchControls.style.flexDirection = FlexDirection.Row;
            searchControls.style.marginBottom = 5;
            searchControls.style.marginLeft = 5;
            searchControls.style.marginRight = 5;

            // Add search field to search controls
            searchField = new TextField("Search");
            searchField.labelElement.style.minWidth = 50;
            searchField.labelElement.style.width = 50;
            searchField.style.flexGrow = 1;
            
            searchControls.Add(searchField);
            
            // Add clear button
            var clearButton = new Button(() =>
            {
                searchField.value = "";
                FilterEventsList("");
            })
            {
                text = "×",
                tooltip = "Clear search",
                style =
                {
                    width = 20,
                    height = 20,
                    marginLeft = 2,
                    fontSize = 18,
                    unityTextAlign = TextAnchor.MiddleCenter,
                    paddingTop = 0,
                    paddingBottom = 0,
                    paddingLeft = 0,
                    paddingRight = 0
                }
            };
            
            // Disable clear button when search is empty
            clearButton.SetEnabled(!string.IsNullOrEmpty(currentSearchTerm));
            
            searchControls.Add(clearButton);
            
            // Add search mode dropdown
            var searchModeDropdown = new EnumField(searchMode);
            searchModeDropdown.style.width = 100;
            searchModeDropdown.style.marginLeft = 5;
            searchModeDropdown.tooltip = "Search mode:\n" +
                "• All - Searches in event names (English & Japanese), parameters, summaries, and timestamps\n" +
                "• Name Only - Searches only in event display names (English) and original names (Japanese)";
            searchModeDropdown.RegisterValueChangedCallback(evt =>
            {
                searchMode = (SearchMode)evt.newValue;
                SavePreferences();
                // Re-filter with current search term
                if (!string.IsNullOrEmpty(currentSearchTerm))
                {
                    FilterEventsList(currentSearchTerm);
                }
            });
            searchControls.Add(searchModeDropdown);
            
            // Set up search field callback
            searchField.value = currentSearchTerm;
            searchField.RegisterValueChangedCallback(evt =>
            {
                if (listView != null)
                {
                    FilterEventsList(evt.newValue);
                }
                // Enable/disable clear button based on search text
                clearButton.SetEnabled(!string.IsNullOrEmpty(evt.newValue));
            });

            // Top section for toggle controls
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

            // Get events - apply current search filter if any
            var allEvents = AnimationClipWatcher.GetParsedEvents(currentClip);
            var eventsToShow = allEvents;
            
            if (!string.IsNullOrEmpty(currentSearchTerm))
            {
                var lowerSearchTerm = currentSearchTerm.ToLower();
                eventsToShow = allEvents.Where(evt =>
                {
                    if (searchMode == SearchMode.NameOnly)
                    {
                        // Only search in display name and original name (Japanese name)
                        return evt.displayName.ToLower().Contains(lowerSearchTerm) ||
                               evt.originalName.ToLower().Contains(lowerSearchTerm);
                    }
                    else // SearchMode.All
                    {
                        // Check display name
                        if (evt.displayName.ToLower().Contains(lowerSearchTerm))
                            return true;

                        // Check original name (backing function name)
                        if (evt.originalName.ToLower().Contains(lowerSearchTerm))
                            return true;

                        // Check function name parameter
                        if (evt.backingAnimationEvent.functionName.ToLower().Contains(lowerSearchTerm))
                            return true;

                        // Check string parameter
                        if (!string.IsNullOrEmpty(evt.backingAnimationEvent.stringParameter) &&
                            evt.backingAnimationEvent.stringParameter.ToLower().Contains(lowerSearchTerm))
                            return true;

                        // Check summary
                        if (!string.IsNullOrEmpty(evt.Summary) &&
                            evt.Summary.ToLower().Contains(lowerSearchTerm))
                            return true;

                        // Check time as string
                        if (evt.backingAnimationEvent.time.ToString("F3").Contains(currentSearchTerm))
                            return true;

                        return false;
                    }
                }).ToList();
            }

            // Create ListView
            listView = new ListView(eventsToShow, 45, MakeEventItem, BindEventItem);
            listView.selectionType = SelectionType.Multiple;
            listView.style.flexGrow = 1;
            
            // Create container for empty state UI
            emptyStateContainer = new VisualElement();
            emptyStateContainer.style.display = eventsToShow.Count == 0 ? DisplayStyle.Flex : DisplayStyle.None;
            
            // Show help box when no events
            noEventsHelpBox = new HelpBox("No events in this clip.", HelpBoxMessageType.Info);
            noEventsHelpBox.style.marginTop = 10;
            noEventsHelpBox.style.marginLeft = 10;
            noEventsHelpBox.style.marginRight = 10;
            emptyStateContainer.Add(noEventsHelpBox);
            
            // Add paste button
            emptyStatePasteButton = new Button(() =>
            { 
                var clipData = EditorGUIUtility.systemCopyBuffer;
                var eventList = JsonUtility.FromJson<SerializableAnimationEventList>(clipData);
                if (eventList != null && eventList.events != null)
                {
                    var eventsToPaste = eventList.events.Select(e => e.ToAnimationEvent()).ToList();
                    foreach (var evt in eventsToPaste)
                    {
                        AnimationClipWatcher.AddEventProgrammatically(currentClip, evt, "Paste Events");
                    }
                }
            })
            {
                text = "Paste Events from Clipboard",
                style =
                {
                    marginTop = 10,
                    marginLeft = 10,
                    marginRight = 10,
                    height = 30
                }
            };
            
            // Enable/disable based on clipboard content
            emptyStatePasteButton.SetEnabled(CanPasteEvent());
            emptyStateContainer.Add(emptyStatePasteButton);
            

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
                listView.selectedIndex = eventsToShow.FindIndex(item => item.Uuid == selectedEvents[0]);
            }


            // Operations panel at the bottom
            operationsPanel.style.borderTopWidth = 1;
            operationsPanel.style.borderTopColor = new StyleColor(Color.gray);
            operationsPanel.style.paddingTop = 10;
            operationsPanel.style.paddingBottom = 10;

            // Initialize operations panel (disabled by default)
            UpdateOperationsPanel(operationsPanel, selectedEvents, currentClip, editor);

            // Add all sections to root container
            rootContainer.Add(searchControls);
            rootContainer.Add(topControls);
            twoPlaneSplitView = new TwoPaneSplitView(1, 250, TwoPaneSplitViewOrientation.Vertical);
            // take up maximum height without flexGrow
            // Set explicit height to fill available space
            twoPlaneSplitView.style.height = 1000;

            // Create a container for the list view and help box
            var listContainer = new VisualElement();
            listContainer.style.flexGrow = 1;
            listContainer.Add(emptyStateContainer);
            listContainer.Add(listView);
            
            twoPlaneSplitView.Add(listContainer);
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
            bool hasMultipleSelection = selectedEvents.Count > 1;

            var events = AnimationClipWatcher.GetParsedEvents(currentClip);
            var selectedEventItems = selectedEvents
                .Select(uuid => events.FirstOrDefault(item => item.Uuid == uuid))
                .Where(item => item != null)
                .ToList();

            if (selectedEventItems.Count == 0)
            {
                panel.Add(new HelpBox("No event selected", HelpBoxMessageType.None));
                return;
            }

            // Handle multi-selection case
            if (hasMultipleSelection)
            {
                var multiSelectLabel = new Label($"{selectedEventItems.Count} events selected")
                {
                    style =
                    {
                        unityFontStyleAndWeight = FontStyle.Bold,
                        color = new StyleColor(Color.white),
                        fontSize = 20,
                        marginBottom = 10,
                        paddingLeft = 5,
                        paddingRight = 5
                    }
                };
                panel.Add(multiSelectLabel);

                // Multi-select controls
                var multiSelectControls = new VisualElement()
                {
                    style = { flexDirection = FlexDirection.Row, marginBottom = 10 }
                };

                // Copy button
                var multiCopyButton = new Button(() =>
                {
                    CopyMultipleEventsToClipboard(selectedEventItems);
                })
                {
                    text = "Copy Events",
                    tooltip = "Copy selected events to clipboard"
                };

                // Delete button
                var multiDeleteButton = new Button(() =>
                {
                    DeleteMultipleAnimationEvents(currentClip, selectedEventItems);
                })
                {
                    text = "Delete Events",
                    tooltip = "Delete selected events"
                };
                multiDeleteButton.AddToClassList("delete-button");

                multiSelectControls.Add(multiCopyButton);
                multiSelectControls.Add(multiDeleteButton);

                // Add spacing between buttons
                foreach (var child in multiSelectControls.Children())
                {
                    child.style.marginLeft = 2;
                    child.style.marginRight = 2;
                }

                panel.Add(multiSelectControls);

                // Show summary of selected events
                var summaryBox = new Box()
                {
                    style = { 
                        marginTop = 10,
                        paddingTop = 10,
                        paddingBottom = 10,
                        paddingLeft = 10,
                        paddingRight = 10
                    }
                };
                
                var summaryLabel = new Label("Selected Events:")
                {
                    style = { unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 5 }
                };
                summaryBox.Add(summaryLabel);

                foreach (var evt in selectedEventItems.Take(10)) // Show max 10 events
                {
                    var eventSummary = new Label($"• {evt.displayName} at {evt.backingAnimationEvent.time:F3}s")
                    {
                        style = { fontSize = 11, marginLeft = 10 }
                    };
                    summaryBox.Add(eventSummary);
                }

                if (selectedEventItems.Count > 10)
                {
                    var moreLabel = new Label($"... and {selectedEventItems.Count - 10} more")
                    {
                        style = { fontSize = 11, marginLeft = 10, unityFontStyleAndWeight = FontStyle.Italic }
                    };
                    summaryBox.Add(moreLabel);
                }

                panel.Add(summaryBox);
                return;
            }

            // Single selection - existing code continues
            var selectedEventItem = selectedEventItems[0];
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
            deleteButton.AddToClassList("delete-button");
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

            // space between parameter editors
            foreach (var child in parameterEditors.Children())
            {
                child.style.marginLeft = 5;
                child.style.marginRight = 5;
            }

            var helpBox = new HelpBox(selectedEventItem.Explanation, HelpBoxMessageType.Info);
            panel.Add(helpBox);
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
                // Check if we have multiple selection
                if (selectedEvents.Count > 1)
                {
                    // Multi-selection context menu
                    evt.menu.AppendAction($"Copy {selectedEvents.Count} Events", (a) => 
                    {
                        var events = AnimationClipWatcher.GetParsedEvents(getAttachedClip());
                        var selectedItems = selectedEvents
                            .Select(uuid => events.FirstOrDefault(e => e.Uuid == uuid))
                            .Where(e => e != null)
                            .ToList();
                        CopyMultipleEventsToClipboard(selectedItems);
                    });

                    // No paste option for multi-selection as the behavior is currently undefined
                    
                    evt.menu.AppendAction($"Delete {selectedEvents.Count} Events", (a) => 
                    {
                        var events = AnimationClipWatcher.GetParsedEvents(getAttachedClip());
                        var selectedItems = selectedEvents
                            .Select(uuid => events.FirstOrDefault(e => e.Uuid == uuid))
                            .Where(e => e != null)
                            .ToList();
                        DeleteMultipleAnimationEvents(getAttachedClip(), selectedItems);
                    });
                }
                else
                {
                    // Single selection context menu (existing code)
                    evt.menu.AppendAction("Copy Event", (a) => CopyEventToClipboard(sourceEvent));
                    
                    // Show context-aware paste options
                    if (CanPasteEvent())
                    {
                        var clipData = EditorGUIUtility.systemCopyBuffer;
                        var eventList = JsonUtility.FromJson<SerializableAnimationEventList>(clipData);
                        int eventCount = 0;
                        bool isSingleEvent = false;
                        
                        if (eventList != null && eventList.events != null && eventList.events.Count > 0)
                        {
                            eventCount = eventList.events.Count;
                            isSingleEvent = eventCount == 1;
                        }
                        
                        if (isSingleEvent)
                        {
                            evt.menu.AppendAction("Paste Over Event", (a) => PasteEventFromClipboard(sourceEvent));
                        }
                        
                        // Customize paste text based on number of events
                        string pasteText = eventCount == 1 
                            ? "Paste as New Event" 
                            : $"Paste {eventCount} Events as New";
                        evt.menu.AppendAction(pasteText, (a) =>
                        {
                            PasteMultipleEventsFromClipboard(getAttachedClip(), sourceEvent);
                        });
                    }
                    
                    // Add duplicate option
                    evt.menu.AppendAction("Duplicate Event", (a) => DuplicateEvent(sourceEvent));
                    // Add delete option
                    evt.menu.AppendAction("Delete Event", (a) => DeleteAnimationEvent(getAttachedClip(), sourceEvent));
                }
            }
        }

        private void CopyEventToClipboard(ParsedEngageAnimationEvent evt)
        {
            // Always copy as a list for consistency
            var events = new List<ParsedEngageAnimationEvent> { evt };
            CopyMultipleEventsToClipboard(events);
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
            try
            {
                var clipData = EditorGUIUtility.systemCopyBuffer;
                if (string.IsNullOrEmpty(clipData)) return false;

                var eventList = JsonUtility.FromJson<SerializableAnimationEventList>(clipData);
                if (eventList != null && eventList.events != null && eventList.events.Count > 0)
                    return true;
                return false;
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
                AnimationEvent sourceEvent = null;
                
                var eventList = JsonUtility.FromJson<SerializableAnimationEventList>(clipData);
                if (eventList != null && eventList.events != null && eventList.events.Count > 0)
                {
                    // If it's a single-event list, use "paste over" behavior
                    if (eventList.events.Count == 1)
                    {
                        sourceEvent = eventList.events[0].ToAnimationEvent();
                    }
                    else
                    {
                        // Multiple events - paste them all at the target time
                        PasteMultipleEventsFromClipboard(getAttachedClip(), targetEvent);
                        return;
                    }
                }

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

        private void DuplicateEvent(ParsedEngageAnimationEvent sourceEvent)
        {
            // Clone the event and offset time slightly
            var clone = sourceEvent.backingAnimationEvent.Clone();
            clone.time += 1f / 60f; // Offset by one frame

            AnimationClipWatcher.AddEventProgrammatically(getAttachedClip(), clone, "Duplicate Event");
        }

        private void CopyMultipleEventsToClipboard(List<ParsedEngageAnimationEvent> events)
        {
            var serializedEvents = events.Select(evt => SerializableAnimationEvent.FromAnimationEvent(evt.backingAnimationEvent)).ToList();
            var data = JsonUtility.ToJson(new SerializableAnimationEventList { events = serializedEvents }, true);
            EditorGUIUtility.systemCopyBuffer = data;
            Debug.Log($"Copied {events.Count} events to clipboard");
        }

        private void PasteMultipleEventsFromClipboard(AnimationClip clip, ParsedEngageAnimationEvent referenceEvent)
        {
            try
            {
                var clipData = EditorGUIUtility.systemCopyBuffer;
                List<AnimationEvent> eventsToPaste = new List<AnimationEvent>();

                // Try to parse as list format first
                var eventList = JsonUtility.FromJson<SerializableAnimationEventList>(clipData);
                if (eventList != null && eventList.events != null)
                {
                    eventsToPaste = eventList.events.Select(e => e.ToAnimationEvent()).ToList();
                }

                if (eventsToPaste.Count > 0)
                {
                    // Offset times to start at reference event time
                    float minTime = eventsToPaste.Min(e => e.time);
                    float offset = referenceEvent.backingAnimationEvent.time - minTime;

                    foreach (var evt in eventsToPaste)
                    {
                        var clone = evt.Clone();
                        clone.time += offset;
                        // TODO: this is inefficient as we will be parsing the resulting AnimationClip over and over again
                        AnimationClipWatcher.AddEventProgrammatically(clip, clone, "Paste Events");
                    }

                    Debug.Log($"Pasted {eventsToPaste.Count} event{(eventsToPaste.Count > 1 ? "s" : "")}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to paste events: {ex.Message}");
            }
        }

        private void DeleteMultipleAnimationEvents(AnimationClip clip, List<ParsedEngageAnimationEvent> events)
        {
            if (!EditorUtility.DisplayDialog(
                "Delete Multiple Animation Events",
                $"Are you sure you want to delete {events.Count} selected events?",
                "Delete",
                "Cancel"))
            {
                return;
            }

            foreach (var evt in events)
            {
                // TODO: this is inefficient as we will be parsing the resulting AnimationClip over and over again
                AnimationClipWatcher.DeleteEventProgrammatically(clip, evt, $"Delete Multiple Events");
            }
        }

        [Serializable]
        private class SerializableAnimationEventList
        {
            public List<SerializableAnimationEvent> events;
        }

        private void BindEventItem(VisualElement element, int index)
        {
            var editor = GetAnimationWindow();
            float currentTime = editor.time;
            AnimationClip currentClip = getAttachedClip();

            // Get the current filtered events from the ListView's itemsSource
            var filteredEvents = listView.itemsSource as List<ParsedEngageAnimationEvent>;
            if (filteredEvents == null || index >= filteredEvents.Count) return;

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
using System;
using System.Collections.Generic;
using System.Linq;
using DivineDragon.EngageAnimationEvents;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace DivineDragon.Windows
{
    public class AddEventSearchWindow : EditorWindow
    {
        public AnimationClip targetClip;
        public float targetTime;
        public ParsedEngageAnimationEvent eventToModify;
        
        private string searchTerm = "";
        private Vector2 scrollPosition;
        private List<EngageAnimationEventParser<ParsedEngageAnimationEvent>> filteredEvents;
        private TextField searchField;
        private int selectedIndex = 0;
        private ScrollView scrollView;
        
        private void OnEnable()
        {
            titleContent = new GUIContent("Add Event");
            minSize = new Vector2(600, 400);
        }
        
        private void OnLostFocus()
        {
            // Keep the window open when Unity loses focus
            // The window will only close on Cancel, Escape, or clicking away
        }
        
        private void OnGUI()
        {
            Event e = Event.current;
            if (e.type == EventType.KeyDown)
            {
                bool handled = false;
                
                if (e.keyCode == KeyCode.DownArrow)
                {
                    NavigateSelection(1);
                    handled = true;
                }
                else if (e.keyCode == KeyCode.UpArrow)
                {
                    NavigateSelection(-1);
                    handled = true;
                }
                else if ((e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter) && selectedIndex >= 0)
                {
                    if (filteredEvents != null && selectedIndex < filteredEvents.Count)
                    {
                        AddEvent(filteredEvents[selectedIndex]);
                        handled = true;
                    }
                }
                else if (e.keyCode == KeyCode.Escape)
                {
                    if (eventToModify != null && string.IsNullOrEmpty(eventToModify.backingAnimationEvent.functionName))
                    {
                        AnimationClipWatcher.DeleteEventProgrammatically(targetClip, eventToModify, "Cancel Event Creation");
                    }
                    Close();
                    handled = true;
                }
                
                if (handled)
                {
                    e.Use();
                    Repaint();
                }
            }
        }
        
        private void CreateGUI()
        {
            var root = rootVisualElement;
            root.style.paddingTop = 5;
            root.style.paddingLeft = 5;
            root.style.paddingRight = 5;
            root.style.paddingBottom = 5;
            root.style.flexDirection = FlexDirection.Column;
            root.style.flexGrow = 1;
            
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.divinedragon.animation_tools/Editor/AnimationTools.uss");
            if (styleSheet != null)
            {
                root.styleSheets.Add(styleSheet);
            }
            
            searchField = new TextField("Search");
            searchField.style.marginBottom = 5;
            searchField.RegisterValueChangedCallback(evt => 
            {
                searchTerm = evt.newValue;
                selectedIndex = 0;
                FilterEvents();
            });
            searchField.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.DownArrow)
                {
                    NavigateSelection(1);
                    evt.StopPropagation();
                    evt.PreventDefault();
                }
                else if (evt.keyCode == KeyCode.UpArrow)
                {
                    NavigateSelection(-1);
                    evt.StopPropagation();
                    evt.PreventDefault();
                }
                else if ((evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter) && selectedIndex >= 0)
                {
                    if (filteredEvents != null && selectedIndex < filteredEvents.Count)
                    {
                        AddEvent(filteredEvents[selectedIndex]);
                        evt.StopPropagation();
                        evt.PreventDefault();
                    }
                }
                else if (evt.keyCode == KeyCode.Escape)
                {
                    if (eventToModify != null && string.IsNullOrEmpty(eventToModify.backingAnimationEvent.functionName))
                    {
                        AnimationClipWatcher.DeleteEventProgrammatically(targetClip, eventToModify, "Cancel Event Creation");
                    }
                    Close();
                    evt.StopPropagation();
                    evt.PreventDefault();
                }
            }, TrickleDown.TrickleDown);
            
            root.Add(searchField);
            var hintsLabel = new Label("↑↓ Navigate • Enter Select • Esc Close")
            {
                style =
                {
                    fontSize = 10,
                    color = new Color(0.6f, 0.6f, 0.6f),
                    marginBottom = 5,
                    unityTextAlign = TextAnchor.MiddleCenter
                }
            };
            root.Add(hintsLabel);
            var columnsContainer = new VisualElement();
            columnsContainer.style.flexDirection = FlexDirection.Row;
            columnsContainer.style.flexGrow = 1;
            scrollView = new ScrollView();
            scrollView.style.width = Length.Percent(50);
            scrollView.style.borderRightWidth = 1;
            scrollView.style.borderRightColor = new Color(0.3f, 0.3f, 0.3f);
            scrollView.focusable = false;
            
            var descriptionPanel = new VisualElement();
            descriptionPanel.name = "description-panel";
            descriptionPanel.style.width = Length.Percent(50);
            descriptionPanel.style.paddingLeft = 10;
            descriptionPanel.style.paddingRight = 10;
            descriptionPanel.style.paddingTop = 10;
            descriptionPanel.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.2f);
            var eventNameLabel = new Label();
            eventNameLabel.name = "event-name";
            eventNameLabel.style.fontSize = 14;
            eventNameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            eventNameLabel.style.marginBottom = 8;
            eventNameLabel.style.color = new Color(0.4f, 0.6f, 0.8f);
            descriptionPanel.Add(eventNameLabel);
            var descriptionLabel = new Label();
            descriptionLabel.name = "description-text";
            descriptionLabel.style.fontSize = 11;
            descriptionLabel.style.color = new Color(0.8f, 0.8f, 0.8f);
            descriptionLabel.style.whiteSpace = WhiteSpace.Normal;
            descriptionPanel.Add(descriptionLabel);
            columnsContainer.Add(scrollView);
            columnsContainer.Add(descriptionPanel);
            
            root.Add(columnsContainer);
            var bottomControls = new VisualElement();
            bottomControls.style.flexDirection = FlexDirection.Row;
            bottomControls.style.marginTop = 5;
            bottomControls.style.justifyContent = Justify.FlexEnd;
            bottomControls.style.flexShrink = 0;
            bottomControls.style.height = 25;
            var createButton = new Button(() =>
            {
                if (selectedIndex >= 0 && selectedIndex < filteredEvents.Count)
                {
                    AddEvent(filteredEvents[selectedIndex]);
                }
            })
            {
                text = "Add",
                style =
                {
                    height = 25,
                    width = 80
                }
            };
            createButton.name = "create-button";
            bottomControls.Add(createButton);
            
            root.Add(bottomControls);
            searchField.Focus();
            searchField.SelectAll();
            
            void UpdateResults()
            {
                scrollView.Clear();

                
                if (filteredEvents == null || filteredEvents.Count == 0)
                {
                    var noResultsLabel = new Label("No events found");
                    noResultsLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                    noResultsLabel.style.marginTop = 20;
                    scrollView.Add(noResultsLabel);
                    
                    var createButton = root.Q<Button>("create-button");
                    createButton?.SetEnabled(false);
                    
                    var eventNameLabel = root.Q<Label>("event-name");
                    var descriptionText = root.Q<Label>("description-text");
                    if (eventNameLabel != null) eventNameLabel.text = "No events found";
                    if (descriptionText != null) descriptionText.text = "";
                    
                    return;
                }
                
                for (int i = 0; i < filteredEvents.Count; i++)
                {
                    var index = i;
                    var parser = filteredEvents[i];
                    var evt = parser.sampleParsedEvent;
                    
                    var itemContainer = new VisualElement();
                    itemContainer.style.flexDirection = FlexDirection.Row;
                    itemContainer.style.paddingLeft = 10;
                    itemContainer.style.paddingRight = 10;
                    itemContainer.style.paddingTop = 5;
                    itemContainer.style.paddingBottom = 5;
                    itemContainer.style.marginBottom = 2;
                    itemContainer.focusable = false;
                    
                    itemContainer.RegisterCallback<MouseEnterEvent>(e => 
                    {
                        if (index != selectedIndex)
                            itemContainer.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 0.2f);
                    });
                    itemContainer.RegisterCallback<MouseLeaveEvent>(e => 
                    {
                        if (index != selectedIndex)
                            itemContainer.style.backgroundColor = Color.clear;
                    });
                    
                    itemContainer.RegisterCallback<MouseDownEvent>(e =>
                    {
                        if (e.button == 0)
                        {
                            selectedIndex = index;
                            UpdateSelection();
                            e.StopPropagation();
                            
                            searchField.Focus();
                        }
                    });
                    var nameContainer = new VisualElement();
                    nameContainer.style.flexDirection = FlexDirection.Row;
                    nameContainer.style.flexGrow = 1;
                    nameContainer.style.alignItems = Align.Center;
                    nameContainer.focusable = false;
                    
                    CreateHighlightedText(nameContainer, evt.displayName, searchTerm);
                    
                    itemContainer.Add(nameContainer);
                    
                    var categoryLabel = new Label(evt.category.GetDescription());
                    categoryLabel.style.fontSize = 10;
                    categoryLabel.style.color = new Color(0.6f, 0.6f, 0.6f);
                    categoryLabel.focusable = false;
                    itemContainer.Add(categoryLabel);
                    
                    itemContainer.userData = index;
                    
                    scrollView.Add(itemContainer);
                }
                
                var createBtn = root.Q<Button>("create-button");
                createBtn?.SetEnabled(true);
                
                UpdateSelection();
            }
            
            void CreateHighlightedText(VisualElement container, string text, string searchTerm)
            {
                if (string.IsNullOrEmpty(searchTerm) || string.IsNullOrEmpty(text))
                {
                    var label = new Label(text);
                    label.style.marginLeft = 0;
                    label.style.marginRight = 0;
                    label.style.marginTop = 0;
                    label.style.marginBottom = 0;
                    label.style.paddingLeft = 0;
                    label.style.paddingRight = 0;
                    label.style.paddingTop = 0;
                    label.style.paddingBottom = 0;
                    label.focusable = false;
                    container.Add(label);
                    return;
                }
                
                var textLower = text.ToLower();
                var searchLower = searchTerm.ToLower();
                var lastIndex = 0;
                
                while (true)
                {
                    var matchIndex = textLower.IndexOf(searchLower, lastIndex);
                    if (matchIndex < 0)
                    {
                        if (lastIndex < text.Length)
                        {
                            var remainingLabel = new Label(text.Substring(lastIndex));
                            remainingLabel.style.marginLeft = 0;
                            remainingLabel.style.marginRight = 0;
                            remainingLabel.style.marginTop = 0;
                            remainingLabel.style.marginBottom = 0;
                            remainingLabel.style.paddingLeft = 0;
                            remainingLabel.style.paddingRight = 0;
                            remainingLabel.style.paddingTop = 0;
                            remainingLabel.style.paddingBottom = 0;
                            remainingLabel.focusable = false;
                            container.Add(remainingLabel);
                        }
                        break;
                    }
                    
                    if (matchIndex > lastIndex)
                    {
                        var beforeLabel = new Label(text.Substring(lastIndex, matchIndex - lastIndex));
                        beforeLabel.style.marginLeft = 0;
                        beforeLabel.style.marginRight = 0;
                        beforeLabel.style.marginTop = 0;
                        beforeLabel.style.marginBottom = 0;
                        beforeLabel.style.paddingLeft = 0;
                        beforeLabel.style.paddingRight = 0;
                        beforeLabel.style.paddingTop = 0;
                        beforeLabel.style.paddingBottom = 0;
                        beforeLabel.focusable = false;
                        container.Add(beforeLabel);
                    }
                    
                    var matchLabel = new Label(text.Substring(matchIndex, searchTerm.Length));
                    matchLabel.style.backgroundColor = new Color(1f, 0.8f, 0.2f, 0.3f);
                    matchLabel.style.color = Color.white;
                    matchLabel.style.marginLeft = 0;
                    matchLabel.style.marginRight = 0;
                    matchLabel.style.marginTop = 0;
                    matchLabel.style.marginBottom = 0;
                    matchLabel.style.paddingLeft = 0;
                    matchLabel.style.paddingRight = 0;
                    matchLabel.style.paddingTop = 0;
                    matchLabel.style.paddingBottom = 0;
                    matchLabel.focusable = false;
                    container.Add(matchLabel);
                    
                    lastIndex = matchIndex + searchTerm.Length;
                }
            }
            
            // Helper to update selection visuals
            void UpdateSelection()
            {
                var children = scrollView.Children().ToList();
                VisualElement selectedElement = null;
                
                for (int i = 0; i < children.Count; i++)
                {
                    var child = children[i];
                    if (child.userData is int index)
                    {
                        if (index == selectedIndex)
                        {
                            child.style.backgroundColor = new Color(0.3f, 0.5f, 0.7f, 0.3f);
                            child.style.borderLeftWidth = 3;
                            child.style.borderLeftColor = new Color(0.4f, 0.6f, 0.8f);
                            child.style.paddingLeft = 7;
                            
                            selectedElement = child;
                            
                            // Ensure selected item is visible
                            scrollView.ScrollTo(child);
                        }
                        else
                        {
                            child.style.backgroundColor = Color.clear;
                            child.style.borderLeftWidth = 0;
                            child.style.paddingLeft = 10;
                        }
                    }
                }
                
                // Update description panel
                var eventNameLabel = root.Q<Label>("event-name");
                var descriptionText = root.Q<Label>("description-text");
                var createButton = root.Q<Button>("create-button");
                
                if (selectedIndex >= 0 && selectedIndex < filteredEvents.Count)
                {
                    var evt = filteredEvents[selectedIndex].sampleParsedEvent;
                    if (eventNameLabel != null) eventNameLabel.text = evt.displayName;
                    if (descriptionText != null) descriptionText.text = evt.Explanation;
                    if (createButton != null) 
                    {
                        createButton.SetEnabled(true);
                    }
                }
                else
                {
                    if (eventNameLabel != null) eventNameLabel.text = "No event selected";
                    if (descriptionText != null) descriptionText.text = "Select an event to see its details.";
                    if (createButton != null) createButton.SetEnabled(false);
                }
            }
            
            // Store update function for filtering
            searchField.userData = new System.Action(UpdateResults);
            
            // Initial filter - must happen before UpdateResults
            FilterEvents();
            UpdateResults();
            
            // Ensure create button is enabled if we have results
            if (filteredEvents != null && filteredEvents.Count > 0)
            {
                var createBtn = root.Q<Button>("create-button");
                if (createBtn != null)
                {
                    createBtn.SetEnabled(true);
                }
            }
        }
        
        private void NavigateSelection(int direction)
        {
            if (filteredEvents == null || filteredEvents.Count == 0) return;
            
            int newIndex = selectedIndex + direction;
            
            // Wrap around
            if (newIndex < 0) newIndex = filteredEvents.Count - 1;
            if (newIndex >= filteredEvents.Count) newIndex = 0;
            
            selectedIndex = newIndex;
            
            // Update visual selection by calling the update function
            if (searchField?.userData is System.Action updateAction)
            {
                // Find the UpdateSelection function and call it
                // This is cleaner than duplicating the logic
                var updateSelectionAction = rootVisualElement.Q<TextField>()?.userData as System.Action;
                if (updateSelectionAction != null)
                {
                    // Get the UpdateResults action which contains UpdateSelection
                    var scrollView = rootVisualElement.Q<ScrollView>();
                    if (scrollView != null)
                    {
                        // First update the visual selection
                        var children = scrollView.Children().ToList();
                        VisualElement selectedElement = null;
                        
                        for (int i = 0; i < children.Count; i++)
                        {
                            var child = children[i];
                            if (child.userData is int index)
                            {
                                if (index == selectedIndex)
                                {
                                    child.style.backgroundColor = new Color(0.3f, 0.5f, 0.7f, 0.3f);
                                    child.style.borderLeftWidth = 3;
                                    child.style.borderLeftColor = new Color(0.4f, 0.6f, 0.8f);
                                    child.style.paddingLeft = 7;
                                    selectedElement = child;
                                    scrollView.ScrollTo(child);
                                }
                                else
                                {
                                    child.style.backgroundColor = Color.clear;
                                    child.style.borderLeftWidth = 0;
                                    child.style.paddingLeft = 10;
                                }
                            }
                        }
                        
                        // Update description panel
                        var eventNameLabel = rootVisualElement.Q<Label>("event-name");
                        var descriptionText = rootVisualElement.Q<Label>("description-text");
                        var createButton = rootVisualElement.Q<Button>("create-button");
                        
                        if (selectedIndex >= 0 && selectedIndex < filteredEvents.Count)
                        {
                            var evt = filteredEvents[selectedIndex].sampleParsedEvent;
                            if (eventNameLabel != null)
                            {
                                eventNameLabel.text = evt.displayName;
                            }
                            if (descriptionText != null)
                            {
                                descriptionText.text = evt.Explanation;
                            }
                            if (createButton != null)
                            {
                                createButton.SetEnabled(true);
                            }
                        }
                    }
                }
            }
        }
        
        
        private void FilterEvents()
        {
            // Force static constructor to run if it hasn't already
            var dummy = new AnimationEventParser();
            
            var allEvents = AnimationEventParser.SupportedEvents;
            
            // Safety check - ensure we have events
            if (allEvents == null || allEvents.Count == 0)
            {
                // AnimationEventParser.SupportedEvents is empty or null
                filteredEvents = new List<EngageAnimationEventParser<ParsedEngageAnimationEvent>>();
                return;
            }
            
            if (string.IsNullOrEmpty(searchTerm))
            {
                filteredEvents = allEvents.OrderBy(e => e.sampleParsedEvent.displayName).ToList();
            }
            else
            {
                var lowerSearchTerm = searchTerm.ToLower();
                
                filteredEvents = allEvents.Where(e =>
                {
                    // Search in display name
                    if (e.sampleParsedEvent.displayName.ToLower().Contains(lowerSearchTerm))
                        return true;
                        
                    // Search in original/Japanese names
                    foreach (var rule in e.matchRules)
                    {
                        if (rule is FunctionNameMatchRule fnRule && fnRule.functionName.ToLower().Contains(lowerSearchTerm))
                            return true;
                        if (rule is FunctionNameStringParameterMatchRule fnsRule && fnsRule.stringParameter.ToLower().Contains(lowerSearchTerm))
                            return true;
                    }
                    if (IsFuzzyMatch(e.sampleParsedEvent.displayName.ToLower(), lowerSearchTerm))
                        return true;
                    
                    return false;
                }).ToList();
                filteredEvents = filteredEvents.OrderBy(e =>
                {
                    var name = e.sampleParsedEvent.displayName.ToLower();
                    if (name == lowerSearchTerm) return 0;
                    if (name.StartsWith(lowerSearchTerm)) return 1;
                    if (name.Contains(lowerSearchTerm)) return 2;
                    return 3;
                }).ToList();
            }
            if (searchField?.userData is System.Action updateAction)
            {
                updateAction();
            }
        }
        
        private bool IsFuzzyMatch(string text, string pattern)
        {
            int patternIndex = 0;
            foreach (char c in text)
            {
                if (patternIndex < pattern.Length && c == pattern[patternIndex])
                {
                    patternIndex++;
                }
            }
            return patternIndex == pattern.Length;
        }
        
        
        private void AddEvent(EngageAnimationEventParser<ParsedEngageAnimationEvent> parser)
        {
            if (targetClip == null) return;
            
            if (eventToModify != null)
            {
                var modifiedEvent = parser.Create();
                modifiedEvent.time = eventToModify.backingAnimationEvent.time;
                AnimationClipWatcher.ReplaceEventProgrammatically(targetClip, eventToModify, modifiedEvent, 
                    $"Configure Event as {parser.sampleParsedEvent.displayName}");
            }
            else
            {
                var newEvent = parser.Create();
                newEvent.time = targetTime;
                AnimationClipWatcher.AddEventProgrammatically(targetClip, newEvent, 
                    $"Add {parser.sampleParsedEvent.displayName} Event");
            }
            
            Close();
        }
    }
}
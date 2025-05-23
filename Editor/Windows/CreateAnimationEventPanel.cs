using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace DivineDragon.Windows
{
    public static class EnumExtensions
    {
        public static string GetDescription(this System.Enum enumValue)
        {
            var field = enumValue.GetType().GetField(enumValue.ToString());
            if (field == null)
                return enumValue.ToString();

            if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
            {
                return attribute.Description;
            }

            return enumValue.ToString();
        }
    }
    
    public class CreateAnimationEventPanel: AnimationEditorInspectorHelper
    {
        
        [MenuItem("Divine Dragon/Animation Tools/Event Palette")]
        public static void ShowExample()
        {
            CreateAnimationEventPanel wnd = GetWindow<CreateAnimationEventPanel>();
            wnd.titleContent = new GUIContent("Event Palette");
        }

        public new void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            root = rootVisualElement;
            CreateAnimationEventPanelUI(root);
        }
         
        protected override void OnUnderlyingAnimationClipChanged()
        {
            root.Clear();
            CreateAnimationEventPanelUI(root);
        }

        public void CreateAnimationEventPanelUI(VisualElement myInspector)
        {
            // Create the scrollable view first
            var scrollable = new ScrollView();
            
            // Declare RebuildEventList function
            Action<string> RebuildEventList = (string searchTerm) =>
            {
                scrollable.Clear();
                
                List<EngageAnimationEventParser<ParsedEngageAnimationEvent>> events = AnimationEventParser.SupportedEvents;
                
                // Filter events based on search term
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    events = events.Where(e => 
                    {
                        var lowerSearchTerm = searchTerm.ToLower();
                        
                        // Check display name
                        if (e.sampleParsedEvent.displayName.ToLower().Contains(lowerSearchTerm))
                            return true;
                            
                        // Check explanation/tooltip
                        if (!string.IsNullOrEmpty(e.sampleParsedEvent.Explanation) && 
                            e.sampleParsedEvent.Explanation.ToLower().Contains(lowerSearchTerm))
                            return true;
                            
                        // Check category description
                        var categoryDescription = e.sampleParsedEvent.category.GetDescription();
                        if (!string.IsNullOrEmpty(categoryDescription) && 
                            categoryDescription.ToLower().Contains(lowerSearchTerm))
                            return true;
                            
                        // Check Japanese names from match rules
                        foreach (var rule in e.matchRules)
                        {
                            if (rule is FunctionNameMatchRule fnRule && fnRule.functionName.Contains(searchTerm))
                                return true;
                            if (rule is FunctionNameStringParameterMatchRule fnsRule && fnsRule.stringParameter.Contains(searchTerm))
                                return true;
                        }
                        
                        return false;
                    }).ToList();
                }
                
                Dictionary<ParsedEngageAnimationEvent.EventCategory, HashSet<EngageAnimationEventParser<ParsedEngageAnimationEvent>>> categorizedEvents = 
                    new Dictionary<ParsedEngageAnimationEvent.EventCategory, HashSet<EngageAnimationEventParser<ParsedEngageAnimationEvent>>>();

                foreach (ParsedEngageAnimationEvent.EventCategory category in System.Enum.GetValues(typeof(ParsedEngageAnimationEvent.EventCategory)))
                {
                    categorizedEvents[category] = new HashSet<EngageAnimationEventParser<ParsedEngageAnimationEvent>>();
                }
                
                foreach (var eventParser in events)
                {
                    ParsedEngageAnimationEvent.EventCategory category = eventParser.sampleParsedEvent.category;
                    categorizedEvents[category].Add(eventParser);
                }
                
                foreach (var kvp in categorizedEvents)
                {
                    var category = kvp.Key;
                    var eventParsers = kvp.Value;
                    
                    // Skip empty categories
                    if (eventParsers.Count == 0) continue;
                    
                    // Create a container for the category
                    var categoryContainer = new VisualElement
                    {
                        style =
                        {
                            marginTop = 8,
                            marginBottom = 2,
                            marginLeft = 10,
                            marginRight = 10
                        }
                    };
                    
                    // Create a subtle section header
                    var headerLabel = new Label(category.GetDescription())
                    {
                        style =
                        {
                            fontSize = 12,
                            unityFontStyleAndWeight = FontStyle.Bold,
                            color = new Color(0.7f, 0.7f, 0.7f, 1f),
                            marginBottom = 5,
                            paddingBottom = 2,
                            borderBottomWidth = 1,
                            borderBottomColor = new Color(0.3f, 0.3f, 0.3f, 0.3f)
                        }
                    };
                    categoryContainer.Add(headerLabel);

                    // Create a container for the event buttons
                    var eventsContainer = new VisualElement
                    {
                        style =
                        {
                            paddingLeft = 5
                        }
                    };

                    foreach (var eventParser in eventParsers)
                    {
                        var eventButton = new Button(() =>
                        {
                            addAnimationEvent(getAttachedClip(), CurrentTime, eventParser.sampleParsedEvent.displayName);
                        })
                        {
                            text = eventParser.sampleParsedEvent.displayName,
                            tooltip = eventParser.sampleParsedEvent.Explanation,
                            style =
                            {
                                marginTop = 1,
                                marginBottom = 1,
                                paddingTop = 2,
                                paddingBottom = 2
                            }
                        };
                        eventsContainer.Add(eventButton);
                    }
                    
                    categoryContainer.Add(eventsContainer);
                    scrollable.Add(categoryContainer);
                }
            };
            
            // Create a container for search field and clear button
            var searchContainer = new VisualElement()
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    height = 20,
                    marginBottom = 5,
                    marginTop = 5
                }
            };
            
            // Add search field
            var searchField = new TextField("Search");
            searchField.labelElement.style.minWidth = 50;
            searchField.labelElement.style.width = 50;
            searchField.style.flexGrow = 3;
            searchField.style.height = 20;
            searchContainer.Add(searchField);
            
            // Add clear button
            var clearButton = new Button(() =>
            {
                searchField.value = "";
                RebuildEventList("");
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
            clearButton.SetEnabled(!string.IsNullOrEmpty(searchField.value));
            
            searchContainer.Add(clearButton);
            
            // Set up search functionality
            searchField.RegisterValueChangedCallback(evt =>
            {
                RebuildEventList(evt.newValue);
                // Enable/disable clear button based on search text
                clearButton.SetEnabled(!string.IsNullOrEmpty(evt.newValue));
            });
            
            // Initial build
            RebuildEventList("");
            
            myInspector.Add(searchContainer);
            myInspector.Add(scrollable);
        }
        
        private void addAnimationEvent(AnimationClip currentClip, float time, string animType)
        {
            var parser = AnimationEventParser.displayNameToParser[animType];

            AnimationEvent newEvent = parser.Create();
            newEvent.time = time;
            // In the future: set meaningful defaults in the parser.Create() method?
            AnimationClipWatcher.AddEventProgrammatically(currentClip, newEvent, "Add Animation Event");
        }
    }
}
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
            // Add search field at the top
            var searchField = new TextField("Search Events")
            {
                style =
                {
                    marginBottom = 10,
                    marginLeft = 10,
                    marginRight = 10,
                    marginTop = 10
                }
            };
            
            var scrollable = new ScrollView();

            void RebuildEventList(string searchTerm = "")
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
                    
                    var categoryContainer = new Foldout
                    {
                        text = category.GetDescription(),
                        value = !string.IsNullOrEmpty(searchTerm) ? true : EditorPrefs.GetBool($"CreateAnimationEventPanel_Foldout_{category}", true),
                        style =
                        {
                            // set margins for the category container
                            marginTop = 5,
                            marginBottom = 5,
                            marginLeft = 10,
                            marginRight = 10
                        }
                    };
                    
                    // Only save foldout state when not searching
                    if (string.IsNullOrEmpty(searchTerm))
                    {
                        categoryContainer.RegisterValueChangedCallback(evt =>
                        {
                            EditorPrefs.SetBool($"CreateAnimationEventPanel_Foldout_{category}", evt.newValue);
                        });
                    }

                    foreach (var eventParser in eventParsers)
                    {
                        var eventButton = new Button(() =>
                        {
                            addAnimationEvent(getAttachedClip(), CurrentTime, eventParser.sampleParsedEvent.displayName);
                        })
                        {
                            text = eventParser.sampleParsedEvent.displayName,
                            tooltip = eventParser.sampleParsedEvent.Explanation,
                        };
                        categoryContainer.Add(eventButton);
                    }
                    scrollable.Add(categoryContainer);
                }
            }
            
            // Set up search functionality
            searchField.RegisterValueChangedCallback(evt =>
            {
                RebuildEventList(evt.newValue);
            });
            
            // Initial build
            RebuildEventList();
            
            myInspector.Add(searchField);
            myInspector.Add(scrollable);
        }
        
        private void addAnimationEvent(AnimationClip currentClip, float time, string animType)
        {
            Undo.RegisterCompleteObjectUndo(currentClip, "Add Animation Event");
            var parser = AnimationEventParser.displayNameToParser[animType];

            AnimationEvent newEvent = parser.Create();
            newEvent.time = time;
            // In the future: set meaningful defaults in the parser.Create() method?
            AnimationClipWatcher.AddEventProgrammatically(currentClip, newEvent);
        }
    }
}
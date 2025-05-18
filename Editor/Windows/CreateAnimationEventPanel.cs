using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace DivineDragon.Windows
{
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
            var scrollable = new ScrollView();

            List<EngageAnimationEventParser<ParsedEngageAnimationEvent>> events = AnimationEventParser.SupportedEvents;
            
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
                var categoryContainer = new Foldout
                {
                    text = category.ToString(),
                    style =
                    {
                        // set margins for the category container
                        marginTop = 10,
                        marginBottom = 10,
                        marginLeft = 10,
                        marginRight = 10
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
                    };
                    categoryContainer.Add(eventButton);
                }
                scrollable.Add(categoryContainer);
            }
            
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
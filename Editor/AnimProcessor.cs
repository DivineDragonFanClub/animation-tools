using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Combat;
using DivineDragon.EngageAnimationEvents;
using UnityEngine;
using UnityEngine.UIElements;

namespace DivineDragon
{
    public class AnimationEventParser
    {
        public static List<EngageAnimationEventParser<ParsedEngageAnimationEvent>> SupportedEvents =
            Assembly.GetAssembly(typeof(EngageAnimationEventParser<ParsedEngageAnimationEvent>))
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(EngageAnimationEventParser<ParsedEngageAnimationEvent>)) &&
                            !t.IsAbstract)
                .Select(t => (EngageAnimationEventParser<ParsedEngageAnimationEvent>)Activator.CreateInstance(t))
                .ToList();

        public static Dictionary<string, EngageAnimationEventParser<ParsedEngageAnimationEvent>> displayNameToParser =>
            SupportedEvents
                .ToDictionary(parser => parser.sampleParsedEvent.displayName, parser => parser);

        private static Dictionary<string, List<EngageAnimationEventParser<ParsedEngageAnimationEvent>>>
            _functionNameToParsers;

        public AnimationEventParser()
        {
            InitializeParserHashmap();
        }

        private static void InitializeParserHashmap()
        {
            if (_functionNameToParsers != null)
                return;

            _functionNameToParsers =
                new Dictionary<string, List<EngageAnimationEventParser<ParsedEngageAnimationEvent>>>();

            foreach (var parser in SupportedEvents)
            {
                foreach (var rule in parser.matchRules)
                {
                    string functionName = null;

                    if (rule is FunctionNameMatchRule functionRule)
                    {
                        functionName = functionRule.functionName;
                    }
                    else if (rule is FunctionNameStringParameterMatchRule stringParamRule)
                    {
                        functionName = stringParamRule.functionName;
                    }

                    if (functionName != null)
                    {
                        if (!_functionNameToParsers.TryGetValue(functionName, out var parsers))
                        {
                            parsers = new List<EngageAnimationEventParser<ParsedEngageAnimationEvent>>();
                            _functionNameToParsers[functionName] = parsers;
                        }

                        if (!parsers.Contains(parser))
                        {
                            parsers.Add(parser);
                        }
                    }
                }
            }
        }
        
        public static List<ParsedEngageAnimationEvent> ParseAnimationEvents(AnimationEvent[] events)
        {
            var startTime = DateTime.Now;
            List<ParsedEngageAnimationEvent> parsedEvents = new List<ParsedEngageAnimationEvent>();

            // Make sure hashmap is initialized
            if (_functionNameToParsers == null)
                InitializeParserHashmap();

            for (int i = 0; i < events.Length; i++)
            {
                AnimationEvent animEvent = events[i];
                ParsedEngageAnimationEvent parsedEvent = null;
                bool wasParsed = false;

                // First check if we have parsers for this function name
                if (_functionNameToParsers.TryGetValue(animEvent.functionName, out var candidateParsers))
                {
                    // If only one parser, we can use it directly
                    if (candidateParsers.Count == 1)
                    {
                        var parser = candidateParsers[0];
                        parsedEvent = parser.ParseFrom(animEvent);
                        wasParsed = true;
                    }
                    else
                    {
                        // Multiple parsers for same function name - check each one
                        foreach (var parser in candidateParsers)
                        {
                            if (parser.IsMatch(animEvent))
                            {
                                parsedEvent = parser.ParseFrom(animEvent);
                                wasParsed = true;
                                break;
                            }
                        }
                    }
                }

                if (!wasParsed)
                {
                    parsedEvent = new UnknownEvent(animEvent);
                }

                if (parsedEvent == null)
                {
                    Debug.LogWarning(
                        $"Failed to parse AnimationEvent at index {i} with function name: {animEvent.functionName}");
                    continue;
                }

                parsedEvents.Add(parsedEvent);
            }

            var endTime = DateTime.Now;
            var elapsedTime = endTime - startTime;
            Debug.Log($"Parsed {parsedEvents.Count} AnimationEvents in {elapsedTime.TotalMilliseconds} ms");
            return parsedEvents;
        }
        
        public static List<ParsedEngageAnimationEvent> GetParsedEventsAroundTime(
            List<ParsedEngageAnimationEvent> events, float time, float tolerance = 0.1f)
        {
            List<ParsedEngageAnimationEvent> parsedEventsAroundTime = new List<ParsedEngageAnimationEvent>();
            foreach (ParsedEngageAnimationEvent animEvent in events)
            {
                if (Mathf.Abs(animEvent.backingAnimationEvent.time - time) <= tolerance)
                {
                    parsedEventsAroundTime.Add(animEvent);
                }
            }

            return parsedEventsAroundTime;
        }
    }

    public abstract class MatchRule
    {
        public abstract bool isMatch(AnimationEvent animEvent);
    }

    public class FunctionNameMatchRule : MatchRule
    {
        public string functionName;

        public FunctionNameMatchRule(string functionName)
        {
            this.functionName = functionName;
        }

        public override bool isMatch(AnimationEvent animEvent)
        {
            return animEvent.functionName == functionName;
        }
    }

    public class FunctionNameStringParameterMatchRule : MatchRule
    {
        public string functionName;
        public string stringParameter;

        public FunctionNameStringParameterMatchRule(string functionName, string stringParameter)
        {
            this.functionName = functionName;
            this.stringParameter = stringParameter;
        }

        public override bool isMatch(AnimationEvent animEvent)
        {
            return animEvent.functionName == functionName && animEvent.stringParameter == stringParameter;
        }
    }

    public abstract class ParsedEngageAnimationEvent
    {
        public AnimationEvent backingAnimationEvent;
        
        public string Uuid { get; set; } = Guid.NewGuid().ToString();

        public enum ExposedPropertyType
        {
            Float,
            Int,
            String,
            ObjectReference,
            FunctionName
        }

        public enum EventCategory
        {
            [System.ComponentModel.Description("Foot Left")]
            FootLeft,
            [System.ComponentModel.Description("Foot Right")]
            FootRight,
            [System.ComponentModel.Description("Sound")]
            Sound,
            [System.ComponentModel.Description("Attacking Character")]
            AttackingCharacter,
            [System.ComponentModel.Description("Particle")]
            Particle,
            [System.ComponentModel.Description("Camera")]
            Camera,
            [System.ComponentModel.Description("Cancels")]
            Cancels,
            [System.ComponentModel.Description("Attack Specifics")]
            AttackSpecifics,
            [System.ComponentModel.Description("Opponent")]
            Opponent,
            [System.ComponentModel.Description("Weapon Control")]
            WeaponControl,
            [System.ComponentModel.Description("Motion Control")]
            MotionControl,
            [System.ComponentModel.Description("Riders")]
            Riders,
            [System.ComponentModel.Description("Uncategorized")]
            Uncategorized
        }

        public abstract HashSet<ExposedPropertyType> exposedProperties { get; }

        public virtual EventCategory category => EventCategory.Uncategorized;

        // English string to display in the UI instead of the occasional Japanese
        public abstract string displayName { get; }
        
        // Original Japanese name of this event
        public virtual string originalName => backingAnimationEvent.functionName;
        
        public virtual VisualElement MakeSpecialEditor(Action<ParsedEngageAnimationEvent, AnimationEvent> onSave,
            List<ParsedEngageAnimationEvent> events)
        {
            return null;
        }

        public virtual void OnScrubbedTo(AnimationEditor go, List<ParsedEngageAnimationEvent> events)
        {
            // Default implementation - subclasses can override if needed
        }

        public virtual void AlwaysRender(AnimationEditor go, List<ParsedEngageAnimationEvent> events)
        {
            // Implement any rendering logic that should always be done
        }
        
        public virtual string Explanation
        {
            get
            {
                return "This event is not yet implemented.";
            }
        }

        public virtual string Summary
        {
            get
            {
                return "This event is not yet implemented.";
            }
        }

        public static T GetFirstEventOfType<T>(List<ParsedEngageAnimationEvent> events)
            where T : ParsedEngageAnimationEvent
        {
            foreach (var e in events)
            {
                if (e is T typedEvent)
                {
                    return typedEvent;
                }
            }

            return null;
        }

        public static readonly IEqualityComparer<ParsedEngageAnimationEvent> EventComparer = new EventEqualityComparer();

        private class EventEqualityComparer : IEqualityComparer<ParsedEngageAnimationEvent>
        {
            public bool Equals(ParsedEngageAnimationEvent x, ParsedEngageAnimationEvent y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (x is null || y is null) return false;
                var a = x.backingAnimationEvent;
                var b = y.backingAnimationEvent;
                return a.time == b.time &&
                       a.functionName == b.functionName &&
                       a.intParameter == b.intParameter &&
                       a.floatParameter == b.floatParameter &&
                       a.stringParameter == b.stringParameter &&
                       Equals(a.objectReferenceParameter, b.objectReferenceParameter);
            }

            public int GetHashCode(ParsedEngageAnimationEvent obj)
            {
                var a = obj.backingAnimationEvent;
                unchecked
                {
                    int hash = 17;
                    hash = hash * 23 + a.time.GetHashCode();
                    hash = hash * 23 + (a.functionName?.GetHashCode() ?? 0);
                    hash = hash * 23 + a.intParameter.GetHashCode();
                    hash = hash * 23 + a.floatParameter.GetHashCode();
                    hash = hash * 23 + (a.stringParameter?.GetHashCode() ?? 0);
                    hash = hash * 23 + (a.objectReferenceParameter?.GetHashCode() ?? 0);
                    return hash;
                }
            }
        }
    }

    public abstract class EngageAnimationEventParser<T> where T : ParsedEngageAnimationEvent
    {
        // Used to express which animation events can be parsed into this type of event
        public abstract MatchRule[] matchRules { get; }

        public bool IsMatch(AnimationEvent animEvent)
        {
            foreach (MatchRule matchRule in matchRules)
            {
                if (matchRule.isMatch(animEvent))
                {
                    return true;
                }
            }

            return false;
        }

        public abstract T ParseFrom(AnimationEvent animEvent);

        public virtual AnimationEvent Create()
        {
            var myEvent = new AnimationEvent();
            // Use the first match rule to create a new AnimationEvent
            if (matchRules.Length == 0)
            {
                return myEvent;
            }
            var matchRule = matchRules[0];
            if (matchRule is FunctionNameMatchRule)
            {
                myEvent.functionName = (matchRule as FunctionNameMatchRule).functionName;
            }
            else if (matchRule is FunctionNameStringParameterMatchRule)
            {
                myEvent.functionName = (matchRule as FunctionNameStringParameterMatchRule).functionName;
                myEvent.stringParameter = (matchRule as FunctionNameStringParameterMatchRule).stringParameter;
            }

            return myEvent;
        }

        public T sampleParsedEvent => ParseFrom(Create());
    }
}
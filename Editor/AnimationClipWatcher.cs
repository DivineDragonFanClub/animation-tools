using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Combat;

[InitializeOnLoad]
public static class AnimationClipWatcher
{
    // Store hashes of watched clips to detect changes
    private class ClipHashData
    {
        public int eventsHash;
        public AnimationClip clip;
    }

    private static Dictionary<int, ClipHashData> _watchedClips = new Dictionary<int, ClipHashData>();

    private static Dictionary<int, List<ParsedEngageAnimationEvent>> _parsedEventsCache =
        new Dictionary<int, List<ParsedEngageAnimationEvent>>();


    // Event that fires when a clip's events change
    public static event Action<AnimationClip, HashSet<string>> OnClipEventsChanged;


    // Static constructor gets called when Unity starts and after domain reloads
    static AnimationClipWatcher()
    {
        EditorApplication.update += CheckForChanges;
        AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
        AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;

        Debug.Log("AnimationClipWatcher initialized as editor service");
    }

    private static void OnBeforeAssemblyReload()
    {
        // Save state before domain reload if needed
        EditorApplication.update -= CheckForChanges;
    }

    private static void OnAfterAssemblyReload()
    {
        // Ensure we're properly connected
        EditorApplication.update += CheckForChanges;

        // Restore watched clips
        RestoreWatchedClips();
    }

    private static void RestoreWatchedClips()
    {
        foreach (string guid in AnimationClipWatcherState.instance.GetWatchedClipGUIDs())
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!string.IsNullOrEmpty(path))
            {
                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                if (clip != null)
                {
                    WatchClip(clip);
                }
            }
        }
    }
    // Start watching a clip for changes

    public static void WatchClip(AnimationClip clip)
    {
        if (clip == null) return;

        int instanceID = clip.GetInstanceID();
        if (!_watchedClips.ContainsKey(instanceID))
        {
            _watchedClips[instanceID] = new ClipHashData
            {
                clip = clip,
                eventsHash = ComputeEventsHash(clip.events)
            };

            // Store in persistent state
            string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(clip));
            if (!string.IsNullOrEmpty(guid))
            {
                AnimationClipWatcherState.instance.AddClipGUID(guid);
            }

            // Cache parsed events
            _parsedEventsCache[instanceID] = AnimationEventParser.ParseAnimationEvents(clip.events);

            Debug.Log($"Started watching clip: {clip.name}");
        }
    }

    // Stop watching a clip
    public static void UnwatchClip(AnimationClip clip)
    {
        if (clip == null) return;

        int instanceID = clip.GetInstanceID();
        if (_watchedClips.ContainsKey(instanceID))
        {
            _watchedClips.Remove(instanceID);
            _parsedEventsCache.Remove(instanceID);
            Debug.Log($"Stopped watching clip: {clip.name}");
        }
    }

    // Compute a hash of animation events to detect changes
    private static int ComputeEventsHash(AnimationEvent[] events)
    {
        unchecked
        {
            int hash = 17;

            foreach (var evt in events)
            {
                hash = hash * 23 + evt.time.GetHashCode();
                hash = hash * 23 + (evt.functionName?.GetHashCode() ?? 0);
                hash = hash * 23 + evt.intParameter.GetHashCode();
                hash = hash * 23 + evt.floatParameter.GetHashCode();
                hash = hash * 23 + (evt.stringParameter?.GetHashCode() ?? 0);
                hash = hash * 23 + (evt.objectReferenceParameter != null
                    ? evt.objectReferenceParameter.GetInstanceID()
                    : 0);
            }

            return hash;
        }
    }

    // Check all watched clips for changes
    private static void CheckForChanges()
    {
        // Skip checking if user is dragging something in the editor
        // Actually, the animation track still lags even if this is skipped - why?
        // if (EditorGUIUtility.hotControl != 0)
        // {
        //     return;
        // }

        List<int> keysToRemove = new List<int>();

        foreach (var entry in _watchedClips)
        {
            int instanceID = entry.Key;
            ClipHashData data = entry.Value;

            // Make sure clip reference is still valid
            if (data.clip == null)
            {
                keysToRemove.Add(instanceID);
                continue;
            }

            int currentHash = ComputeEventsHash(data.clip.events);
            if (currentHash != data.eventsHash)
            {
                // Update stored hash
                data.eventsHash = currentHash;

                // Notify listeners
                Debug.Log($"Animation events changed in clip outside of our tool control: {data.clip.name}");
                // Update parsed events cache

                _parsedEventsCache[instanceID] = AnimationEventParser.ParseAnimationEvents(data.clip.events);

                OnClipEventsChanged?.Invoke(data.clip, new HashSet<string>());
            }
        }

        // Clean up any invalid clips
        foreach (int key in keysToRemove)
        {
            _watchedClips.Remove(key);
        }
    }

    public static void ReplaceEventProgrammatically(AnimationClip clip, ParsedEngageAnimationEvent eventToEdit,
        AnimationEvent newEvent)
    {
        if (clip == null || eventToEdit == null || newEvent == null)
            return;

        
        AnimationEvent[] currentEvents = clip.events;
        bool eventFound = false;
        AnimationEvent[] updatedEvents = new AnimationEvent[currentEvents.Length];

        for (int i = 0; i < currentEvents.Length; i++)
        {
            // Check if this is the matching event we want to replace
            bool isMatchingEvent = !eventFound && EventsMatch(currentEvents[i], eventToEdit.backingAnimationEvent);

            // Replace the event if it matches, otherwise keep the original
            if (isMatchingEvent)
            {
                updatedEvents[i] = newEvent;
                eventFound = true; // Mark that we've found and replaced the event
            }
            else
            {
                updatedEvents[i] = currentEvents[i]; // Keep the original event
            }
        }

        if (!eventFound)
        {
            Debug.LogWarning($"Could not find matching event to replace in clip {clip.name}");
            return;
        }

        ApplyEventsChanges(clip, updatedEvents, eventToEdit);
    
    }

    public static void DeleteEventProgrammatically(AnimationClip clip, ParsedEngageAnimationEvent eventToDelete)
    {
        if (clip == null || eventToDelete == null)
            return;

        
        AnimationEvent[] currentEvents = clip.events;
        List<AnimationEvent> updatedEvents = new List<AnimationEvent>(currentEvents.Length - 1);
        bool eventFound = false;

        foreach (var evt in currentEvents)
        {
            if (!eventFound && EventsMatch(evt, eventToDelete.backingAnimationEvent))
            {
                eventFound = true;
                continue; // Skip this event (delete)
            }

            updatedEvents.Add(evt);
        }

        if (!eventFound)
        {
            Debug.LogWarning($"Could not find matching event to delete in clip {clip.name}");
            return;
        }

        ApplyEventsChanges(clip, updatedEvents.ToArray());
    
    }

    public static void AddEventProgrammatically(AnimationClip clip, AnimationEvent eventToAdd)
    {
        if (clip == null || eventToAdd == null)
            return;
        
        AnimationEvent[] currentEvents = clip.events;
        AnimationEvent[] updatedEvents = new AnimationEvent[currentEvents.Length + 1];

        // Copy existing events
        Array.Copy(currentEvents, updatedEvents, currentEvents.Length);

        // Add new event at the end
        updatedEvents[currentEvents.Length] = eventToAdd;

        // Sort the events by time. Is this actually needed?
        Array.Sort(updatedEvents, (a, b) => a.time.CompareTo(b.time));

        ApplyEventsChanges(clip, updatedEvents);
        
    }

    // Common helper to apply event changes and update cache/notifications
    private static void ApplyEventsChanges(AnimationClip clip, AnimationEvent[] updatedEvents, ParsedEngageAnimationEvent modifiedParsedEngageAnimationEvent = null)
    {
        AnimationUtility.SetAnimationEvents(clip, updatedEvents);
        EditorUtility.SetDirty(clip);

        // Store old parsed events to preserve UUIDs where possible
        int instanceID = clip.GetInstanceID();
        List<ParsedEngageAnimationEvent> oldParsedEvents = null;
        _parsedEventsCache.TryGetValue(instanceID, out oldParsedEvents);
    
        // Parse the updated events
        List<ParsedEngageAnimationEvent> newParsedEvents = AnimationEventParser.ParseAnimationEvents(updatedEvents);
        
        bool replacementUuidUsed = false;

        if (modifiedParsedEngageAnimationEvent != null)
        {
            // preemptively remove the modified event from the old list, using the UUID
            var matchingOldEvent = oldParsedEvents?.FirstOrDefault(oldEvent => 
                oldEvent.Uuid == modifiedParsedEngageAnimationEvent.Uuid);

            if (matchingOldEvent != null)
            {
                oldParsedEvents.Remove(matchingOldEvent);
            }
            else
            {
                Debug.LogWarning($"Could not find matching event to replace in clip {clip.name}");
            }
        }
        var newUUIDs = new HashSet<string>();

        // Try to preserve UUIDs for matching events
        if (oldParsedEvents != null)
        {
            foreach (var newEvent in newParsedEvents)
            {
                // Try to find a matching event in the old list
                var matchingOldEvent = oldParsedEvents.FirstOrDefault(oldEvent => 
                    EventsMatch(oldEvent.backingAnimationEvent, newEvent.backingAnimationEvent));
                
                if (matchingOldEvent != null)
                {
                    // Transfer the UUID from the matching old event
                    newEvent.Uuid = matchingOldEvent.Uuid;
                    
                    // Remove the matching old event from the list to avoid duplicates, since events are not necessarily unique
                    oldParsedEvents.Remove(matchingOldEvent);
                }
                // If no match, and there's a replacement UUID, assign it
                else if (modifiedParsedEngageAnimationEvent != null)
                {
                    if (replacementUuidUsed)
                    {
                        Debug.Log($"Multiple new events exist - not sure which to assign replacement UUID in clip {clip.name}");
                    } else
                    {
                        newEvent.Uuid = modifiedParsedEngageAnimationEvent.Uuid;
                        replacementUuidUsed = true;
                    }
                }
                else
                {
                    newUUIDs.Add(newEvent.Uuid);
                }
            }
        }
        
        // Sort the new parsed events by time
        newParsedEvents.Sort((a, b) => a.backingAnimationEvent.time.CompareTo(b.backingAnimationEvent.time));
    
        // Update cache with UUID-preserved events
        _parsedEventsCache[instanceID] = newParsedEvents;

        // Notify listeners
        OnClipEventsChanged?.Invoke(clip, newUUIDs);

        // Update hash
        if (_watchedClips.TryGetValue(instanceID, out ClipHashData data))
        {
            data.eventsHash = ComputeEventsHash(clip.events);
        }
    }

    // Helper to determine if two animation events are the same
    private static bool EventsMatch(AnimationEvent a, AnimationEvent b)
    {
        // Compare the essential properties to identify if it's the same event
        return a.time == b.time &&
               a.functionName == b.functionName &&
               a.floatParameter == b.floatParameter &&
               a.intParameter == b.intParameter &&
               a.stringParameter == b.stringParameter &&
               a.objectReferenceParameter == b.objectReferenceParameter;
    }

    // Get list of parsed events for a specific clip
    public static List<ParsedEngageAnimationEvent> GetParsedEvents(AnimationClip clip)
    {
        if (clip == null) return null;

        int instanceID = clip.GetInstanceID();
        if (_parsedEventsCache.ContainsKey(instanceID))
        {
            return _parsedEventsCache[instanceID];
        }

        return null;
    }
}
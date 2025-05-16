using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DivineDragon
{
    [FilePath("Library/AnimationClipWatcherState.asset", FilePathAttribute.Location.ProjectFolder)]
    public class AnimationClipWatcherState : ScriptableSingleton<AnimationClipWatcherState>
    {
        // Use GUIDs rather than direct references for persistence
        [SerializeField] private List<string> watchedClipGUIDs = new List<string>();
    
        public void AddClipGUID(string guid)
        {
            if (!watchedClipGUIDs.Contains(guid))
            {
                watchedClipGUIDs.Add(guid);
                Save(true);
            }
        }
    
        public void ClearClipGUIDs()
        {
            watchedClipGUIDs.Clear();
            Save(true);
        }
    
        public List<string> GetWatchedClipGUIDs()
        {
            return new List<string>(watchedClipGUIDs);
        }
    }
}
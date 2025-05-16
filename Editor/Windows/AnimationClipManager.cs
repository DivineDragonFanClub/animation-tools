using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Combat;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Code.Combat.Editor
{
    public class AnimationClipManager : EditorWindow
    {
        [Serializable]
        private class ClipData
        {
            public string clipName;
            public string notes;
            public string clipGuid;
        }

        [Serializable]
        private class ClipLibrary
        {
            public List<ClipData> clips = new List<ClipData>();
        }

        private ClipLibrary library = new ClipLibrary();
        private string libraryPath;
        private string searchQuery = "";
        private ClipData selectedClip;
        private Animation selectedAnimation;
        private bool showFavoritesOnly = false;

        // UI Elements
        private VisualElement root;
        private ListView clipsListView;
        private Label clipNameLabel;
        private TextField notesField;
        private TextField searchField;
        private VisualElement detailsPanel;
        private Button applyButton;
        private Button favoriteButton;
        private Button selectAnimationClipButton;


        [MenuItem("Divine Dragon/Animation Tools/Clip Manager")]
        public static void ShowWindow()
        {
            AnimationClipManager window = GetWindow<AnimationClipManager>();
            window.titleContent = new GUIContent("Animation Clips");
            window.Show();
        }

        private void OnEnable()
        {
            LoadLibrary();
            CreateUI();
        }

        private void CreateUI()
        {
            root = rootVisualElement;
            
            // Create split view layout
            var splitView = new TwoPaneSplitView(0, 250, TwoPaneSplitViewOrientation.Vertical);
            root.Add(splitView);
            
            // Left panel - Clip list
            var leftPanel = new VisualElement();
            
            // Explain the purpose of this panel
            var explanationLabel = new TextElement
            {
                text = "Drag and drop animation clips here to add them to the library.\n" +
                       "Select a clip to view its details and apply it to Animation Editor."
            };;
            explanationLabel.style.marginBottom = 10;
            explanationLabel.style.fontSize = 12;
            explanationLabel.style.color = new Color(1, 1, 1, 0.7f);
            
            // hide the explanation label behind a foldout
            var foldout = new Foldout { text = "Help" };
            foldout.Add(explanationLabel);
            foldout.value = false;
            leftPanel.Add(foldout);
            
            leftPanel.style.paddingLeft = 10;
            leftPanel.style.paddingTop = 10;
            leftPanel.style.paddingRight = 10;
            leftPanel.style.paddingBottom = 10;
            searchField = new TextField("Search") { name = "searchField" };
            searchField.RegisterValueChangedCallback(evt => {
                searchQuery = evt.newValue;
                RefreshClipsList();
            });
            
            leftPanel.Add(searchField);
            
            // Clips list
            clipsListView = new ListView();
            clipsListView.makeItem = () => new Label();
            clipsListView.bindItem = (element, i) => {
                var filteredClips = GetFilteredClips();
                if (i < filteredClips.Count)
                {
                    var clip = filteredClips[i];
                    ((Label)element).text = clip.clipName;

                    // if clip is active, add a little text to the label
                    if (IsClipAppliedToSelectedAnimation(clip))
                    {
                        ((Label)element).text += " (active)";
                    }
                }
            };
            clipsListView.itemsSource = GetFilteredClips();
            clipsListView.onSelectionChange += items => {
                if (items.Count() > 0)
                {
                    selectedClip = items.First() as ClipData;
                    UpdateDetailsPanel();
                }
            };
            clipsListView.style.flexGrow = 1;
            
            leftPanel.Add(clipsListView);
            
            // Buttons
            var buttonPanel = new VisualElement();
            buttonPanel.style.flexDirection = FlexDirection.Row;
            buttonPanel.style.flexWrap = Wrap.Wrap;
            
            leftPanel.Add(buttonPanel);
            
            var dropArea = leftPanel;
            dropArea.style.marginBottom = 10;
            dropArea.style.borderTopWidth = 1;
            dropArea.style.borderRightWidth = 1;
            dropArea.style.borderBottomWidth = 1;
            dropArea.style.borderLeftWidth = 1;

            // Add drag and drop functionality
            dropArea.RegisterCallback<DragEnterEvent>(OnDragEnter);
            dropArea.RegisterCallback<DragLeaveEvent>(OnDragLeave);
            dropArea.RegisterCallback<DragUpdatedEvent>(OnDragUpdated);
            dropArea.RegisterCallback<DragPerformEvent>(OnDragPerform);

            
            // Right panel - Details
            detailsPanel = new VisualElement();
            
            // add some padding
            detailsPanel.style.paddingLeft = 10;
            detailsPanel.style.paddingTop = 10;
            detailsPanel.style.paddingRight = 10;
            detailsPanel.style.paddingBottom = 10;
            
            clipNameLabel = new Label
            {
                name = "",
                style =
                {
                    fontSize = 16,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    paddingBottom = 5
                }
            };
            detailsPanel.Add(clipNameLabel);
            
            
            // Notes field
            notesField = new TextField("Notes")
            {
                multiline = true,
                style =
                {
                    whiteSpace = WhiteSpace.Normal,
                    unityTextAlign = TextAnchor.UpperLeft,
                    paddingTop = 2,
                    flexDirection = FlexDirection.Column,
                }
            };
            
            // make the label not take so much space
            notesField.labelElement.style.marginBottom = 3;
            
            notesField.RegisterValueChangedCallback(evt =>
            {
                if (selectedClip != null)
                {
                    selectedClip.notes = evt.newValue;
                    SaveLibrary();
                }
            });
            notesField.isDelayed = true;
            detailsPanel.Add(notesField);
            
            // Action buttons
            var actionButtonPanel = new VisualElement();
            actionButtonPanel.style.flexDirection = FlexDirection.Row;
            actionButtonPanel.style.flexWrap = Wrap.Wrap;

            applyButton = new Button(() => ApplySelectedClip()) { text = "Apply Clip" };
            var deleteButton = new Button(() => DeleteSelectedClip()) { text = "Remove" };
            deleteButton.tooltip = "Remove this clip from this panel (does not delete the clip from the project)";
            
            selectAnimationClipButton = new Button(() =>
            {
                if (selectedClip != null)
                {
                    AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDatabase.GUIDToAssetPath(selectedClip.clipGuid));
                    if (clip != null)
                    {
                        Selection.activeObject = clip;
                        EditorGUIUtility.PingObject(clip);
                        // Switch to Project view
                        // Switch to Project view
                        Type projectBrowserType = Type.GetType("UnityEditor.ProjectBrowser,UnityEditor");
                        if (projectBrowserType != null)
                        {
                            FocusWindowIfItsOpen(projectBrowserType);
                        }
                    }
                }
            }) { text = "Select Clip" };
            
            actionButtonPanel.Add(applyButton);
            actionButtonPanel.Add(favoriteButton);
            actionButtonPanel.Add(selectAnimationClipButton);
            actionButtonPanel.Add(deleteButton);
            detailsPanel.Add(actionButtonPanel);
            
            // Add panels to split view
            splitView.Add(leftPanel);
            splitView.Add(detailsPanel);
            
            UpdateDetailsPanel();
        }
        private List<ClipData> GetFilteredClips()
        {
            return library.clips.Where(clip => 
                string.IsNullOrEmpty(searchQuery) || 
                 clip.clipName.ToLower().Contains(searchQuery.ToLower()) || 
                 clip.notes.ToLower().Contains(searchQuery.ToLower())
            ).ToList();
        }

        private void RefreshClipsList()
        {
            clipsListView.itemsSource = GetFilteredClips();
            clipsListView.Refresh();
        }

        private void UpdateDetailsPanel()
        {
            if (selectedClip == null)
            {
                clipNameLabel.text = "No Clip Selected";
                notesField.value = "";
                notesField.SetEnabled(false);
                return;
            }

            notesField.SetEnabled(true);
            clipNameLabel.text = selectedClip.clipName;
            notesField.value = selectedClip.notes;
            UpdateApplyButtonState();
        }
        
        private void ApplySelectedClip()
        {
            GameObject selectedGameObject;
            
            AnimationEditor[] editors = FindObjectsOfType<AnimationEditor>();
            if (editors.Length > 0)
            {
                selectedGameObject = editors[0].gameObject;
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "No Animation Editor found in the scene", "OK");
                return;
            }
            
            // Ensure target has Animation component
            Animation anim = selectedGameObject.GetComponent<Animation>();
            if (anim == null)
            {
                anim = Undo.AddComponent<Animation>(selectedGameObject);
            }
            
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDatabase.GUIDToAssetPath(selectedClip.clipGuid));
            if (clip == null)
            {
                EditorUtility.DisplayDialog("Error", "Could not load animation clip", "OK");
                return;
            }
            
            // Record the Animation component for Undo
            Undo.RecordObject(anim, "Change Animation Clip");

            // Remove the clip if it already exists (by name)
            if (anim.GetClip(clip.name) != null)
            {
                anim.RemoveClip(clip.name);
            }

            // Add the clip
            anim.AddClip(clip, clip.name);
            
            AnimationUtility.SetAnimationClips(anim, new[] { clip });

            // Set it as the default clip
            anim.clip = clip;
            
            EditorUtility.SetDirty(anim);
            UpdateApplyButtonState();
            RefreshClipsList();
            
            // Focus on the animation window
            Type animationWindowType = Type.GetType("UnityEditor.AnimationWindow,UnityEditor");
            if (animationWindowType != null)
            {
                FocusWindowIfItsOpen(animationWindowType);
            }
            
            // Select the gameObject
            Selection.activeGameObject = selectedGameObject;
        }
        private void DeleteSelectedClip()
        {
            if (selectedClip == null) return;
                
            if (EditorUtility.DisplayDialog("Confirm Delete", 
                $"Delete '{selectedClip.clipName}' from the library?", "Yes", "No"))
            {
                library.clips.Remove(selectedClip);
                selectedClip = null;
                SaveLibrary();
                RefreshClipsList();
                UpdateDetailsPanel();
            }
        }

        private void LoadLibrary()
        {
            libraryPath = "Assets/AnimationClipLibrary.json";
            
            if (File.Exists(libraryPath))
            {
                try
                {
                    string json = File.ReadAllText(libraryPath);
                    library = JsonUtility.FromJson<ClipLibrary>(json);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error loading clip library: {e.Message}");
                    library = new ClipLibrary();
                }
            }
        }

        private void SaveLibrary()
        {
            try
            {
                string directoryPath = Path.GetDirectoryName(libraryPath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                
                string json = JsonUtility.ToJson(library, true);
                File.WriteAllText(libraryPath, json);
                AssetDatabase.Refresh();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error saving clip library: {e.Message}");
            }
        }

        private void UpdateLibrary()
        {
            bool libraryChanged = false;
            List<ClipData> clipsToRemove = new List<ClipData>();
    
            // Check each clip in the library
            foreach (var clip in library.clips)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(clip.clipGuid);
        
                // Check if the asset still exists
                if (string.IsNullOrEmpty(assetPath))
                {
                    // Asset doesn't exist anymore, mark for removal
                    clipsToRemove.Add(clip);
                    libraryChanged = true;
                    continue;
                }
        
                // Load the clip to check its current name
                AnimationClip animClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath);
                if (animClip == null)
                {
                    // Asset isn't an animation clip anymore or can't be loaded
                    clipsToRemove.Add(clip);
                    libraryChanged = true;
                    continue;
                }
        
                // Update the name if it changed
                if (clip.clipName != animClip.name)
                {
                    clip.clipName = animClip.name;
                    libraryChanged = true;
                }
            }
    
            // Remove deleted clips
            foreach (var clip in clipsToRemove)
            {
                library.clips.Remove(clip);
            }
    
            // Save changes if needed
            if (libraryChanged)
            {
                SaveLibrary();
                RefreshClipsList();
            }
        }
        
        private void OnFocus()
        {
            UpdateLibrary();
        }

        private void OnProjectChange()
        {
            UpdateLibrary();
        }
        
        private void OnDragEnter(DragEnterEvent evt)
        {
            // Highlight the drop area
            var dropArea = evt.currentTarget as VisualElement;
            dropArea.style.backgroundColor = new Color(0.3f, 0.5f, 0.8f, 0.3f);
        }

        private void OnDragLeave(DragLeaveEvent evt)
        {
            // Reset drop area styling
            var dropArea = evt.currentTarget as VisualElement;
            dropArea.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.3f);
        }

        private void OnDragUpdated(DragUpdatedEvent evt)
        {
            // Check if any of the dragged items are animation clips
            bool hasAnimationClips = DragAndDrop.objectReferences
                .Any(obj => obj is AnimationClip);
        
            if (hasAnimationClips)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            }
        }

        private void OnDragPerform(DragPerformEvent evt)
        {
            // Reset drop area styling
            var dropArea = evt.currentTarget as VisualElement;
            dropArea.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.3f);
    
            // Process dropped animation clips
            int addedCount = 0;
            bool validDrop = false;

            foreach (var obj in DragAndDrop.objectReferences)
            {
                if (obj is AnimationClip clip)
                {
                    validDrop = true;
                    string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(clip));

                    // Skip if clip already exists in library
                    if (library.clips.Any(c => c.clipGuid == guid))
                        continue;

                    ClipData newClip = new ClipData
                    {
                        clipName = clip.name,
                        clipGuid = guid,
                        notes = ""
                    };

                    library.clips.Add(newClip);
                    addedCount++;
                }
            }

            if (validDrop)
            {
                if (addedCount > 0)
                {
                    SaveLibrary();
                    RefreshClipsList();
                    // EditorUtility.DisplayDialog("Import Complete", $"Added {addedCount} clips", "OK");
                }
                DragAndDrop.AcceptDrag();
            }
            else
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
            }
        }
        
        private bool IsClipAppliedToSelectedAnimation(ClipData clipData = null)
        {
            // Use the provided clip or fall back to the selected clip
            ClipData clipToCheck = clipData ?? selectedClip;
            
            AnimationEditor[] editors = FindObjectsOfType<AnimationEditor>();
            GameObject selectedGameObject;
            if (editors.Length > 0)
            {
                selectedGameObject = editors[0].gameObject;
            }
            else
            {
                return false;
            }
            
            // Ensure target has Animation component
            Animation anim = selectedGameObject.GetComponent<Animation>();
            if (anim == null)
            {
                anim = Undo.AddComponent<Animation>(selectedGameObject);
            }
            
            if (anim == null)
                return false;

            // Load the clip from path
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDatabase.GUIDToAssetPath(clipToCheck.clipGuid));
            if (clip == null)
                return false;

            // Check if this clip is already applied
            AnimationClip[] clips = AnimationUtility.GetAnimationClips(anim);
            return clips != null && clips.Contains(clip);
        }

        private void UpdateApplyButtonState()
        {
            if (applyButton != null)
            {
                bool isApplied = IsClipAppliedToSelectedAnimation();
                applyButton.text = isApplied ? "Apply again (Clip currently in use)" : "Apply clip";
                applyButton.tooltip = isApplied ? "This clip is being edited by the Animation Editor already - clicking this will refocus it" : "Apply this clip to the selected Animation Editor";
            }
        }
    }
}
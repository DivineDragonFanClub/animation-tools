using Combat;
using DivineDragon.EngageAnimationEvents;
using DivineDragon.Windows;
using UnityEditor;
using UnityEngine;
using Camera = UnityEngine.Camera;

namespace DivineDragon
{
    [CustomEditor(typeof(AnimationEditor))]
    public class AnimationEditorInspector : Editor
    {

        public AnimationEditor GetAnimationEditor()
        {
            return (AnimationEditor)target;
        }
        
        private void OnEnable()
        {
            // Start watching the clip
            AnimationClip clip = getAttachedClip();
            if (clip != null)
            {
                AnimationClipWatcher.WatchClip(clip);
            }
        }
        
        // check if the underlying clip has changed
        // and if so, start watching the new clip
        private AnimationClip _lastWatchedClip = null;

        // Check if the underlying clip has changed and update the watcher
        private void CheckForClipChanges()
        {
            AnimationClip currentClip = getAttachedClip();
    
            // If the clip reference has changed
            if (currentClip != _lastWatchedClip)
            {
                // Unwatch the previous clip if it exists
                if (_lastWatchedClip != null)
                {
                    AnimationClipWatcher.UnwatchClip(_lastWatchedClip);
                    Debug.Log($"Stopped watching clip: {_lastWatchedClip.name}");
                }
        
                // Watch the new clip if it exists
                if (currentClip != null)
                {
                    AnimationClipWatcher.WatchClip(currentClip);
                    Debug.Log($"Now watching clip: {currentClip.name}");
                }
        
                // Update our reference
                _lastWatchedClip = currentClip;
            }
        }
        

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
        }
        

        public AnimationClip getAttachedClip()
        {
            AnimationEditor animationEditor = GetAnimationEditor();
            Animation attachedAnimation = animationEditor.GetComponent<Animation>();
            if (attachedAnimation == null)
            {
                return null;
            }

            if (attachedAnimation.clip == null)
            {
                return null;
            }

            return attachedAnimation.clip;
        }
        
        private float CurrentTime
        {
            get
            {
                AnimationEditor animationEditor = (AnimationEditor)target;
                return animationEditor?.currentTime ?? 0f;
            }
            set
            {
                AnimationEditor animationEditor = (AnimationEditor)target;
                if (animationEditor == null)
                    return;
                
                // Without this check, the inspector will be locked into refreshing every frame
                if (Mathf.Abs(animationEditor.currentTime - value) > Mathf.Epsilon)
                {
                    animationEditor.currentTime = value;
                    EditorUtility.SetDirty(animationEditor); // Mark the object as dirty so the changes are saved
                }
            }
        }

        // Constructor
        AnimationEditorInspector()
        {
            Debug.Log("Creating AnimationEditorInspector");
            EditorApplication.update += OnEditorUpdate;
            // Undo.undoRedoPerformed += OnUndoRedoPerformed;
        }

        private void OnDestroy()
        {
            Debug.Log("Destroying AnimationEditorInspector");
            // Undo.undoRedoPerformed -= OnUndoRedoPerformed;
            EditorApplication.update -= OnEditorUpdate;
        }

        // private void OnUndoRedoPerformed()
        // {
        //     var events = AnimationEditorCache.GetEventsForEditor(GetAnimationEditor());
        //     events.Clear();            
        //     events.AddRange(AnimationEventParser.ParseAnimationEvents(getAttachedClip().events));
        //     events.NotifyChanged();
        // }

        private AnimationWindow GetAnimationWindow()
        {
            // https://discussions.unity.com/t/macos-unity-editor-stealing-focus-constantly/829983/19
            //  So quickly after posting this I found the cause of the focus stealing on our end after some more debugging. I ended up finding that it was some custom editor windows causing the focus steal due to some EditorWindow.GetWindow method invokes not passing false for the focus param (which is default true). After making those small changes we were no longer seeing the focus stealing issues. Although this may be unrelated to this issue, hopefully this could be the key to addressing this issue for some.
            return EditorWindow.GetWindow<AnimationWindow>(false, null, false);
        }
        
        private void OnEditorUpdate()
        {   
            var _editor = GetAnimationWindow();
            CurrentTime = _editor.time;
            CheckForClipChanges();
        }
        private void OnSceneGUI()
        {
            AnimationEditor animationEditor = (AnimationEditor)target;
            var editor = GetAnimationWindow();
            float currentTime = editor.time;
            var privateEvents = AnimationClipWatcher.GetParsedEvents(getAttachedClip());
            float tolerance = 0.01f; // The tolerance around the time
            if (privateEvents == null)
            {
                return;
            }

            foreach (var animEvent in privateEvents)
            {
                // if current time is in the tolerance range
                if (animEvent.backingAnimationEvent.time >= currentTime - tolerance &&
                    animEvent.backingAnimationEvent.time <= currentTime + tolerance)
                {
                    if (animationEditor.AlwaysRenderLabels || IsEventSelected(animEvent))
                    {
                        animEvent.OnScrubbedTo(animationEditor, privateEvents);
                    }
                }
            }

            foreach (var animEvent in privateEvents)
            {
                if (animationEditor.AlwaysRenderVec3Events)
                {
                    animEvent.AlwaysRender(animationEditor, privateEvents);
                } else if (IsEventSelected(animEvent))
                {
                    animEvent.AlwaysRender(animationEditor, privateEvents);
                } else if (animEvent.GetType() == typeof(Hit))
                {
                    if (animationEditor.AlwaysRenderHitEvents)
                    {
                        animEvent.AlwaysRender(animationEditor, privateEvents);
                    }
                }
            }

            if (animationEditor.AlwaysRenderCameraPosition)
            { 
                RenderCameraInfo();
            }
        }

        private bool IsEventSelected(ParsedEngageAnimationEvent animEvent)
        {
            // if the window is open
            if (!EditorWindow.HasOpenInstances<EventViewerV2>())
            {
                return false;
            }
            // get the first instance
            var eventViewer = EditorWindow.GetWindow<EventViewerV2>(false, null, false);
            var selectedEvents = eventViewer.GetSelectedEvents();
            if (selectedEvents != null)
            {
                // check if the event is selected
                if (selectedEvents.Contains(animEvent.Uuid))
                {
                    return true;
                }
            }

            return false;
        }
        private void RenderCameraInfo()
        {
            AnimationEditor animationEditor = (AnimationEditor)target;
            Transform camLookAtLoc = animationEditor.transform.Find("c_trans/camLookAt_loc");
            Transform camFollowLoc = animationEditor.transform.Find("c_trans/camFollow_loc");
            Transform lookAtLoc = animationEditor.transform.Find("c_trans/c_hip_jnt/lookAt_loc");

            if (camLookAtLoc == null || camFollowLoc == null || lookAtLoc == null)
                return;


            // Draw lookAt_loc indicator
            Handles.color = Color.red;
            Handles.SphereHandleCap(0, lookAtLoc.position, Quaternion.identity, 0.1f, EventType.Repaint);
            Handles.Label(lookAtLoc.position, "lookAt_loc Position");

            // Draw camera position indicator
            Handles.color = Color.cyan;
            Handles.SphereHandleCap(0, camFollowLoc.position, Quaternion.identity, 0.1f, EventType.Repaint);
            Handles.Label(camFollowLoc.position, "camFollow_loc Position");

            // Draw camera rotation indicator
            Handles.color = Color.magenta;
            Handles.ArrowHandleCap(0, camFollowLoc.position, camFollowLoc.rotation, 0.5f, EventType.Repaint);
            Handles.Label(camFollowLoc.position + camFollowLoc.forward * 0.5f, "camFollow_loc Rotation");


            // Draw look target indicator
            Handles.color = Color.yellow;
            Handles.SphereHandleCap(0, camLookAtLoc.position, Quaternion.identity, 0.1f, EventType.Repaint);
            Handles.Label(camLookAtLoc.position, "camLookAt_loc Position");

            // Draw line from camera to target with arrow
            Handles.color = Color.white;
            Vector3 direction = (camLookAtLoc.position - camFollowLoc.position).normalized;
            Handles.DrawLine(camFollowLoc.position, camLookAtLoc.position);

            // Draw arrow at the end
            float size = HandleUtility.GetHandleSize(camLookAtLoc.position) * 0.2f;
            if (direction != Vector3.zero)
            {
                Handles.ConeHandleCap(0, camLookAtLoc.position - direction * size * 0.5f,
                    Quaternion.LookRotation(direction), size, EventType.Repaint);
            }
            
            // grab the main camera
            Camera mainCamera = Camera.main;
            
            // position the main camera at the camFollow_loc position
            mainCamera.transform.position = camFollowLoc.position;
            
            Vector3 followPos = camFollowLoc.position;
            Vector3 lookAtPos = camLookAtLoc.position;
            Vector3 cameraDirection = lookAtPos - followPos;
            
            if (cameraDirection.magnitude > 0.0f)
            {
                Quaternion lookRotation = Quaternion.LookRotation(cameraDirection, Vector3.up);
                
                Vector3 localEuler = camFollowLoc.localRotation.eulerAngles;
                
                float zRotation = -localEuler.y;
                
                Quaternion zRotationQuat = Quaternion.Euler(0, 0, zRotation);
                
                mainCamera.transform.rotation = lookRotation * zRotationQuat;
            }
        }
    }
}
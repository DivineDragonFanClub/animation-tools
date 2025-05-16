using Combat;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace DivineDragon.Windows
{
    public class AnimationEditorInspectorHelper: EditorWindow
    {
        // Field to track the previously attached clip
        private AnimationClip _lastAttachedClip;
        
        // Start watching for clip changes
        private void OnEnable()
        {
            // search for an AnimationEditor in the scene.
            animationEditor = FindObjectOfType<AnimationEditor>();
    
            // Start tracking clip changes
            _lastAttachedClip = getAttachedClip();
            EditorApplication.update += CheckForClipChanges;
        }

        // Check if the attached clip has changed
        private void CheckForClipChanges()
        {
            AnimationClip currentClip = getAttachedClip();
    
            if (currentClip != _lastAttachedClip)
            {
                // Update our reference
                _lastAttachedClip = currentClip;
        
                // Notify derived classes about the change
                OnUnderlyingAnimationClipChanged();
            }
        }
        
        // Also a cheat to do the initial render
        protected virtual void OnUnderlyingAnimationClipChanged()
        {
            animationEditor = FindObjectOfType<AnimationEditor>();
    
            // Start tracking clip changes
            _lastAttachedClip = getAttachedClip();
            EditorApplication.update += CheckForClipChanges;
            Debug.Log("AnimationEditorInspectorHelper.OnUnderlyingAnimationClipChanged");
        }
        
        protected float CurrentTime
        {
            get
            {
                return animationEditor.currentTime;
            }
        }
        
        protected AnimationWindow GetAnimationWindow()
        {
            // https://discussions.unity.com/t/macos-unity-editor-stealing-focus-constantly/829983/19
            //  So quickly after posting this I found the cause of the focus stealing on our end after some more debugging. I ended up finding that it was some custom editor windows causing the focus steal due to some EditorWindow.GetWindow method invokes not passing false for the focus param (which is default true). After making those small changes we were no longer seeing the focus stealing issues. Although this may be unrelated to this issue, hopefully this could be the key to addressing this issue for some.
            return GetWindow<AnimationWindow>(false, null, false);
        }
        
        public AnimationClip getAttachedClip()
        {
            // just be aggressive and search for the AnimationEditor in the scene
            animationEditor = FindObjectOfType<AnimationEditor>();
            if (animationEditor == null)
            {
                return null;
            }
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
        
        

        protected VisualElement root;
        protected AnimationEditor animationEditor;
        
        public float Tolerance
        {
            get
            {
                return animationEditor.tolerance;
            }
            set
            {
                animationEditor.tolerance = value;
                EditorUtility.SetDirty(animationEditor); // Mark the object as dirty so the changes are saved
            }
        }
        
        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            root = rootVisualElement;
            OnUnderlyingAnimationClipChanged();
        }
    }
}
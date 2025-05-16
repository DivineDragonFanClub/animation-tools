using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace DivineDragon.Windows
{
    public class PrefetchedCurveHelper: AnimationEditorInspectorHelper
    {
        [MenuItem("Divine Dragon/Animation Tools/Prefetched Curve Helper")]
        public static void ShowExample()
        {
            PrefetchedCurveHelper wnd = GetWindow<PrefetchedCurveHelper>();
            wnd.titleContent = new GUIContent("Prefetched Curve Helper");
        }

        protected override void OnUnderlyingAnimationClipChanged()
        {
            root.Clear();
            CreateCurveUI(root);
        }

        public void CreateCurveUI(VisualElement myInspector)
        {
            // Notify the user this only works for the right hand
            var label = new HelpBox(
                "This currently only works for right hand melee weapons.",
                HelpBoxMessageType.Info);
            // Expose the prefetched curve bridge slot
            var prefetchedCurveBridge = new ObjectField("Prefetched Curve Bridge")
            {
                objectType = typeof(PrefetchedCurve_Bridge),
                value = animationEditor.bridge
            };

            prefetchedCurveBridge.RegisterValueChangedCallback(evt =>
            {
                animationEditor.bridge = (PrefetchedCurve_Bridge)evt.newValue;
                EditorUtility.SetDirty(animationEditor);
            });

            myInspector.Add(prefetchedCurveBridge);

            // Expose the RightRoot slot
            var rightRoot = new ObjectField("Right Root")
            {
                objectType = typeof(Transform),
                value = animationEditor.RightRoot
            };

            rightRoot.RegisterValueChangedCallback(evt =>
            {
                animationEditor.RightRoot = (Transform)evt.newValue;
                EditorUtility.SetDirty(animationEditor);
            });

            myInspector.Add(rightRoot);

            // Expose the RightTip slot
            var rightTip = new ObjectField("Right Tip")
            {
                objectType = typeof(Transform),
                value = animationEditor.RightTip
            };

            rightTip.RegisterValueChangedCallback(evt =>
            {
                animationEditor.RightTip = (Transform)evt.newValue;
                EditorUtility.SetDirty(animationEditor);
            });

            myInspector.Add(rightTip);

            // write the current position of RightRoot and RightTip into the bridge
            var writePositionButton = new Button(() =>
            {
                if (animationEditor.bridge == null)
                {
                    Debug.LogError("Bridge is null, cannot write positions");
                    return;
                }

                if (animationEditor.RightRoot == null)
                {
                    Debug.LogError("RightRoot is null, cannot write positions");
                    return;
                }

                if (animationEditor.RightTip == null)
                {
                    Debug.LogError("RightTip is null, cannot write positions");
                    return;
                }
                
                // Get the current animation clip
                var currentClip = getAttachedClip();

                // Sample multiple frames of the animation
                var startTime = 0f;
                var endTime = currentClip.length;
                var sampleCount = (int)(endTime * 90f); // Number of samples to take
                var timeStep = endTime / (sampleCount - 1); // Time between samples

                animationEditor.bridge.RightHand.RootX = new AnimationCurve();


                animationEditor.bridge.RightHand.RootY = new AnimationCurve();


                animationEditor.bridge.RightHand.RootZ = new AnimationCurve();


                animationEditor.bridge.RightHand.TipX = new AnimationCurve();


                animationEditor.bridge.RightHand.TipY = new AnimationCurve();


                animationEditor.bridge.RightHand.TipZ = new AnimationCurve();
                for (float time = startTime; time <= endTime; time += timeStep)
                {
                    // Sample the animation at this time
                    currentClip.SampleAnimation(animationEditor.gameObject, time);


                    // Add keyframe for this time
                    animationEditor.bridge.RightHand.RootX.AddKey(new Keyframe(time,
                        animationEditor.RightRoot.position.x));
                    animationEditor.bridge.RightHand.RootY.AddKey(new Keyframe(time,
                        animationEditor.RightRoot.position.y));
                    animationEditor.bridge.RightHand.RootZ.AddKey(new Keyframe(time,
                        animationEditor.RightRoot.position.z));
                    animationEditor.bridge.RightHand.TipX.AddKey(
                        new Keyframe(time, animationEditor.RightTip.position.x));
                    animationEditor.bridge.RightHand.TipY.AddKey(
                        new Keyframe(time, animationEditor.RightTip.position.y));
                    animationEditor.bridge.RightHand.TipZ.AddKey(
                        new Keyframe(time, animationEditor.RightTip.position.z));
                }

                // smooth all tangents
                for (int i = 0; i < animationEditor.bridge.RightHand.RootX.length; i++)
                {
                    animationEditor.bridge.RightHand.RootX.SmoothTangents(i, 0);
                    animationEditor.bridge.RightHand.RootY.SmoothTangents(i, 0);
                    animationEditor.bridge.RightHand.RootZ.SmoothTangents(i, 0);
                    animationEditor.bridge.RightHand.TipX.SmoothTangents(i, 0);
                    animationEditor.bridge.RightHand.TipY.SmoothTangents(i, 0);
                    animationEditor.bridge.RightHand.TipZ.SmoothTangents(i, 0);
                }

                // Restore to current time
                currentClip.SampleAnimation(animationEditor.gameObject, CurrentTime);
                EditorUtility.SetDirty(animationEditor.bridge);
            })
            {
                text = "Write Positions"
            };
            myInspector.Add(writePositionButton);
        }
    }
}
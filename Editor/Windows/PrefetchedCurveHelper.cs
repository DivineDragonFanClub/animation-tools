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

            // Try to auto-assign the PrefetchedCurve_Bridge from animation events
            bool foundBridge = false;
            var attachedClip = getAttachedClip();
            if (attachedClip != null)
            {
                var parsedEvents = DivineDragon.AnimationClipWatcher.GetParsedEvents(attachedClip);
                foreach (var evt in parsedEvents)
                {
                    // Look for Generic Object event with string parameter "PC"
                    if (evt.displayName == "Generic Object" && evt.backingAnimationEvent.stringParameter == "PC")
                    {
                        var obj = evt.backingAnimationEvent.objectReferenceParameter as PrefetchedCurve_Bridge;
                        if (obj != null)
                        {
                            animationEditor.bridge = obj;
                            prefetchedCurveBridge.value = obj;
                            foundBridge = true;
                            break;
                        }
                    }
                }
            }
            if (!foundBridge)
            {
                var warn = new HelpBox(
                    "No PrefetchedCurve_Bridge found in animation events. Click the button below to create one along with the needed animation event.",
                    HelpBoxMessageType.Warning);
                myInspector.Add(warn);

                // Only show the create button if not autodetected
                var createBridgeAndEventButton = new Button(() =>
                {
                    // 1. Create the asset
                    var asset = ScriptableObject.CreateInstance<PrefetchedCurve_Bridge>();
                    string guid = System.Guid.NewGuid().ToString("N").Substring(0, 8);
                    string path = $"Assets/PrefetchedCurve_Bridge_{guid}.asset";
                    path = AssetDatabase.GenerateUniqueAssetPath(path);
                    AssetDatabase.CreateAsset(asset, path);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    // 2. Add the Generic Object event to the current animation clip
                    var currentClip = getAttachedClip();
                    if (currentClip != null)
                    {
                        // Create the AnimationEvent
                        var animEvent = new AnimationEvent();
                        animEvent.time = 0f;
                        animEvent.functionName = "汎用Object"; // Generic Object
                        animEvent.stringParameter = "PC";
                        animEvent.objectReferenceParameter = asset;
                        Undo.RegisterCompleteObjectUndo(currentClip, "Add PrefetchedCurve_Bridge Event");
                        DivineDragon.AnimationClipWatcher.AddEventProgrammatically(currentClip, animEvent);
                        Debug.Log($"Added Generic Object event with PrefetchedCurve_Bridge to {currentClip.name}");
                    }
                    else
                    {
                        Debug.LogWarning("No animation clip attached. Only the asset was created.");
                    }

                    // 3. Reveal the asset in the Project window
                    EditorGUIUtility.PingObject(asset);
                    Selection.activeObject = asset;

                    // 4. Refresh the UI to reflect the new event/bridge
                    myInspector.Clear();
                    CreateCurveUI(myInspector);
                })
                {
                    text = "Create PrefetchedCurve_Bridge Asset + Event"
                };
                myInspector.Add(createBridgeAndEventButton);
            }


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
                text = "Write Positions to PrefetchedCurve_Bridge",
                tooltip = "Steps through the animation and writes the positions of RightRoot and RightTip to the PrefetchedCurve_Bridge."
            };
            myInspector.Add(writePositionButton);

            // Notify the user this only works for the right hand
            var label = new HelpBox(
                "This currently only works for right hand melee weapons.",
                HelpBoxMessageType.Info);
            myInspector.Add(label);
            
            var createRightHandHelpersButton = new Button(() =>
            {
                var go = animationEditor?.gameObject;
                if (go == null)
                {
                    Debug.LogError("Animation Editor GameObject is null");
                    return;
                }

                // Recursively find the transform named r_wpn1_loc
                Transform FindChildRecursive(Transform parent, string name)
                {
                    if (parent.name == name) return parent;
                    foreach (Transform child in parent)
                    {
                        var result = FindChildRecursive(child, name);
                        if (result != null) return result;
                    }
                    return null;
                }

                var rWpn1Loc = FindChildRecursive(go.transform, "r_wpn1_loc");
                if (rWpn1Loc == null)
                {
                    Debug.LogError("Could not find 'r_wpn1_loc' transform");
                    return;
                }

                // Create Right_Weapon_Root
                var rootGO = new GameObject("Right_Weapon_Root");
                Undo.RegisterCreatedObjectUndo(rootGO, "Create Right_Weapon_Root");
                var root = rootGO.transform;
                root.parent = rWpn1Loc;
                root.localPosition = new Vector3(0, 0, 0.05f);
                root.localRotation = Quaternion.Euler(0, 0, 0);
                root.localScale = new Vector3(0.05f, 0.05f, 0.05f);

                // Create Right_Weapon_Tip
                var tipGO = new GameObject("Right_Weapon_Tip");
                Undo.RegisterCreatedObjectUndo(tipGO, "Create Right_Weapon_Tip");
                var tip = tipGO.transform;
                tip.parent = rWpn1Loc;
                tip.localPosition = new Vector3(0, 0, 1.05f);
                tip.localRotation = Quaternion.Euler(0, 0, 0);
                tip.localScale = new Vector3(0.05f, 0.05f, 0.05f);

                // Register undo for animationEditor assignment
                Undo.RecordObject(animationEditor, "Assign RightRoot and RightTip");
                animationEditor.RightRoot = root;
                animationEditor.RightTip = tip;
                EditorUtility.SetDirty(animationEditor);

                // Create Right_Weapon_Visualizer (Cylinder)
                var visualizerGO = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                Undo.RegisterCreatedObjectUndo(visualizerGO, "Create Right_Weapon_Visualizer");
                var visualizer = visualizerGO.transform;
                visualizer.name = "Right_Weapon_Visualizer";
                visualizer.parent = rWpn1Loc;
                visualizer.localPosition = new Vector3(0, 0, 0.55f);
                visualizer.localRotation = Quaternion.Euler(90, 0, 0); // y axis
                visualizer.localScale = new Vector3(0.05f, 0.53f, 0.05f);

                // Keep the collider, but remove the mesh renderer
                var meshRenderer = visualizer.GetComponent<MeshRenderer>();
                if (meshRenderer != null)
                {
                    Object.DestroyImmediate(meshRenderer);
                }
                var meshFilter = visualizer.GetComponent<MeshFilter>();
                if (meshFilter != null)
                {
                    Object.DestroyImmediate(meshFilter);
                }

                // Refresh the UI fields to show the new assignments
                rightRoot.value = root;
                rightTip.value = tip;

                Debug.Log("Created and assigned Right_Weapon_Root, Right_Weapon_Tip, and Right_Weapon_Visualizer (collider only, no mesh) under 'r_wpn1_loc'.");
            })
            {
                text = "Create Right Hand Helpers (Warning - only an approximate position)",
                tooltip = "Creates Right_Weapon_Root, Right_Weapon_Tip, and Right_Weapon_Visualizer (collider only, no mesh) under 'r_wpn1_loc', so that we can create the animation curve."
            };


            myInspector.Add(createRightHandHelpersButton);


            // Add a refresh button to re-run the bridge auto-detection logic
            var refreshButton = new Button(() =>
            {
                myInspector.Clear();
                CreateCurveUI(myInspector);
            })
            {
                text = "Refresh Panel"
            };
            myInspector.Add(refreshButton);
        }
    }
}
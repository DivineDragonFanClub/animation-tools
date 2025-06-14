using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace DivineDragon.Windows
{
    public enum HandMode
    {
        RightHand,
        LeftHand
    }
    public class PrefetchedCurveHelper: AnimationEditorInspectorHelper
    {
        private HandMode currentHandMode = HandMode.RightHand;
        
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
            // Bridge Configuration Section
            var bridgeSection = new VisualElement();
            bridgeSection.style.paddingLeft = 10;
            bridgeSection.style.paddingRight = 10;
            bridgeSection.style.paddingTop = 5;
            bridgeSection.style.paddingBottom = 5;
            
            var bridgeSectionLabel = new Label("Bridge Configuration");
            bridgeSectionLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            bridgeSectionLabel.style.fontSize = 14;
            bridgeSectionLabel.style.marginBottom = 10;
            bridgeSection.Add(bridgeSectionLabel);
            
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

            bridgeSection.Add(prefetchedCurveBridge);

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
                bridgeSection.Add(warn);

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
                        DivineDragon.AnimationClipWatcher.AddEventProgrammatically(currentClip, animEvent, "Add PrefetchedCurve_Bridge Event");
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
                bridgeSection.Add(createBridgeAndEventButton);
            }
            
            // Add the bridge section to the inspector
            myInspector.Add(bridgeSection);
            
            // Add separator after bridge section
            var bridgeSeparator = new VisualElement();
            bridgeSeparator.style.height = 10;
            bridgeSeparator.style.borderBottomWidth = 1;
            bridgeSeparator.style.borderBottomColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            bridgeSeparator.style.marginBottom = 5;
            bridgeSeparator.style.marginTop = 5;
            myInspector.Add(bridgeSeparator);

            // Curve Writing Section
            var curveSection = new VisualElement();
            curveSection.style.paddingLeft = 10;
            curveSection.style.paddingRight = 10;
            curveSection.style.paddingTop = 5;
            curveSection.style.paddingBottom = 5;
            
            var curveSectionLabel = new Label("Trail Curve Writing");
            curveSectionLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            curveSectionLabel.style.fontSize = 14;
            curveSectionLabel.style.marginBottom = 10;
            curveSection.Add(curveSectionLabel);
            
            var handMode = new EnumField("Hand", currentHandMode);
            handMode.RegisterValueChangedCallback(evt =>
            {
                // Store the new value
                currentHandMode = (HandMode)evt.newValue;
                // Refresh UI when mode changes
                myInspector.Clear();
                CreateCurveUI(myInspector);
            });
            curveSection.Add(handMode);
            
            bool isRightHand = currentHandMode == HandMode.RightHand;
            string handName = isRightHand ? "Right" : "Left";

            // Expose the Root slot (either Right or Left based on mode)
            var rootLabel = isRightHand ? "Right Root" : "Left Root";
            var root = new ObjectField(rootLabel)
            {
                objectType = typeof(Transform),
                value = isRightHand ? animationEditor.RightRoot : animationEditor.LeftRoot
            };

            root.RegisterValueChangedCallback(evt =>
            {
                if (isRightHand)
                    animationEditor.RightRoot = (Transform)evt.newValue;
                else
                    animationEditor.LeftRoot = (Transform)evt.newValue;
                EditorUtility.SetDirty(animationEditor);
            });

            curveSection.Add(root);

            // Expose the Tip slot (either Right or Left based on mode)
            var tipLabel = isRightHand ? "Right Tip" : "Left Tip";
            var tip = new ObjectField(tipLabel)
            {
                objectType = typeof(Transform),
                value = isRightHand ? animationEditor.RightTip : animationEditor.LeftTip
            };

            tip.RegisterValueChangedCallback(evt =>
            {
                if (isRightHand)
                    animationEditor.RightTip = (Transform)evt.newValue;
                else
                    animationEditor.LeftTip = (Transform)evt.newValue;
                EditorUtility.SetDirty(animationEditor);
            });

            curveSection.Add(tip);

            // write the current position of Root and Tip into the bridge
            var writePositionButton = new Button(() =>
            {
                if (animationEditor.bridge == null)
                {
                    Debug.LogError("Bridge is null, cannot write positions");
                    return;
                }

                Transform currentRoot = isRightHand ? animationEditor.RightRoot : animationEditor.LeftRoot;
                Transform currentTip = isRightHand ? animationEditor.RightTip : animationEditor.LeftTip;

                if (currentRoot == null)
                {
                    Debug.LogError($"{handName}Root is null, cannot write positions");
                    return;
                }

                if (currentTip == null)
                {
                    Debug.LogError($"{handName}Tip is null, cannot write positions");
                    return;
                }

                // Register undo for the bridge before modifying its curves
                Undo.RecordObject(animationEditor.bridge, $"Write {handName}Hand Positions to PrefetchedCurve_Bridge");

                // Get the current animation clip
                var currentClip = getAttachedClip();

                // Sample multiple frames of the animation
                var startTime = 0f;
                var endTime = currentClip.length;
                var sampleCount = (int)(endTime * 90f); // Number of samples to take
                var timeStep = endTime / (sampleCount - 1); // Time between samples

                // Get the appropriate TrailTrack
                TrailTrack targetTrack = isRightHand ? animationEditor.bridge.RightHand : animationEditor.bridge.LeftHand;

                targetTrack.RootX = new AnimationCurve();
                targetTrack.RootY = new AnimationCurve();
                targetTrack.RootZ = new AnimationCurve();
                targetTrack.TipX = new AnimationCurve();
                targetTrack.TipY = new AnimationCurve();
                targetTrack.TipZ = new AnimationCurve();
                
                for (float time = startTime; time <= endTime; time += timeStep)
                {
                    // Sample the animation at this time
                    currentClip.SampleAnimation(animationEditor.gameObject, time);

                    // Add keyframe for this time
                    targetTrack.RootX.AddKey(new Keyframe(time, currentRoot.position.x));
                    targetTrack.RootY.AddKey(new Keyframe(time, currentRoot.position.y));
                    targetTrack.RootZ.AddKey(new Keyframe(time, currentRoot.position.z));
                    targetTrack.TipX.AddKey(new Keyframe(time, currentTip.position.x));
                    targetTrack.TipY.AddKey(new Keyframe(time, currentTip.position.y));
                    targetTrack.TipZ.AddKey(new Keyframe(time, currentTip.position.z));
                }

                // smooth all tangents
                for (int i = 0; i < targetTrack.RootX.length; i++)
                {
                    targetTrack.RootX.SmoothTangents(i, 0);
                    targetTrack.RootY.SmoothTangents(i, 0);
                    targetTrack.RootZ.SmoothTangents(i, 0);
                    targetTrack.TipX.SmoothTangents(i, 0);
                    targetTrack.TipY.SmoothTangents(i, 0);
                    targetTrack.TipZ.SmoothTangents(i, 0);
                }

                // Restore to current time
                currentClip.SampleAnimation(animationEditor.gameObject, CurrentTime);
                EditorUtility.SetDirty(animationEditor.bridge);
            })
            {
                text = $"Write {handName} Hand Positions to PrefetchedCurve_Bridge",
                tooltip = $"Steps through the animation and writes the positions of {handName}Root and {handName}Tip to the PrefetchedCurve_Bridge."
            };
            curveSection.Add(writePositionButton);
            
            // Add the curve section to the inspector
            myInspector.Add(curveSection);

            // Add visual separator
            var separator = new VisualElement();
            separator.style.height = 10;
            separator.style.borderBottomWidth = 1;
            separator.style.borderBottomColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            separator.style.marginBottom = 5;
            separator.style.marginTop = 5;
            myInspector.Add(separator);

            // Weapon Assignment Section
            var weaponSection = new VisualElement();
            weaponSection.style.paddingLeft = 10;
            weaponSection.style.paddingRight = 10;
            weaponSection.style.paddingTop = 5;
            weaponSection.style.paddingBottom = 5;
            
            var weaponSectionLabel = new Label("Weapon Assignment");
            weaponSectionLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            weaponSectionLabel.style.fontSize = 14;
            weaponSectionLabel.style.marginBottom = 10;
            weaponSection.Add(weaponSectionLabel);

            // Add a field to drop in a weapon GameObject
            GameObject weaponObject = null;
            var weaponField = new ObjectField("Weapon")
            {
                objectType = typeof(GameObject),
                allowSceneObjects = true
            };
            weaponField.RegisterValueChangedCallback(evt =>
            {
                weaponObject = evt.newValue as GameObject;
            });
            weaponSection.Add(weaponField);

            // Add a label to inform the user to drop in a weapon
            var label2 = new HelpBox(
                $"Drop in a weapon GameObject, then click the button below to attach it to the character and automatically set the {handName} Root and Tip.",
                HelpBoxMessageType.Info);

            weaponSection.Add(label2);

            // Add a button to attach and set weapon
            string wpnLocName = isRightHand ? "r_wpn1_loc" : "l_wpn1_loc";
            var attachAndSetWeaponButton = new Button(() =>
            {
                var go = animationEditor?.gameObject;
                if (go == null)
                {
                    Debug.LogError("Animation Editor GameObject is null");
                    return;
                }
                if (weaponObject == null)
                {
                    Debug.LogError("No weapon object assigned");
                    return;
                }
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
                
                var wpn1Loc = FindChildRecursive(go.transform, wpnLocName);
                if (wpn1Loc == null)
                {
                    Debug.LogError($"Could not find '{wpnLocName}' transform");
                    return;
                }
                // Attach weapon to wpn1_loc if not already parented
                if (weaponObject.transform.parent != wpn1Loc)
                {
                    Undo.SetTransformParent(weaponObject.transform, wpn1Loc, $"Attach Weapon to {wpnLocName}");
                    weaponObject.transform.localPosition = Vector3.zero;
                    weaponObject.transform.localRotation = Quaternion.identity;
                }
                // Find TrailRoot and TrailTip
                Transform trailRoot = weaponObject.transform.Find("TrailRoot");
                Transform trailTip = weaponObject.transform.Find("TrailTip");
                if (trailRoot == null || trailTip == null)
                {
                    Debug.LogError("Weapon must have children named 'TrailRoot' and 'TrailTip'");
                    return;
                }
                Undo.RecordObject(animationEditor, $"Assign {handName}Root and {handName}Tip");
                if (isRightHand)
                {
                    animationEditor.RightRoot = trailRoot;
                    animationEditor.RightTip = trailTip;
                }
                else
                {
                    animationEditor.LeftRoot = trailRoot;
                    animationEditor.LeftTip = trailTip;
                }
                EditorUtility.SetDirty(animationEditor);
                root.value = trailRoot;
                tip.value = trailTip;
                Debug.Log($"Weapon attached to {wpnLocName} and TrailRoot/TrailTip assigned.");
            })
            {
                text = $"Attach Weapon and Set {handName} Root/Tip",
                tooltip = $"Attach the weapon to {wpnLocName} and assign TrailRoot/TrailTip."
            };
            weaponSection.Add(attachAndSetWeaponButton);
            
            // Add the weapon section to the inspector
            myInspector.Add(weaponSection);
            
            // Add separator before refresh
            var refreshSeparator = new VisualElement();
            refreshSeparator.style.height = 10;
            refreshSeparator.style.borderBottomWidth = 1;
            refreshSeparator.style.borderBottomColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            refreshSeparator.style.marginBottom = 5;
            refreshSeparator.style.marginTop = 5;
            myInspector.Add(refreshSeparator);
            
            // Refresh Section
            var refreshSection = new VisualElement();
            refreshSection.style.paddingLeft = 10;
            refreshSection.style.paddingRight = 10;
            refreshSection.style.paddingTop = 5;
            refreshSection.style.paddingBottom = 10;
            
            // Add a refresh button to re-run the bridge auto-detection logic
            var refreshButton = new Button(() =>
            {
                myInspector.Clear();
                CreateCurveUI(myInspector);
            })
            {
                text = "Refresh Panel",
                tooltip = "Re-detect PrefetchedCurve_Bridge and refresh all UI elements"
            };
            refreshButton.style.height = 30;
            refreshSection.Add(refreshButton);
            
            myInspector.Add(refreshSection);
        }
    }
}
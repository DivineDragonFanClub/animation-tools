using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace DivineDragon.Windows
{
    public class CameraAnimationWindow : AnimationEditorInspectorHelper
    {
        private const string AutoKeyframeKey = "CameraAnimationWindow_AutoKeyframe";
        
        // UI Constants
        private const float KEYFRAME_TOLERANCE = 0.001f;
        private const float PAN_SENSITIVITY = 0.01f;
        private const float ZOOM_SENSITIVITY = 0.1f;
        private const float TILT_SENSITIVITY = 0.5f;
        private const float DEFAULT_LOOK_DISTANCE = 5f;
        private const int UPDATE_INTERVAL_MS = 10;
        
        [MenuItem("Divine Dragon/Animation Tools/Camera Preview")]
        public static void ShowWindow()
        {
            CameraAnimationWindow wnd = GetWindow<CameraAnimationWindow>();
            wnd.titleContent = new GUIContent("Camera Preview");
        }
        
        private bool autoKeyframe = false;
        
        // Camera preview
        private Camera previewCamera;
        private RenderTexture previewRenderTexture;
        private GameObject previewCameraGO;
        private Vector2 lastMousePosition;
        private bool isDragging = false;
        
        // Camera transform references
        private Transform camLookAtLoc;
        private Transform camFollowLoc;
        
        // Cached values for detecting changes
        private Vector3 lastCamLookAtPos;
        private Vector3 lastCamFollowPos;
        private Quaternion lastCamFollowRot;
        
        // UI element references
        private Button resetViewButton;
        
        private void LoadPreferences()
        {
            autoKeyframe = EditorPrefs.GetBool(AutoKeyframeKey, false);
        }
        
        private void SavePreferences()
        {
            EditorPrefs.SetBool(AutoKeyframeKey, autoKeyframe);
        }
        
        protected override void OnUnderlyingAnimationClipChanged()
        {
            base.OnUnderlyingAnimationClipChanged();
            if (root == null) return;
            
            root.Clear();
            LoadPreferences();
            CreateUI();
        }
        
        public void OnDestroy()
        {
            SavePreferences();
            EditorApplication.update -= OnUpdate;
            CleanupPreviewCamera();
        }
        
        private void SetupPreviewCamera()
        {
            if (previewCameraGO == null)
            {
                previewCameraGO = new GameObject("Camera Preview");
                previewCameraGO.hideFlags = HideFlags.HideAndDontSave;
                previewCamera = previewCameraGO.AddComponent<Camera>();
                previewCamera.clearFlags = CameraClearFlags.Skybox;
                previewCamera.cullingMask = -1; // Render everything
                previewCamera.enabled = false; // We'll render manually
            }
            
            // Create render texture with dynamic resolution based on window size
            int textureWidth = Mathf.Min(2048, Mathf.Max(1024, (int)(position.width * 2))); // 2x resolution for sharpness
            int textureHeight = (int)(textureWidth * 0.5625f); // 16:9 aspect ratio
            
            if (previewRenderTexture == null || 
                previewRenderTexture.width != textureWidth || 
                previewRenderTexture.height != textureHeight)
            {
                if (previewRenderTexture != null)
                {
                    previewRenderTexture.Release();
                }
                
                previewRenderTexture = new RenderTexture(textureWidth, textureHeight, 24);
                previewRenderTexture.antiAliasing = 4; // Enable 4x MSAA for smoother edges
                previewRenderTexture.Create();
                
                previewCamera.targetTexture = previewRenderTexture;
            }
            
            UpdatePreviewCamera();
        }
        
        private void CleanupPreviewCamera()
        {
            if (previewCameraGO != null)
            {
                DestroyImmediate(previewCameraGO);
                previewCameraGO = null;
                previewCamera = null;
            }
            
            if (previewRenderTexture != null)
            {
                previewRenderTexture.Release();
                previewRenderTexture = null;
            }
        }
        
        private void UpdatePreviewCamera()
        {
            if (previewCamera == null || camFollowLoc == null || camLookAtLoc == null) return;
            
            // Position preview camera to match the animation camera setup
            previewCamera.transform.position = camFollowLoc.position;
            
            Vector3 lookDirection = (camLookAtLoc.position - camFollowLoc.position).normalized;
            if (lookDirection.magnitude > 0.0f)
            {
                Quaternion lookRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
                
                // Apply the same camera tilt system as RenderCameraInfo
                Vector3 localEuler = camFollowLoc.localRotation.eulerAngles;
                float zRotation = -localEuler.y;  // Y rotation becomes Z rotation (tilt)
                
                Quaternion zRotationQuat = Quaternion.Euler(0, 0, zRotation);
                previewCamera.transform.rotation = lookRotation * zRotationQuat;
            }
        }
        
        private void OnUpdate()
        {
            // Check if window has been resized and update render texture if needed
            if (previewCamera != null && previewRenderTexture != null)
            {
                int desiredWidth = Mathf.Min(2048, Mathf.Max(1024, (int)(position.width * 2)));
                int desiredHeight = (int)(desiredWidth * 0.5625f);
                
                if (previewRenderTexture.width != desiredWidth || previewRenderTexture.height != desiredHeight)
                {
                    SetupPreviewCamera(); // This will recreate the render texture with new size
                }
            }
            
            // Force repaint for real-time updates
            Repaint();
        }
        
        private void LateUpdatePreviewCamera()
        {
            // Update and render preview camera
            if (previewCamera != null)
            {
                UpdatePreviewCamera();
                previewCamera.Render();
            }
        }
        
        private bool HasTransformChanged()
        {
            if (camLookAtLoc == null || camFollowLoc == null) return false;
            
            return camLookAtLoc.position != lastCamLookAtPos ||
                   camFollowLoc.position != lastCamFollowPos ||
                   camFollowLoc.rotation != lastCamFollowRot;
        }
        
        private void UpdateCachedTransforms()
        {
            if (camLookAtLoc != null)
                lastCamLookAtPos = camLookAtLoc.position;
            if (camFollowLoc != null)
            {
                lastCamFollowPos = camFollowLoc.position;
                lastCamFollowRot = camFollowLoc.rotation;
            }
        }
        
        private void UpdateButtonToggleState(Button button, bool isActive, string textPrefix)
        {
            button.text = isActive ? $"{textPrefix}: ON" : $"{textPrefix}: OFF";
            button.style.backgroundColor = isActive ? new StyleColor(new Color(0.4f, 0.6f, 0.4f, 0.5f)) : new StyleColor(StyleKeyword.Initial);
        }
        
        private void CreateUI()
        {
            var container = new VisualElement();
            container.style.paddingTop = 10;
            container.style.paddingLeft = 10;
            container.style.paddingRight = 10;
            
            // Get transform references
            UpdateTransformReferences();
            
            if (camLookAtLoc == null || camFollowLoc == null)
            {
                var warning = new HelpBox("Camera transforms not found. Make sure you have selected an object with AnimationEditor component.", HelpBoxMessageType.Warning);
                container.Add(warning);
                root.Add(container);
                return;
            }
            
            // Setup preview camera
            SetupPreviewCamera();
            
            // Camera preview section
            var previewSection = CreateCameraPreviewSection();
            container.Add(previewSection);
            
            // Current values section
            var currentValuesSection = CreateCurrentValuesSection();
            container.Add(currentValuesSection);
            
            // Tools section
            var toolsSection = CreateToolsSection();
            container.Add(toolsSection);
            
            root.Add(container);
            
            // Start update loop
            EditorApplication.update -= OnUpdate;
            EditorApplication.update += OnUpdate;
        }
        
        private void UpdateTransformReferences()
        {
            if (animationEditor == null) 
            {
                Debug.LogWarning("animationEditor is null in UpdateTransformReferences");
                return;
            }
            
            camLookAtLoc = animationEditor.transform.Find("c_trans/camLookAt_loc");
            camFollowLoc = animationEditor.transform.Find("c_trans/camFollow_loc");
            
            Debug.Log($"Transform references updated - camLookAtLoc: {camLookAtLoc}, camFollowLoc: {camFollowLoc}");
            
            UpdateCachedTransforms();
        }
        
        
        private VisualElement CreateCameraPreviewSection()
        {
            var section = new Box();
            section.style.marginBottom = 10;
            section.style.paddingTop = 5;
            section.style.paddingBottom = 5;
            section.style.paddingLeft = 5;
            section.style.paddingRight = 5;
            
            // Create title container to hold label and help button
            var titleContainer = new VisualElement();
            titleContainer.style.flexDirection = FlexDirection.Row;
            titleContainer.style.alignItems = Align.Center;
            titleContainer.style.marginBottom = 5;
            
            var title = new Label("Camera Preview");
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleContainer.Add(title);
            
            // Help button right next to the title
            var helpButton = new Button(() => {})
            {
                text = "?",
                tooltip = "Camera Preview Controls:\n\n" +
                         "• Drag: Pan camera\n" +
                         "• Shift+Drag: Dutch tilt (camera roll)\n" +
                         "• Scroll: Zoom in/out\n\n" +
                         "Note: Auto-keyframe must be enabled to save changes while interacting with the preview."
            };
            helpButton.style.marginLeft = 5;
            helpButton.style.width = 20;
            helpButton.style.height = 20;
            SetBorderRadius(helpButton.style, 10);
            helpButton.style.fontSize = 12;
            helpButton.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleContainer.Add(helpButton);
            
            section.Add(titleContainer);
            
            if (previewRenderTexture != null)
            {
                // Create preview container
                var previewContainer = new IMGUIContainer(() =>
                {
                    if (previewRenderTexture == null) return;
                    
                    // Make the preview responsive to window width
                    float windowWidth = position.width - 40; // Account for padding
                    float maxWidth = Mathf.Min(windowWidth, 800); // Cap at 800px max
                    float minWidth = 300; // Minimum width
                    float width = Mathf.Max(minWidth, maxWidth);
                    float height = width * 0.5625f; // 16:9 aspect ratio
                    
                    var rect = GUILayoutUtility.GetRect(width, height, GUILayout.ExpandWidth(false));
                    
                    // Handle mouse events for camera interaction
                    HandlePreviewMouseEvents(rect);
                    
                    // Update camera just before drawing to minimize jitter
                    LateUpdatePreviewCamera();
                    
                    // Draw the preview texture
                    GUI.DrawTexture(rect, previewRenderTexture, ScaleMode.ScaleToFit);
                });
                
                previewContainer.style.flexGrow = 1;
                previewContainer.style.alignSelf = Align.Stretch;
                previewContainer.style.marginLeft = 10;
                previewContainer.style.marginRight = 10;
                section.Add(previewContainer);
                
                // Preview controls container
                var controlsContainer = new VisualElement();
                controlsContainer.style.flexDirection = FlexDirection.Column;
                controlsContainer.style.marginTop = 5;
                
                // First row: Auto-keyframe and Reset
                var firstRow = new VisualElement();
                firstRow.style.flexDirection = FlexDirection.Row;
                firstRow.style.justifyContent = Justify.Center;
                firstRow.style.marginBottom = 5;
                
                // Auto-keyframe button
                Button autoKeyframeButton = null;
                autoKeyframeButton = new Button(() =>
                {
                    autoKeyframe = !autoKeyframe;
                    SavePreferences();
                    UpdateButtonToggleState(autoKeyframeButton, autoKeyframe, "Auto-Keyframe");
                    
                    // Update reset button state
                    if (resetViewButton != null)
                    {
                        resetViewButton.SetEnabled(!autoKeyframe);
                        resetViewButton.tooltip = autoKeyframe ? "Reset is disabled when Auto-Keyframe is on" : "Reset camera to match current animation camera";
                    }
                });
                autoKeyframeButton.tooltip = "Automatically create keyframes when you interact with the camera preview.\n\n" +
                                           "When ON:\n" +
                                           "• Pan, zoom, and tilt changes are immediately saved as keyframes\n" +
                                           "• Reset View button is disabled (nothing to reset)\n\n" +
                                           "When OFF:\n" +
                                           "• Preview changes are temporary until you manually keyframe\n" +
                                           "• Use Reset View to undo preview changes";
                UpdateButtonToggleState(autoKeyframeButton, autoKeyframe, "Auto-Keyframe");
                firstRow.Add(autoKeyframeButton);
                
                resetViewButton = new Button(() => ResetPreviewCamera())
                {
                    text = "Reset View",
                    tooltip = autoKeyframe ? "Reset is disabled when Auto-Keyframe is on" : "Reset camera to match current animation camera"
                };
                resetViewButton.SetEnabled(!autoKeyframe);
                resetViewButton.style.marginLeft = 5;
                firstRow.Add(resetViewButton);
                
                // Second row: Copy Scene Camera and Apply to Scene
                var secondRow = new VisualElement();
                secondRow.style.flexDirection = FlexDirection.Row;
                secondRow.style.justifyContent = Justify.Center;
                
                var copySceneCameraButton = new Button(() => CopySceneCameraTransform())
                {
                    text = "Copy Scene Camera Angle",
                    tooltip = "Copy position and rotation from the Scene view camera"
                };
                secondRow.Add(copySceneCameraButton);
                
                var applyToSceneCameraButton = new Button(() => ApplyToSceneCamera())
                {
                    text = "Apply Camera Angle to Scene",
                    tooltip = "Apply current preview camera view to the Scene view camera (excluding Dutch tilt)"
                };
                applyToSceneCameraButton.style.marginLeft = 5;
                secondRow.Add(applyToSceneCameraButton);
                
                controlsContainer.Add(firstRow);
                controlsContainer.Add(secondRow);
                section.Add(controlsContainer);
            }
            
            return section;
        }
        
        private void HandlePreviewMouseEvents(Rect rect)
        {
            Event e = Event.current;
            
            if (rect.Contains(e.mousePosition))
            {
                if (e.type == EventType.MouseDown && e.button == 0)
                {
                    isDragging = true;
                    lastMousePosition = e.mousePosition;
                    e.Use();
                }
                else if (e.type == EventType.MouseDrag && isDragging)
                {
                    Vector2 deltaMousePosition = e.mousePosition - lastMousePosition;
                    
                    // Check if Shift is held for Dutch tilt (camera rotation)
                    if (e.shift)
                    {
                        DutchTiltCamera(deltaMousePosition);
                    }
                    else
                    {
                        // Pan the camera based on mouse movement
                        PanPreviewCamera(deltaMousePosition);
                    }
                    
                    lastMousePosition = e.mousePosition;
                    e.Use();
                }
                else if (e.type == EventType.MouseUp && e.button == 0)
                {
                    isDragging = false;
                    e.Use();
                }
                else if (e.type == EventType.ScrollWheel)
                {
                    // Zoom with scroll wheel
                    ZoomPreviewCamera(e.delta.y);
                    e.Use();
                }
            }
            
            if (e.type == EventType.MouseUp)
            {
                isDragging = false;
            }
        }
        private void PanPreviewCamera(Vector2 mouseDelta)
        {
            if (camFollowLoc == null || camLookAtLoc == null) return;
            
            float sensitivity = PAN_SENSITIVITY;
            
            // Calculate right and up vectors relative to camera
            Vector3 lookDirection = (camLookAtLoc.position - camFollowLoc.position).normalized;
            Vector3 rightVector = Vector3.Cross(lookDirection, Vector3.up).normalized;
            Vector3 upVector = Vector3.Cross(rightVector, lookDirection).normalized;
            
            // Apply pan movement (positive mouseDelta.x for right movement)
            Vector3 panMovement = (rightVector * mouseDelta.x + upVector * mouseDelta.y) * sensitivity;
            
            camFollowLoc.position += panMovement;
            camLookAtLoc.position += panMovement;
            
            UpdateCachedTransforms();
            
            // Auto-keyframe if enabled
            if (autoKeyframe)
            {
                Debug.Log("Auto-keyframing from pan");
                KeyframeCurrentPose();
            }
        }
        
        private void ZoomPreviewCamera(float scrollDelta)
        {
            if (camFollowLoc == null || camLookAtLoc == null) return;
            
            float zoomSensitivity = ZOOM_SENSITIVITY;
            
            Vector3 lookDirection = (camLookAtLoc.position - camFollowLoc.position).normalized;
            Vector3 zoomMovement = lookDirection * scrollDelta * zoomSensitivity;
            
            camFollowLoc.position += zoomMovement;
            
            UpdateCachedTransforms();
            
            // Auto-keyframe if enabled
            if (autoKeyframe)
            {
                Debug.Log("Auto-keyframing from zoom");
                KeyframeCurrentPose();
            }
        }
        
        private void DutchTiltCamera(Vector2 mouseDelta)
        {
            if (camFollowLoc == null) return;
            
            float tiltSensitivity = TILT_SENSITIVITY;
            
            // The camera system uses Y rotation of camFollowLoc to create Z rotation (tilt) in the final camera
            // Use horizontal mouse movement for tilt rotation
            float tiltAmount = mouseDelta.x * tiltSensitivity;
            
            // Get current rotation and modify Y rotation
            Vector3 currentEuler = camFollowLoc.localRotation.eulerAngles;
            currentEuler.y += tiltAmount;
            
            // Keep Y rotation in reasonable range (-180 to 180)
            if (currentEuler.y > 180f)
                currentEuler.y -= 360f;
            else if (currentEuler.y < -180f)
                currentEuler.y += 360f;
            
            camFollowLoc.localRotation = Quaternion.Euler(currentEuler);
            
            UpdateCachedTransforms();
            
            // Auto-keyframe if enabled
            if (autoKeyframe)
            {
                Debug.Log("Auto-keyframing from dutch tilt");
                KeyframeCurrentPose();
            }
        }
        
        private void ResetPreviewCamera()
        {
            UpdatePreviewCamera();
        }
        
        private void CopySceneCameraTransform()
        {
            if (camFollowLoc == null || camLookAtLoc == null) return;
            
            // Get the Scene view camera
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null || sceneView.camera == null)
            {
                Debug.LogWarning("No active Scene view found");
                return;
            }
            
            Camera sceneCam = sceneView.camera;
            Transform sceneCamTransform = sceneCam.transform;
            
            // Set the camera follow position to match the scene camera
            camFollowLoc.position = sceneCamTransform.position;
            
            // Calculate where the camera is looking
            // The scene camera's forward direction determines the look-at point
            float lookDistance = DEFAULT_LOOK_DISTANCE; // Default distance for the look-at point
            
            // Try to maintain the current distance between camFollow and camLookAt
            float currentDistance = Vector3.Distance(camFollowLoc.position, camLookAtLoc.position);
            if (currentDistance > 0.1f)
            {
                lookDistance = currentDistance;
            }
            
            // Set the look-at position
            camLookAtLoc.position = camFollowLoc.position + sceneCamTransform.forward * lookDistance;
            
            // Handle camera tilt/roll
            // Extract the Z rotation from the scene camera
            Vector3 sceneEuler = sceneCamTransform.rotation.eulerAngles;
            float zRotation = sceneEuler.z;
            
            // In the animation system, Y rotation of camFollowLoc becomes Z rotation
            // So we need to set Y rotation to match the scene camera's Z rotation
            Vector3 followEuler = camFollowLoc.localRotation.eulerAngles;
            followEuler.y = -zRotation; // Negative because of the conversion
            camFollowLoc.localRotation = Quaternion.Euler(followEuler);
            
            UpdateCachedTransforms();
            
            // Auto-keyframe if enabled
            if (autoKeyframe)
            {
                Debug.Log("Auto-keyframing from scene camera copy");
                KeyframeCurrentPose();
            }
            
            Debug.Log($"Copied Scene camera transform - Position: {sceneCamTransform.position}, Rotation: {sceneEuler}");
        }
        
        private void ApplyToSceneCamera()
        {
            if (camFollowLoc == null || camLookAtLoc == null) return;
            
            // Get the Scene view camera
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null || sceneView.camera == null)
            {
                Debug.LogWarning("No active Scene view found");
                return;
            }
            
            // Calculate the camera position and look direction
            Vector3 cameraPosition = camFollowLoc.position;
            Vector3 lookAtPosition = camLookAtLoc.position;
            
            // Set the scene camera position
            sceneView.pivot = lookAtPosition;
            sceneView.rotation = Quaternion.LookRotation(lookAtPosition - cameraPosition);
            sceneView.size = Vector3.Distance(cameraPosition, lookAtPosition) * 0.5f;
            
            // Note: Dutch tilt (Z rotation) cannot be applied to Scene view camera
            // as Unity's Scene view doesn't support roll rotation
            
            Debug.Log($"Applied preview camera to Scene view - Position: {cameraPosition}, LookAt: {lookAtPosition}");
            
            // Repaint the scene view to show the changes
            sceneView.Repaint();
        }
        
        private VisualElement CreateCurrentValuesSection()
        {
            var section = new Box();
            section.style.marginBottom = 10;
            section.style.paddingTop = 5;
            section.style.paddingBottom = 5;
            section.style.paddingLeft = 5;
            section.style.paddingRight = 5;
            
            // Header with title and keyframe all button
            var headerContainer = new VisualElement();
            headerContainer.style.flexDirection = FlexDirection.Row;
            headerContainer.style.justifyContent = Justify.SpaceBetween;
            headerContainer.style.alignItems = Align.Center;
            headerContainer.style.marginBottom = 5;
            
            var title = new Label("Current Transform Values");
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            headerContainer.Add(title);
            
            var keyframeAllButton = new Button(() => KeyframeCurrentPose());
            keyframeAllButton.text = "Key All";
            keyframeAllButton.tooltip = "Keyframe all camera transforms";
            keyframeAllButton.style.fontSize = 12;
            keyframeAllButton.style.paddingLeft = 8;
            keyframeAllButton.style.paddingRight = 8;
            keyframeAllButton.style.paddingTop = 2;
            keyframeAllButton.style.paddingBottom = 2;
            headerContainer.Add(keyframeAllButton);
            
            section.Add(headerContainer);
            
            // Declare button references for later use in the update loop
            Button camLookAtPrevButton = null;
            Button camLookAtKeyButton = null;
            Button camLookAtNextButton = null;
            Button camFollowPosPrevButton = null;
            Button camFollowPosKeyButton = null;
            Button camFollowPosNextButton = null;
            Button camFollowRotPrevButton = null;
            Button camFollowRotKeyButton = null;
            Button camFollowRotNextButton = null;
            
            // CamLookAt position
            var camLookAtContainer = new VisualElement();
            camLookAtContainer.style.flexDirection = FlexDirection.Row;
            camLookAtContainer.style.alignItems = Align.Center;
            camLookAtContainer.style.marginBottom = 5;
            camLookAtContainer.style.width = 400;
            
            // Previous keyframe button
            camLookAtPrevButton = CreateNavigationButton("◄", "Go to previous keyframe", () => {
                NavigateToKeyframe(camLookAtLoc, true, false, false);
            });
            camLookAtContainer.Add(camLookAtPrevButton);
            
            camLookAtKeyButton = CreateKeyframeToggleButton("Toggle keyframe for CamLookAt position", camLookAtLoc, true);
            camLookAtContainer.Add(camLookAtKeyButton);
            
            // Next keyframe button
            camLookAtNextButton = CreateNavigationButton("►", "Go to next keyframe", () => {
                NavigateToKeyframe(camLookAtLoc, true, false, true);
            });
            camLookAtNextButton.style.marginRight = 5;
            camLookAtContainer.Add(camLookAtNextButton);
            
            var camLookAtField = new Vector3Field("Cam Look At Position");
            camLookAtField.style.flexDirection = FlexDirection.Column;
            camLookAtField.style.minWidth = 300;  // Ensure enough space for X, Y, Z fields
            camLookAtField.value = camLookAtLoc.position;
            camLookAtField.tooltip = "Position of the camera look-at target (bone: camLookAt_loc)";
            camLookAtField.RegisterValueChangedCallback(evt =>
            {
                camLookAtLoc.position = evt.newValue;
                UpdateCachedTransforms();
                if (autoKeyframe)
                {
                    Debug.Log("Auto-keyframing from CamLookAt field edit");
                    KeyframeCurrentPose();
                }
            });
            camLookAtContainer.Add(camLookAtField);
            
            section.Add(camLookAtContainer);
            
            // CamFollow position
            var camFollowPosContainer = new VisualElement();
            camFollowPosContainer.style.flexDirection = FlexDirection.Row;
            camFollowPosContainer.style.alignItems = Align.Center;
            camFollowPosContainer.style.marginBottom = 5;
            camFollowPosContainer.style.width = 400;
            
            // Previous keyframe button
            camFollowPosPrevButton = new Button(() => {
                var clip = getAttachedClip();
                var animWindow = GetAnimationWindow();
                if (clip != null && animWindow != null)
                {
                    float prevTime = GetPreviousKeyframeTime(clip, camFollowLoc, animWindow.time, true, false);
                    if (prevTime >= 0)
                    {
                        animWindow.time = prevTime;
                        animWindow.Repaint();
                    }
                }
            });
            camFollowPosPrevButton.text = "◄";
            camFollowPosPrevButton.tooltip = "Go to previous keyframe";
            camFollowPosPrevButton.style.width = 20;
            camFollowPosPrevButton.style.height = 20;
            camFollowPosPrevButton.style.marginRight = 2;
            camFollowPosPrevButton.style.fontSize = 12;
            camFollowPosContainer.Add(camFollowPosPrevButton);
            
            camFollowPosKeyButton = new Button(() => {
                var clip = getAttachedClip();
                var animWindow = GetAnimationWindow();
                if (clip != null && animWindow != null)
                {
                    float currentTime = animWindow.time;
                    if (HasKeyframeAtTime(clip, camFollowLoc, currentTime, true, false))
                    {
                        RemoveKeyframeForTransform(camFollowLoc, currentTime, true, false);
                    }
                    else
                    {
                        SetKeyframeForTransform(camFollowLoc, true, false);
                    }
                }
            });
            camFollowPosKeyButton.text = "◆";
            camFollowPosKeyButton.tooltip = "Toggle keyframe for CamFollow position";
            camFollowPosKeyButton.style.width = 25;
            camFollowPosKeyButton.style.height = 20;
            camFollowPosKeyButton.style.marginRight = 2;
            camFollowPosKeyButton.style.fontSize = 14;
            camFollowPosContainer.Add(camFollowPosKeyButton);
            
            // Next keyframe button
            camFollowPosNextButton = new Button(() => {
                var clip = getAttachedClip();
                var animWindow = GetAnimationWindow();
                if (clip != null && animWindow != null)
                {
                    float nextTime = GetNextKeyframeTime(clip, camFollowLoc, animWindow.time, true, false);
                    if (nextTime >= 0)
                    {
                        animWindow.time = nextTime;
                        animWindow.Repaint();
                    }
                }
            });
            camFollowPosNextButton.text = "►";
            camFollowPosNextButton.tooltip = "Go to next keyframe";
            camFollowPosNextButton.style.width = 20;
            camFollowPosNextButton.style.height = 20;
            camFollowPosNextButton.style.marginRight = 5;
            camFollowPosNextButton.style.fontSize = 12;
            camFollowPosContainer.Add(camFollowPosNextButton);
            
            var camFollowPosField = new Vector3Field("Cam Follow Position");
            camFollowPosField.style.flexDirection = FlexDirection.Column;
            camFollowPosField.style.minWidth = 300; // Ensure enough space for X, Y, Z fields
            camFollowPosField.value = camFollowLoc.position;
            camFollowPosField.tooltip = "Position of the camera (bone: camFollow_loc)";
            camFollowPosField.RegisterValueChangedCallback(evt =>
            {
                camFollowLoc.position = evt.newValue;
                UpdateCachedTransforms();
                if (autoKeyframe)
                {
                    Debug.Log("Auto-keyframing from CamFollow position field edit");
                    KeyframeCurrentPose();
                }
            });
            camFollowPosContainer.Add(camFollowPosField);
            
            section.Add(camFollowPosContainer);
            
            // CamFollow rotation
            var camFollowRotContainer = new VisualElement();
            camFollowRotContainer.style.flexDirection = FlexDirection.Row;
            camFollowRotContainer.style.alignItems = Align.Center;
            camFollowRotContainer.style.marginBottom = 5;
            camFollowRotContainer.style.width = 400;
            
            // Previous keyframe button
            camFollowRotPrevButton = new Button(() => {
                var clip = getAttachedClip();
                var animWindow = GetAnimationWindow();
                if (clip != null && animWindow != null)
                {
                    float prevTime = GetPreviousKeyframeTime(clip, camFollowLoc, animWindow.time, false, true);
                    if (prevTime >= 0)
                    {
                        animWindow.time = prevTime;
                        animWindow.Repaint();
                    }
                }
            });
            camFollowRotPrevButton.text = "◄";
            camFollowRotPrevButton.tooltip = "Go to previous keyframe";
            camFollowRotPrevButton.style.width = 20;
            camFollowRotPrevButton.style.height = 20;
            camFollowRotPrevButton.style.marginRight = 2;
            camFollowRotPrevButton.style.fontSize = 12;
            camFollowRotContainer.Add(camFollowRotPrevButton);
            
            camFollowRotKeyButton = new Button(() => {
                var clip = getAttachedClip();
                var animWindow = GetAnimationWindow();
                if (clip != null && animWindow != null)
                {
                    float currentTime = animWindow.time;
                    if (HasKeyframeAtTime(clip, camFollowLoc, currentTime, false, true))
                    {
                        RemoveKeyframeForTransform(camFollowLoc, currentTime, false, true);
                    }
                    else
                    {
                        SetKeyframeForTransform(camFollowLoc, false, true);
                    }
                }
            });
            camFollowRotKeyButton.text = "◆";
            camFollowRotKeyButton.tooltip = "Toggle keyframe for CamFollow rotation";
            camFollowRotKeyButton.style.width = 25;
            camFollowRotKeyButton.style.height = 20;
            camFollowRotKeyButton.style.marginRight = 2;
            camFollowRotKeyButton.style.fontSize = 14;
            camFollowRotContainer.Add(camFollowRotKeyButton);
            
            // Next keyframe button
            camFollowRotNextButton = new Button(() => {
                var clip = getAttachedClip();
                var animWindow = GetAnimationWindow();
                if (clip != null && animWindow != null)
                {
                    float nextTime = GetNextKeyframeTime(clip, camFollowLoc, animWindow.time, false, true);
                    if (nextTime >= 0)
                    {
                        animWindow.time = nextTime;
                        animWindow.Repaint();
                    }
                }
            });
            camFollowRotNextButton.text = "►";
            camFollowRotNextButton.tooltip = "Go to next keyframe";
            camFollowRotNextButton.style.width = 20;
            camFollowRotNextButton.style.height = 20;
            camFollowRotNextButton.style.marginRight = 5;
            camFollowRotNextButton.style.fontSize = 12;
            camFollowRotContainer.Add(camFollowRotNextButton);
            
            var camFollowRotField = new Vector3Field("Cam Follow Rotation");
            camFollowRotField.style.flexDirection = FlexDirection.Column;
            camFollowRotField.style.minWidth = 300;  // Ensure enough space for X, Y, Z fields
            camFollowRotField.value = camFollowLoc.rotation.eulerAngles;
            camFollowRotField.tooltip = "Rotation of the camera in Euler angles (bone: camFollow_loc). Y rotation controls Dutch tilt.";
            camFollowRotField.RegisterValueChangedCallback(evt =>
            {
                camFollowLoc.rotation = Quaternion.Euler(evt.newValue);
                UpdateCachedTransforms();
                if (autoKeyframe)
                {
                    Debug.Log("Auto-keyframing from CamFollow rotation field edit");
                    KeyframeCurrentPose();
                }
            });
            camFollowRotContainer.Add(camFollowRotField);
            
            section.Add(camFollowRotContainer);
            
            // Update fields and keyframe indicators in real-time
            section.schedule.Execute(() =>
            {
                var animWindow = GetAnimationWindow();
                var clip = getAttachedClip();
                if (animWindow != null && clip != null)
                {
                    float currentTime = animWindow.time;
                    
                    // Update CamLookAt
                    if (camLookAtLoc != null)
                    {
                        bool hasFocus = false;
                        camLookAtField.Query<TextField>().ForEach(tf => { if (tf.focusController?.focusedElement == tf) hasFocus = true; });
                        if (!hasFocus)
                            camLookAtField.SetValueWithoutNotify(camLookAtLoc.position);
                        
                        // Update keyframe indicator
                        bool hasKeyframe = HasKeyframeAtTime(clip, camLookAtLoc, currentTime, true, false);
                        camLookAtKeyButton.text = hasKeyframe ? "◆" : "◇";
                        camLookAtKeyButton.style.color = Color.white;
                        
                        // Enable/disable prev/next buttons
                        float prevTime = GetPreviousKeyframeTime(clip, camLookAtLoc, currentTime, true, false);
                        float nextTime = GetNextKeyframeTime(clip, camLookAtLoc, currentTime, true, false);
                        camLookAtPrevButton.SetEnabled(prevTime >= 0);
                        camLookAtNextButton.SetEnabled(nextTime >= 0);
                    }
                    
                    // Update CamFollow
                    if (camFollowLoc != null)
                    {
                        bool hasPosFieldFocus = false;
                        camFollowPosField.Query<TextField>().ForEach(tf => { if (tf.focusController?.focusedElement == tf) hasPosFieldFocus = true; });
                        if (!hasPosFieldFocus)
                            camFollowPosField.SetValueWithoutNotify(camFollowLoc.position);
                        
                        bool hasRotFieldFocus = false;
                        camFollowRotField.Query<TextField>().ForEach(tf => { if (tf.focusController?.focusedElement == tf) hasRotFieldFocus = true; });
                        if (!hasRotFieldFocus)
                            camFollowRotField.SetValueWithoutNotify(camFollowLoc.rotation.eulerAngles);
                        
                        // Update keyframe indicators
                        bool hasPosKeyframe = HasKeyframeAtTime(clip, camFollowLoc, currentTime, true, false);
                        camFollowPosKeyButton.text = hasPosKeyframe ? "◆" : "◇";
                        camFollowPosKeyButton.style.color = Color.white;
                        
                        // Enable/disable prev/next buttons for position
                        float posPrevTime = GetPreviousKeyframeTime(clip, camFollowLoc, currentTime, true, false);
                        float posNextTime = GetNextKeyframeTime(clip, camFollowLoc, currentTime, true, false);
                        camFollowPosPrevButton.SetEnabled(posPrevTime >= 0);
                        camFollowPosNextButton.SetEnabled(posNextTime >= 0);
                        
                        bool hasRotKeyframe = HasKeyframeAtTime(clip, camFollowLoc, currentTime, false, true);
                        camFollowRotKeyButton.text = hasRotKeyframe ? "◆" : "◇";
                        camFollowRotKeyButton.style.color = Color.white;
                        
                        // Enable/disable prev/next buttons for rotation
                        float rotPrevTime = GetPreviousKeyframeTime(clip, camFollowLoc, currentTime, false, true);
                        float rotNextTime = GetNextKeyframeTime(clip, camFollowLoc, currentTime, false, true);
                        camFollowRotPrevButton.SetEnabled(rotPrevTime >= 0);
                        camFollowRotNextButton.SetEnabled(rotNextTime >= 0);
                    }
                }
            }).Every(UPDATE_INTERVAL_MS);
            
            return section;
        }
        
        private void KeyframeCurrentPose()
        {
            KeyframeCamLookAt();
            KeyframeCamFollow();
            UpdateCachedTransforms();
        }
        
        private void KeyframeCamLookAt()
        {
            if (camLookAtLoc == null) return;
            SetKeyframeForTransform(camLookAtLoc, true, false);
        }
        
        private void KeyframeCamFollow()
        {
            if (camFollowLoc == null) return;
            SetKeyframeForTransform(camFollowLoc, true, true);
        }
        
        
        private void SetKeyframeForTransform(Transform transform, bool position, bool rotation)
        {
            var animWindow = GetAnimationWindow();
            if (animWindow == null) 
            {
                Debug.LogError("Animation window is null");
                return;
            }
            
            var clip = getAttachedClip();
            if (clip == null) 
            {
                Debug.LogError("Animation clip is null");
                return;
            }
            
            float time = animWindow.time;
            string path = AnimationUtility.CalculateTransformPath(transform, animationEditor.transform);
            
            Debug.Log($"Setting keyframe for {transform.name} at time {time}, path: {path}");
            
            // Record the object for undo
            Undo.RecordObject(clip, "Set Camera Keyframe");
            
            if (position)
            {
                AnimationCurve xCurve = AnimationUtility.GetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), "m_LocalPosition.x")) ?? new AnimationCurve();
                AnimationCurve yCurve = AnimationUtility.GetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), "m_LocalPosition.y")) ?? new AnimationCurve();
                AnimationCurve zCurve = AnimationUtility.GetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), "m_LocalPosition.z")) ?? new AnimationCurve();
                
                SetOrUpdateKeyframe(xCurve, time, transform.localPosition.x);
                SetOrUpdateKeyframe(yCurve, time, transform.localPosition.y);
                SetOrUpdateKeyframe(zCurve, time, transform.localPosition.z);
                
                AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), "m_LocalPosition.x"), xCurve);
                AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), "m_LocalPosition.y"), yCurve);
                AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), "m_LocalPosition.z"), zCurve);
            }
            
            if (rotation)
            {
                AnimationCurve xCurve = AnimationUtility.GetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), "localEulerAnglesRaw.x")) ?? new AnimationCurve();
                AnimationCurve yCurve = AnimationUtility.GetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), "localEulerAnglesRaw.y")) ?? new AnimationCurve();
                AnimationCurve zCurve = AnimationUtility.GetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), "localEulerAnglesRaw.z")) ?? new AnimationCurve();
                
                Vector3 euler = transform.localEulerAngles;
                SetOrUpdateKeyframe(xCurve, time, euler.x);
                SetOrUpdateKeyframe(yCurve, time, euler.y);
                SetOrUpdateKeyframe(zCurve, time, euler.z);
                
                AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), "localEulerAnglesRaw.x"), xCurve);
                AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), "localEulerAnglesRaw.y"), yCurve);
                AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), "localEulerAnglesRaw.z"), zCurve);
            }
            
            // Mark the clip as dirty to save changes
            EditorUtility.SetDirty(clip);
            
            // Refresh the Animation window
            if (animWindow != null)
            {
                animWindow.Repaint();
            }
        }
        
        private void SetOrUpdateKeyframe(AnimationCurve curve, float time, float value)
        {
            // Find if keyframe exists at this time
            for (int i = 0; i < curve.length; i++)
            {
                if (Mathf.Approximately(curve[i].time, time))
                {
                    // Update existing keyframe
                    var key = curve[i];
                    key.value = value;
                    curve.MoveKey(i, key);
                    return;
                }
            }
            
            // Add new keyframe
            curve.AddKey(time, value);
        }
        
        private bool HasKeyframeAtTime(AnimationClip clip, Transform transform, float time, bool checkPosition, bool checkRotation)
        {
            string path = AnimationUtility.CalculateTransformPath(transform, animationEditor.transform);
            float tolerance = KEYFRAME_TOLERANCE; // Small tolerance for floating point comparison
            
            if (checkPosition)
            {
                var xCurve = AnimationUtility.GetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), "m_LocalPosition.x"));
                var yCurve = AnimationUtility.GetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), "m_LocalPosition.y"));
                var zCurve = AnimationUtility.GetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), "m_LocalPosition.z"));
                
                if (xCurve != null && yCurve != null && zCurve != null)
                {
                    bool hasX = HasKeyframeInCurve(xCurve, time, tolerance);
                    bool hasY = HasKeyframeInCurve(yCurve, time, tolerance);
                    bool hasZ = HasKeyframeInCurve(zCurve, time, tolerance);
                    
                    return hasX && hasY && hasZ;
                }
            }
            
            if (checkRotation)
            {
                var xCurve = AnimationUtility.GetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), "localEulerAnglesRaw.x"));
                var yCurve = AnimationUtility.GetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), "localEulerAnglesRaw.y"));
                var zCurve = AnimationUtility.GetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), "localEulerAnglesRaw.z"));
                
                if (xCurve != null && yCurve != null && zCurve != null)
                {
                    bool hasX = HasKeyframeInCurve(xCurve, time, tolerance);
                    bool hasY = HasKeyframeInCurve(yCurve, time, tolerance);
                    bool hasZ = HasKeyframeInCurve(zCurve, time, tolerance);
                    
                    return hasX && hasY && hasZ;
                }
            }
            
            return false;
        }
        
        private void RemoveKeyframeForTransform(Transform transform, float time, bool position, bool rotation)
        {
            var animWindow = GetAnimationWindow();
            if (animWindow == null) return;
            
            var clip = getAttachedClip();
            if (clip == null) return;
            
            string path = AnimationUtility.CalculateTransformPath(transform, animationEditor.transform);
            float tolerance = 0.001f;
            
            // Record the object for undo
            Undo.RecordObject(clip, "Remove Camera Keyframe");
            
            if (position)
            {
                RemoveKeyframeFromCurve(clip, path, "m_LocalPosition.x", time, tolerance);
                RemoveKeyframeFromCurve(clip, path, "m_LocalPosition.y", time, tolerance);
                RemoveKeyframeFromCurve(clip, path, "m_LocalPosition.z", time, tolerance);
            }
            
            if (rotation)
            {
                RemoveKeyframeFromCurve(clip, path, "localEulerAnglesRaw.x", time, tolerance);
                RemoveKeyframeFromCurve(clip, path, "localEulerAnglesRaw.y", time, tolerance);
                RemoveKeyframeFromCurve(clip, path, "localEulerAnglesRaw.z", time, tolerance);
            }
            
            // Mark the clip as dirty to save changes
            EditorUtility.SetDirty(clip);
            
            // Refresh the Animation window
            if (animWindow != null)
            {
                animWindow.Repaint();
            }
        }
        
        private float GetNextKeyframeTime(AnimationClip clip, Transform transform, float currentTime, bool checkPosition, bool checkRotation)
        {
            string path = AnimationUtility.CalculateTransformPath(transform, animationEditor.transform);
            float nextTime = float.MaxValue;
            float tolerance = 0.001f;
            
            if (checkPosition)
            {
                var xCurve = AnimationUtility.GetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), "m_LocalPosition.x"));
                if (xCurve != null)
                {
                    for (int i = 0; i < xCurve.length; i++)
                    {
                        if (xCurve[i].time > currentTime + tolerance && xCurve[i].time < nextTime)
                        {
                            nextTime = xCurve[i].time;
                        }
                    }
                }
            }
            
            if (checkRotation)
            {
                var xCurve = AnimationUtility.GetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), "localEulerAnglesRaw.x"));
                if (xCurve != null)
                {
                    for (int i = 0; i < xCurve.length; i++)
                    {
                        if (xCurve[i].time > currentTime + tolerance && xCurve[i].time < nextTime)
                        {
                            nextTime = xCurve[i].time;
                        }
                    }
                }
            }
            
            return nextTime == float.MaxValue ? -1f : nextTime;
        }
        
        private float GetPreviousKeyframeTime(AnimationClip clip, Transform transform, float currentTime, bool checkPosition, bool checkRotation)
        {
            string path = AnimationUtility.CalculateTransformPath(transform, animationEditor.transform);
            float prevTime = -1f;
            float tolerance = 0.001f;
            
            if (checkPosition)
            {
                var xCurve = AnimationUtility.GetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), "m_LocalPosition.x"));
                if (xCurve != null)
                {
                    for (int i = xCurve.length - 1; i >= 0; i--)
                    {
                        if (xCurve[i].time < currentTime - tolerance && xCurve[i].time > prevTime)
                        {
                            prevTime = xCurve[i].time;
                        }
                    }
                }
            }
            
            if (checkRotation)
            {
                var xCurve = AnimationUtility.GetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), "localEulerAnglesRaw.x"));
                if (xCurve != null)
                {
                    for (int i = xCurve.length - 1; i >= 0; i--)
                    {
                        if (xCurve[i].time < currentTime - tolerance && xCurve[i].time > prevTime)
                        {
                            prevTime = xCurve[i].time;
                        }
                    }
                }
            }
            
            return prevTime;
        }
        
        private VisualElement CreateToolsSection()
        {
            var section = new Box();
            section.style.marginBottom = 10;
            section.style.paddingTop = 5;
            section.style.paddingBottom = 5;
            section.style.paddingLeft = 5;
            section.style.paddingRight = 5;
            
            var title = new Label("Tools");
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.marginBottom = 5;
            section.Add(title);
            
            // Copy keyframes button
            var copyKeyframesButton = new Button(() => ShowCopyKeyframesWindow());
            copyKeyframesButton.text = "Copy Camera Animation Keyframes from Another Clip";
            copyKeyframesButton.tooltip = "Open a window to copy camera keyframes from another animation clip";
            copyKeyframesButton.style.marginTop = 5;
            copyKeyframesButton.style.marginBottom = 5;
            copyKeyframesButton.style.height = 30;
            section.Add(copyKeyframesButton);
            
            return section;
        }
        
        private void ShowCopyKeyframesWindow()
        {
            var window = ScriptableObject.CreateInstance<CopyCameraKeyframesWindow>();
            window.targetWindow = this;
            window.ShowUtility();
        }
        
        public void CopyKeyframesFromClip(AnimationClip sourceClip)
        {
            if (sourceClip == null) return;
            
            var targetClip = getAttachedClip();
            if (targetClip == null) return;
            
            // Record undo
            Undo.RecordObject(targetClip, "Copy Camera Keyframes");
            
            // Get the transform paths
            string camLookAtPath = AnimationUtility.CalculateTransformPath(camLookAtLoc, animationEditor.transform);
            string camFollowPath = AnimationUtility.CalculateTransformPath(camFollowLoc, animationEditor.transform);
            
            // Copy camLookAt position curves
            CopyCurve(sourceClip, targetClip, camLookAtPath, "m_LocalPosition.x");
            CopyCurve(sourceClip, targetClip, camLookAtPath, "m_LocalPosition.y");
            CopyCurve(sourceClip, targetClip, camLookAtPath, "m_LocalPosition.z");
            
            // Copy camFollow position curves
            CopyCurve(sourceClip, targetClip, camFollowPath, "m_LocalPosition.x");
            CopyCurve(sourceClip, targetClip, camFollowPath, "m_LocalPosition.y");
            CopyCurve(sourceClip, targetClip, camFollowPath, "m_LocalPosition.z");
            
            // Copy camFollow rotation curves
            CopyCurve(sourceClip, targetClip, camFollowPath, "localEulerAnglesRaw.x");
            CopyCurve(sourceClip, targetClip, camFollowPath, "localEulerAnglesRaw.y");
            CopyCurve(sourceClip, targetClip, camFollowPath, "localEulerAnglesRaw.z");
            
            EditorUtility.SetDirty(targetClip);
            
            // Refresh the Animation window
            var animWindow = GetAnimationWindow();
            if (animWindow != null)
            {
                animWindow.Repaint();
            }
            
            Debug.Log($"Successfully copied camera keyframes from {sourceClip.name} to {targetClip.name}");
        }
        
        private void CopyCurve(AnimationClip sourceClip, AnimationClip targetClip, string path, string propertyName)
        {
            var binding = EditorCurveBinding.FloatCurve(path, typeof(Transform), propertyName);
            var sourceCurve = AnimationUtility.GetEditorCurve(sourceClip, binding);
            
            if (sourceCurve != null && sourceCurve.length > 0)
            {
                AnimationUtility.SetEditorCurve(targetClip, binding, sourceCurve);
            }
        }
        
        // Helper methods for UI creation
        private Button CreateNavigationButton(string text, string tooltip, Action onClick)
        {
            var button = new Button(onClick);
            button.text = text;
            button.tooltip = tooltip;
            button.style.width = 20;
            button.style.height = 20;
            button.style.marginRight = 2;
            button.style.fontSize = 12;
            return button;
        }
        
        private Button CreateKeyframeToggleButton(string tooltip, Transform transform, bool isPosition)
        {
            var button = new Button(() => ToggleKeyframe(transform, isPosition));
            button.text = "◆";
            button.tooltip = tooltip;
            button.style.width = 25;
            button.style.height = 20;
            button.style.marginRight = 2;
            button.style.fontSize = 14;
            return button;
        }
        
        private void NavigateToKeyframe(Transform transform, bool checkPosition, bool checkRotation, bool goNext)
        {
            var clip = getAttachedClip();
            var animWindow = GetAnimationWindow();
            if (clip != null && animWindow != null)
            {
                float targetTime = goNext 
                    ? GetNextKeyframeTime(clip, transform, animWindow.time, checkPosition, checkRotation)
                    : GetPreviousKeyframeTime(clip, transform, animWindow.time, checkPosition, checkRotation);
                    
                if (targetTime >= 0)
                {
                    animWindow.time = targetTime;
                    animWindow.Repaint();
                }
            }
        }
        
        private void ToggleKeyframe(Transform transform, bool isPosition)
        {
            var clip = getAttachedClip();
            var animWindow = GetAnimationWindow();
            if (clip != null && animWindow != null)
            {
                float currentTime = animWindow.time;
                if (HasKeyframeAtTime(clip, transform, currentTime, isPosition, !isPosition))
                {
                    RemoveKeyframeForTransform(transform, currentTime, isPosition, !isPosition);
                }
                else
                {
                    if (transform == camLookAtLoc)
                        KeyframeCamLookAt();
                    else
                        SetKeyframeForTransform(transform, isPosition, !isPosition);
                }
            }
        }
        
        private bool HasKeyframeInCurve(AnimationCurve curve, float time, float tolerance)
        {
            if (curve == null) return false;
            
            for (int i = 0; i < curve.length; i++)
            {
                if (Mathf.Abs(curve[i].time - time) < tolerance)
                {
                    return true;
                }
            }
            return false;
        }
        
        private void RemoveKeyframeFromCurve(AnimationClip clip, string path, string propertyName, float time, float tolerance)
        {
            var curve = AnimationUtility.GetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), propertyName));
            if (curve != null)
            {
                for (int i = curve.length - 1; i >= 0; i--)
                {
                    if (Mathf.Abs(curve[i].time - time) < tolerance)
                    {
                        curve.RemoveKey(i);
                        break;
                    }
                }
                AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), propertyName), curve);
            }
        }
        
        private void SetBorderRadius(IStyle style, float radius)
        {
            style.borderTopLeftRadius = radius;
            style.borderTopRightRadius = radius;
            style.borderBottomLeftRadius = radius;
            style.borderBottomRightRadius = radius;
        }
    }
    
    public class CopyCameraKeyframesWindow : EditorWindow
    {
        public CameraAnimationWindow targetWindow;
        private AnimationClip sourceClip;
        
        void OnGUI()
        {
            titleContent = new GUIContent("Copy Camera Keyframes");
            minSize = new Vector2(400, 150);
            
            EditorGUILayout.Space(10);
            
            EditorGUILayout.LabelField("Copy Camera Animation Keyframes", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            
            EditorGUILayout.HelpBox("Select an animation clip to copy all camera keyframes (camLookAt and camFollow) to the current animation.", MessageType.Info);
            
            EditorGUILayout.Space(10);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Source Clip:", GUILayout.Width(80));
            sourceClip = EditorGUILayout.ObjectField(sourceClip, typeof(AnimationClip), false) as AnimationClip;
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(10);
            
            EditorGUILayout.BeginHorizontal();
            
            GUI.enabled = sourceClip != null;
            if (GUILayout.Button("Copy Keyframes", GUILayout.Height(30)))
            {
                if (targetWindow != null && sourceClip != null)
                {
                    targetWindow.CopyKeyframesFromClip(sourceClip);
                    Close();
                }
            }
            GUI.enabled = true;
            
            if (GUILayout.Button("Cancel", GUILayout.Height(30)))
            {
                Close();
            }
            
            EditorGUILayout.EndHorizontal();
        }
    }
}
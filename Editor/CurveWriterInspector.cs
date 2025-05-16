using UnityEditor;
using UnityEngine;

namespace Code.Combat.Editor
{
    [CustomEditor(typeof(CurveWriter))]

    public class CurveWriterInspector: UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Write Curve"))
            {
                WriteCurve();
            }
        }

private void WriteCurve()
{
    CurveWriter curveWriter = (CurveWriter)target;

    // Get the root and tip animators
    Animator rootAnimator = curveWriter.rootAnimator;
    Animator tipAnimator = curveWriter.tipAnimator;

    if (rootAnimator == null || tipAnimator == null)
    {
        Debug.LogError("Root or Tip animator is missing!");
        return;
    }

// Get the current playing clips
    AnimationClip rootClip = null;
    if (rootAnimator.runtimeAnimatorController != null && 
        rootAnimator.GetCurrentAnimatorClipInfo(0).Length > 0)
    {
        rootClip = rootAnimator.GetCurrentAnimatorClipInfo(0)[0].clip;
    }

    AnimationClip tipClip = null;
    if (tipAnimator.runtimeAnimatorController != null && 
        tipAnimator.GetCurrentAnimatorClipInfo(0).Length > 0)
    {
        tipClip = tipAnimator.GetCurrentAnimatorClipInfo(0)[0].clip;
    }

    if (rootClip == null || tipClip == null)
    {
        Debug.LogError("Could not find animation clips in the animators!");
        return;
    }
    
    if (rootClip == null || tipClip == null)
    {
        Debug.LogError("Root or Tip animation clip is missing!");
        return;
    }
    
    // Set up sampling parameters
    float startTime = 0f;
    float endTime = Mathf.Max(rootClip.length, tipClip.length);
    int sampleCount = (int)(endTime * 90f); // 90 samples per second
    float timeStep = endTime / (sampleCount - 1);
    
    // Get or add Animator component
    Animator animator = curveWriter.GetComponent<Animator>();
    if (animator == null)
        animator = curveWriter.gameObject.AddComponent<Animator>();
    
    animator.applyRootMotion = true;
    Vector3 initialPosition = curveWriter.transform.position;
    Quaternion initialRotation = curveWriter.transform.rotation;
    
    // Initialize animation curves
    curveWriter.curve.RightHand.RootX = new AnimationCurve();
    curveWriter.curve.RightHand.RootY = new AnimationCurve();
    curveWriter.curve.RightHand.RootZ = new AnimationCurve();
    curveWriter.curve.RightHand.TipX = new AnimationCurve();
    curveWriter.curve.RightHand.TipY = new AnimationCurve();
    curveWriter.curve.RightHand.TipZ = new AnimationCurve();
    
    // Start animation mode
    AnimationMode.StartAnimationMode();
    // Sample animations
    for (float time = startTime; time <= endTime; time += timeStep)
    {
        // Sample root animation
        AnimationMode.BeginSampling();
        AnimationMode.SampleAnimationClip(curveWriter.rootGameObject, rootClip, time);
        
        // Record root position
        Vector3 rootPos = curveWriter.rootTransform.position;
        curveWriter.curve.RightHand.RootX.AddKey(new Keyframe(time, rootPos.x));
        curveWriter.curve.RightHand.RootY.AddKey(new Keyframe(time, rootPos.y));
        curveWriter.curve.RightHand.RootZ.AddKey(new Keyframe(time, rootPos.z));
        
        AnimationMode.EndSampling();
        
        // Sample tip animation
        AnimationMode.BeginSampling();
        AnimationMode.SampleAnimationClip(curveWriter.tipGameObject, tipClip, time);
        
        // Record tip position
        Vector3 tipPos = curveWriter.tipTransform.position;
        curveWriter.curve.RightHand.TipX.AddKey(new Keyframe(time, tipPos.x));
        curveWriter.curve.RightHand.TipY.AddKey(new Keyframe(time, tipPos.y));
        curveWriter.curve.RightHand.TipZ.AddKey(new Keyframe(time, tipPos.z));
        
        AnimationMode.EndSampling();
    }
    
    // Smooth all tangents
    for (int i = 0; i < curveWriter.curve.RightHand.RootX.length; i++)
    {
        curveWriter.curve.RightHand.RootX.SmoothTangents(i, 0);
        curveWriter.curve.RightHand.RootY.SmoothTangents(i, 0);
        curveWriter.curve.RightHand.RootZ.SmoothTangents(i, 0);
        curveWriter.curve.RightHand.TipX.SmoothTangents(i, 0);
        curveWriter.curve.RightHand.TipY.SmoothTangents(i, 0);
        curveWriter.curve.RightHand.TipZ.SmoothTangents(i, 0);
    }
    
    // Restore original transform
    curveWriter.transform.position = initialPosition;
    curveWriter.transform.rotation = initialRotation;
    
    // Mark as dirty
    EditorUtility.SetDirty(curveWriter.curve);
    Debug.Log("Curves written successfully!");
    AnimationMode.StopAnimationMode();

}
    }
}
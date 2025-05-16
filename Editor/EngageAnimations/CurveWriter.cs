using UnityEngine;

namespace DivineDragon.EngageAnimations
{
    public class CurveWriter: MonoBehaviour
    {
        public Animator rootAnimator;
        public GameObject rootGameObject;
        public Transform rootTransform;
        public Animator tipAnimator;
        public GameObject tipGameObject;
        public Transform tipTransform;
        public PrefetchedCurve_Bridge curve;
    }
}
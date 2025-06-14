using System.Collections.Generic;
using UnityEngine;

namespace Combat
{
    public class AnimationEditor: MonoBehaviour
    {
        public float tolerance = 0.01f;
        public float currentTime = 0;
        public PrefetchedCurve_Bridge bridge;
        public Transform RightRoot;
        public Transform RightTip;
        public Transform LeftRoot;
        public Transform LeftTip;
        
        public bool AlwaysRenderHitEvents = true;
        public bool AlwaysRenderVec3Events = true;
        public bool AlwaysRenderCameraPosition = true;
        public bool AlwaysRenderLabels = true;
        
        // private List<ParsedEngageAnimationEvent> parsedEventsCache = new List<ParsedEngageAnimationEvent>();

        public void Vec3(AnimationEvent animEvent)
        {
            
        }
        
        public void ワールド時間(AnimationEvent animEvent)
        {
            
        }
        
        public void 表情(AnimationEvent animEvent)
        {
            
        }
        
        public void 音汎用(AnimationEvent animEvent)
        {
            
        }
        
        public void 音必殺ボイス(AnimationEvent animEvent)
        {
            
        }
        
        public void 左足上昇(AnimationEvent animEvent)
        {
            
        }
        
        public void 右足上昇(AnimationEvent animEvent)
        {
            
        }
        
        public void 地面パーティクル(AnimationEvent animEvent)
        {
            
        }
        
        public void 左足接地(AnimationEvent animEvent)
        {
            
        }
        
        public void 右足接地(AnimationEvent animEvent)
        {
            
        }

        public void カメラ(AnimationEvent animEvent)
        {
            
        }
        
        public void パーティクル(AnimationEvent animEvent)
        {
            
        }
        
        public void パーティクル削除(AnimationEvent animEvent)
        {
            
        }
        
        public void ジャンプ(AnimationEvent animEvent)
        {
            
        }
        
        
    }
    
    
}
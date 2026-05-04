using MxM;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Animations;

namespace Threepeat
{
    [System.Serializable]
    public class NGCharacter_AnimationHelper : NGCharacterHelper
    {

        public NGCharacter_AnimationHelper(NGCharacter ch) : base(ch) { }

        public override void Initialize()
        {
            
        }

        public class AnimationEventLayerInfo
        {
            public MxMBlendSpace blendSpace = null;
            public float animDuration = 0f;
            public string animName = null;
        }


        // This class holds all cached info about the layer
        public class BlendSpaceLayerInfo
        {
            public Dictionary<string, int> animationHashMap = null;
            public AnimationEventLayerInfo[] animationInfos = null;
            public MxMBlendSpaceLayers bslayer = null;
            public bool blendSpaceActive = false;

            public void Initialize(MxMBlendSpaceLayers blendSpaceLayer, MxMBlendSpace space, int numAnimations)
            {
                if (numAnimations <= 0)
                {
                    Debug.LogWarningFormat("MMLC: BlendSpace( {0} ) contains no animations", space.BlendSpaceName);
                    return;
                }
                animationHashMap = new Dictionary<string, int>();
                animationInfos = new AnimationEventLayerInfo[numAnimations];
                int animHashIndex = 0;

                bslayer = blendSpaceLayer;
                blendSpaceActive = false;

                bool altNamesAvailable = (space.GetType() == typeof(MMCAnimationEventLayer));

                MMCAnimationEventLayer aelayer = altNamesAvailable ? (MMCAnimationEventLayer)space : null;
                Debug.LogFormat("BlendSpace type = {0}", space.GetType().Name);

                foreach (AnimationClip clip in space.Clips)
                {
                    animationHashMap[clip.name] = animHashIndex;
                    AnimationEventLayerInfo aeli = new AnimationEventLayerInfo();
                    aeli.blendSpace = space;
                    aeli.animDuration = clip.length;
                    if (altNamesAvailable)
                    {
                        aeli.animName = aelayer.GetAnimNameFromClipName(clip.name);
                        animationHashMap[aeli.animName] = animHashIndex;
                    }
                    else
                    {
                        aeli.animName = clip.name;
                    }
                    animationInfos[animHashIndex] = aeli;
                    animHashIndex++;
                }
            }
        }


        // lookup dictionary layerName -> layerHash
        private Dictionary<string, int> layerHashMap = null;

        // lookup dictionary layerHash -> all cached info about the layer
        private Dictionary<int, BlendSpaceLayerInfo> eventLayerHashMap = null;

        public void SetControllerMask(AvatarMask amask, uint controllerLayer = 1)
        {
            if (amask == null)
            {
                amask = new AvatarMask();
            }

            character.mxmAnimator.AnimatorControllerMask = amask;
            AnimationLayerMixerPlayable mixerPlayable = character.mxmAnimator.LayerMixerPlayable;
            mixerPlayable.SetLayerMaskFromAvatarMask(controllerLayer, amask);
        }

        public void BlendToAnimator(float timeToBlend = 0.25f, bool pauseMxM = true)
        {
            if (pauseMxM)
            {
                character.mxmAnimator.Pause();
            }

            character.mxmAnimator.BlendInControllerWithTime(timeToBlend);
        }

        public void BlendFromAnimator(float timeToBlend = 0.25f)
        {
            if (character.mxmAnimator.IsPaused)
            {
                character.mxmAnimator.UnPause();
            }

            character.mxmAnimator.BlendOutControllerWithTime(timeToBlend);
        }



        public void InitializeBlendspaceLayers()
        {
            MxMBlendSpaceLayers[] allLayers = character.GetComponentsInChildren<MxMBlendSpaceLayers>();

            layerHashMap = new Dictionary<string, int>();
            eventLayerHashMap = new Dictionary<int, BlendSpaceLayerInfo>();

            int currLayerHash = 0;

            //PERFORMANCE-NOTE for users: do this info collection in the editor and save it off in the object to
            // avoid using reflection on character initialization.

            FieldInfo fibs = typeof(MxMBlendSpaceLayers).GetField("m_blendSpaces", BindingFlags.NonPublic | BindingFlags.Instance);

            // populate layerHashMap and eventLayerHashMap
            foreach (MxMBlendSpaceLayers lyr in allLayers)
            {
                MxMBlendSpace[] spaces = (MxMBlendSpace[])fibs.GetValue(lyr);

                foreach (MxMBlendSpace spc in spaces)
                {
                    layerHashMap[spc.BlendSpaceName] = currLayerHash;
                    BlendSpaceLayerInfo info = new BlendSpaceLayerInfo();
                    info.Initialize(lyr, spc, spc.Clips.Count);
                    eventLayerHashMap[currLayerHash] = info;
                    currLayerHash++;
                }
            }
        }
        public int AnimationLayerNameToHash(string layerName)
        {
            if (layerHashMap.ContainsKey(layerName))
            {
                return layerHashMap[layerName];
            }
            return -1;
        }

        public int AnimationNameToHash(string layerName, string animName)
        {
            int layerHash = AnimationLayerNameToHash(layerName);
            return AnimationNameToHash(layerHash, animName);
        }

        public int AnimationNameToHash(int layerHash, string animName)
        {
            BlendSpaceLayerInfo info = null;
            if (!eventLayerHashMap.TryGetValue(layerHash, out info))
            {
                return -1;
            }
            int val;

            if (!info.animationHashMap.TryGetValue(animName, out val))
            {
                return -1;
            }

            return val;
        }

        public delegate void CompletionCallback(string layerName, string animName);

        public void PlayOneshot(string layerName, string animName, CompletionCallback onComplete = null, float transitionInTime = 0.25f, float transitionOutTime = 0.25f, bool endOfChain = true)
        {
            PlayOneshot(AnimationLayerNameToHash(layerName), AnimationNameToHash(layerName, animName), onComplete, transitionInTime, transitionOutTime, endOfChain);
        }

        public void PlayOneshotSameLayer(string layerName, string animName, CompletionCallback onComplete = null, float transitionInTime = 0.25f, float transitionOutTime = 0.25f, bool endOfChain = true)
        {
            PlayOneshotSameLayer(AnimationLayerNameToHash(layerName), AnimationNameToHash(layerName, animName), onComplete, transitionInTime, transitionOutTime, endOfChain);
        }


        public void PlayLoop(string layerName, string animName, float transitionInTime = 0.25f)
        {
            PlayLoop(AnimationLayerNameToHash(layerName), AnimationNameToHash(layerName, animName), transitionInTime);
        }

        protected BlendSpaceLayerInfo GetAnimationEventLayerFromHash(int layerHash)
        {
            if ((eventLayerHashMap == null) || !eventLayerHashMap.ContainsKey(layerHash))
            {
                return null;
            }
            return eventLayerHashMap[layerHash];
        }

        public bool PlayLoop(int layerHash, int animHash, float transitionInTime = 0.25f)
        {
            BlendSpaceLayerInfo evtInfo = GetAnimationEventLayerFromHash(layerHash);

            if (evtInfo == null)
            {
                return false;
            }

            evtInfo.bslayer.SetBlendSpacePosition(MMCAnimationEventLayer.GetPositionByIndex(animHash));
            evtInfo.blendSpaceActive = true;

            MxMBlendSpace blendSpace = evtInfo.animationInfos[animHash].blendSpace;

            //bslayer.SetBlendSpacePositionX(1f);
            evtInfo.bslayer.SetBlendSpace(blendSpace, 0f);
            character.mxmAnimator.SetLayerWeight(evtInfo.bslayer.LayerId, 0f);
            evtInfo.bslayer.TransitionToBlendSpace(blendSpace, 1f / transitionInTime, 0f);
            character.mxmAnimator.BlendInLayerWithTime(evtInfo.bslayer.LayerId, transitionInTime);
            evtInfo.blendSpaceActive = true;
            //StartCoroutine(WaitSecondsAndBlendBack(evtInfo.animationInfos[animHash].animDuration, evtInfo.bslayer, blendSpace.BlendSpaceName, evtInfo.animationInfos[animHash].animName, transitionOutTime, onComplete));
            return true;
        }

        private Dictionary<int, Coroutine> sameLayerTransitionCoroutine = new Dictionary<int, Coroutine>();

        // <summary>This method assumes the blend space is already playing/blended/active</summary>
        public bool PlayLoopSameLayer(string layerName, string animName, float transitionInTime = 0.25f)
        {
            return PlayLoopSameLayer(AnimationLayerNameToHash(layerName), AnimationNameToHash(layerName, animName));
        }

        // <summary>This method assumes the blend space is already playing/blended/active</summary>
        public bool PlayLoopSameLayer(int layerHash, int animHash, float transitionInTime = 0.25f)
        {
            BlendSpaceLayerInfo evtInfo = GetAnimationEventLayerFromHash(layerHash);

            if (evtInfo == null)
            {
                return false;
            }
            Coroutine tempco = oneshotCoroutine[layerHash];
            if (tempco != null)
            {
                character.StopCoroutine(tempco);
            }

            Vector2 currPos = evtInfo.bslayer.GetBlendSpaceState(evtInfo.bslayer.CurrentBlendSpace).Position;
            Vector2 wantedPos = MMCAnimationEventLayer.GetPositionByIndex(animHash);

            evtInfo.blendSpaceActive = true;

            MxMBlendSpace blendSpace = evtInfo.animationInfos[animHash].blendSpace;
            Coroutine temp = null;
            if (sameLayerTransitionCoroutine.ContainsKey(layerHash))
            {
                temp = sameLayerTransitionCoroutine[layerHash];
            }

            if (temp != null)
            {
                character.StopCoroutine(temp);
            }
            sameLayerTransitionCoroutine[layerHash] = character.StartCoroutine(TraverseBlendSpace(currPos, wantedPos, transitionInTime, evtInfo, layerHash));

            //bslayer.SetBlendSpacePositionX(1f);
            //evtInfo.bslayer.SetBlendSpace(blendSpace, 0f);
            //mxmAnimator.SetLayerWeight(evtInfo.bslayer.LayerId, 0f);
            //evtInfo.bslayer.TransitionToBlendSpace(blendSpace, 1f / transitionInTime, 0f);
            //mxmAnimator.BlendInLayerWithTime(evtInfo.bslayer.LayerId, transitionInTime);
            //evtInfo.blendSpaceActive = true;
            //StartCoroutine(WaitSecondsAndBlendBack(evtInfo.animationInfos[animHash].animDuration, evtInfo.bslayer, blendSpace.BlendSpaceName, evtInfo.animationInfos[animHash].animName, transitionOutTime, onComplete));
            return true;
        }

        private IEnumerator TraverseBlendSpace(Vector2 currPos, Vector2 wantedPos, float transitionInTime, BlendSpaceLayerInfo evtInfo, int layerHash)
        {
            float startTime = Time.time;
            float endTime = startTime + transitionInTime;
            float timeFrac;

            while (Time.time < endTime)
            {
                timeFrac = (Time.time - startTime) / transitionInTime;
                evtInfo.bslayer.SetBlendSpacePosition(Vector2.Lerp(currPos, wantedPos, timeFrac));
                yield return null;
            }
            evtInfo.bslayer.SetBlendSpacePosition(wantedPos);
            sameLayerTransitionCoroutine.Remove(layerHash);
        }

        protected Dictionary<int, Coroutine> oneshotCoroutine = new Dictionary<int, Coroutine>();

        // <summary>endOfChain == false will stop MMLC from blending back off the layer when the animation is complete (for chains of events)</summary>
        public bool PlayOneshot(int layerHash, int animHash, CompletionCallback onComplete = null, float transitionInTime = 0.25f, float transitionOutTime = 0.2f, bool endOfChain = true)
        {
            BlendSpaceLayerInfo evtInfo = GetAnimationEventLayerFromHash(layerHash);

            if (evtInfo == null)
            {
                return false;
            }

            evtInfo.bslayer.SetBlendSpacePosition(MMCAnimationEventLayer.GetPositionByIndex(animHash));
            evtInfo.blendSpaceActive = true;

            MxMBlendSpace blendSpace = evtInfo.animationInfos[animHash].blendSpace;

            //bslayer.SetBlendSpacePositionX(1f);
            evtInfo.bslayer.SetBlendSpace(blendSpace, 0f);
            character.mxmAnimator.SetLayerWeight(evtInfo.bslayer.LayerId, 0f);
            evtInfo.bslayer.TransitionToBlendSpace(blendSpace, 1f / transitionInTime, 0f);
            character.mxmAnimator.BlendInLayerWithTime(evtInfo.bslayer.LayerId, transitionInTime);
            evtInfo.blendSpaceActive = true;

            oneshotCoroutine[layerHash] = character.StartCoroutine(WaitSecondsAndBlendBack(evtInfo, evtInfo.animationInfos[animHash].animDuration, evtInfo.bslayer, blendSpace.BlendSpaceName, evtInfo.animationInfos[animHash].animName, transitionOutTime, onComplete, endOfChain, layerHash));
            return true;
        }

        public bool PlayOneshotSameLayer(int layerHash, int animHash, CompletionCallback onComplete = null, float transitionInTime = 0.25f, float transitionOutTime = 0.2f, bool endOfChain = true)
        {
            BlendSpaceLayerInfo evtInfo = GetAnimationEventLayerFromHash(layerHash);

            if (evtInfo == null)
            {
                return false;
            }

            MxMBlendSpace blendSpace = evtInfo.animationInfos[animHash].blendSpace;
            evtInfo.bslayer.SetBlendSpace(blendSpace, 0f);
            evtInfo.bslayer.SetBlendSpacePosition(MMCAnimationEventLayer.GetPositionByIndex(animHash));
            evtInfo.blendSpaceActive = true;


            //bslayer.SetBlendSpacePositionX(1f);
            //mxmAnimator.SetLayerWeight(evtInfo.bslayer.LayerId, 0f);
            //evtInfo.bslayer.TransitionToBlendSpace(blendSpace, 1f / transitionInTime, 0f);
            //mxmAnimator.BlendInLayerWithTime(evtInfo.bslayer.LayerId, transitionInTime);
            evtInfo.blendSpaceActive = true;

            oneshotCoroutine[layerHash] = character.StartCoroutine(WaitSecondsAndBlendBack(evtInfo, evtInfo.animationInfos[animHash].animDuration, evtInfo.bslayer, blendSpace.BlendSpaceName, evtInfo.animationInfos[animHash].animName, transitionOutTime, onComplete, endOfChain, layerHash));
            return true;
        }

        public void StopEventLayer(string layerName, float transitionTime = 0.2f)
        {
            StopEventLayer(AnimationLayerNameToHash(layerName), transitionTime);
        }

        public void StopEventLayer(int layerHash, float transitionTime = 0.2f)
        {
            BlendSpaceLayerInfo evtInfo = GetAnimationEventLayerFromHash(layerHash);

            if (evtInfo == null)
            {
                return;
            }
            evtInfo.blendSpaceActive = false;
            character.mxmAnimator.BlendOutLayerWithTime(evtInfo.bslayer.LayerId, transitionTime);
        }

        private IEnumerator WaitSecondsAndBlendBack(BlendSpaceLayerInfo evtInfo, float timeToWaitBeforeStartingTransition, MxMBlendSpaceLayers bslayer, string bsName, string animName, float transitionOutTime, CompletionCallback onComplete = null, bool endOfChain = true, int layerHash = 0)
        {
            yield return new WaitForSeconds(timeToWaitBeforeStartingTransition);
            if (endOfChain)
            {
                evtInfo.blendSpaceActive = false;
                character.mxmAnimator.BlendOutLayerWithTime(bslayer.LayerId, transitionOutTime);
            }
            if (onComplete != null)
            {
                onComplete(bsName, animName);
            }
            oneshotCoroutine.Remove(layerHash);
            yield return null;
        }

    }
}
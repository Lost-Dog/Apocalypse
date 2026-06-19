using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MxM;
using System;
using System.Reflection;
using System.Linq;

namespace Threepeat
{
    [CreateAssetMenu(fileName = "MMCAnimationEventLayer", menuName = "Threepeat/MMC Animation Event Layer", order = 1)]
    public class MMCAnimationEventLayer : MxMBlendSpace
    {
        public const int numRows = 10;
        public const int numCols = 10;
        public const float rowStep = 1f / (float)numRows;
        public const float colStep = 1f / (float)numCols;

        [SerializeField] private AvatarMask avatarMask = null;
        [SerializeField] private List<string> animEventInfos = null;
        [SerializeField] private bool m_additive = false;
        [SerializeField] private bool m_applyFootIk = false;

        public int AnimationNameToHash(string animName)
        {
            return animEventInfos.IndexOf(animName);
        }

        public string GetAnimNameFromClipName(string clipName)
        {
            int idx = 0;
            foreach (AnimationClip clip in Clips)
            {
                if (clip.name.Equals(clipName))
                {
                    return animEventInfos[idx];
                }
                idx++;
            }
            return null;
        }

        public static Vector2 GetPositionByIndex(int ii)
        {
            int rowIndex = ii / numCols;  // integer division
            int colIndex = ii % numCols;

            return new Vector2(Mathf.Clamp01(colIndex * colStep), Mathf.Clamp01(rowIndex * rowStep));
        }

        public bool AddToCharacter(NGCharacter character)
        {
            bool doCombineSameSettingsBlendSpacesIntoSingleLayer = false;

            MxMBlendSpaceLayers[] bslayers = character.GetComponentsInChildren<MxMBlendSpaceLayers>();
            string bsname = BlendSpaceName;

            int avatarMatchIndex = -1;
            
            FieldInfo fi = typeof(MxMBlendSpaceLayers).GetField("m_mask", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo fibs = typeof(MxMBlendSpaceLayers).GetField("m_blendSpaces", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo fimtb = typeof(MxMBlendSpaceLayers).GetField("m_maxTransitionBlends", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo fiadd = typeof(MxMBlendSpaceLayers).GetField("m_additive", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo fifik = typeof(MxMBlendSpaceLayers).GetField("m_applyFootIk", BindingFlags.NonPublic | BindingFlags.Instance);

            for (int ii=0; ii < bslayers.Length; ii++)
            {
                MxMBlendSpace bs = bslayers[ii].GetBlendSpaceFromName(bsname);
                if (bs != null)
                {
                    Debug.Log("Can't add MMC Animation Event Layer, as a blend space layer already exists with same name");
                    return false;
                }

                if (doCombineSameSettingsBlendSpacesIntoSingleLayer)
                {
                    bool additive = (bool)fiadd.GetValue(bslayers[ii]);

                    if (additive != this.m_additive)
                    {
                        continue;
                    }

                    AvatarMask amask = (AvatarMask)fi.GetValue(bslayers[ii]);

                    if (avatarMatchIndex < 0)
                    {
                        if ((amask == null) && (avatarMask == null))
                        {
                            avatarMatchIndex = ii;
                        }
                        else if ((amask != null) && (avatarMask != null) && (amask.Equals(avatarMask)))
                        {
                            avatarMatchIndex = ii;
                        }
                    }
                }
                
            }

            if (avatarMatchIndex >= 0)
            {
                // add to existing
                Debug.Log("Found existing Blend Space Layers with matching mask, adding to that one.");
                //Debug.LogFormat("newlayer( {0} ), this( {1} )", bslayers[avatarMatchIndex], this);
                MxMBlendSpace[] spaces = (MxMBlendSpace[])fibs.GetValue(bslayers[avatarMatchIndex]);
                spaces = spaces.Append(this).ToArray();
                fibs.SetValue(bslayers[avatarMatchIndex], spaces);
                fimtb.SetValue(bslayers[avatarMatchIndex], 8);
            }
            else
            {
                Debug.Log("Adding new Blend Space Layers");
                MxMBlendSpaceLayers newlayer = character.mxmAnimator.gameObject.AddComponent<MxMBlendSpaceLayers>();
                //Debug.LogFormat("newlayer( {0} ), this( {1} )", newlayer, this);
                MxMBlendSpace[] spaces = (MxMBlendSpace[])fibs.GetValue(newlayer);
                if (spaces != null)
                {
                    spaces = spaces.Append(this).ToArray();
                }
                else
                {
                    spaces = new MxMBlendSpace[1] { this };
                }
                fibs.SetValue(newlayer, spaces);
                fimtb.SetValue(newlayer, 8);
                fifik.SetValue(newlayer, m_applyFootIk);
                fiadd.SetValue(newlayer, m_additive);

                fi.SetValue(newlayer, avatarMask);
            }

            

            return true;
        }
    }
}
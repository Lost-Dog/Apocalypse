// Copyright © 2017-2024 Vault Break Studios Pty Ltd

using UnityEngine.Playables;

namespace MxM
{
    //============================================================================================
    /**
    *  @brief A simple playable behaviour that allows MxM access to the animation update to 
    *  separate the scheduling and collection of jobs as much as possible
    *         
    *********************************************************************************************/
    public class MotionMatchingPlayable : PlayableBehaviour
    {
        private MxMAnimator m_mxmAnimator;
        //============================================================================================
        /**
        *  @brief Sets the reference to the MxMAnimator. This is done when MxM is initialized
        *  
        *  @param [MxMAnimator] a_mxmAnimator - a reference to the MxMAnimator that this playable
        *  belongs to.
        *         
        *********************************************************************************************/
        public void SetMxMAnimator(MxMAnimator a_mxmAnimator)
        {
            m_mxmAnimator = a_mxmAnimator;
        }

        //============================================================================================
        /**
        *  @brief Triggers the second phase update of the MxMAniamtor
        *  
        *  @param [Playable] playable - the playable
        *  @param [FrameData] info - info about the current farame
        *         
        *********************************************************************************************/
        public override void PrepareFrame(Playable playable, FrameData info)
        {
            if (!m_mxmAnimator.IsPaused)
            {
                m_mxmAnimator.MxMUpdate_Phase2();
            }
            else
            {
                // If the animator is paused but a pose search was scheduled this frame, the
                // pose cost jobs are still holding write-safety handles on their NativeArrays.
                // Complete them now so LateUpdate can safely read the results (or so the
                // safety system doesn't carry the lock into subsequent frames).
                m_mxmAnimator.StopJobs();
            }
        }
    }//End of class: MotionMatchingPlayable
}//End of namespace: MxM
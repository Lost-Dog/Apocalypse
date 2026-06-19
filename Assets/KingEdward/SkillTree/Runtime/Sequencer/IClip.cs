using GameCreator.Runtime.Common;

namespace KingEdward.SkillTree
{
    /// <summary>
    /// Base interface for all skill sequencer clips
    /// </summary>
    public interface ISkillClip
    {
        float TimeStart { get; set; }
        float TimeEnd { get; set; }
        string ClipName { get; }
        
        bool IsInRange(float normalizedTime);
        void Execute(float normalizedTime, Args args);
        void OnEnter(Args args);
        void OnExit(Args args);
    }
}

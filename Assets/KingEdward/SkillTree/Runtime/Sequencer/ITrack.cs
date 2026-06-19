using GameCreator.Runtime.Common;

namespace KingEdward.SkillTree
{
    /// <summary>
    /// Base interface for all skill sequencer tracks
    /// </summary>
    public interface ISkillTrack
    {
        string TrackName { get; }
        float TrackHeight { get; }
        
        void Execute(float normalizedTime, Args args);
        void Reset();
    }
}

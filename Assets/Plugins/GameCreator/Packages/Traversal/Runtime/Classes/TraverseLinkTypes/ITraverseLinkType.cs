using GameCreator.Runtime.Characters;
using UnityEngine;

namespace GameCreator.Runtime.Traversal
{
    public interface ITraverseLinkType
    {
        // PROPERTIES: ----------------------------------------------------------------------------
        
        Vector3 LocalPointA { get; }
        Vector3 LocalPointB { get; }
        
        // METHODS: -------------------------------------------------------------------------------
        
        Vector3 GetLocalPositionA(Character character, TraverseLink traverseLink);
        Vector3 GetLocalPositionB(Character character, TraverseLink traverseLink);
        
        Quaternion GetLocalRotationA(Character character, TraverseLink traverseLink);
        Quaternion GetLocalRotationB(Character character, TraverseLink traverseLink);

        // METHODS: -------------------------------------------------------------------------------

        TraverseLinkData ToTraverseLinkData(Character character, TraverseLink traverseLink);
        
        void OnDrawGizmos(Transform transform);
    }
}
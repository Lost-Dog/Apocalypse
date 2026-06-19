using System.Collections.Generic;
using UnityEngine;

namespace Threepeat
{
    public class MMCEventCheckCache
    {
        // keyed by CollisionCastEnum, extend by using any integer > CollisionCastMax
        public Dictionary<int, CollisionCastInfo> cachedCasts = new Dictionary<int, CollisionCastInfo>();

        public enum CollisionCastEnum
        {
            KneeLevelFwd,
            ChestLevelFwd,
            HeadLevelFwd,
            HighMantleFwd,
            ReallyHighMantleFwd,
            ObstacleRampCheck,
            OneFootOverFwd,
            OneFootOverFwdHigh,
            LowVaultMantleTopOriginDown,
            HighVaultMantleTopOriginDown,
            HighVaultMantleTopDownDeepCheck,
            CeilingCollisionCheck,
            CollisionCastMax
        }

        public class CollisionCastInfo
        {
            public bool collision = false;
            public RaycastHit hit;
        }

        public void Clear()
        {
            cachedCasts.Clear();
        }
        public bool IsCastInCache(int castKey)
        {
            return cachedCasts.ContainsKey(castKey);
        }

        public bool CachedCast(CollisionCastEnum castKey, out CollisionCastInfo result)
        {
            return CachedCast((int)castKey, out result);
        }

        public bool CachedCast(int castKey, out CollisionCastInfo result)
        {
            if (IsCastInCache(castKey))
            {
                result = cachedCasts[castKey];
                return result.collision;
            }

            result = null;
            return false;
        }

        public bool SphereCast(int castKey, Vector3 origin, float radius, Vector3 direction, out RaycastHit hitInfo, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore)
        {
            CollisionCastInfo info;
            if (IsCastInCache(castKey))
            {
                info = cachedCasts[castKey];
                hitInfo = info.hit;
                return info.collision;
            }
            else
            {
                info = new CollisionCastInfo();
                info.collision = Physics.SphereCast(origin, radius, direction, out info.hit, maxDistance, layerMask, queryTriggerInteraction);
                cachedCasts.Add(castKey, info);
                hitInfo = info.hit;
                return info.collision;
            }
        }

        public bool Raycast(int castKey, Vector3 origin, Vector3 direction, out RaycastHit hitInfo, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
        {
            CollisionCastInfo info;
            if (IsCastInCache(castKey))
            {
                info = cachedCasts[castKey];
                hitInfo = info.hit;
                return info.collision;
            }
            else
            {
                info = new CollisionCastInfo();
                info.collision = Physics.Raycast(origin, direction, out info.hit, maxDistance, layerMask, queryTriggerInteraction);
                cachedCasts.Add(castKey, info);
                hitInfo = info.hit;
                return info.collision;
            }
        }

    }
}
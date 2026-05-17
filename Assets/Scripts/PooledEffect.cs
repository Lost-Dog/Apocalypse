using UnityEngine;

/// <summary>
/// Specialized pooled object for particle effects and VFX.
/// Automatically returns to pool when particle system finishes.
/// </summary>
[RequireComponent(typeof(PooledObject))]
public class PooledEffect : MonoBehaviour
{
    [Header("Effect Settings")]
    [Tooltip("Automatically detect particle systems and return when finished")]
    public bool autoReturnWhenFinished = true;

    [Tooltip("Return to pool this many seconds after all particles finish")]
    public float returnDelay = 0.5f;

    private ParticleSystem[] particleSystems;
    private PooledObject pooledObject;
    private float finishTime;
    private bool isWaitingToReturn;

    private void Awake()
    {
        pooledObject = GetComponent<PooledObject>();
        particleSystems = GetComponentsInChildren<ParticleSystem>();
    }

    private void OnEnable()
    {
        isWaitingToReturn = false;
        finishTime = 0f;

        // Play all particle systems
        for (int i = 0; i < particleSystems.Length; i++)
        {
            particleSystems[i].Play();
        }
    }

    private void Update()
    {
        if (!autoReturnWhenFinished) return;

        // Check if all particle systems are done
        bool allFinished = true;
        for (int i = 0; i < particleSystems.Length; i++)
        {
            if (particleSystems[i].IsAlive())
            {
                allFinished = false;
                break;
            }
        }

        if (allFinished && !isWaitingToReturn)
        {
            isWaitingToReturn = true;
            finishTime = Time.time;
        }

        if (isWaitingToReturn && Time.time - finishTime >= returnDelay)
        {
            pooledObject.ReturnToPool();
        }
    }

    /// <summary>
    /// Stop all particle systems and return to pool immediately
    /// </summary>
    public void StopAndReturn()
    {
        for (int i = 0; i < particleSystems.Length; i++)
        {
            particleSystems[i].Stop();
        }

        pooledObject.ReturnToPool();
    }
}

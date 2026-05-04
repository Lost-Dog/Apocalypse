using UnityEngine;
using GameCreator.Runtime.Perception;

[DisallowMultipleComponent]
public class Awareness : MonoBehaviour
{
    [Header("Alert Settings")]
    [Tooltip("How far this character can alert other enemies (in world units)")]
    [SerializeField] public float alertRange = 10f;

    private Perception m_Perception;

    private bool m_HasAlerted = false;

    private void Awake()
    {
        m_Perception = GetComponent<Perception>();
        if (m_Perception == null)
        {
            Debug.LogError("Awareness script requires a Perception component on the same GameObject.");
        }
    }

    private void OnEnable()
    {
        if (m_Perception != null)
        {
            m_Perception.EventChangeAwarenessLevel += OnAwarenessChanged;
        }
    }

    private void OnDisable()
    {
        if (m_Perception != null)
        {
            m_Perception.EventChangeAwarenessLevel -= OnAwarenessChanged;
        }
    }

    /// <param name="target">The GameObject that this awareness update is for.</param>
    /// <param name="level">The new awareness level.</param>
    private void OnAwarenessChanged(GameObject target, float level)
    {
        if (level >= 1f && !m_HasAlerted)
        {
            AlertNearbyEnemies();
            m_HasAlerted = true;
        }
        else if (level < 1f)
        {
            m_HasAlerted = false;
        }
    }

    private void AlertNearbyEnemies()
    {
        if (m_Perception == null) return;

        GameObject trackedTarget = null;

        foreach (var tracker in m_Perception.TrackerList)
        {
            if (tracker.Value.Awareness >= 1f)
            {
                trackedTarget = tracker.Value.Target;
                break;
            }
        }

        if (trackedTarget == null) return;

        Collider[] colliders = Physics.OverlapSphere(transform.position, alertRange);
        foreach (Collider col in colliders)
        {
            if (col.gameObject.CompareTag("Enemy"))
            {
                Perception enemyPerception = col.gameObject.GetComponent<Perception>();
                if (enemyPerception != null)
                {
                    enemyPerception.Track(trackedTarget);

                    enemyPerception.SetAwareness(trackedTarget, 1f);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, alertRange);
    }
}

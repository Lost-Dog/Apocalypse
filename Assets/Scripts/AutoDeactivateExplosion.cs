using UnityEngine;
using System.Collections;

public class AutoDeactivateExplosion : MonoBehaviour
{
    [Header("Auto-Deactivate Settings")]
    public float deactivateDelay = 3f;
    public bool deactivateOnEnable = true;
    
    [Header("Particle Control")]
    public bool stopParticlesBeforeDeactivate = true;
    public float particleStopDelay = 1f;
    
    private void OnEnable()
    {
        if (deactivateOnEnable)
        {
            StartCoroutine(DeactivateAfterDelay());
        }
    }
    
    private IEnumerator DeactivateAfterDelay()
    {
        yield return new WaitForSeconds(deactivateDelay);
        
        if (stopParticlesBeforeDeactivate)
        {
            ParticleSystem[] particles = GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem ps in particles)
            {
                ps.Stop();
            }
            
            yield return new WaitForSeconds(particleStopDelay);
        }
        
        gameObject.SetActive(false);
    }
    
    public void DeactivateNow()
    {
        StopAllCoroutines();
        gameObject.SetActive(false);
    }
}

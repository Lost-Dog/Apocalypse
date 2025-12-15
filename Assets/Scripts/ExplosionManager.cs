using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ExplosionManager : MonoBehaviour
{
    [Header("Explosion Targets")]
    [Tooltip("List of GameObjects that can have random explosions")]
    public List<GameObject> explosionTargets = new List<GameObject>();
    
    [Header("Timing Settings")]
    public float minTimeBetweenExplosions = 5f;
    public float maxTimeBetweenExplosions = 15f;
    public float explosionDuration = 3f;
    
    [Header("Random Selection")]
    [Range(1, 10)]
    public int minExplosionsPerCycle = 1;
    [Range(1, 10)]
    public int maxExplosionsPerCycle = 3;
    
    [Header("System Control")]
    public bool startOnAwake = true;
    public bool continuousExplosions = true;
    
    [Header("Component Names")]
    public string explosionComponentTag = "Explosion";
    
    [Header("Audio (Optional)")]
    public AudioClip explosionSound;
    [Range(0f, 1f)] public float soundVolume = 0.7f;
    public float soundMaxDistance = 50f;
    
    [Header("Debug")]
    public bool showDebugInfo = false;
    
    private bool isRunning = false;
    private List<GameObject> availableTargets = new List<GameObject>();
    
    private void Start()
    {
        InitializeTargets();
        
        if (startOnAwake)
        {
            StartExplosions();
        }
    }
    
    private void InitializeTargets()
    {
        availableTargets.Clear();
        
        foreach (GameObject target in explosionTargets)
        {
            if (target != null)
            {
                availableTargets.Add(target);
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"ExplosionManager initialized with {availableTargets.Count} targets");
        }
    }
    
    public void StartExplosions()
    {
        if (isRunning) return;
        
        isRunning = true;
        StartCoroutine(ExplosionCycle());
        
        if (showDebugInfo)
        {
            Debug.Log("ExplosionManager started");
        }
    }
    
    public void StopExplosions()
    {
        isRunning = false;
        StopAllCoroutines();
        
        if (showDebugInfo)
        {
            Debug.Log("ExplosionManager stopped");
        }
    }
    
    private IEnumerator ExplosionCycle()
    {
        while (isRunning)
        {
            float waitTime = Random.Range(minTimeBetweenExplosions, maxTimeBetweenExplosions);
            yield return new WaitForSeconds(waitTime);
            
            if (!isRunning) break;
            
            TriggerRandomExplosions();
            
            if (!continuousExplosions)
            {
                isRunning = false;
                break;
            }
        }
    }
    
    private void TriggerRandomExplosions()
    {
        if (availableTargets.Count == 0)
        {
            Debug.LogWarning("ExplosionManager: No targets available!");
            return;
        }
        
        int explosionCount = Random.Range(minExplosionsPerCycle, maxExplosionsPerCycle + 1);
        explosionCount = Mathf.Min(explosionCount, availableTargets.Count);
        
        List<GameObject> selectedTargets = new List<GameObject>();
        List<GameObject> tempTargets = new List<GameObject>(availableTargets);
        
        for (int i = 0; i < explosionCount; i++)
        {
            if (tempTargets.Count == 0) break;
            
            int randomIndex = Random.Range(0, tempTargets.Count);
            GameObject selectedTarget = tempTargets[randomIndex];
            selectedTargets.Add(selectedTarget);
            tempTargets.RemoveAt(randomIndex);
        }
        
        foreach (GameObject target in selectedTargets)
        {
            ActivateExplosion(target);
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"Triggered {selectedTargets.Count} explosions");
        }
    }
    
    private void ActivateExplosion(GameObject target)
    {
        if (target == null) return;
        
        Transform explosionTransform = target.transform.Find(explosionComponentTag);
        
        if (explosionTransform == null)
        {
            if (showDebugInfo)
            {
                Debug.LogWarning($"No explosion component found on {target.name} with tag '{explosionComponentTag}'");
            }
            return;
        }
        
        GameObject explosionObj = explosionTransform.gameObject;
        explosionObj.SetActive(true);
        
        ParticleSystem[] particles = explosionObj.GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem ps in particles)
        {
            ps.Play();
        }
        
        if (explosionSound != null)
        {
            PlayExplosionSound(target.transform.position);
        }
        
        StartCoroutine(DeactivateAfterDuration(explosionObj, explosionDuration));
        
        if (showDebugInfo)
        {
            Debug.Log($"Activated explosion on {target.name}");
        }
    }
    
    private IEnumerator DeactivateAfterDuration(GameObject explosionObj, float duration)
    {
        yield return new WaitForSeconds(duration);
        
        if (explosionObj != null)
        {
            ParticleSystem[] particles = explosionObj.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem ps in particles)
            {
                ps.Stop();
            }
            
            yield return new WaitForSeconds(1f);
            
            explosionObj.SetActive(false);
        }
    }
    
    private void PlayExplosionSound(Vector3 position)
    {
        AudioSource.PlayClipAtPoint(explosionSound, position, soundVolume);
    }
    
    public void TriggerSingleExplosion()
    {
        if (availableTargets.Count == 0) return;
        
        int randomIndex = Random.Range(0, availableTargets.Count);
        ActivateExplosion(availableTargets[randomIndex]);
    }
    
    public void TriggerExplosionAt(int index)
    {
        if (index >= 0 && index < availableTargets.Count)
        {
            ActivateExplosion(availableTargets[index]);
        }
    }
    
    public void AddTarget(GameObject target)
    {
        if (target != null && !explosionTargets.Contains(target))
        {
            explosionTargets.Add(target);
            availableTargets.Add(target);
        }
    }
    
    public void RemoveTarget(GameObject target)
    {
        explosionTargets.Remove(target);
        availableTargets.Remove(target);
    }
    
    public void ClearAllTargets()
    {
        explosionTargets.Clear();
        availableTargets.Clear();
    }
}

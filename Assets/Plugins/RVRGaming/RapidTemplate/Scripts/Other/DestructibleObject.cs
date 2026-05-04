using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Stats;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DestructibleObject : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private PropertyGetGameObject targetObject = GetGameObjectPlayer.Create();
    public Attribute healthAttribute;
    private Traits characterTraits;

    [Header("Destruction Settings")]
    public GameObject originalObject;
    public GameObject destroyedObject;
    [Space]
    [SerializeField] public InstructionList onDestroy = new InstructionList();

    [Header("Optional Settings")]
    public float forceMagnitude;
    public bool applyForceOnDestruction;

    [Header("Cleanup Settings")]
    public bool enableCleanup;
    public float cleanupDelay;
    public float shrinkDuration;

    [Header("Audio and Visual Effects")]
    public AudioClip impactSound;
    public AudioClip destructionSound;
    public GameObject impactVFX;
    public GameObject destructionVFX;

    private void Start()
    {
        GameObject target = targetObject.Get(gameObject);
        if (target != null)
        {
            characterTraits = target.GetComponent<Traits>();
        }

        if (characterTraits == null)
        {
            Debug.LogError("Traits component not found on the target object!");
            return;
        }

        originalObject.SetActive(true);
        destroyedObject.SetActive(false);
    }

    private void Update()
    {
        CheckDestruction();
    }

    public void RunInstructions()
    {
        _ = onDestroy.Run(new Args(this.gameObject));
    }

    private void CheckDestruction()
    {
        double currentHealth = characterTraits.RuntimeAttributes.Get(healthAttribute.ID).Value;

        if (currentHealth <= 0)
        {
            TriggerDestruction();
        }
    }

    public void ApplyDamage(double damage)
    {
        if (impactSound != null) AudioSource.PlayClipAtPoint(impactSound, transform.position);
        if (impactVFX != null) Instantiate(impactVFX, transform.position, Quaternion.identity);
    }

    private void TriggerDestruction()
    {
        MeshCollider meshCollider = GetComponent<MeshCollider>();
        if (meshCollider != null)
        {
            meshCollider.enabled = false;
        }

        originalObject.SetActive(false);
        destroyedObject.SetActive(true);
        RunInstructions();

        if (destructionSound != null) AudioSource.PlayClipAtPoint(destructionSound, transform.position);
        if (destructionVFX != null) Instantiate(destructionVFX, transform.position, Quaternion.identity);

        if (applyForceOnDestruction)
        {
            Rigidbody[] rigidbodies = destroyedObject.GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody rb in rigidbodies)
            {
                rb.AddExplosionForce(forceMagnitude, transform.position, 5.0f);
            }
        }

        if (enableCleanup)
        {
            StartCoroutine(Cleanup());
        }
    }

    private IEnumerator Cleanup()
    {
        yield return new WaitForSeconds(cleanupDelay);

        float timer = 0;
        Vector3 originalScale = destroyedObject.transform.localScale;

        while (timer < shrinkDuration)
        {
            float shrinkFactor = timer / shrinkDuration;
            destroyedObject.transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, shrinkFactor);

            timer += Time.deltaTime;
            yield return null;
        }

        destroyedObject.SetActive(false); 
        Destroy(gameObject);
    }
}

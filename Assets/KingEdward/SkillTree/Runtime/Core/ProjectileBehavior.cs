using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.VisualScripting;
using KingEdward;

namespace KingEdward.SkillTree
{
[Icon(SkillTreePaths.PROJECTILE_BEHAVIOR)]
[AddComponentMenu("KingEdward/Projectiles/Projectile Behavior")]
public class ProjectileBehavior : MonoBehaviour
{
    [Header("Basic Settings")]
    [SerializeField] private PropertyGetGameObject m_Caster = GetGameObjectSelf.Create();
    [SerializeField] private PropertyGetGameObject m_Target = GetGameObjectPlayer.Create();
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private float startDelay = 0f;

    [SerializeField] private ProjectileType projectileType = ProjectileType.Straight;
    [SerializeField] private bool rotateToMovement = true;
    [SerializeField] private float rotationSpeed = 10f;

    [SerializeField] private bool destroyOnHit = true;
    [SerializeField] private float hitRadius = 0.5f;
    [SerializeField] private LayerMask hitLayers = -1;
    [TagArray] [SerializeField] private string[] hitTags = new string[0];

    [SerializeField] private InstructionList m_OnHitInstructions = new InstructionList();
    [SerializeField] private ConditionList m_CanHitConditions = new ConditionList();

    [Header("Curve Settings")]
    [SerializeField] private float curveHeight = 3f;
    [SerializeField] private AnimationCurve curveShape = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private CurveDirection curveDirection = CurveDirection.Auto;
    [SerializeField] private float customAngle = 45f; 
    
    [Header("Spiral Settings")]
    [SerializeField] private float spiralRadius = 2f;
    [SerializeField] private float spiralSpeed = 5f;
    [SerializeField] private float spiralTightness = 1f;
    
    [Header("Homing Settings")]
    [SerializeField] private float homingStrength = 3f;
    [SerializeField] private float homingDelay = 0.2f;
    [SerializeField] private float maxTurnRate = 180f;

    [Header("Wave Settings")]
    [SerializeField] private float waveAmplitude = 1.5f;
    [SerializeField] private float waveFrequency = 3f;
    
    [Header("Boomerang Settings")]
    [SerializeField] private float boomerangRange = 10f;
    [SerializeField] private float boomerangReturnSpeed = 15f;
    [SerializeField] private float boomerangCurvature = 3f;
    [SerializeField] private bool destroyOnComplete = false;
    [SerializeField] private bool followTargetOnReturn = false;
    [SerializeField] private PropertyGetGameObject m_ReturnTarget = GetGameObjectPlayer.Create();
    
    [Header("Orbit Settings")]
    [SerializeField] private float orbitRadius = 3f;
    [SerializeField] private float orbitSpeed = 5f;
    [SerializeField] private int orbitCount = 2;
    
    [Header("Zigzag Settings")]
    [SerializeField] private float zigzagAmplitude = 2f;
    [SerializeField] private float zigzagFrequency = 4f;
    
    [Header("Artillery Settings")]
    [SerializeField] private float artilleryHeight = 10f;
    [SerializeField] private float artilleryArcTime = 1.5f;
    [SerializeField] private AnimationCurve artilleryArcCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private bool artilleryUseGravity = true;
    [SerializeField] [Range(0f, 1f)] private float artilleryLockTargetAt = 0.3f;
    [SerializeField] [Range(0f, 1f)] private float artilleryStartCurveAt = 0.2f;
    [SerializeField] private float artilleryGroundOffset = -1.5f;
    
    public enum ProjectileType
    {
        Straight,        
        Curve,          
        Spiral,        
        Homing,        
        Wave,           
        Boomerang,     
        Orbit,          
        Zigzag,
        Artillery
    }
    
    public enum CurveDirection
    {
        Auto,           
        Horizontal,    
        Vertical,      
        Custom       
    }

    private Vector3 startPosition;
    private Vector3 currentDirection;
    private float currentTime;
    private bool hasHitTarget = false;
    private System.Collections.Generic.HashSet<GameObject> hitObjects = new System.Collections.Generic.HashSet<GameObject>();
    private Transform targetTransform;
    private GameObject casterObject;

    private float spiralAngle = 0f;
    private float wavePhase = 0f;
    private float zigzagPhase = 0f;
    private int currentOrbit = 0;
    private float orbitAngle = 0f;
    private Vector3 orbitCenter;

    private Vector3 artilleryLockedTarget;
    private bool artilleryTargetLocked = false;
    private Vector3 previousPosition;

    void OnEnable()
    {
        if (startDelay > 0f)
            StartCoroutine(DelayedStart());
        else
            InitializeProjectile();
    }

    System.Collections.IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(startDelay);
        InitializeProjectile();
    }

    void InitializeProjectile()
    {
        startPosition = transform.position;
        previousPosition = startPosition;
        currentTime = 0f;
        hasHitTarget = false;
        hitObjects.Clear();
        artilleryTargetLocked = false;

        if (casterObject == null)
        {
            casterObject = m_Caster.Get(gameObject);
            if (casterObject == null)
                casterObject = gameObject;
        }

        GameObject targetObject = m_Target.Get(gameObject);
        if (targetObject != null)
        {
            targetTransform = targetObject.transform;
            currentDirection = (targetTransform.position - startPosition).normalized;
        }
        else
        {
            currentDirection = transform.forward;
            targetTransform = null;
            CreateVirtualTarget();
        }
        InitializeTypeSpecific();
    }
    
    void CreateVirtualTarget()
    {
        if (targetTransform == null)
            currentDirection = transform.forward;

        switch (projectileType)
        {
            case ProjectileType.Straight:
            case ProjectileType.Curve:
            case ProjectileType.Homing:
            case ProjectileType.Wave:
            case ProjectileType.Zigzag:
                break;
            case ProjectileType.Spiral:
                break;
            case ProjectileType.Boomerang:
                boomerangRange = 15f;
                break;
            case ProjectileType.Orbit:
                orbitCenter = startPosition + currentDirection * 5f;
                break;
        }
    }
    
    void InitializeTypeSpecific()
    {
        switch (projectileType)
        {
            case ProjectileType.Spiral:
                spiralAngle = 0f;
                break;
            case ProjectileType.Wave:
                wavePhase = 0f;
                break;
            case ProjectileType.Zigzag:
                zigzagPhase = 0f;
                break;
            case ProjectileType.Orbit:
                if (targetTransform != null)
                {
                    orbitCenter = targetTransform.position;
                    orbitAngle = Vector3.SignedAngle(Vector3.forward, currentDirection, Vector3.up) * Mathf.Deg2Rad;
                }
                currentOrbit = 0;
                break;
            case ProjectileType.Boomerang:
                break;
            case ProjectileType.Artillery:
                artilleryTargetLocked = false;
                break;
        }
    }
    
    void Update()
    {
        if (hasHitTarget) return;

        currentTime += Time.deltaTime;

        if (currentTime >= lifetime)
        {
            DestroyProjectile();
            return;
        }

        Vector3 newPosition = CalculateNewPosition();
        if (rotateToMovement && projectileType != ProjectileType.Boomerang)
            UpdateRotation(newPosition);

        transform.position = newPosition;
        previousPosition = newPosition;
        CheckCollision();
    }
    
    Vector3 CalculateNewPosition()
    {
        Vector3 baseMovement = currentDirection * speed * Time.deltaTime;
        Vector3 offset = Vector3.zero;
        
        switch (projectileType)
        {
            case ProjectileType.Straight:
                return transform.position + baseMovement;
                
            case ProjectileType.Curve:
                return CalculateCurvePosition();
                
            case ProjectileType.Spiral:
                offset = CalculateSpiralOffset();
                break;
                
            case ProjectileType.Homing:
                UpdateHomingDirection();
                return transform.position + currentDirection * speed * Time.deltaTime;
                
            case ProjectileType.Wave:
                offset = CalculateWaveOffset();
                break;
                
            case ProjectileType.Boomerang:
                return CalculateBoomerangPosition();
                
            case ProjectileType.Orbit:
                return CalculateOrbitPosition();
                
            case ProjectileType.Zigzag:
                offset = CalculateZigzagOffset();
                break;
                
            case ProjectileType.Artillery:
                return CalculateArtilleryPosition();
        }
        
        return transform.position + baseMovement + offset;
    }
    
    Vector3 CalculateCurvePosition()
    {
        Vector3 targetPos = targetTransform != null ? targetTransform.position : startPosition + currentDirection * 20f;
        
        float totalDistance = Vector3.Distance(startPosition, targetPos);
        float progress = (speed * currentTime) / totalDistance;
        progress = Mathf.Clamp01(progress);
        
        Vector3 basePosition = Vector3.Lerp(startPosition, targetPos, progress);
        Vector3 curveDirectionVector = GetCurveDirection();
        float curveValue = curveShape.Evaluate(progress);
        Vector3 curveOffset = curveDirectionVector * curveValue * curveHeight;
        basePosition += curveOffset;
        
        return basePosition;
    }
    
    Vector3 GetCurveDirection()
    {
        switch (curveDirection)
        {
            case CurveDirection.Auto:
                if (targetTransform != null)
                    return Vector3.Cross(currentDirection, Vector3.up).normalized;
                return Vector3.up;
            case CurveDirection.Horizontal:
                return Vector3.Cross(currentDirection, Vector3.up).normalized;
            case CurveDirection.Vertical:
                return Vector3.up;
            case CurveDirection.Custom:
                Vector3 right = Vector3.Cross(currentDirection, Vector3.up).normalized;
                Vector3 up = Vector3.up;
                float angleRad = customAngle * Mathf.Deg2Rad;
                Vector3 customDirection = right * Mathf.Sin(angleRad) + up * Mathf.Cos(angleRad);
                return customDirection.normalized;
                
            default:
                return Vector3.up;
        }
    }
    
    Vector3 CalculateSpiralOffset()
    {
        spiralAngle += spiralSpeed * Time.deltaTime;
        
        Vector3 right = Vector3.Cross(currentDirection, Vector3.up).normalized;
        Vector3 up = Vector3.Cross(right, currentDirection).normalized;
        float timeFactor = 1f - (currentTime / lifetime);
        float currentRadius = spiralRadius * timeFactor * spiralTightness;
        
        Vector3 spiralOffset = right * Mathf.Cos(spiralAngle) * currentRadius +
                              up * Mathf.Sin(spiralAngle) * currentRadius;
        
        return spiralOffset;
    }
    
    void UpdateHomingDirection()
    {
        if (currentTime < homingDelay || targetTransform == null) return;
        
        Vector3 targetPos = targetTransform.position;
        Vector3 targetDirection = (targetPos - transform.position).normalized;
        float maxTurnThisFrame = maxTurnRate * Time.deltaTime;
        currentDirection = Vector3.RotateTowards(currentDirection, targetDirection,
            maxTurnThisFrame * Mathf.Deg2Rad, 0f);
        currentDirection = Vector3.Slerp(currentDirection, targetDirection, 
            homingStrength * Time.deltaTime);
    }
    
    Vector3 CalculateWaveOffset()
    {
        wavePhase += waveFrequency * Time.deltaTime;
        
        Vector3 right = Vector3.Cross(currentDirection, Vector3.up).normalized;
        Vector3 waveOffset = right * Mathf.Sin(wavePhase) * waveAmplitude;
        
        return waveOffset;
    }
    
    Vector3 CalculateBoomerangPosition()
    {
        Vector3 targetPos;
        
        if (targetTransform != null)
        {
            targetPos = targetTransform.position;
        }
        else
        {
            targetPos = startPosition + transform.forward * boomerangRange;
        }

        float distanceToTarget = Vector3.Distance(startPosition, targetPos);
        float timeToTarget = distanceToTarget / speed;
        float timeToReturn = distanceToTarget / boomerangReturnSpeed;
        float totalTime = timeToTarget + timeToReturn;
        float phase = currentTime / totalTime;

        if (phase >= 2.0f)
        {
            hasHitTarget = true;
            
            if (destroyOnComplete)
            {
                DestroyProjectile();
            }
            else
            {
                ReturnToPool();
            }
            return GetReturnPosition();
        }

        Vector3 currentPosition;
        if (phase <= 1.0f)
        {
            float progressToTarget = phase;
            currentPosition = Vector3.Lerp(startPosition, targetPos, progressToTarget);
        }
        else
        {
            float returnProgress = (phase - 1.0f);
            Vector3 returnPos = GetReturnPosition();
            currentPosition = Vector3.Lerp(targetPos, returnPos, returnProgress);
        }

        float curveValue = Mathf.Sin(phase * Mathf.PI);
        Vector3 curveDirection = targetTransform != null
            ? (targetPos - startPosition).normalized
            : transform.forward;
        
        Vector3 rightDirection = Vector3.Cross(curveDirection, Vector3.up).normalized;
        Vector3 curveOffset = rightDirection * curveValue * boomerangCurvature;
        
        return currentPosition + curveOffset;
    }
    
    Vector3 GetReturnPosition()
    {
        if (followTargetOnReturn)
        {
            GameObject returnTarget = m_ReturnTarget.Get(gameObject);
            if (returnTarget != null)
            {
                return returnTarget.transform.position;
            }
        }
        return startPosition;
    }
    
    Vector3 CalculateOrbitPosition()
    {
        Vector3 center = targetTransform != null ? targetTransform.position : orbitCenter;
        
        orbitAngle += orbitSpeed * Time.deltaTime;
        float orbitProgress = orbitAngle / (2f * Mathf.PI);
        float currentRadius = orbitRadius * Mathf.Pow(0.8f, currentOrbit);
        float nextRadius = orbitRadius * Mathf.Pow(0.8f, currentOrbit + 1);
        float smoothRadius = Mathf.Lerp(currentRadius, nextRadius, orbitProgress);
        Vector3 orbitPosition = center + new Vector3(
            Mathf.Cos(orbitAngle) * smoothRadius,
            0,
            Mathf.Sin(orbitAngle) * smoothRadius
        );
        if (orbitAngle >= 2f * Mathf.PI)
        {
            currentOrbit++;
            orbitAngle -= 2f * Mathf.PI;
            if (currentOrbit >= orbitCount)
            {
                return transform.position + currentDirection * speed * Time.deltaTime;
            }
        }
        
        return orbitPosition;
    }
    
    Vector3 CalculateZigzagOffset()
    {
        zigzagPhase += zigzagFrequency * Time.deltaTime;
        
        Vector3 right = Vector3.Cross(currentDirection, Vector3.up).normalized;
        Vector3 zigzagOffset = right * Mathf.Sin(zigzagPhase) * zigzagAmplitude;
        
        return zigzagOffset;
    }
    
    Vector3 CalculateArtilleryPosition()
    {
        float progress = Mathf.Clamp01(currentTime / artilleryArcTime);

        if (!artilleryTargetLocked && progress >= artilleryLockTargetAt)
        {
            Vector3 rawTarget = targetTransform != null ? targetTransform.position : startPosition + currentDirection * 20f;
            artilleryLockedTarget = rawTarget + Vector3.up * artilleryGroundOffset;
            artilleryTargetLocked = true;
        }

        Vector3 targetPos;
        if (artilleryTargetLocked)
        {
            targetPos = artilleryLockedTarget;
        }
        else
        {
            Vector3 rawTarget = targetTransform != null ? targetTransform.position : startPosition + currentDirection * 20f;
            targetPos = rawTarget + Vector3.up * artilleryGroundOffset;
        }

        Vector3 horizontalPosition;
        if (progress < artilleryStartCurveAt)
        {
            float straightProgress = progress / artilleryStartCurveAt;
            float straightDistance = Vector3.Distance(startPosition, targetPos) * artilleryStartCurveAt;
            horizontalPosition = startPosition + currentDirection * (straightDistance * straightProgress);
        }
        else
        {
            float curveProgress = (progress - artilleryStartCurveAt) / (1f - artilleryStartCurveAt);
            float straightDistance = Vector3.Distance(startPosition, targetPos) * artilleryStartCurveAt;
            Vector3 straightEndPos = startPosition + currentDirection * straightDistance;
            horizontalPosition = Vector3.Lerp(straightEndPos, targetPos, curveProgress);
        }

        float verticalOffset;
        if (artilleryUseGravity)
        {
            float peakTime = 0.5f;
            if (progress < peakTime)
            {
                float ascendProgress = progress / peakTime;
                verticalOffset = artilleryHeight * artilleryArcCurve.Evaluate(ascendProgress);
            }
            else
            {
                float descendProgress = (progress - peakTime) / (1f - peakTime);
                verticalOffset = artilleryHeight * (1f - Mathf.Pow(descendProgress, 1.5f));
            }
        }
        else
        {
            verticalOffset = artilleryHeight * artilleryArcCurve.Evaluate(progress) * Mathf.Sin(progress * Mathf.PI);
        }
        
        return horizontalPosition + Vector3.up * verticalOffset;
    }
    
    void UpdateRotation(Vector3 newPosition)
    {
        Vector3 moveDirection = (newPosition - previousPosition).normalized;
        
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            
            if (projectileType == ProjectileType.Orbit)
            {
                Vector3 toCenter = (orbitCenter - transform.position).normalized;
                targetRotation = Quaternion.LookRotation(toCenter);
            }
            
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 
                rotationSpeed * Time.deltaTime);
        }
    }
    
    void CheckCollision()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, hitRadius, hitLayers);
        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.gameObject == gameObject) continue;

            if (hitTags.Length > 0)
            {
                bool hasValidTag = false;
                foreach (string tag in hitTags)
                {
                    if (hitCollider.CompareTag(tag))
                    {
                        hasValidTag = true;
                        break;
                    }
                }
                if (!hasValidTag) continue;
            }

            if (projectileType == ProjectileType.Boomerang)
            {
                Vector3 targetPos = targetTransform != null ? targetTransform.position : startPosition + transform.forward * boomerangRange;
                float distanceToTarget = Vector3.Distance(startPosition, targetPos);
                float timeToTarget = distanceToTarget / speed;
                float totalTime = timeToTarget + (distanceToTarget / boomerangReturnSpeed);
                float phase = currentTime / totalTime;
                if (phase <= 1.0f) continue;
            }

            if (hitObjects.Contains(hitCollider.gameObject)) continue;
            if (CanHitTarget(hitCollider.gameObject))
            {
                OnHitTarget(hitCollider.gameObject);
                return;
            }
        }
    }
    
    bool CanHitTarget(GameObject hitObject)
    {
        if (m_CanHitConditions.Length == 0) return true;
        Args args = new Args(gameObject, hitObject);
        return m_CanHitConditions.Check(args, CheckMode.And);
    }
    
    async void OnHitTarget(GameObject hitObject)
    {
        hitObjects.Add(hitObject);

        if (m_OnHitInstructions.Length > 0)
        {
            Args args = new Args(gameObject, hitObject);
            await m_OnHitInstructions.Run(args);
        }
        if (destroyOnHit && this != null && gameObject != null)
        {
            hasHitTarget = true;
            DestroyProjectile();
        }
    }
    
    void DestroyProjectile()
    {
        if (this != null && gameObject != null)
        {
            Destroy(gameObject);
        }
    }

    public void SetTarget(GameObject newTarget)
    {
        m_Target = GetGameObjectInstance.Create(newTarget);
        InitializeProjectile();
    }
    
    public void SetProjectileType(ProjectileType newType)
    {
        projectileType = newType;
        InitializeProjectile();
    }
    
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }
    
    public void SetHitRadius(float newRadius)
    {
        hitRadius = newRadius;
    }
    
    public void SetDestroyOnHit(bool destroy)
    {
        destroyOnHit = destroy;
    }
    
    public void SetStartDelay(float delay)
    {
        startDelay = delay;
    }
    
    public void SetBoomerangCurvature(float newCurvature)
    {
        boomerangCurvature = newCurvature;
    }
    
    public void SetFollowTargetOnReturn(bool follow)
    {
        followTargetOnReturn = follow;
    }
    
    public void SetReturnTarget(GameObject newReturnTarget)
    {
        m_ReturnTarget = GetGameObjectInstance.Create(newReturnTarget);
    }

    public void ReturnToPool()
    {
        hasHitTarget = false;
        hitObjects.Clear();
        currentTime = 0f;
        casterObject = null;
        targetTransform = null;
        gameObject.SetActive(false);
    }

    public InstructionList GetHitInstructions()
    {
        return m_OnHitInstructions;
    }
    
    public ConditionList GetCanHitConditions()
    {
        return m_CanHitConditions;
    }
    
    public GameObject GetCaster()
    {
        return casterObject;
    }
    
    public void SetCaster(GameObject newCaster)
    {
        casterObject = newCaster;
    }


    public static void RegisterCasterFromArgs(GameObject instance, Args args)
    {
        if (instance == null || args?.Self == null) return;
        var pb = instance.GetComponent<ProjectileBehavior>();
        if (pb != null) pb.SetCaster(args.Self);
    }

    void OnDrawGizmosSelected()
    {
        GameObject targetObject = m_Target.Get(gameObject);
        if (targetObject == null) return;
        Transform target = targetObject.transform;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, target.position);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, hitRadius);
        switch (projectileType)
        {
            case ProjectileType.Curve:
                DrawCurveGizmo(target);
                break;
            case ProjectileType.Spiral:
                DrawSpiralGizmo();
                break;
            case ProjectileType.Orbit:
                DrawOrbitGizmo(target);
                break;
            case ProjectileType.Artillery:
                DrawArtilleryGizmo(target);
                break;
        }
    }
    
    void DrawCurveGizmo(Transform target)
    {
        Gizmos.color = Color.yellow;
        Vector3 start = transform.position;
        Vector3 end = target.position;
        
        Vector3 curveDir = GetCurveDirection();
        
        int segments = 20;
        for (int i = 0; i < segments; i++)
        {
            float t1 = (float)i / segments;
            float t2 = (float)(i + 1) / segments;
            
            Vector3 pos1 = Vector3.Lerp(start, end, t1);
            Vector3 pos2 = Vector3.Lerp(start, end, t2);
            
            Vector3 offset1 = curveDir * curveShape.Evaluate(t1) * curveHeight;
            Vector3 offset2 = curveDir * curveShape.Evaluate(t2) * curveHeight;
            
            pos1 += offset1;
            pos2 += offset2;
            
            Gizmos.DrawLine(pos1, pos2);
        }
        Gizmos.color = Color.cyan;
        Vector3 center = Vector3.Lerp(start, end, 0.5f);
        Gizmos.DrawRay(center, curveDir * curveHeight);
    }
    
    void DrawSpiralGizmo()
    {
        Gizmos.color = Color.cyan;
        Vector3 center = transform.position;
        
        for (int i = 0; i < 30; i++)
        {
            float angle = (float)i / 30f * Mathf.PI * 4f;
            float radius = spiralRadius * (1f - (float)i / 30f) * spiralTightness;
            
            Vector3 pos = center + new Vector3(
                Mathf.Cos(angle) * radius,
                0,
                Mathf.Sin(angle) * radius
            );
            
            Gizmos.DrawWireSphere(pos, 0.1f);
        }
    }
    
    void DrawOrbitGizmo(Transform target)
    {
        Gizmos.color = Color.magenta;
        Vector3 center = target.position;
        
        for (int orbit = 0; orbit < orbitCount; orbit++)
        {
            float radius = orbitRadius * Mathf.Pow(0.8f, orbit);
            
            for (int i = 0; i < 36; i++)
            {
                float angle1 = (float)i / 36f * 360f * Mathf.Deg2Rad;
                float angle2 = (float)(i + 1) / 36f * 360f * Mathf.Deg2Rad;
                
                Vector3 pos1 = center + new Vector3(
                    Mathf.Cos(angle1) * radius,
                    0,
                    Mathf.Sin(angle1) * radius
                );
                
                Vector3 pos2 = center + new Vector3(
                    Mathf.Cos(angle2) * radius,
                    0,
                    Mathf.Sin(angle2) * radius
                );
                
                Gizmos.DrawLine(pos1, pos2);
            }
        }
    }
    
    void DrawArtilleryGizmo(Transform target)
    {
        Gizmos.color = Color.green;
        Vector3 start = transform.position;
        Vector3 end = target.position;
        
        int segments = 30;
        Vector3 previousPos = start;
        
        for (int i = 1; i <= segments; i++)
        {
            float t = (float)i / segments;
            Vector3 horizontalPos = Vector3.Lerp(start, end, t);
            float verticalOffset;
            
            if (artilleryUseGravity)
            {
                float peakTime = 0.5f;
                if (t < peakTime)
                {
                    float ascendProgress = t / peakTime;
                    verticalOffset = artilleryHeight * artilleryArcCurve.Evaluate(ascendProgress);
                }
                else
                {
                    float descendProgress = (t - peakTime) / (1f - peakTime);
                    verticalOffset = artilleryHeight * (1f - Mathf.Pow(descendProgress, 1.5f));
                }
            }
            else
            {
                verticalOffset = artilleryHeight * artilleryArcCurve.Evaluate(t) * Mathf.Sin(t * Mathf.PI);
            }
            Vector3 currentPos = horizontalPos + Vector3.up * verticalOffset;
            Gizmos.DrawLine(previousPos, currentPos);
            previousPos = currentPos;
        }
        Gizmos.color = Color.yellow;
        Vector3 peakPos = Vector3.Lerp(start, end, 0.5f) + Vector3.up * artilleryHeight;
        Gizmos.DrawWireSphere(peakPos, 0.3f);
    }
}
}
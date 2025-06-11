using UnityEngine;

public class AIControl : MonoBehaviour
{
    public enum AiMode
    {
        Followplayer,
        FollowWayPoint,
        Passive 
    };

    [Header("AI Behavior")]
    [Tooltip("Ai Mode")]
    public AiMode aiMode = AiMode.Followplayer;
    
    [Tooltip("Ai Speed")]
    [Range(0.1f, 1.0f)]
    public float pursuitSpeed = 0.8f;
    
    [Tooltip("How aggressively the AI turns toward the target")]
    [Range(0.5f, 5.0f)]
    public float turnStrength = 2.0f;
    
    [Tooltip("How much to predict the player's movement (Dalam Detik)")]
    [Range(0f, 1.0f)]
    public float predictionTime = 0.3f;

    [Header("Steering Settings")]
    [Tooltip("How smoothly the AI turns (lower = smoother but less responsive)")]
    [Range(0.1f, 2.0f)]
    public float steeringResponseRate = 0.7f;
    
    [Tooltip("How sensitive the steering is to angle differences (higher = more sensitive)")]
    [Range(15f, 90f)]
    public float steeringAngleDivisor = 45f;
    
    [Tooltip("Reduces turn strength when nearly aligned with target (prevents overshooting)")]
    [Range(0.1f, 1.0f)]
    public float alignmentSteeringFactor = 0.5f;
    
    [Tooltip("Minimal speed reduction when turning (higher = less slowdown in turns)")]
    [Range(0.5f, 1.0f)]
    public float minTurnSpeedFactor = 0.7f;

    [Header("Anti-Circling System")]
    [Tooltip("Enables the anti-circling system to prevent AI from getting stuck")]
    public bool useAntiCircling = true;
    
    [Tooltip("How long to track position history for circle detection (seconds)")]
    [Range(0.5f, 5.0f)]
    public float circlingDetectionTime = 2.0f;
    
    [Tooltip("How long to perform recovery maneuvers when circling is detected (seconds)")]
    [Range(0.5f, 2.0f)]
    public float recoveryDuration = 0.8f;
    
    [Tooltip("Minimum progress toward target to avoid being considered stuck")]
    [Range(0.1f, 1.0f)]
    public float minDistanceProgress = 0.3f;

    [Header("Debugging")]
    public bool showDebugVisuals = true;
    public bool showDebugLogs = false;
    [Range(0, 10)] public float debugRayLength = 3.0f;

    // Local Variables 
    private Vector3 targetPosition = Vector3.zero;
    private Transform targetTransform = null;
    private float previousAngleToTarget = 0f;
    private float currentSteeringAngle = 0f;
    
    // Anti-circling variables
    private Vector3 lastPosition;
    private float lastDistance;
    private float stuckTimer = 0f;
    private float recoveryTimer = 0f;
    private bool isRecovering = false;
    private int recoveryDirection = 1;
    
    // Position history for circular pattern detection
    private Vector3[] positionHistory = new Vector3[10]; 
    private float positionHistoryTimer = 0f;
    private int historyIndex = 0;
    private float rotationSum = 0f;
    private Vector3 previousDirection = Vector3.zero;
    
    public float verticalInput { get; private set; }
    public float horizontalInput { get; private set; }
    
    void Start()
    {
        lastPosition = transform.position;
        
        // Initialize position history with current position
        for (int i = 0; i < positionHistory.Length; i++)
        {
            positionHistory[i] = transform.position;
        }
        
        if (aiMode == AiMode.Followplayer)
        {
            FindPlayer();
        }
    }

    void Update()
    {
        switch (aiMode)
        {
            case AiMode.Followplayer:
                FollowPlayer();
                break;
            case AiMode.FollowWayPoint:
                FollowWayPoints();
                break;
        }

        if (useAntiCircling)
        {
            UpdatePositionHistory();
            CheckForCircling();
        }

        if (isRecovering)
        {
            ApplyRecoveryInputs();
        }
        else
        {
            CalculateInputs();
        }
        
        if (showDebugVisuals)
        {
            DrawDebugVisuals();
        }
    }

    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            targetTransform = playerObj.transform;
            lastDistance = Vector3.Distance(transform.position, targetTransform.position);
            if (showDebugLogs) Debug.Log("AI found player at " + targetTransform.position);
        }
        else
        {
            Debug.Log("No Player with tag Player is found");
        }
    }

    void FollowPlayer()
    {
        if (targetTransform == null)
        {
            FindPlayer();
            return;
        }

        targetPosition = targetTransform.position;
        
        if (!isRecovering)
        {
            Rigidbody2D playerRb = targetTransform.GetComponent<Rigidbody2D>();
            if (playerRb != null && playerRb.velocity.magnitude > 0.5f)
            {
                // Predict where player will be
                targetPosition += (Vector3)(playerRb.velocity * predictionTime);
            }
        }
    }

    void FollowWayPoints()
    {
        // -------------------------------- ADD LATER
        if (showDebugLogs) Debug.LogWarning("Waypoint following not implemented yet");
    }
    
    void UpdatePositionHistory()
    {
        positionHistoryTimer += Time.deltaTime;
        float recordInterval = circlingDetectionTime / positionHistory.Length;
        
        if (positionHistoryTimer >= recordInterval)
        {
            positionHistoryTimer = 0f;
            positionHistory[historyIndex] = transform.position;
            historyIndex = (historyIndex + 1) % positionHistory.Length;
            
            if (previousDirection != Vector3.zero && targetTransform != null)
            {
                Vector3 currentDirection = (targetPosition - transform.position).normalized;
                
                float angle = Vector3.SignedAngle(previousDirection, currentDirection, Vector3.forward);
                
                rotationSum = rotationSum * 0.9f + angle;
                
                previousDirection = currentDirection;
            }
            else if (targetTransform != null)
            {
                previousDirection = (targetPosition - transform.position).normalized;
            }
        }
    }

    void CheckForCircling()
    {
        if (targetTransform == null) return;
        
        float distanceMoved = Vector3.Distance(transform.position, lastPosition);
        float currentDistance = Vector3.Distance(transform.position, targetPosition);
        float distanceChange = Mathf.Abs(lastDistance - currentDistance);
        
        bool isMovingInCircle = false;
        
        if (Mathf.Abs(rotationSum) > 270f)
        {
            isMovingInCircle = true;
        }
        
        bool notMakingProgress = distanceChange < minDistanceProgress && 
                                distanceMoved < pursuitSpeed * Time.deltaTime * 2f;
        
        if ((isMovingInCircle || notMakingProgress) && !isRecovering)
        {
            stuckTimer += Time.deltaTime;
            
            if (stuckTimer > circlingDetectionTime * 0.7f)
            {
                isRecovering = true;
                recoveryTimer = 0f;
                recoveryDirection = (Random.value > 0.5f) ? 1 : -1;
                
                if (showDebugLogs) Debug.Log("AI detected circling pattern - recovering");
            }
        }
        else
        {
            stuckTimer = Mathf.Max(0, stuckTimer - Time.deltaTime * 0.5f);
        }
        
        lastPosition = transform.position;
        lastDistance = currentDistance;
    }
    
    void ApplyRecoveryInputs()
    {
        recoveryTimer += Time.deltaTime;
        
        if (recoveryTimer < recoveryDuration * 0.5f)
        {
            verticalInput = -0.5f;
            horizontalInput = 0.8f * recoveryDirection; 
        }
        else
        {
            verticalInput = pursuitSpeed; 
            horizontalInput = -0.8f * recoveryDirection;
        }
        
        if (recoveryTimer >= recoveryDuration)
        {
            isRecovering = false;
            stuckTimer = 0f;
            rotationSum = 0f;
        }
    }

    void CalculateInputs()
    {
        if (targetTransform == null) return;
        
        Vector2 vectorToTarget = targetPosition - transform.position;
        float distanceToTarget = vectorToTarget.magnitude;
        vectorToTarget.Normalize();
        
        Vector2 carForward = transform.right;
        
        float angleToTarget = -Vector2.SignedAngle(carForward, vectorToTarget);
        
        currentSteeringAngle = Mathf.Lerp(currentSteeringAngle, angleToTarget, 
                                        steeringResponseRate * Time.deltaTime * 10);
        
        horizontalInput = Mathf.Clamp(currentSteeringAngle / steeringAngleDivisor * turnStrength, -1f, 1f);
        
        float angleDeltaFromPrevious = Mathf.Abs(angleToTarget - previousAngleToTarget);
        if (Mathf.Abs(angleToTarget) < 10f && angleDeltaFromPrevious < 5f)
        {
            horizontalInput *= alignmentSteeringFactor;
        }
        
        if (Mathf.Sign(angleToTarget) != Mathf.Sign(horizontalInput))
        {
            horizontalInput = Mathf.Clamp(angleToTarget / (steeringAngleDivisor * 0.66f) * turnStrength, -1f, 1f);
        }
        
        // Set vertical input (speed) - ALWAYS use pursuit speed for ramming
        // Only reduce speed slightly for very sharp turns to maintain momentum
        float turnFactor = 1.0f - (Mathf.Abs(horizontalInput) * 0.3f);
        
        turnFactor = Mathf.Max(turnFactor, minTurnSpeedFactor);
        
        verticalInput = pursuitSpeed * turnFactor;
        
        previousAngleToTarget = angleToTarget;
        
        // Debug logging
        if (showDebugLogs)
        {
            Debug.Log($"AI: Angle={angleToTarget:F1}° Distance={distanceToTarget:F1} " +
                      $"Inputs: H={horizontalInput:F2} V={verticalInput:F2}");
        }
    }
    
    // Debugging CODE Settings
    // ____________________________________________________________________________________________
    // Debugging CODE Settings 

    void DrawDebugVisuals()
    {
        // Car forward direction (blue)
        Debug.DrawRay(transform.position, transform.right * debugRayLength, Color.blue);
        
        // Direction to target (red)
        if (targetTransform != null)
        {
            Vector3 dirToTarget = (targetPosition - transform.position).normalized;
            Debug.DrawRay(transform.position, dirToTarget * debugRayLength, Color.red);
            
            // Draw line to actual target (white)
            Debug.DrawLine(transform.position, targetPosition, Color.white);
            
            // Draw a circle at target position (green = normal, yellow = recovery)
            Color targetColor = isRecovering ? Color.yellow : Color.green;
            DebugDrawCircle(targetPosition, 0.5f, targetColor, 16);
            
            // Draw position history path if anti-circling is active
            if (useAntiCircling)
            {
                for (int i = 0; i < positionHistory.Length - 1; i++)
                {
                    int nextIndex = (i + 1) % positionHistory.Length;
                    Debug.DrawLine(positionHistory[i], positionHistory[nextIndex], Color.grey);
                }
                
                // Draw stuck indicator
                if (stuckTimer > 0.5f)
                {
                    float alpha = Mathf.Min(1.0f, stuckTimer / circlingDetectionTime);
                    Color stuckColor = new Color(1, 0, 1, alpha);
                    DebugDrawCircle(transform.position, 1.0f, stuckColor, 20);
                }
            }
        }
    }
    
    // Helper method to draw circles for debugging  
    void DebugDrawCircle(Vector3 center, float radius, Color color, int segments = 32)
    {
        float angleStep = 360f / segments;
        
        for (int i = 0; i < segments; i++)
        {
            float angle1 = i * angleStep * Mathf.Deg2Rad;
            float angle2 = (i + 1) * angleStep * Mathf.Deg2Rad;
            
            Vector3 point1 = center + new Vector3(Mathf.Cos(angle1) * radius, Mathf.Sin(angle1) * radius, 0);
            Vector3 point2 = center + new Vector3(Mathf.Cos(angle2) * radius, Mathf.Sin(angle2) * radius, 0);
            
            Debug.DrawLine(point1, point2, color);
        }
    }
}
using UnityEngine;
using UnityEngine.AI;

public class NavMeshAIControl : MonoBehaviour
{
    [Header("Navigation Settings")]
    public Transform target;
    public float stoppingDistance = 2.0f;
    
    [Header("Movement Parameters")]
    [Range(0.1f, 1.0f)]
    public float pursuitSpeed = 0.8f;
    [Range(0.5f, 5.0f)]
    public float turnStrength = 2.0f;
    [Range(0.1f, 2.0f)]
    public float steeringResponseRate = 0.7f;
    [Range(0.5f, 1.0f)]
    public float minTurnSpeedFactor = 0.7f;
    
    [Header("Path Following")]
    public float cornerLookAheadDistance = 1.0f;
    public float pathDirectionSmoothTime = 0.2f;
    
    [Header("Debugging")]
    public bool showDebugVisuals = true;
    public Color pathColor = Color.green;
    public Color nextPointColor = Color.yellow;
    
    // NavMesh components
    private NavMeshAgent navAgent;
    private NavMeshPath path;
    
    // Movement outputs for car controller
    public float verticalInput { get; private set; }
    public float horizontalInput { get; private set; }
    
    // Path following
    private Vector3 targetPosition;
    private Vector3 currentPathDirection;
    private Vector3 smoothedPathDirection;
    private Vector3 pathDirectionVelocity;
    private float currentSteeringAngle = 0f;
    private int currentPathIndex = 0;
    
    private void Start()
    {
        // Initialize NavMeshAgent
        navAgent = gameObject.AddComponent<NavMeshAgent>();
        navAgent.updateRotation = false;
        navAgent.updateUpAxis = false;
        navAgent.updatePosition = false; // Important: We'll manually update position with car physics
        
        // Set basic NavMeshAgent properties
        navAgent.speed = 3.5f; // This won't directly control car speed
        navAgent.angularSpeed = 360;
        navAgent.acceleration = 8;
        navAgent.stoppingDistance = stoppingDistance;
        navAgent.radius = 0.5f;
        navAgent.height = 0.5f;
        
        // Initialize path
        path = new NavMeshPath();
        
        // Find player if target is not assigned
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
                Debug.Log("NavMeshAI found player target");
            }
            else
            {
                Debug.LogWarning("No target assigned and no Player found");
            }
        }
    }
    
    private void Update()
    {
        if (target == null) return;
        
        // Calculate path to target
        navAgent.CalculatePath(target.position, path);
        
        // Process path and get next point to move toward
        ProcessPath();
        
        // Calculate steering inputs for the car controller
        CalculateInputs();
        
        // Debug visualization
        if (showDebugVisuals)
        {
            DrawDebugVisuals();
        }
        
        // Update agent position to match our actual position
        // This is important since we're not using navAgent.updatePosition
        navAgent.nextPosition = transform.position;
    }
    
    private void ProcessPath()
    {
        if (path.status != NavMeshPathStatus.PathComplete || path.corners.Length < 2)
        {
            // If we can't find a valid path, just aim directly at target
            targetPosition = target.position;
            currentPathDirection = (targetPosition - transform.position).normalized;
            currentPathIndex = 0;
            return;
        }
        
        // Find the current point to aim for
        Vector3 closestPoint = path.corners[0];
        float closestDistance = float.MaxValue;
        int closestIndex = 0;
        
        // Find which path segment we're currently on
        for (int i = 0; i < path.corners.Length - 1; i++)
        {
            Vector3 pointOnLine = FindClosestPointOnLine(path.corners[i], path.corners[i + 1], transform.position);
            float distance = Vector3.Distance(transform.position, pointOnLine);
            
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPoint = pointOnLine;
                closestIndex = i;
            }
        }
        
        currentPathIndex = closestIndex;
        
        // Look ahead on the path based on our speed and look-ahead distance
        float lookAheadDistance = cornerLookAheadDistance;
        float distanceCovered = 0;
        Vector3 lookAheadPoint = closestPoint;
        int lookAheadIndex = closestIndex;
        
        while (lookAheadIndex < path.corners.Length - 1 && distanceCovered < lookAheadDistance)
        {
            float segmentLength = Vector3.Distance(path.corners[lookAheadIndex], path.corners[lookAheadIndex + 1]);
            
            if (distanceCovered + segmentLength > lookAheadDistance)
            {
                // Interpolate to find the exact look-ahead point
                float t = (lookAheadDistance - distanceCovered) / segmentLength;
                lookAheadPoint = Vector3.Lerp(path.corners[lookAheadIndex], path.corners[lookAheadIndex + 1], t);
                break;
            }
            
            distanceCovered += segmentLength;
            lookAheadIndex++;
            lookAheadPoint = path.corners[lookAheadIndex];
        }
        
        targetPosition = lookAheadPoint;
        
        // Calculate direction to target point
        currentPathDirection = (targetPosition - transform.position).normalized;
        
        // Smooth out direction changes for more natural movement
        smoothedPathDirection = Vector3.SmoothDamp(
            smoothedPathDirection, 
            currentPathDirection, 
            ref pathDirectionVelocity, 
            pathDirectionSmoothTime
        );
    }
    
    private void CalculateInputs()
    {
        Vector2 vectorToTarget = targetPosition - transform.position;
        float distanceToTarget = vectorToTarget.magnitude;
        
        if (distanceToTarget < 0.1f)
        {
            // We've reached our current path point
            verticalInput = 0;
            horizontalInput = 0;
            return;
        }
        
        vectorToTarget.Normalize();
        
        // Calculate angle between car forward and target direction
        Vector2 carForward = transform.right; // Assuming car's forward is along right axis (+X)
        float angleToTarget = -Vector2.SignedAngle(carForward, vectorToTarget);
        
        // Smooth the steering angle for more natural movement
        currentSteeringAngle = Mathf.Lerp(currentSteeringAngle, angleToTarget, 
                                         steeringResponseRate * Time.deltaTime * 10);
        
        // Calculate horizontal input (steering)
        float steeringAngleDivisor = 45f; // Convert angle to [-1, 1] range
        horizontalInput = Mathf.Clamp(currentSteeringAngle / steeringAngleDivisor * turnStrength, -1f, 1f);
        
        // Calculate vertical input (speed) - reduce speed in sharp turns
        float turnFactor = 1.0f - (Mathf.Abs(horizontalInput) * 0.3f);
        turnFactor = Mathf.Max(turnFactor, minTurnSpeedFactor);
        
        // Slow down when approaching target or sharp turns
        float approachFactor = 1.0f;
        if (distanceToTarget < stoppingDistance * 2)
        {
            approachFactor = distanceToTarget / (stoppingDistance * 2);
        }
        
        // Final speed
        verticalInput = pursuitSpeed * turnFactor * approachFactor;
        
        // Stop if we're at the final destination
        if (distanceToTarget <= stoppingDistance && Vector3.Distance(transform.position, target.position) <= stoppingDistance)
        {
            verticalInput = 0;
        }
    }
    
    private Vector3 FindClosestPointOnLine(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
    {
        Vector3 line = lineEnd - lineStart;
        float lineLength = line.magnitude;
        line.Normalize();
        
        Vector3 pointVector = point - lineStart;
        float dotProduct = Vector3.Dot(pointVector, line);
        dotProduct = Mathf.Clamp(dotProduct, 0f, lineLength);
        
        return lineStart + line * dotProduct;
    }
    
    private void DrawDebugVisuals()
    {
        // Draw path
        if (path != null && path.corners.Length > 0)
        {
            for (int i = 0; i < path.corners.Length - 1; i++)
            {
                Debug.DrawLine(path.corners[i], path.corners[i + 1], pathColor);
            }
            
            // Draw current target point
            if (currentPathIndex < path.corners.Length - 1)
            {
                Debug.DrawLine(transform.position, targetPosition, nextPointColor);
                DebugDrawCircle(targetPosition, 0.3f, nextPointColor);
            }
        }
        
        // Car forward direction
        Debug.DrawRay(transform.position, transform.right * 2.0f, Color.blue);
        
        // Direction to target
        Debug.DrawRay(transform.position, currentPathDirection * 2.0f, Color.red);
        
        // Smoothed direction
        Debug.DrawRay(transform.position, smoothedPathDirection * 2.0f, Color.magenta);
    }
    
    private void DebugDrawCircle(Vector3 center, float radius, Color color, int segments = 16)
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
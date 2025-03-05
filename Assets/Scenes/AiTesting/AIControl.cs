using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class AIControl : MonoBehaviour
{
    public enum AiMode
    {
        Followplayer,
        FollowWayPoint
    };
    [Header("AI Settings")]
    public AiMode aiMode;
    
    // Local Variabels 
    Vector3 targetPosition = Vector3.zero;
    Transform targetTransform = null;
    
    //Components
    private carNew _carNew;
    public float verticalInput { get; private set; }
    public float horizontalInput { get; private set; }
    void Start()
    {
        _carNew = GetComponent<carNew>();

    }

    // Update is called once per frame
    void Update()
    {
        Vector2 inputVector = Vector2.zero;

        inputVector.x = 1f;
        inputVector.y = 1f;
        
        verticalInput = 0.1f;
        horizontalInput = 0.1f;
        
        // -_carNew.
    }
}

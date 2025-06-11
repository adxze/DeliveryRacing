using UnityEngine;
using UnityEngine.SceneManagement;
public class AICarDriving : MonoBehaviour
{
    [Header("Car Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float driftFactor = 0.95f;

    [Header("Sprite Stacking")]
    [SerializeField] public Sprite[] carSprites;
    [SerializeField] public Sprite[] carTemp;
    [SerializeField] public Sprite[] carSpritesFull;
    [SerializeField] private float layerSpacing = 0.1f;
    [SerializeField] private float perspectiveOffset = 0.05f;
    [SerializeField] private Vector2 offsetDirection = new Vector2(0, 1);
    [SerializeField] private float tweakLayerPositioning = 0.5f;

    [Header("Shadow Settings")]
    [SerializeField] public Color shadowColor = new Color(0, 0, 0, 0.5f);
    [SerializeField] private Vector2 lightDirection = new Vector2(1f, -1f);
    [SerializeField] private float shadowLength = 0.2f;
    [SerializeField] private float shadowAlphaMultiplier = 0.7f;

    [Header("Depth Perspective (Auto-Adjusting)")]
    public float depthAngleX = 0f;
    public float depthAngleY = 0f;
    public float depthAngleLerpSpeed = -10f;

    [Header("Collider Settings")]
    [SerializeField] private Vector2 initialColliderOffset = Vector2.zero; 
    
    [Header("Adding Movement")]
    [Tooltip("Assign a MovementInput component here (or AI input ).")]
    [SerializeField] private bool IsPlayer; 
    public MovementInput input;
    public AIControl aiInput;
    
    
    private Vector2 movement;
    private float currentRotation;
    private GameObject[] spriteLayers;
    private GameObject[] shadowLayers;
    private Rigidbody2D rb;
    private CapsuleCollider2D capsuleCollider;

    private bool _hasPakage;
    
    //experiment 


    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        
        // Set initial offset for the collider
        if (capsuleCollider != null)
        {
            capsuleCollider.offset = initialColliderOffset;
        }
        
        CreateSpriteStack();
    }

    private void CreateSpriteStack()
    {
        spriteLayers = new GameObject[carSprites.Length];
        shadowLayers = new GameObject[carSprites.Length];

        for (int i = 0; i < carSprites.Length; i++)
        {
            // Create shadow layer first
            GameObject shadowLayer = new GameObject($"CarShadow_{i}");
            shadowLayer.transform.SetParent(transform);

            SpriteRenderer shadowSR = shadowLayer.AddComponent<SpriteRenderer>();
            shadowSR.sprite = carSprites[i];
            shadowSR.color = new Color(
                shadowColor.r,
                shadowColor.g,
                shadowColor.b,
                shadowColor.a * (1f - ((float)i / carSprites.Length) * shadowAlphaMultiplier)
            );
            shadowSR.sortingOrder = i - carSprites.Length;

            // Create main sprite layer
            GameObject layer = new GameObject($"CarLayer_{i}");
            layer.transform.SetParent(transform);

            SpriteRenderer sr = layer.AddComponent<SpriteRenderer>();
            sr.sprite = carSprites[i];
            sr.sortingOrder = i;

            Vector3 basePos = new Vector3(
                offsetDirection.x * perspectiveOffset * i * tweakLayerPositioning,
                offsetDirection.y * perspectiveOffset * i * tweakLayerPositioning,
                -i * layerSpacing
            );

            layer.transform.localPosition = basePos;
            shadowLayer.transform.localPosition = basePos; // Initial position, will be updated in Update

            spriteLayers[i] = layer;
            shadowLayers[i] = shadowLayer;
        }
    }

    private void Update()
    {
        float verticalInput = 0;
        float horizontalInput = 0;
        
        if (IsPlayer && input != null)
        {
            verticalInput = input.verticalInput;
            horizontalInput = input.horizontalInput;
        }
        else if (!IsPlayer && aiInput != null)
        {
            verticalInput = aiInput.verticalInput;
            horizontalInput = aiInput.horizontalInput;
        }

        // Updated movement logic
        if (IsPlayer)
        {
            // Original player movement logic
            movement = transform.right * (verticalInput * moveSpeed);

            if (Mathf.Abs(verticalInput) > 0.1f)
            {
                float forwardSign = Mathf.Sign(verticalInput);
                rb.angularVelocity = -horizontalInput * rotationSpeed * forwardSign;
            }
            else
            {
                rb.angularVelocity *= driftFactor;
            }
        }
        else
        {
            // Modified AI movement logic
            movement = transform.right * (verticalInput * moveSpeed);
            
            // When moving, apply steering directly without the verticalInput dependency
            if (Mathf.Abs(verticalInput) > 0.1f)
            {
                // Direct rotation control for AI - removes the circular motion
                float targetRotation = -horizontalInput * rotationSpeed;
                
                // Apply smoother rotation for AI to prevent overshooting
                rb.angularVelocity = Mathf.Lerp(rb.angularVelocity, targetRotation, Time.deltaTime * 5f);
            }
            else
            {
                // Apply drift when not actively moving (same as player)
                rb.angularVelocity *= driftFactor;
            }
        }
    }

    private void FixedUpdate()
    {
        rb.velocity = movement;
        UpdateDepthAngles();
        UpdateLayerPerspective();
        CapsuleRotation();
    }

    private void UpdateDepthAngles()
    {
        float carRotation = transform.eulerAngles.z;
        depthAngleX = Mathf.Clamp(Mathf.Sin(carRotation * Mathf.Deg2Rad) * 4f, -4f, 4f);
        depthAngleY = Mathf.Clamp(Mathf.Cos(carRotation * Mathf.Deg2Rad) * 3f, -3f, 3f);
    }

    private Vector2 WorldToLocalDirection(Vector2 worldDir)
    {
        // Convert the world direction to local space based on the car's rotation
        float angle = -transform.eulerAngles.z * Mathf.Deg2Rad;
        return new Vector2(
            worldDir.x * Mathf.Cos(angle) - worldDir.y * Mathf.Sin(angle),
            worldDir.x * Mathf.Sin(angle) + worldDir.y * Mathf.Cos(angle)
        );
    }

    private void UpdateLayerPerspective()
    {
        float angle = transform.eulerAngles.z * Mathf.Deg2Rad;
        Vector2 rotatedOffset = new Vector2(
            Mathf.Cos(angle) * offsetDirection.x - Mathf.Sin(angle) * offsetDirection.y,
            Mathf.Sin(angle) * offsetDirection.x + Mathf.Cos(angle) * offsetDirection.y
        );

        // Convert world space light direction to local space
        Vector2 normalizedLightDir = lightDirection.normalized;
        Vector2 localLightDir = WorldToLocalDirection(normalizedLightDir);

        for (int i = 0; i < spriteLayers.Length; i++)
        {
            Vector3 newPosition = new Vector3(
                rotatedOffset.x * perspectiveOffset * i,
                rotatedOffset.y * perspectiveOffset * i,
                -i * layerSpacing
            );

            float depthFactor = (carSprites.Length > 1) ? (float)i / (carSprites.Length - 1) : 0f;

            float tiltOffsetX = Mathf.Tan(depthAngleX * Mathf.Deg2Rad) * depthFactor;
            float tiltOffsetY = Mathf.Tan(depthAngleY * Mathf.Deg2Rad) * depthFactor;

            newPosition.x += tiltOffsetX;
            newPosition.y += tiltOffsetY;

            // Update main sprite position
            spriteLayers[i].transform.localPosition = newPosition;

            // Calculate shadow position in local space
            Vector3 shadowPos = newPosition + new Vector3(
                -localLightDir.x * shadowLength,
                -localLightDir.y * shadowLength,
                0.01f
            );
            shadowLayers[i].transform.localPosition = shadowPos;
        }
    }

    void OnTriggerEnter2D(Collider2D target)
    {

        if(target.CompareTag("Item") && !_hasPakage && this.CompareTag("Player"))
        {
            spriteLayers[6].GetComponent<SpriteRenderer>().sprite = carSpritesFull[0];
            // shadowLayers[6].GetComponent<SpriteRenderer>().sprite = carSpritesFull[0];
            spriteLayers[7].GetComponent<SpriteRenderer>().sprite = carSpritesFull[1];
            // shadowLayers[7].GetComponent<SpriteRenderer>().sprite = carSpritesFull[1];
            spriteLayers[8].GetComponent<SpriteRenderer>().sprite = carSpritesFull[2];
            // shadowLayers[8].GetComponent<SpriteRenderer>().sprite = carSpritesFull[2];
            Destroy(target.gameObject, 0.1f);
            _hasPakage = true;

        }
        if(target.CompareTag("Point") && _hasPakage && this.CompareTag("Player"))
        {
            spriteLayers[6].GetComponent<SpriteRenderer>().sprite = carSprites[6];
            spriteLayers[7].GetComponent<SpriteRenderer>().sprite = carSprites[7];
            spriteLayers[8].GetComponent<SpriteRenderer>().sprite = carSprites[8];
            _hasPakage = false;
            ScoreManager.instance.addPoints(10);
        }

        if (target.CompareTag("Death"))
        {
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.name);
        }
        
    }
    
    private void CapsuleRotation()
    {
        if (spriteLayers.Length > 1 && capsuleCollider != null)
        {
            // Get the world position of sprite layer 1
            Vector3 targetPosition = spriteLayers[1].transform.position;
        
            // Adjust the collider's offset to match the sprite layer's position relative to the car
            Vector3 localOffset = transform.InverseTransformPoint(targetPosition);
        
            // Add the initial offset to maintain the original relative positioning
            capsuleCollider.offset = new Vector2(localOffset.x, localOffset.y) + initialColliderOffset;
        }
    }
}
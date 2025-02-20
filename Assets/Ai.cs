using UnityEngine;

public class Ai : MonoBehaviour
{
    [Header("Car Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float driftFactor = 0.95f;

    [Header("Sprite Stacking")]
    [SerializeField] private Sprite[] carSprites;
    [SerializeField] private float layerSpacing = 0.1f;
    [SerializeField] private float perspectiveOffset = 0.05f;
    [SerializeField] private Vector2 offsetDirection = new Vector2(0, 1);
    [SerializeField] private float tweakLayerPositioning = 0.5f;

    [Header("Shadow Settings")]
    [SerializeField] private Color shadowColor = new Color(0, 0, 0, 0.5f);
    [SerializeField] private Vector2 lightDirection = new Vector2(1f, -1f);
    [SerializeField] private float shadowLength = 0.2f;
    [SerializeField] private string shadowSortingLayer = "Shadows";

    [Header("Depth Perspective (Auto-Adjusting)")]
    public float depthAngleX = 0f;
    public float depthAngleY = 0f;
    public float depthAngleLerpSpeed = -10f;

    private Vector2 movement;
    private float currentRotation;
    private GameObject[] spriteLayers;
    private GameObject shadowLayer; // Single shadow layer
    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        CreateShadowSortingLayer();
        CreateSpriteStack();
    }

    private void CreateShadowSortingLayer()
    {
        Debug.Log("Please ensure you have created a Sorting Layer named '" + shadowSortingLayer + "' in Unity's Tag & Layers settings");
    }

    private void CreateSpriteStack()
    {
        spriteLayers = new GameObject[carSprites.Length];

        // Create single shadow layer using the base sprite
        shadowLayer = new GameObject("CarShadow");
        shadowLayer.transform.SetParent(transform);
        SpriteRenderer shadowSR = shadowLayer.AddComponent<SpriteRenderer>();
        shadowSR.sprite = carSprites[0]; // Use only the base sprite for shadow
        shadowSR.color = shadowColor;
        shadowSR.sortingLayerName = shadowSortingLayer;
        shadowSR.sortingOrder = 0;

        // Create sprite layers
        for (int i = 0; i < carSprites.Length; i++)
        {
            GameObject layer = new GameObject($"CarLayer_{i}");
            layer.transform.SetParent(transform);

            SpriteRenderer sr = layer.AddComponent<SpriteRenderer>();
            sr.sprite = carSprites[i];
            sr.sortingOrder = i + 1; // Ensure sprites are above shadow

            Vector3 basePos = new Vector3(
                offsetDirection.x * perspectiveOffset * i * tweakLayerPositioning,
                offsetDirection.y * perspectiveOffset * i * tweakLayerPositioning,
                -i * layerSpacing
            );

            layer.transform.localPosition = basePos;
            spriteLayers[i] = layer;
        }

        // Initialize shadow position
        shadowLayer.transform.localPosition = Vector3.zero;
    }

    private void Update()
    {
        float verticalInput = Input.GetAxis("Vertical");
        float horizontalInput = Input.GetAxis("Horizontal");

        movement = transform.right * verticalInput * moveSpeed;

        if (Mathf.Abs(verticalInput) > 0.1f)
        {
            float rotation = horizontalInput * rotationSpeed * Time.deltaTime;
            currentRotation = rotation;
        }
        else
        {
            currentRotation *= driftFactor;
        }

        transform.Rotate(0, 0, -currentRotation);

        UpdateDepthAngles();
        UpdateLayerPerspective();
    }

    private void FixedUpdate()
    {
        rb.velocity = movement;
    }

    private void UpdateDepthAngles()
    {
        float carRotation = transform.eulerAngles.z;
        depthAngleX = Mathf.Clamp(Mathf.Sin(carRotation * Mathf.Deg2Rad) * 4f, -4f, 4f);
        depthAngleY = Mathf.Clamp(Mathf.Cos(carRotation * Mathf.Deg2Rad) * 3f, -3f, 3f);
    }

    private Vector2 WorldToLocalDirection(Vector2 worldDir)
    {
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

        // Update sprite layers
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

            spriteLayers[i].transform.localPosition = newPosition;
        }

        // Update shadow position using world-to-local light direction
        Vector2 normalizedLightDir = lightDirection.normalized;
        Vector2 localLightDir = WorldToLocalDirection(normalizedLightDir);
        
        Vector3 shadowPos = new Vector3(
            -localLightDir.x * shadowLength,
            -localLightDir.y * shadowLength,
            0.01f
        );
        shadowLayer.transform.localPosition = shadowPos;
    }
}
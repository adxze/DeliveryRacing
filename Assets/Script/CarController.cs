// CarController.cs
using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("Car Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float driftFactor = 0.95f;

    [Header("Sprite Stacking")]
    [SerializeField] private Sprite[] carSprites;
    [SerializeField] private float layerSpacing = 0.1f;
    [SerializeField] private float perspectiveOffset = 0.05f; // Controls how much each layer is offset
    [SerializeField] private Vector2 offsetDirection = new Vector2(0, 1); // Direction of the perspective shift
    [SerializeField] private Color shadowColor = new Color(0, 0, 0, 0.5f);

    [SerializeField] private float tewakLayerPositioning = 0.5f;

    private Vector2 movement;
    private float currentRotation;
    private GameObject[] spriteLayers;
    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        CreateSpriteStack();
    }

    private void CreateSpriteStack()
    {
        spriteLayers = new GameObject[carSprites.Length];
        
        for (int i = 0; i < carSprites.Length; i++)
        {
            // Create layer
            GameObject layer = new GameObject($"CarLayer_{i}");
            layer.transform.SetParent(transform);
            
            // Add and setup SpriteRenderer
            SpriteRenderer spriteRenderer = layer.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = carSprites[i];
            // spriteRenderer.sortingOrder = carSprites.Length - i;
            spriteRenderer.sortingOrder = i; // Instead of carSprites.Length - i

            // Calculate perspective offset
            float heightFactor = (float)i / (carSprites.Length - 1);
            Vector3 perspectivePosition = new Vector3(
                offsetDirection.x * perspectiveOffset * i * tewakLayerPositioning,
                offsetDirection.y * perspectiveOffset * i * tewakLayerPositioning,
                // offsetDirection.x * perspectiveOffset * (carSprites.Length - i - 1),
                // offsetDirection.y * perspectiveOffset * (carSprites.Length - i - 1),
                -i * layerSpacing // Original
                // i * layerSpacing // Modified - buat ganti arah
            );
            
            layer.transform.localPosition = perspectivePosition;

            // Add shadow for bottom layer
            // if (i == carSprites.Length - 1)
            // {
            //     GameObject shadow = new GameObject("Shadow");
            //     shadow.transform.SetParent(transform);
                
            //     // Position shadow slightly offset from the bottom layer
            //     Vector3 shadowPos = perspectivePosition;
            //     shadowPos.x += 0.2f;
            //     shadowPos.y -= 0.2f;
            //     shadowPos.z += 0.1f;
                
            //     shadow.transform.localPosition = shadowPos;
            //     shadow.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
                
            //     SpriteRenderer shadowRenderer = shadow.AddComponent<SpriteRenderer>();
            //     shadowRenderer.sprite = carSprites[i];
            //     shadowRenderer.color = shadowColor;
            //     shadowRenderer.sortingOrder = -1;
            // }

            spriteLayers[i] = layer;
        }
    }

    private void Update()
    {
        // Get input
        float verticalInput = Input.GetAxis("Vertical");
        float horizontalInput = Input.GetAxis("Horizontal");

        // Calculate movement
        // movement = transform.up * verticalInput * moveSpeed;

        movement = transform.right * verticalInput * moveSpeed;

        
        // Calculate rotation
        if (Mathf.Abs(verticalInput) > 0.1f)
        {
            float rotation = horizontalInput * rotationSpeed * Time.deltaTime;
            currentRotation = rotation;
        }
        else
        {
            currentRotation *= driftFactor;
        }

        // Apply rotation
        transform.Rotate(0, 0, -currentRotation);
        // transform.Rotate(0, 0, currentRotation);

        // Update layer positions based on rotation
        UpdateLayerPerspective();
    }

    private void UpdateLayerPerspective()
    {
        for (int i = 0; i < spriteLayers.Length; i++)
        {
            float angle = transform.eulerAngles.z * Mathf.Deg2Rad;
            Vector2 rotatedOffset = new Vector2(
                Mathf.Cos(angle) * offsetDirection.x - Mathf.Sin(angle) * offsetDirection.y,
                Mathf.Sin(angle) * offsetDirection.x + Mathf.Cos(angle) * offsetDirection.y
            );

            Vector3 newPosition = new Vector3(
                rotatedOffset.x * perspectiveOffset * i,
                rotatedOffset.y * perspectiveOffset * i,
                -i * layerSpacing
            );

            spriteLayers[i].transform.localPosition = newPosition;
        }
    }

    private void FixedUpdate()
    {
        // Apply movement
        rb.velocity = movement;
    }
}
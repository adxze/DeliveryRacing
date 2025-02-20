using UnityEngine;

public class mouse : MonoBehaviour
{
    [Header("Mouse Control Settings")]
    [SerializeField] private float moveSpeed = 5f; // Speed of movement
    [SerializeField] private float rotationSpeed = 200f; // How fast the car rotates
    [SerializeField] private float stopDistance = 0.2f; // Distance where the car stops moving

    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        MoveTowardMouse();
    }

    private void MoveTowardMouse()
    {
        // Get mouse position in world space
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f; // Ensure it's 2D

        // Calculate direction to mouse
        Vector3 direction = (mousePosition - transform.position).normalized;

        // Check distance to prevent jittering when close to the mouse
        float distance = Vector3.Distance(transform.position, mousePosition);
        if (distance > stopDistance)
        {
            // Move toward the mouse
            rb.velocity = direction * moveSpeed;

            // Rotate smoothly toward the mouse direction
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            float newAngle = Mathf.LerpAngle(transform.eulerAngles.z, targetAngle, Time.deltaTime * (rotationSpeed / 100f));
            transform.rotation = Quaternion.Euler(0, 0, newAngle);
        }
        else
        {
            // Stop moving when close enough
            rb.velocity = Vector2.zero;
        }
    }
}

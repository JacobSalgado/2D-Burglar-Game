using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // public variables
    public float moveSpeed = 5f;

    // private variables
    private Rigidbody2D rb;
    private Vector2 movementInput;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        // Read horizontal and vertical input
        movementInput.x = Input.GetAxisRaw("Horizontal");
        movementInput.y = Input.GetAxisRaw("Vertical");

        // Normalize the movement input to prevent faster diagonal movement
        movementInput.Normalize();
    }

    private void FixedUpdate()
    {
        // apply velocity to the rigidbody2d based on input and speed
        rb.linearVelocity = movementInput * moveSpeed;
    }
}

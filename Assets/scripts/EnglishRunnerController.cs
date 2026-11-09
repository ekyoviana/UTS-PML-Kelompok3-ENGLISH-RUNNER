using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class EnglishRunnerController : MonoBehaviour
{
    [Header("Movement")]
    public float forwardSpeed = 8f;
    public float laneDistance = 3f;
    public float laneChangeSpeed = 10f;

    [Header("Jump")]
    public float jumpForce = 7f;

    private Rigidbody rb;

    private int currentLane = 1;
    private bool isGrounded = true;

    private Vector3 targetPosition;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        targetPosition = transform.position;
    }

    private void Update()
    {
        HandleInput();
        MoveToLane();
    }

    private void FixedUpdate()
    {
        // Karakter terus bergerak ke depan
        rb.linearVelocity = new Vector3(
            rb.linearVelocity.x,
            rb.linearVelocity.y,
            forwardSpeed
        );
    }

    private void HandleInput()
    {
        if (Keyboard.current == null)
            return;

        // Ke kiri
        if (Keyboard.current.leftArrowKey.wasPressedThisFrame ||
            Keyboard.current.aKey.wasPressedThisFrame)
        {
            ChangeLane(-1);
        }

        // Ke kanan
        if (Keyboard.current.rightArrowKey.wasPressedThisFrame ||
            Keyboard.current.dKey.wasPressedThisFrame)
        {
            ChangeLane(1);
        }

        // Lompat
        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            Jump();
        }
    }

    private void ChangeLane(int direction)
    {
        currentLane += direction;

        currentLane = Mathf.Clamp(currentLane, 0, 2);

        targetPosition = new Vector3(
            (currentLane - 1) * laneDistance,
            transform.position.y,
            transform.position.z
        );
    }

    private void MoveToLane()
    {
        Vector3 newPosition = transform.position;

        float targetX = (currentLane - 1) * laneDistance;

        newPosition.x = Mathf.Lerp(
            newPosition.x,
            targetX,
            laneChangeSpeed * Time.deltaTime
        );

        transform.position = newPosition;
    }

    private void Jump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        isGrounded = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Word"))
        {
            WordItem word = other.GetComponent<WordItem>();

            if (word != null)
            {
                word.Collect();
            }
        }

        if (other.CompareTag("Obstacle"))
        {
            EnglishRunnerGame.Instance.GameOver();
        }
    }
}
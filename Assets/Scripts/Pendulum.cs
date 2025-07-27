using UnityEngine;

public class Pendulum : MonoBehaviour
{
    private Rigidbody2D rb;
    public float amplitude = 100f;  
    public float frequency = 1f;   
    public float c = 2.0f;
    public HingeJoint2D ball;

    private float startTime;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startTime = Time.time;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Space was pressed, droping ball");
            ball.enabled = false;
            Debug.Log("Droped ball");
        }

        float time = Time.time - startTime;
        float angularVelocity = Mathf.Sin(time * frequency * c * Mathf.PI) * amplitude;
        rb.angularVelocity = angularVelocity;
    }

}

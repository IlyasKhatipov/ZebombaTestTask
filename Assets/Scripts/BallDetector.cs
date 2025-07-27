using UnityEngine;

public class BallDetector : MonoBehaviour
{
    public int index; 

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameManager.Instance.OnBallLanded(index, collision.gameObject);
    }
}

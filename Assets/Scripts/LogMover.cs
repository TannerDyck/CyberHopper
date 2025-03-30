using UnityEngine;

public class LogMover : MonoBehaviour
{
    public float speed = 2f;

    void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);


        if (Mathf.Abs(transform.position.x) > 20f)
        {
            Destroy(gameObject);
        }
    }
}

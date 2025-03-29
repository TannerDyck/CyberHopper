using UnityEngine;

public class LogMover : MonoBehaviour
{
    public float speed = 2f;
    public float wrapX = 10f; 

    private void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);

        if (speed > 0 && transform.position.x > wrapX)
        {
            transform.position = new Vector3(-wrapX, transform.position.y, transform.position.z);
        }
        else if (speed < 0 && transform.position.x < -wrapX)
        {
            transform.position = new Vector3(wrapX, transform.position.y, transform.position.z);
        }
    }
}

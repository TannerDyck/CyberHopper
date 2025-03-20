using System.Drawing;
using System.Numerics;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class MoveCycle : MonoBehaviour
{
    public UnityEngine.Vector2 direction = UnityEngine.Vector2.right;
    public float speed = 1f;
    public int size = 1;
    private UnityEngine.Vector3 leftEdge;
    private UnityEngine.Vector3 rightEdge;

    private void Start()
    {
        leftEdge = Camera.main.ViewportToWorldPoint(UnityEngine.Vector3.zero);
        rightEdge = Camera.main.ViewportToWorldPoint(UnityEngine.Vector2.right);
    }

    private void Update()
    {
        if (direction.x > 0 && (transform.position.x - size) > rightEdge.x)
        {
            UnityEngine.Vector3 position = transform.position;
            position.x = leftEdge.x - size;
            transform.position = position;
        }
        else if (direction.x < 0 && (transform.position.x + size) < leftEdge.x)
        {
            UnityEngine.Vector3 position = transform.position;
            position.x = rightEdge.x + size;
            transform.position = position;
        }
        else 
        {
            transform.Translate(direction * speed * Time.deltaTime);
        }
    }
}

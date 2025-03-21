using UnityEngine;

public class PowerUpEffect : MonoBehaviour
{
  public float lifetime = 1.0f;

  void Start()
  {
    // Destroy the effect after lifetime seconds
    Destroy(gameObject, lifetime);
  }
}
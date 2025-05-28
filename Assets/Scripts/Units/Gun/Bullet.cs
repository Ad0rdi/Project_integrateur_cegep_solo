/* Original author name: Gilles
 * Creation date: ???
 * Goal: Code the bullet used in guns
 * Modification listing:
 * 2025/05/11:
 *      Author Name: Donavan Sirois
 *      Goal: Rehauled the entire system to be more flexible, thus allowing the creation of multiple different guns. Also added damage.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float _speed = 30f;
    [SerializeField] private float _destroyTime = 3f;
    [SerializeField] private float _damage = 1f;
    private Rigidbody2D _rb;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, _destroyTime);
        _rb.velocity = transform.right * _speed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.TryGetComponent<Creature>(out Creature creatureComponent))
        {
            creatureComponent.TakeDamage(_damage);
        }

        Destroy(gameObject);
    }
}

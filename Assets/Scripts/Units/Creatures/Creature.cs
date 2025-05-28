/* Original author name: Gilles
 * Creation date: ???
 * Goal: Code the bullet used in guns
 * Modification listing:
 * 
 * 
 * 
*/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Creature : MonoBehaviour
{
    [SerializeField] float _health, _maxHealth = 3f;
    [SerializeField] float _damage = 3f;
    private Healthbar _healthbar;

    private void Awake()
    {
        Invoke(nameof(GetHealthBar), 1f);
    }

    private void GetHealthBar()
    {
        _healthbar = GameObject.Find("HealthbarOutline").GetComponent<Healthbar>();
    }

    private void Start()
    {
        _health = _maxHealth;
    }

    public void TakeDamage(float damageAmount)
    {
        _health -= damageAmount;

        if (_health <= 0)
        {
            Destroy(gameObject);
        }    
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_healthbar == null)
        {
            return;
        }
        if (collision.gameObject.TryGetComponent<PlayerMovement>(out PlayerMovement playerComponenet))
        {
            _healthbar.TakeDamage(_damage);
        }
    }
}

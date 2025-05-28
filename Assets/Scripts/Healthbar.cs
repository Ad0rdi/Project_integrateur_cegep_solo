using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class Healthbar : MonoBehaviour
{
    public Image _healthBar;
    public float _healthAmount = 10f;
    [SerializeField] Managers.MenuManager _menuManager;

    public void TakeDamage(float damage)
    {
        _healthAmount -= damage;
        _healthBar.fillAmount = _healthAmount / 100f;

        if (_healthAmount <= 0)
        {
            _menuManager.GameOver();
        }
    }
}

using System;
using UnityEngine;
using UnityEngine.Events;

public class PlayerButton : MonoBehaviour
{
    public static Action AttackSkipping;
    public static Action MoveSkipping;
    public static Action<GameObject> CardsTaking;

    private SpriteRenderer _spriteRenderer;

    //0 - кнопка выключена, 1 - "Бито", 2 - "Взять"
    private int _status = 0;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void OnMouseUpAsButton()
    {
        if (_status == 1)
        {
            SetStatus(0);
            MoveSkipping?.Invoke();
            AttackSkipping?.Invoke();
        }
        else if (_status == 2)
        {
            SetStatus(0);
            CardsTaking?.Invoke(GetComponentInParent<PlayerField>().gameObject);
        }
    }

    public void SetStatus(int value)
    {
        _status = value;
        if (_status == 0)
        {
            _spriteRenderer.enabled = false;
        }
        else if (_status == 1)
        {
            _spriteRenderer.enabled = true;
            _spriteRenderer.color = Color.green;
        }
        else if (_status == 2)
        {
            _spriteRenderer.enabled = true;
            _spriteRenderer.color = Color.red;
        }
    }
}

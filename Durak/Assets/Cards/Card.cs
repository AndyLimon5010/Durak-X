using System;
using UnityEngine;

public class Card : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer _spriteRenderer;

    [SerializeField]
    private string _name;

    private Draggable _draggable;

    private void Awake()
    {
        _draggable = GetComponent<Draggable>();
    }

    public char GetSuit()
    {
        return _name[0];
    }
    public int GetValue()
    {
        string str = "";
        for (int i = 2; i < _name.Length; i++)
        {
            str += _name[i].ToString();
        }
        return Convert.ToInt32(str);
    }
    public void SetDraggble(bool value)
    {
        _draggable.enabled = value;
    }
    public void SetParameters(string name, Sprite sprite)
    {
        _name = name;
        _spriteRenderer.sprite = sprite;
    }
}

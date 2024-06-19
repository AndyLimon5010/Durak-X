using System;
using System.Collections.Generic;
using UnityEngine;

public class Draggable : MonoBehaviour
{
    public static Action<Card> PlayerTryingToAttack;
    public static Action<Card, List<GameObject>> PlayerTryingToDefend;

    private readonly int _cardOnFieldLayerNumber = 9;
    private readonly string _playfieldLayerMaskName = "Playfield";
    private readonly string _cardOnFieldLayerMaskName = "CardOnField";

    private Collider2D _collider;

    private Vector3 _startPosition;

    private List<GameObject> _cardsOnField = new();
    private bool _isDragging = false;
    private bool _isAboveField = false;
    private bool _isAttacking = false;
    private bool _isDefending = false;

    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
    }
    private void Update()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (_collider == Physics2D.OverlapPoint(mousePos))
        {
            Take();
        }
        if (_isDragging)
        {
            Drag();
        }
        if (_isDragging == true && Input.GetMouseButtonUp(0) && Input.touchCount <= 0)
        {
            Put();
        }
    }
    private void OnEnable()
    {
        _startPosition = transform.position;

        Playfield.CardNotPut += CardNotPutHandler;
        Playfield.CardAttacked += CardAttackedHandler;
        Playfield.CardDefended += CardDefendedHandler;
        Playfield.CardTransferred += CardTransferredHandler;
    }
    private void OnDisable()
    {
        Playfield.CardNotPut -= CardNotPutHandler;
        Playfield.CardAttacked -= CardAttackedHandler;
        Playfield.CardDefended -= CardDefendedHandler;
        Playfield.CardTransferred -= CardTransferredHandler;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer(_playfieldLayerMaskName))
        {
            _isAboveField = true;
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer(_cardOnFieldLayerMaskName))
        {
            _cardsOnField.Add(collision.gameObject);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer(_playfieldLayerMaskName))
        {
            _isAboveField = false;
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer(_cardOnFieldLayerMaskName))
        {
            _cardsOnField.Remove(collision.gameObject);
        }
    }

    public void SetStartPosition()
    {
        _startPosition = transform.position;
    }

    private void CardAttackedHandler(GameObject empty)
    {
        if (_isAttacking == true)
        {
            _isAttacking = false;
            gameObject.layer = _cardOnFieldLayerNumber;
            enabled = false;
        }
    }
    private void CardDefendedHandler(GameObject empty)
    {
        if (_isDefending == true)
        {
            _isDefending = false;
            enabled = false;
        }
    }
    private void CardNotPutHandler()
    {
        if (_isAttacking == true || _isDefending == true)
        {
            _isAttacking = false;
            _isDefending = false;

            RerturnStartPosition();
        }
    }
    private void CardTransferredHandler(Draggable draggable)
    {
        if (draggable == this)
        {
            Put();
        }
    }

    private void Drag()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = Camera.main.transform.position.z + Camera.main.nearClipPlane;
        transform.position = mousePosition;
    }
    private void Put()
    {
        if (_isAboveField == true && GetComponentInParent<PlayerField>().GetAttack() == true)
        {
            _isAttacking = true;
            PlayerTryingToAttack?.Invoke(gameObject.GetComponent<Card>());
        }
        else if (_cardsOnField.Count > 0 && GetComponentInParent<PlayerField>().GetDefense() == true)
        {
            _isDefending = true;
            PlayerTryingToDefend?.Invoke(gameObject.GetComponent<Card>(), _cardsOnField);
        }
        else
        {
            RerturnStartPosition();
        }
        _isAboveField = false;
        _isDragging = false;
    }
    private void Take()
    {
        if (Input.GetMouseButtonDown(0) || Input.touchCount == 1)
        {
            _isDragging = true;
        }
    }
    private void RerturnStartPosition()
    {
        transform.position = _startPosition;
    }
}

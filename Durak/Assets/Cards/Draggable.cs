using System;
using System.Collections.Generic;
using UnityEngine;

public class Draggable : MonoBehaviour
{
    public static Action CardAttacked;
    public static Action CardDefended;
    public static Action<PlayerField> PlayerAttacked;
    public static Action<PlayerField> PlayerDefended;

    private readonly int _cardOnFieldLayerNumber = 9;
    private readonly string _playfieldLayerMaskName = "Playfield";
    private readonly string _cardOnFieldLayerMaskName = "CardOnField";

    private Collider2D _collider;
    private Playfield _playfield;
    private PlayerField _playerField;

    private List<GameObject> _cardsOnField = new();
    private bool _dragging = false;
    private bool _aboveField = false;

    private void Start()
    {
        _collider = GetComponent<Collider2D>();
        _playerField = GetComponentInParent<PlayerField>();
        _dragging = false;
        _aboveField = false;
    }
    private void Update()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (_collider == Physics2D.OverlapPoint(mousePos))
        {
            Take();
        }
        if (_dragging)
        {
            Drag();
        }
        if (_dragging == true && Input.GetMouseButtonUp(0) && Input.touchCount <= 0)
        {
            Put();
        }
    }
    private void OnEnable()
    {
        _playerField = GetComponentInParent<PlayerField>();
        _cardsOnField.Clear();
        _playfield = null;
        _dragging = false;
        _aboveField = false;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer(_playfieldLayerMaskName))
        {
            _aboveField = true;
            _playfield = collision.gameObject.GetComponent<Playfield>();
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
            _aboveField = false;
            _playfield = null;
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer(_cardOnFieldLayerMaskName))
        {
            _cardsOnField.Remove(collision.gameObject);
        }
    }

    private void Attack()
    {
        List<int> cardsOnTableValue = new();
        SetValues(cardsOnTableValue);

        if (cardsOnTableValue.Count == 0)
        {
            AttackFirstTime();
        }
        else
        {
            AttackNextTime(cardsOnTableValue);
        }
    }
    private void AttackFirstTime()
    {
        _playerField.DeleteCard(gameObject);
        _playfield.AddCardValue(gameObject.GetComponent<Card>().GetValue());
        _playfield.AddAttackCard(gameObject);
        gameObject.layer = _cardOnFieldLayerNumber;
        enabled = false;

        CardAttacked?.Invoke();
        PlayerAttacked?.Invoke(_playerField);
    }
    private void AttackNextTime(List<int> cardsOnTableValue)
    {
        int cardValue = gameObject.GetComponent<Card>().GetValue();
        for (int i = 0; i < cardsOnTableValue.Count; i++)
        {
            if (cardsOnTableValue[i] == cardValue)
            {
                _playerField.DeleteCard(gameObject);
                _playfield.AddAttackCard(gameObject);
                gameObject.layer = _cardOnFieldLayerNumber;
                enabled = false;
                CardAttacked?.Invoke();
                PlayerAttacked?.Invoke(_playerField);
                return;
            }
        }

        SetStartPosition();
    }
    private void Defend()
    {
        int nearestCardIndex = FindNearestCardIndex();
        bool isCanPut = IsCanPut(nearestCardIndex);

        if (_cardsOnField[nearestCardIndex].GetComponentsInChildren<Card>().Length == 1 &&
            isCanPut == true)
        {
            _playerField.DeleteCard(gameObject);
            _playfield.AddCardValue(gameObject.GetComponent<Card>().GetValue());
            _playfield.AddDefenseCard(gameObject);
            transform.SetParent(_cardsOnField[nearestCardIndex].transform);
            transform.position = _cardsOnField[nearestCardIndex].transform.position + new Vector3(0.2f, -0.2f, -0.1f);

            enabled = false;

            CardDefended?.Invoke();
            PlayerDefended?.Invoke(_playerField);
        }
        else
        {
            SetStartPosition();
        }
    }
    private void Drag()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = Camera.main.transform.position.z + Camera.main.nearClipPlane;
        transform.position = mousePosition;
    }
    private int FindNearestCardIndex()
    {
        float minDistance = Mathf.Infinity;
        int index = 0;

        for (int i = 0; i < _cardsOnField.Count; i++)
        {
            Vector3 cardPos = transform.TransformPoint(transform.localPosition);
            Vector3 cardOnTablePos = _cardsOnField[i].transform.TransformPoint(
                _cardsOnField[i].transform.localPosition);
            float distance = Vector2.Distance(cardPos, cardOnTablePos);
            if (distance < minDistance)
            {
                minDistance = distance;
                index = i;
            }
        }

        return index;
    }
    private bool IsCanPut(int nearestCardIndex)
    {
        bool isCanPut = false;
        Card cardOnField = _cardsOnField[nearestCardIndex].GetComponent<Card>();
        Card myCard = gameObject.GetComponent<Card>();
        char cardOnFieldSuit = cardOnField.GetSuit();
        char mySuit = myCard.GetSuit();
        if (cardOnFieldSuit == mySuit)
        {
            int cardOnFieldValue = cardOnField.GetValue();
            int myValue = myCard.GetValue();
            if (cardOnFieldSuit == '1' && cardOnFieldValue == 7)
            {
                isCanPut = false;
            }
            else if (cardOnFieldValue < myValue)
            {
                isCanPut = true;
            }
        }
        else if (cardOnFieldSuit != GameTable.Trump &&
        mySuit == GameTable.Trump && cardOnFieldSuit != '1')
        {
            isCanPut = true;
        }

        return isCanPut;
    }
    private void Put()
    {
        if (_aboveField == true && _playerField.GetAttack() == true &&
            _playfield.IsThereFreeSpace() == true)
        {
            Attack();
        }
        else if (_cardsOnField.Count > 0 && _playerField.GetDefense() == true)
        {
            Defend();
        }
        else
        {
            SetStartPosition();
        }
        _aboveField = false;
        _dragging = false;
    }
    private void Take()
    {
        if (Input.GetMouseButtonDown(0) || Input.touchCount == 1)
        {
            _dragging = true;
        }
    }
    private void SetStartPosition()
    {
        _playerField.SetCardPositions();
    }
    private void SetValues(List<int> cardsOnTableValue)
    {
        for (int i = 0; i < _playfield.GetCardsValues().Count; i++)
        {
            cardsOnTableValue.Add(_playfield.GetCardsValues()[i]);
        }
    }
}

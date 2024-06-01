using System;
using System.Collections.Generic;
using UnityEngine;

public class Playfield : MonoBehaviour
{
    public static Action AllCardsDefended;
    public static Action AllSeatsOccupied;

    private readonly string _cardsGoName = "Cards";

    private List<GameObject> _attackCardGos = new();
    private List<GameObject> _defenseCardGos = new();
    private List<int> _cardsValues = new();
    private int _maxAttackCardCount = 6;
    private int _currentAttackCardCount = 0;

    private readonly int _cardLayerNumber = 8;

    private void OnEnable()
    {
        PlayerButton.CardsTaking += CardsTakingHandler;
        Draggable.CardDefended += CardDefendedHandler;
    }
    private void OnDisable()
    {
        PlayerButton.CardsTaking -= CardsTakingHandler;
        Draggable.CardDefended -= CardDefendedHandler;
    }

    public void AddAttackCard(GameObject cardGo)
    {
        _attackCardGos.Add(cardGo);
        _currentAttackCardCount++;
        SetCardPositions();
    }
    public void AddCardValue(int value)
    {
        _cardsValues.Add(value);
    }
    public void AddDefenseCard(GameObject cardGo)
    {
        _defenseCardGos.Add(cardGo);
        if (_defenseCardGos.Count == _maxAttackCardCount)
        {
            DeleteAllCards();
            AllSeatsOccupied?.Invoke();
        }
    }
    public void DeleteAllCards()
    {
        for (int i = 0; i < _attackCardGos.Count; i++)
        {
            Destroy(_attackCardGos[i]);
        }
        for (int i = 0; i < _defenseCardGos.Count; i++)
        {
            Destroy(_defenseCardGos[i]);
        }

        _attackCardGos.Clear();
        _defenseCardGos.Clear();
        _cardsValues.Clear();
        _currentAttackCardCount = 0;
    }
    public List<int> GetCardsValues()
    {
        return _cardsValues;
    }
    public bool IsThereFreeSpace()
    {
        if (_currentAttackCardCount < _maxAttackCardCount)
        {
            return true;
        }

        return false;
    }
    public void SetMaxAttackCardCount(int value)
    {
        if (value > 6)
        {
            value = 6;
        }
        _maxAttackCardCount = value;
    }

    private void CardDefendedHandler()
    {
        if (_attackCardGos.Count == _defenseCardGos.Count)
        {
            AllCardsDefended?.Invoke();
        }
    }
    private void CardsTakingHandler(GameObject playerFieldGo)
    {
        if (_currentAttackCardCount > 0)
        {
            List<GameObject> cardGos = new();
            
            for (int i = 0; i < _attackCardGos.Count; i++)
            {
                _attackCardGos[i].layer = _cardLayerNumber;
                cardGos.Add(_attackCardGos[i]);
            }
            for (int i = 0; i < _defenseCardGos.Count; i++)
            {
                cardGos.Add(_defenseCardGos[i]);
            }

            playerFieldGo.GetComponent<PlayerField>().AddCards(cardGos);

            _attackCardGos.Clear();
            _defenseCardGos.Clear();
            _cardsValues.Clear();
            _currentAttackCardCount = 0;
        }
    }
    
    private void SetCardPositions()
    {
        GameObject cardsGo = transform.Find(_cardsGoName).gameObject;

        for (int i = 0; i < _attackCardGos.Count; i++)
        {
            _attackCardGos[i].transform.SetParent(cardsGo.transform);
        }

        int indentRight = 0, indentDown = 0;
        for (int i = 0; i < _attackCardGos.Count; i++)
        {
            if (i % 3 == 0 && i != 0)
            {
                indentRight = 0;
                indentDown++;
            }
            _attackCardGos[i].transform.localPosition = new Vector3(indentRight * 2f, indentDown * -3f, 0f);
            _attackCardGos[i].transform.localScale = Vector3.one;
            indentRight++;
        }
    }
}

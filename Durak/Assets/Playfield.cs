using System;
using System.Collections.Generic;
using UnityEngine;

public class Playfield : MonoBehaviour
{
    [SerializeField]
    private GameObject _transferSpace;

    public static Action<GameObject> CardAttacked;
    public static Action<GameObject> CardDefended;
    public static Action CardNotPut;
    public static Action CardTransferring;
    public static Action<Draggable> CardTransferred;
    public static Action LimitingMove;
    public static Action AllCardsDefended;
    public static Action AllSeatsOccupied;
    public static Action CardsTook;

    private readonly string _cardsGoName = "Cards";
    private readonly int _cardLayerNumber = 8;

    private List<GameObject> _attackCardGos = new();
    private List<GameObject> _defenseCardGos = new();
    private List<int> _cardsValues = new();
    private int _maxAttackCardCount = 6;
    private bool _isThereDefenseQueen = false;

    private void OnEnable()
    {
        PlayerButton.CardsTaking += CardsTakingHandler;
        Draggable.PlayerTryingToAttack += PlayerTryingToAttackHandler;
        Draggable.PlayerTryingToDefend += PlayerTryingToDefendHandler;
        GameTable.MoveEnded += MoveEndedHandler;
    }
    private void OnDisable()
    {
        PlayerButton.CardsTaking -= CardsTakingHandler;
        Draggable.PlayerTryingToAttack -= PlayerTryingToAttackHandler;
        Draggable.PlayerTryingToDefend -= PlayerTryingToDefendHandler;
        GameTable.MoveEnded -= MoveEndedHandler;
    }

    public void AddCardValue(int value)
    {
        _cardsValues.Add(value);
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
    }
    public List<int> GetCardsValues()
    {
        return _cardsValues;
    }
    public void SetMaxAttackCardCount(int value)
    {
        if (value > 6)
        {
            value = 6;
        }
        _maxAttackCardCount = value;
    }

    private void MoveEndedHandler()
    {
        _isThereDefenseQueen = false;
    }
    private void PlayerTryingToAttackHandler(Card card)
    {
        if (_attackCardGos.Count == 0)
        {
            _cardsValues.Add(card.GetValue());
            CardAttacked?.Invoke(card.gameObject);
            AddAttackCard(card.gameObject);
        }
        else
        {
            if (_cardsValues.Contains(card.GetValue()) == true)
            {
                CardAttacked?.Invoke(card.gameObject);
                AddAttackCard(card.gameObject);
            }
            else
            {
                CardNotPut?.Invoke();
            }
        }
    }
    private void PlayerTryingToDefendHandler(Card card, List<GameObject> cardsOnField)
    {
        GameObject attackCardGo = FindNearestCard(card.gameObject, cardsOnField);
        Card attackCard = attackCardGo.GetComponent<Card>();
        
        if (_cardsValues.Count == 1 && _cardsValues[0] == card.GetValue() &&
            attackCardGo.name == "TransferSpace")
        {
            CardTransferring?.Invoke();
            CardTransferred?.Invoke(card.GetComponent<Draggable>());
        }
        else if (attackCardGo.name != "TransferSpace" &&
            attackCard.GetComponentsInChildren<Card>().Length == 1)
        {
            if (card.GetSuit() == attackCard.GetSuit())
            {
                if (card.GetValue() > attackCard.GetValue())
                {
                    if (attackCard.GetSuit() == '1' && attackCard.GetValue() == 7)
                    {
                        CardNotPut?.Invoke();
                    }
                    else
                    {
                        PrepareToAddDefenseCard(card.gameObject, attackCard.gameObject);
                    }
                }
                else
                {
                    CardNotPut?.Invoke();
                }
            }
            else
            {
                if (card.GetSuit() == GameTable.Trump)
                {
                    if (attackCard.GetSuit() == '1')
                    {
                        CardNotPut?.Invoke();
                    }
                    else
                    {
                        PrepareToAddDefenseCard(card.gameObject, attackCard.gameObject);
                    }
                }
                else
                {
                    CardNotPut?.Invoke();
                }
            }
        }
        else
        {
            CardNotPut?.Invoke();
        }
    }
    private void CardsTakingHandler(GameObject playerFieldGo)
    {
        if (_transferSpace.activeInHierarchy == true)
        {
            _transferSpace.SetActive(false);
        }

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

        CardsTook?.Invoke();
    }

    private void AddAttackCard(GameObject cardGo)
    {
        _attackCardGos.Add(cardGo);
        SetCardPositions();

        if (_attackCardGos.Count == _maxAttackCardCount)
        {
            LimitingMove?.Invoke();
        }
    }
    private void AddDefenseCard(GameObject cardGo, GameObject attackCardGo)
    {
        if (_transferSpace.activeInHierarchy == true)
        {
            _transferSpace.SetActive(false);
        }

        _defenseCardGos.Add(cardGo);
        cardGo.transform.SetParent(attackCardGo.transform);
        cardGo.transform.position = attackCardGo.transform.position + new Vector3(0.2f, -0.2f, -0.1f);

        if (_defenseCardGos.Count == _attackCardGos.Count)
        {
            if (_defenseCardGos.Count == _maxAttackCardCount || _isThereDefenseQueen == true)
            {
                DeleteAllCards();
                AllSeatsOccupied?.Invoke();
            }
            else
            {
                if (_isThereDefenseQueen == false)
                {
                    AllCardsDefended?.Invoke();
                }
            }
        }
    }
    private GameObject FindNearestCard(GameObject cardGo, List<GameObject> cardsOnField)
    {
        float minDistance = Mathf.Infinity;
        int index = 0;

        for (int i = 0; i < cardsOnField.Count; i++)
        {
            Vector3 cardPos = cardGo.transform.TransformPoint(cardGo.transform.localPosition);
            Vector3 cardOnTablePos = cardsOnField[i].transform.TransformPoint(
                cardsOnField[i].transform.localPosition);
            float distance = Vector2.Distance(cardPos, cardOnTablePos);
            if (distance < minDistance)
            {
                minDistance = distance;
                index = i;
            }
        }

        return cardsOnField[index];
    }
    private void PrepareToAddDefenseCard(GameObject card, GameObject attackCard)
    {
        int cardValue = card.GetComponent<Card>().GetValue();
        if (cardValue == 12)
        {
            _isThereDefenseQueen = true;
        }

        if (_cardsValues.Contains(cardValue) == false)
        {
            _cardsValues.Add(cardValue);
        }

        CardDefended?.Invoke(card);
        if (_isThereDefenseQueen == true)
        {
            LimitingMove?.Invoke();
        }
        AddDefenseCard(card, attackCard);
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

        if (_defenseCardGos.Count == 0)
        {
            if (indentRight == 3)
            {
                indentRight = 0;
                indentDown++;
            }
            _transferSpace.transform.localPosition = new Vector3(indentRight * 2f, indentDown * -3f, 0f);
            _transferSpace.SetActive(true);
        }
    }
}

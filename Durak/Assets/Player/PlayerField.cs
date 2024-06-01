using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerField : MonoBehaviour
{
    public static Action<PlayerField> PlayerDroppedOut;

    [SerializeField]
    private List<GameObject> _shirtGo;

    [SerializeField]
    private SpriteRenderer _activityFieldSpriteRenderer;

    private readonly int _maxHandCardCount = 6;
    private readonly string _handCardsGoName = "Hand cards";
    private readonly string _extraCardsGoName = "Extra cards";

    private bool _isAttack = false;
    private bool _isDefense = false;

    private PlayerButton _playerButton;

    private List<GameObject> _handCardsGos = new();
    private List<GameObject> _extraCardsGos = new();

    private List<Card> _handCards = new();

    private void Awake()
    {
        _playerButton = GetComponentInChildren<PlayerButton>();
    }
    private void OnEnable()
    {
        Draggable.CardAttacked += CardAttackedHandler;
        Playfield.AllCardsDefended += AllCardsDefendedHandler;
        PlayerButton.AttackSkipping += AttackSkippingHandle;
        GameTable.MoveStarted += MoveStartedHandler;
    }
    private void OnDisable()
    {
        Draggable.CardAttacked -= CardAttackedHandler;
        Playfield.AllCardsDefended -= AllCardsDefendedHandler;
        PlayerButton.AttackSkipping -= AttackSkippingHandle;
        GameTable.MoveStarted -= MoveStartedHandler;
    }

    public void SetCardsDraggableHandler(bool value)
    {
        SetCardsDraggable(value);
    }

    public void AddCards(List<GameObject> cardGos)
    {
        for (int i = 0; i < cardGos.Count; i++)
        {
            cardGos[i].SetActive(true);
            cardGos[i].transform.position = Vector3.zero;
            cardGos[i].transform.eulerAngles = new Vector3(0f, 0f, 0f);
            _handCardsGos.Add(cardGos[i]);
            SetHandCardPositions();
        }
    }
    public void DeleteCard(GameObject cardGo)
    {
        _handCardsGos.Remove(cardGo);
        SetHandCardPositions();

        if (_isAttack == true)
        {
            _playerButton.SetStatus(0);
        }
    }
    public List<int> FindMinCardValues()
    {
        List<int> minCardsValues = new() { 15, 15, 15, 15};

        for (int i = 0; i < _handCards.Count; i++)
        {
            //!!!!!!!!!!!найти конвертацию напрямую из char в int!!!!!!!!!!!
            int suitAsInt = Convert.ToInt32(char.GetNumericValue(_handCards[i].GetSuit()));
            if (_handCards[i].GetValue() < minCardsValues[suitAsInt - 1])
            {
                minCardsValues[suitAsInt - 1] = _handCards[i].GetValue();
            }
        }

        return minCardsValues;
    }
    public bool GetAttack()
    {
        return _isAttack;
    }
    public int GetHandCardsCount()
    {
        return _handCardsGos.Count;
    }
    public bool GetDefense()
    {
        return _isDefense;
    }
    public void SetAcrivityField(bool value)
    {
        _activityFieldSpriteRenderer.enabled = value;
    }
    public void SetAttack(bool value)
    {
        SetStatus(out _isAttack, value);
    }
    public void SetCardPositions()
    {
        SetHandCardPositions();
    }
    public void SetDefense(bool value)
    {
        SetStatus(out _isDefense, value);
    }
    public void SetStartingCardPositions(List<GameObject> cardGos)
    {
        for (int i = 0; i < _maxHandCardCount; i++)
        {
            _handCardsGos.Add(cardGos[0]);
            cardGos.RemoveAt(0);
        }
        SetHandCardPositions();

        int count = cardGos.Count;
        for (int i = 0; i < count; i++)
        {
            _extraCardsGos.Add(cardGos[0]);
            cardGos.RemoveAt(0);
        }
        SetExtraCardPositions();
    }

    private void AllCardsDefendedHandler()
    {
        if (_isDefense == true)
        {
            _playerButton.SetStatus(0);
        }
        if (_isAttack == true)
        {
            _playerButton.SetStatus(1);
            SetCardsDraggable(true);
        }
    }
    private void AttackSkippingHandle()
    {
        if (_isAttack == false && _isDefense == false)
        {
            SetAttack(true);
            _playerButton.SetStatus(1);
        }
    }
    private void CardAttackedHandler()
    {
        if (_isDefense == true)
        {
            _playerButton.SetStatus(2);
        }
        if (_isAttack == true)
        {
            _playerButton.SetStatus(0);
        }
    }
    private void MoveStartedHandler()
    {
        if (_handCardsGos.Count == 0 && _extraCardsGos.Count != 0)
        {
            for (int i = 0; i < _extraCardsGos.Count; i++)
            {
                _handCardsGos.Add(_extraCardsGos[i]);
                _handCardsGos[i].SetActive(true);
            }

            _extraCardsGos.Clear();
            SetHandCardPositions();

            for (int i = 0; i < _shirtGo.Count; i++)
            {
                _shirtGo[i].SetActive(false);
            }
        }
        else if (_extraCardsGos.Count == 0)
        {
            PlayerDroppedOut?.Invoke(this);
            gameObject.SetActive(false);
        }
    }

    private void SetCardsDraggable(bool value)
    {
        for (int i = 0; i < _handCards.Count; i++)
        {
            _handCards[i].SetDraggble(value);
        }
    }
    private void SetExtraCardPositions()
    {
        GameObject extraCardsGo = transform.Find(_extraCardsGoName).gameObject;

        for (int i = 0; i < _extraCardsGos.Count; i++)
        {
            _extraCardsGos[i].transform.SetParent(extraCardsGo.transform);
            _extraCardsGos[i].transform.localPosition = Vector3.zero;
            _extraCardsGos[i].transform.localScale = Vector3.one;
            _extraCardsGos[i].SetActive(false);
        }
    }
    private void SetHandCardPositions()
    {
        _handCards.Clear();

        GameObject handCardsGo = transform.Find(_handCardsGoName).gameObject;

        for (int i = 0; i < _handCardsGos.Count; i++)
        {
            _handCardsGos[i].transform.SetParent(handCardsGo.transform);
            _handCards.Add(_handCardsGos[i].GetComponent<Card>());
        }

        int indentRight = 0, indentDown = 0, indentForward = 0;
        for (int i = 0; i < _handCardsGos.Count; i++)
        {
            if (i % 6 == 0 && i != 0)
            {
                indentRight = 0;
                indentDown++;
            }
            _handCardsGos[i].transform.localPosition =
                new Vector3(indentRight * 0.5f, indentDown * -1f, indentForward * -0.01f);
            _handCardsGos[i].transform.localScale = Vector3.one;
            indentRight++;
            indentForward++;
        }
    }
    private void SetStatus(out bool state, bool valueState)
    {
        state = valueState;

        SetCardsDraggable(valueState);
    }
}

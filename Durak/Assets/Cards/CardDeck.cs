using System;
using System.Collections.Generic;
using UnityEngine;

public class CardDeck : MonoBehaviour
{
    public static Action<List<List<GameObject>>> CardsDistributed;

    private readonly int _playerCardCountAtGameBeginning = 8;

    [SerializeField]
    private GameObject _cardPrefab;

    [SerializeField]
    private List<Sprite> _cardSprites;

    private int _playerCount;

    //1 - пики, 2 - черви, 3 - буби, 4 - крести
    //11 - валет, 12 - дама, 13 - король, 14 - туз
    private List<string> _deck = new()
                                { "1_6", "1_7", "1_8", "1_9", "1_10", "1_11", "1_12", "1_13", "1_14",
                                "2_6", "2_7", "2_8", "2_9", "2_10", "2_11", "2_12", "2_13", "2_14",
                                "3_6", "3_7", "3_8", "3_9", "3_10", "3_11", "3_12", "3_13", "3_14",
                                "4_6", "4_7", "4_8", "4_9", "4_10", "4_11", "4_12", "4_13", "4_14"};
    private List<List<GameObject>> _playerCardsGos = new();
    private List<GameObject> _deckOnTableGo = new();

    private void OnEnable()
    {
        GameTable.GameStarted += GameStartedHandler;
    }
    private void OnDisable()
    {
        GameTable.GameStarted -= GameStartedHandler;
    }

    private void GameStartedHandler(int playerCount)
    {
        _playerCount = playerCount;
        DistributeDeck();
    }

    private void DistributeCardsToPlayers()
    {
        for (int i = 0; i < _playerCount; i++)
        {
            _playerCardsGos.Add(new List<GameObject>());
            SetCards(_playerCardsGos[i], _playerCardCountAtGameBeginning);
        }
    }
    private void DistributeDeckOnTable()
    {
        SetCards(_deckOnTableGo, _deck.Count);
    }
    private void DistributeDeck()
    {
        DistributeCardsToPlayers();

        DistributeDeckOnTable();

        List<List<GameObject>> decks = new();
        for (int i = 0; i < _playerCount; i++)
        {
            decks.Add(_playerCardsGos[i]);
        }
        decks.Add(_deckOnTableGo);
        CardsDistributed?.Invoke(decks);
    }
    private void SetCardParameters(Card card, string cardName)
    {
        Sprite sprite = _cardSprites[0];
        for (int i = 0; i < _cardSprites.Count; i++)
        {
            if (_cardSprites[i].name == cardName)
            {
                sprite = _cardSprites[i];
                break;
            }
        }
        card.SetParameters(cardName, sprite);
    }
    private void SetCards(List<GameObject> _playerCardsGos, int count)
    {
        for (int i = 0; i < count; i++)
        {
            int index = UnityEngine.Random.Range(0, _deck.Count);
            GameObject newCardGo = Instantiate(_cardPrefab);
            SetCardParameters(newCardGo.GetComponent<Card>(), _deck[index]);
            _playerCardsGos.Add(newCardGo);
            _deck.RemoveAt(index);
        }
    }
}

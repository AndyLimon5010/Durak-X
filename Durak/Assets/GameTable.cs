using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameTable : MonoBehaviour
{
    public static Action<int> GameStarted;
    public static Action EverybodySkipped;
    public static Action MoveEnded;

    public static char Trump;

    private readonly int _maxHandCardCount = 6;

    [SerializeField]
    private List<GameObject> _playerFieldsGos;
    [SerializeField]
    private GameObject _tableDeckGo;
    [SerializeField]
    private GameObject _playfieldGo;

    //Временно
    [SerializeField]
    private TextMeshProUGUI _trumpText;

    private List<PlayerField> _playerFields = new();
    private TableDeck _tableDeck;
    private Playfield _playfield;

    private readonly int _maxPlayerCount = 4;
    private int _playerCount = 4;
    private int _activePlayer;
    private int _nextPlayer;
    private int _skippingPlayersCount = 0;
    private List<PlayerField> _moveQueue = new();
    private int _playersEndedMoveCount = 0;

    private bool _isFirstTurn = true;
    private int _changeNextPlayerIndex;

    private void Awake()
    {
        _tableDeck = _tableDeckGo.GetComponent<TableDeck>();
        _playfield = _playfieldGo.GetComponent<Playfield>();
    }
    private void Start()
    {
        DisableUnnecessaryPlayers();
        SetPlayerFields();

        GameStarted?.Invoke(_playerCount);
    }
    private void OnEnable()
    {
        CardDeck.CardsDistributed += CardsDistributedHandler;
        PlayerButton.AttackSkipping += AttackSkippingHandler;
        Playfield.CardsTook += CardsTookHandler;
        Playfield.CardAttacked += CardAttackedHandler;
        Playfield.CardDefended += CardDefendedHandler;
        Playfield.AllSeatsOccupied += AllSeatsOccupiedHandler;
        Playfield.CardTransferring += CardTransferringHandler;
        Playfield.TryingCardTransfer += TryingCardTransferHandler;
        PlayerField.PlayerDroppedOut += PlayerDroppedOutHandler;
        PlayerField.PlayerEndedMove += PlayerEndedMoveHandler;
    }
    private void OnDisable()
    {
        CardDeck.CardsDistributed -= CardsDistributedHandler;
        PlayerButton.AttackSkipping -= AttackSkippingHandler;
        Playfield.CardsTook -= CardsTookHandler;
        Playfield.CardAttacked -= CardAttackedHandler;
        Playfield.CardDefended -= CardDefendedHandler;
        Playfield.AllSeatsOccupied -= AllSeatsOccupiedHandler;
        Playfield.CardTransferring -= CardTransferringHandler;
        Playfield.TryingCardTransfer -= TryingCardTransferHandler;
        PlayerField.PlayerDroppedOut -= PlayerDroppedOutHandler;
        PlayerField.PlayerEndedMove -= PlayerEndedMoveHandler;
    }

    private void AllSeatsOccupiedHandler()
    {
        EndMove(1);
    }
    private void AttackSkippingHandler()
    {
        _skippingPlayersCount++;
        if (_skippingPlayersCount == _playerCount - 1)
        {
            _skippingPlayersCount = 0;
            _playfield.DeleteAllCards();
            EndMove(1);
        }
    }
    private void CardAttackedHandler(GameObject cardGo)
    {
        _skippingPlayersCount = 0;

        PlayerField playerField = cardGo.GetComponentInParent<PlayerField>();
        if (!_moveQueue.Contains(playerField))
        {
            _moveQueue.Add(playerField);
        }
    }
    private void CardDefendedHandler(GameObject cardGo)
    {
        _skippingPlayersCount = 0;

        PlayerField playerField = cardGo.GetComponentInParent<PlayerField>();
        if (!_moveQueue.Contains(playerField))
        {
            _moveQueue.Insert(0, playerField);
        }
    }
    private void CardsDistributedHandler(List<List<GameObject>> decks)
    {
        for (int i = 0; i < _playerCount; i++)
        {
            _playerFields[i].SetStartingCardPositions(decks[i]);
        }
        _tableDeck.SetStartingCards(decks[decks.Count - 1]);

        StartMove();
    }
    private void CardTransferringHandler()
    {
        _playerFields[_activePlayer - 1].SetAcrivityField(false);
        SetActivePlayer(1);
        int maxAttackCardCount = _playerFields[_nextPlayer - 1].GetHandCardsCount();
        _playfield.SetMaxAttackCardCount(maxAttackCardCount);
        SetPlayersRoles();
    }
    private void CardsTookHandler()
    {
        EndMove(2);
    }
    private void PlayerDroppedOutHandler(PlayerField playerField)
    {
        int playerIndex = _playerFields.IndexOf(playerField);
        _playerFields.Remove(playerField);

        if (_activePlayer == playerIndex + 1)
        {
            _activePlayer--;
            if (_activePlayer == 0)
            {
                _activePlayer = _playerFields.Count;
            }
        }
        else if (_activePlayer < playerIndex + 1)
        {
            _activePlayer -= 2;

            if (_activePlayer == 0)
            {
                _activePlayer = _playerFields.Count;
            }
            else if (_activePlayer == -1)
            {
                _activePlayer = _playerFields.Count - 1;
            }
        }
        
        if (_playerFields.Count == 1)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
    private void PlayerEndedMoveHandler()
    {
        _playersEndedMoveCount++;
        if (_playersEndedMoveCount == _playerCount)
        {
            _playerCount = _playerFields.Count;
            _playersEndedMoveCount = 0;
            StartMove();
        }
    }
    private bool TryingCardTransferHandler(int cardCountOnTable)
    {
        int possibleNextPlayer = _nextPlayer + 1;
        if (possibleNextPlayer == 5)
        {
            possibleNextPlayer = 1;
        }
        int maxAttackCardCount = _playerFields[possibleNextPlayer - 1].GetHandCardsCount();
        if (cardCountOnTable < maxAttackCardCount)
        {
            return true;
        }

        return false;
    }

    private void DisableUnnecessaryPlayers()
    {
        for (int i = _playerCount; i < _maxPlayerCount; i++)
        {
            _playerFieldsGos[i].SetActive(false);
        }
    }
    private void DistributeNewCardsToPlayers()
    {
        List<GameObject> cards = _tableDeck.GetAllCards();
        GameObject lastCard = null;
        if (cards.Count != 0)
        {
            lastCard = cards[^1];
        }

        if (cards.Count > 0)
        {
            for (int i = 1; i < _moveQueue.Count; i++)
            {
                if (cards.Count == 0)
                {
                    break;
                }

                int needCardCount = _maxHandCardCount - _moveQueue[i].GetHandCardsCount();
                if (needCardCount <= 0)
                {
                    break;
                }
                List<GameObject> cardsToPlayer = new();
                for (int j = 0; j < needCardCount; j++)
                {
                    if (cards.Count == 0)
                    {
                        break;
                    }

                    cardsToPlayer.Add(cards[0]);
                    cards.RemoveAt(0);
                }

                _moveQueue[i].AddCards(cardsToPlayer);
            }
        }

        if (cards.Count > 0)
        {
            int needCardCount = _maxHandCardCount - _moveQueue[0].GetHandCardsCount();
            List<GameObject> cardsToPlayer = new();
            for (int i = 0; i < needCardCount; i++)
            {
                if (cards.Count == 0)
                {
                    break;
                }

                cardsToPlayer.Add(cards[0]);
                cards.RemoveAt(0);
            }

            _moveQueue[0].AddCards(cardsToPlayer);
        }

        if (cards.Count == 0 && lastCard != null)
        {
            Trump = lastCard.GetComponent<Card>().GetSuit();
            SetTrump();
            if (Trump == '1')
            {
                Trump = '0';
            }
        }

        _tableDeck.UpdateDeck(cards);
        _moveQueue.Clear();

    }
    private void EndMove(int changeNextPlayerIndex)
    {
        _changeNextPlayerIndex = changeNextPlayerIndex;
        DistributeNewCardsToPlayers();
        _playerFields[_activePlayer - 1].SetAcrivityField(false);

        MoveEnded?.Invoke();
    }
    private int SearchPlayerWithMinCard(char suit, List<List<int>> playerMinCards)
    {
        int playerNumber = 0;
        int minValue = 15;
        //!!!!!!!!!!!найти конвертацию напрямую из char в int!!!!!!!!!!!
        int suitAsInt = Convert.ToInt32(char.GetNumericValue(suit));

        for (int j = 0; j < _playerCount; j++)
        {
            if (playerMinCards[j][suitAsInt - 1] < minValue)
            {
                minValue = playerMinCards[j][suitAsInt - 1];
                playerNumber = j + 1;
            }
        }

        return playerNumber;
    }
    private void SetActivePlayer(int changeIndex)
    {
        _activePlayer += changeIndex;
        if (_activePlayer == _playerCount + 1)
        {
            _activePlayer = 1;
        }
        else if (_activePlayer == _playerCount + 2)
        {
            _activePlayer = 2;
        }

        SetNextPlayer();
    }
    private void SetNextPlayer()
    {
        _nextPlayer = _activePlayer + 1;
        if (_nextPlayer > _playerCount)
        {
            _nextPlayer = 1;
        }
    }
    private void SetPlayerFields()
    {
        for (int i = 0; i < _playerCount; i++)
        {
            _playerFields.Add(_playerFieldsGos[i].GetComponent<PlayerField>());
        }
    }
    private void SetPlayersRoles()
    {
        _playerFields[_activePlayer - 1].SetAcrivityField(true);
        for (int i = 0; i < _playerCount; i++)
        {
            _playerFields[i].SetAttack(false);
            _playerFields[i].SetDefense(false);
        }

        _playerFields[_activePlayer - 1].SetAttack(true);
        _playerFields[_nextPlayer - 1].SetDefense(true);
    }
    private void SetPlayerQueue()
    {
        Trump = _tableDeck.GetTrump();
        SetTrump();

        List<List<int>> playerMinCards = new();
        for (int i = 0; i < _playerCount; i++)
        {
            playerMinCards.Add(_playerFields[i].FindMinCardValues());
        }

        int playerNumber = SearchPlayerWithMinCard(Trump, playerMinCards);

        if (playerNumber == 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (Trump == '1')
        {
            Trump = '0';
        }
        _activePlayer = playerNumber;
        SetNextPlayer();
    }
    private void SetTrump()
    {
        if (Trump == '1')
        {
            _trumpText.text = "Пики";
        }
        else if (Trump == '2')
        {
            _trumpText.text = "Черви";
        }
        else if (Trump == '3')
        {
            _trumpText.text = "Буби";
        }
        else if (Trump == '4')
        {
            _trumpText.text = "Крести";
        }
    }
    private void StartMove()
    {
        if (_isFirstTurn == true)
        {
            _isFirstTurn = false;
            SetPlayerQueue();
            _playfield.SetMaxAttackCardCount(5);
        }
        else
        {
            SetActivePlayer(_changeNextPlayerIndex);
            int maxAttackCardCount = _playerFields[_nextPlayer - 1].GetHandCardsCount();
            _playfield.SetMaxAttackCardCount(maxAttackCardCount);
        }

        SetPlayersRoles();
    }
}

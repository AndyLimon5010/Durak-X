using System.Collections.Generic;
using UnityEngine;

public class TableDeck : MonoBehaviour
{
    [SerializeField]
    private GameObject _cardShirt;
    [SerializeField]
    private GameObject _secretTrumpShirt;

    private readonly string _trumpCardsGoName = "TrumpCards";
    private readonly string _faceDownCardsGoName = "FaceDownCards";

    private List<GameObject> _faceDownCardsGos = new();
    private List<GameObject> _trumpCardsGos = new();

    public List<GameObject> GetAllCards()
    {
        List<GameObject> cards = new();
        for (int i = 0; i < _faceDownCardsGos.Count; i++)
        {
            cards.Add(_faceDownCardsGos[i]);
        }
        for (int i = 0; i < _trumpCardsGos.Count; i++)
        {
            cards.Add(_trumpCardsGos[i]);
        }

        return cards;
    }
    public char GetTrump()
    {
        return _trumpCardsGos[0].GetComponent<Card>().GetSuit();
    }
    public void SetStartingCards(List<GameObject> cardGos)
    {
        int count = cardGos.Count;
        for (int i = 0; i < count - 2; i++)
        {
            _faceDownCardsGos.Add(cardGos[0]);
            cardGos.RemoveAt(0);
        }
        SetFaceDownCardPositions();

        count = cardGos.Count;
        for (int i = 0; i < count; i++)
        {
            _trumpCardsGos.Add(cardGos[0]);
            cardGos.RemoveAt(0);
        }
        SetTrumpCardsPosition();
    }
    public void UpdateDeck(List<GameObject> cards)
    {
        if (cards.Count <= 2)
        {
            _cardShirt.SetActive(false);
        }
        if (cards.Count == 0)
        {
            _secretTrumpShirt.SetActive(false);
        }

        _faceDownCardsGos.Clear();
        _trumpCardsGos.Clear();

        if (cards.Count > 0)
        {
            for (int i = 0; i < 2; i++)
            {
                if (cards.Count == 0)
                {
                    break;
                }
                _trumpCardsGos.Insert(0, cards[cards.Count - 1]);
                cards.RemoveAt(cards.Count - 1);
            }
        }

        for (int i = 0; i < cards.Count; i++)
        {
            _faceDownCardsGos.Add(cards[i]);
        }
    }

    private void SetFaceDownCardPositions()
    {
        GameObject faceDownCardsGo = transform.Find(_faceDownCardsGoName).gameObject;

        for (int i = 0; i < _faceDownCardsGos.Count; i++)
        {
            _faceDownCardsGos[i].transform.SetParent(faceDownCardsGo.transform);
            _faceDownCardsGos[i].transform.localPosition = Vector3.zero;
            _faceDownCardsGos[i].transform.localScale = Vector3.one;
            _faceDownCardsGos[i].SetActive(false);
        }
    }
    private void SetTrumpCardsPosition()
    {
        GameObject trumpCardsGo = transform.Find(_trumpCardsGoName).gameObject;

        _trumpCardsGos[0].transform.SetParent(trumpCardsGo.transform);
        _trumpCardsGos[0].transform.localPosition = new Vector3(0.5f, 0f, 0f);
        _trumpCardsGos[0].transform.eulerAngles = new Vector3(0f, 0f, -90f);
        _trumpCardsGos[0].transform.localScale = Vector3.one;

        _trumpCardsGos[1].transform.SetParent(trumpCardsGo.transform);
        _trumpCardsGos[1].transform.localPosition = Vector3.zero;
        _trumpCardsGos[1].transform.localScale = Vector3.one;
        _trumpCardsGos[1].SetActive(false);
    }
}

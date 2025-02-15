using UnityEngine;

public class StateController : MonoBehaviour
{
    public State CurrentState { get; private set; }

    [SerializeField]
    private GameObject _titleUi;

    [SerializeField]
    private GameObject _fieldUi;

    [SerializeField]
    private GameObject _battleUi;

    [SerializeField]
    private GameObject _resultUi;

    public void ChangeState(State state)
    {
        CurrentState = state;

        _titleUi.SetActive(false);
        _fieldUi.SetActive(false);
        _battleUi.SetActive(false);
        _resultUi.SetActive(false);

        switch (state)
        {
            case State.Title:
                InitializeTitle();
                break;
            case State.Field:
                InitializeField();
                break;
            case State.Battle:
                InitializeBattle();
                break;
            case State.Result:
                InitializeResult();
                break;
        }
    }

    private void InitializeTitle()
    {
        _titleUi.gameObject.SetActive(true);
        Debug.Log("InitializeTitle");
    }

    private void InitializeField()
    {
        _fieldUi.gameObject.SetActive(true);
        Debug.Log("InitializeGame");
    }
    private void InitializeBattle()
    {
        _battleUi.gameObject.SetActive(true);
        Debug.Log("InitializeGame");
    }

    private void InitializeResult()
    {
        _resultUi.gameObject.SetActive(true);
        Debug.Log("InitializeResult");
    }
}

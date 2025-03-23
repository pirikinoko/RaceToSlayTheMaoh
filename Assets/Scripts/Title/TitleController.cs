using UnityEngine;
using UnityEngine.UIElements;

public class TitleController : MonoBehaviour
{
    [SerializeField]
    private StateController _stateContoller;

    private void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        root.Q<Button>("Button-Start_Local").clicked += StartLocalGame;
        root.Q<Button>("Button-Start_Online").clicked += StartOnlineGame;
    }

    private void StartLocalGame()
    {
        _stateContoller.ChangeState(State.Field);
    }

    private void StartOnlineGame()
    {
        _stateContoller.ChangeState(State.Field);
    }
}

using UnityEngine;
using UnityEngine.UIElements;

public class GameBootstrapper : MonoBehaviour
{
    [SerializeField] private MainController _mainController;
    [SerializeField] private StateController _stateController;
    [SerializeField] private FieldController _fieldController;
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private EnemyController _enemyController;
    [SerializeField] private UserController _userController;
    [SerializeField] private BattleController _battleController;
    [SerializeField] private BattleLogController _battleLogController;
    [SerializeField] private CameraController _cameraController;
    [SerializeField] private ImageAnimationHolder _imageAnimationHolder;
    [SerializeField] private UIDocument _overAllUi;
    [SerializeField] private UIDocument _titleUi;
    [SerializeField] private UIDocument _fieldUi;
    [SerializeField] private UIDocument _battleUi;
    [SerializeField] private UIDocument _resultUi;

    private void Awake()
    {
        _mainController.Initialize(_userController, _fieldController, _cameraController, _stateController, _playerController, _enemyController);
        _stateController.Initialize(_mainController, _fieldController, _playerController, _cameraController, _overAllUi, _titleUi, _fieldUi, _battleUi, _resultUi);
        _fieldController.Initialize(_mainController, _stateController, _enemyController, _playerController, _battleController);
        _playerController.Initialize(_mainController);
        _battleController.Initialize(_stateController, _mainController, _userController, _battleLogController, _playerController, _enemyController, _imageAnimationHolder);
        // 必要に応じて他のControllerも同様にInitializeを呼び出してください
    }
}

using UnityEngine;
using UnityEngine.UIElements;
using VContainer;
using VContainer.Unity;

public class GameBootstrapper : LifetimeScope
{
    [SerializeField] private TitleController _titleController;
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
    [SerializeField] private TitleTextData _titleTextData;
    [SerializeField] private UIDocument _overAllUi;
    [SerializeField] private UIDocument _titleUi;
    [SerializeField] private UIDocument _fieldUi;
    [SerializeField] private UIDocument _battleUi;
    [SerializeField] private UIDocument _resultUi;

    protected override void Configure(IContainerBuilder builder)
    {
        // 各種ControllerやUIをインスタンスとして登録
        builder.RegisterInstance(_titleController);
        builder.RegisterInstance(_mainController);
        builder.RegisterInstance(_stateController);
        builder.RegisterInstance(_fieldController);
        builder.RegisterInstance(_playerController);
        builder.RegisterInstance(_enemyController);
        builder.RegisterInstance(_userController);
        builder.RegisterInstance(_battleController);
        builder.RegisterInstance(_battleLogController);
        builder.RegisterInstance(_cameraController);
        builder.RegisterInstance(_imageAnimationHolder);
        builder.RegisterInstance(_titleTextData);
        builder.RegisterInstance(_overAllUi);
        builder.RegisterInstance(_titleUi);
        builder.RegisterInstance(_fieldUi);
        builder.RegisterInstance(_battleUi);
        builder.RegisterInstance(_resultUi);
    }
}
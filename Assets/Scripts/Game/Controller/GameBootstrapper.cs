using BossSlayingTourney.Data;
using BossSlayingTourney.Game.Battle;
using BossSlayingTourney.Game.Effects;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;
using VContainer.Unity;

namespace BossSlayingTourney.Game.Controllers
{

    public class GameBootstrapper : LifetimeScope
    {
        #region Fields - Controllers
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
        #endregion

        #region Fields - Data & UI
        [SerializeField] private ImageAnimationHolder _imageAnimationHolder;
        [SerializeField] private UIDocument _overAllUi;
        [SerializeField] private UIDocument _titleUi;
        [SerializeField] private UIDocument _fieldUi;
        [SerializeField] private UIDocument _battleUi;
        [SerializeField] private UIDocument _resultUi;
        #endregion

        protected override void Configure(IContainerBuilder builder)
        {
            // 各種ControllerやUIをインスタンスとして登録
            RegisterControllers(builder);
            RegisterDataAndUI(builder);
            RegisterUtilities(builder);
        }

        private void RegisterControllers(IContainerBuilder builder)
        {
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
        }

        private void RegisterDataAndUI(IContainerBuilder builder)
        {
            builder.RegisterInstance(_imageAnimationHolder);

            // TitleTextDataは新しいインスタンスとして登録
            builder.Register<TitleTextData>(Lifetime.Singleton);

            builder.RegisterInstance(_overAllUi);
            builder.RegisterInstance(_titleUi);
            builder.RegisterInstance(_fieldUi);
            builder.RegisterInstance(_battleUi);
            builder.RegisterInstance(_resultUi);
        }

        private void RegisterUtilities(IContainerBuilder builder)
        {
            builder.Register<RewardSelecter>(Lifetime.Singleton);
        }
    }
}
using BossSlayingTourney.Core;
using BossSlayingTourney.Network;
using R3;
using UnityEngine;
using VContainer;

namespace BossSlayingTourney.Game.Controllers
{

    public class UserController : MonoBehaviour
    {
        private NetworkManager _networkManager;
        private PlayerController _playerController;

        public int Id;

        public Entity MyEntity;

        [Inject]
        public void Construct(PlayerController playerController)
        {
            _playerController = playerController;
            _networkManager = NetworkManager.Instance;
            Id = _networkManager.GetPlayerId(_networkManager.GetNetworkRunner().LocalPlayer);
        }

        void Start()
        {
            _playerController.OnPlayersInitialized.Subscribe(playerList =>
            {
                MyEntity = playerList.Find(p => p.Id == Id);
            });
        }
    }
}
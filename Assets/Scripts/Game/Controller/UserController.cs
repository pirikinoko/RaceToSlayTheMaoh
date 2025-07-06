using R3;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class UserController : MonoBehaviour
{
    private NetworkManager _networkManager;
    private PlayerController _playerController;

    public int Id;

    public Entity MyEntity;

    void Start()
    {
        _playerController.OnPlayersInitialized.Subscribe(playerList =>
        {
            GetMyEntity(playerList);
        });
    }

    [Inject]
    public void Construct(PlayerController playerController)
    {
        _playerController = playerController;
        _networkManager = NetworkManager.Instance;
        Id = _networkManager.GetPlayerId(_networkManager.GetNetworkRunner().LocalPlayer);
    }

    private void GetMyEntity(List<Entity> playerList)
    {
        MyEntity = playerList.Find(p => p.Id == Id);
    }
}

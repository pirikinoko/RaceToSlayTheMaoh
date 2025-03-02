using R3;
using System.Collections.Generic;
using UnityEngine;

public class UserController : MonoBehaviour
{
    [SerializeField]
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

    private void GetMyEntity(List<Entity> playerList)
    {
        MyEntity = playerList.Find(p => p.Id == Id);
    }
}

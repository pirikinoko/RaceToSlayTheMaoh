using Cysharp.Threading.Tasks;
using UnityEngine;

public class Player : MonoBehaviour
{
    // 移動関連
    [SerializeField]
    private StateController _stateController;

    private Transform _transform;

    private bool _isReadyToMove;

    private bool _isMoving;

    // ステータス関連
    private Parameter _parameter;

    async void Start()
    {
        _transform = GetComponent<Transform>();
    }

    private void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        if (!_isReadyToMove)
        {
            _isReadyToMove = !_isMoving && x == 0 && y == 0;
            return;
        }

        if (_stateController.CurrentState == State.Field)
        {
            if (x != 0)
            {
                y = 0;
            }

            if (x == 0 && y == 0)
            {
                return;
            }

            MoveAsync(new Vector2(x, y)).Forget();
            _isReadyToMove = false;
        }
    }

    private async UniTask MoveAsync(Vector2 direction)
    {
        _isMoving = true;

        var nextPosition = _transform.position + new Vector3(direction.x, direction.y, 0);

        await UniTask.WaitUntil(() =>
        {
            _transform.position = Vector3.MoveTowards(_transform.position, nextPosition, Constants.PlayerMoveSpeed * Time.deltaTime);
            return _transform.position == nextPosition;
        });

        _isMoving = false;
    }

    public void SetParameter(Parameter parameter)
    {
        _parameter = parameter;
    }

    private void ReceiveDamage(int damage)
    {
        _parameter.Hp -= damage;
    }
}

using Cysharp.Threading.Tasks;
using UnityEngine;

public class ControllableCharacter : MonoBehaviour
{
    private StateController _stateController;

    private Transform _transform;

    private bool _isReadyToMove = true;

    private bool _isMoving;

    private void Start()
    {
        _transform = GetComponent<Transform>();
        _stateController = FindFirstObjectByType<StateController>();
    }

    private void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        // 押しっぱなしで移動できないように
        if (!_isReadyToMove)
        {
            //　移動が終わって,かつキー入力がない場合は移動可能にする
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

            var InputedDirection = new Vector2(x, y);

            // 移動先に障害物がないか確認する
            if (!CheckInputedDirectionMovable(InputedDirection))
            {
                return;
            }

            MoveAsync(InputedDirection).Forget();
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

    /// <summary>
    /// Rayを飛ばして、移動先に障害物がないか確認する
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    private bool CheckInputedDirectionMovable(Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(_transform.position, direction, 1f);
        return hit.collider == null && direction.magnitude > 0;
    }
}

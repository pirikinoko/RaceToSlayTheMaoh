using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

public class ControllableCharacter : MonoBehaviour
{
    private MainContrller _mainController;
    private FieldController _fieldController;

    private Transform _transform;

    private bool _isReadyToMove = true;

    private bool _isMoving;

    private int _remainingMoves = 0;

    private void Start()
    {
        _transform = GetComponent<Transform>();
        _fieldController = FindFirstObjectByType<FieldController>();
        _mainController = FindFirstObjectByType<MainContrller>();
    }

    private void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        if (_remainingMoves <= 0)
        {
            return;
        }

        // 押しっぱなしで移動できないように
        if (!_isReadyToMove)
        {
            //　移動が終わって,かつキー入力がない場合は移動可能にする
            _isReadyToMove = !_isMoving && x == 0 && y == 0;
            return;
        }

        // 縦横同時押しの時は、優先度をつける
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
        _remainingMoves--;

        // エンカウントチェック
        if (_fieldController.CheckEncount(gameObject.GetComponent<Entity>()))
        {
            return;
        }

        // 戦闘が発生した場合は,次のターン開始処理はStateControllerに任せる
        // ここでは戦闘が発生しなかった場合の次のターン開始処理を行う
        if (_remainingMoves <= 0)
        {
            _mainController.StartNewTurnAsync().Forget();
        }
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

    public void SetMoves(int moves)
    {
        _remainingMoves = moves;
    }
    public int GetMoves()
    {
        return _remainingMoves;
    }
}

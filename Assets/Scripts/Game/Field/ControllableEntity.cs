using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class ControllableEntity : MonoBehaviour
{
    private MainController _mainController;
    private FieldController _fieldController;
    private PlayerController _playerController;
    private EnemyController _enemyController;

    private Transform _transform;

    private bool _isReadyToMove = true;

    private bool _isMoving;

    private int _remainingMoves = 0;

    private void Start()
    {
        _transform = GetComponent<Transform>();
        _fieldController = FindFirstObjectByType<FieldController>();
        _mainController = FindFirstObjectByType<MainController>();
        _playerController = FindFirstObjectByType<PlayerController>();
        _enemyController = FindFirstObjectByType<EnemyController>();
    }


    /// <summary>
    /// クリックで移動する機能を有効にする
    /// </summary>
    public void EnableClickMovement()
    {
        ShowMovablePositions();
    }

    /// <summary>
    /// 移動可能な位置を強調表示する（BFSで四方向から一マスずつ探索）
    /// </summary>
    private void ShowMovablePositions()
    {
        if (_remainingMoves <= 0)
        {
            return;
        }

        // BFSのためのキューと訪問済みマスの記録
        Queue<(Vector2 position, int movesLeft, Queue<Vector2> path)> queue = new Queue<(Vector2, int, Queue<Vector2>)>();
        HashSet<Vector2> visited = new HashSet<Vector2>();

        // 現在位置をキューに追加
        Vector2 startPos = _transform.position;
        queue.Enqueue((startPos, _remainingMoves, new Queue<Vector2>()));
        visited.Add(startPos);

        while (queue.Count > 0)
        {
            var (currentPos, movesLeft, path) = queue.Dequeue();

            // 移動力を使い切ったら、そのパスはこれ以上探索しない
            if (movesLeft <= 0)
            {
                continue;
            }

            // 四方向を探索
            Vector2[] directions = {
                Vector2.up,
                Vector2.right,
                Vector2.down,
                Vector2.left
            };

            foreach (var dir in directions)
            {
                Vector2 nextPos = currentPos + dir;

                // 既に訪問済みならスキップ
                if (visited.Contains(nextPos))
                {
                    continue;
                }

                // 移動可能かチェック（現在地点から直接Raycastするのではなく、隣接するマスごとにチェック）
                if (CheckAdjacentPositionMovable(currentPos, nextPos))
                {
                    // 訪問済みとしてマーク
                    visited.Add(nextPos);

                    // 移動可能な位置へのパスを記録
                    var newPath = new Queue<Vector2>(path);
                    newPath.Enqueue(nextPos);


                    if (CheckEnemyInthePosition(nextPos))
                    {
                        // ハイライトを生成（敵のマスなので赤色)
                        CreateHighlight(nextPos, newPath, Color.red).Forget();
                        // 敵がいるマスを越えて移動できないのでこれ以上は探索しない
                        continue;
                    }

                    // ハイライトを生成
                    CreateHighlight(nextPos, newPath, Color.white).Forget();

                    // 次の探索のためにキューに追加（移動力を1減らす）
                    queue.Enqueue((nextPos, movesLeft - 1, newPath));
                }
            }
        }
    }

    /// <summary>
    /// 隣接するマス間の移動が可能かチェックする
    /// </summary>
    private bool CheckAdjacentPositionMovable(Vector2 fromPos, Vector2 toPos)
    {
        // 隣接するマス間の障害物をチェック
        Vector2 direction = toPos - fromPos;

        RaycastHit2D hit = Physics2D.Raycast(fromPos, direction, direction.magnitude);
        bool isMovable = hit.collider == null;

        return isMovable;
    }

    private bool CheckEnemyInthePosition(Vector2 position)
    {
        // 敵がいるかどうかをチェック
        var allEntity = new List<Entity>();
        allEntity.AddRange(_playerController.PlayerList);
        allEntity.AddRange(_enemyController.EnemyList);


        foreach (var entity in allEntity)
        {
            if (entity == this.gameObject.GetComponent<Entity>())
            {
                continue;
            }
            if (Vector2.Distance(position, entity.transform.position) < 0.1f)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 移動可能な位置にハイライトオブジェクトを作成する
    /// </summary>
    private async UniTask CreateHighlight(Vector2 position, Queue<Vector2> path, Color color)
    {
        var operation = Addressables.InstantiateAsync("MoveHighlight");
        var highlight = await operation;

        highlight.transform.position = new Vector3(position.x, position.y, 0);

        // クリックハンドラーの追加
        MoveHighlight moveHighlight = highlight.GetComponent<MoveHighlight>();
        moveHighlight.Initialize(this, path, color);
    }

    /// <summary>
    /// 指定された経路を順にたどって移動する
    /// </summary>
    public async UniTask TracePathAsync(Queue<Vector2> path)
    {
        // 強調表示を全て削除
        ClearAllHighlights();

        var hasEncounted = false;
        while (path.Count > 0)
        {
            Vector2 nextPos = path.Dequeue();
            await MoveAsync(nextPos);
            if (_fieldController.CheckEncount(gameObject.GetComponent<Entity>()))
            {
                hasEncounted = true;
                break;
            }
        }

        // 移動完了後、残りがあればまた移動可能マスを表示
        if (!hasEncounted)
        {
            _mainController.StartNewTurnAsync().Forget();
        }
    }

    /// <summary>
    /// 一歩ずつ進む移動処理
    /// </summary>
    private async UniTask MoveAsync(Vector2 targetPos)
    {
        _isMoving = true;

        Vector2 currentPos = _transform.position;
        Vector2 direction = (targetPos - currentPos).normalized;

        // 移動
        await UniTask.WaitUntil(() =>
        {
            _transform.position = Vector3.MoveTowards(_transform.position, targetPos, Constants.PlayerMoveSpeed * Time.deltaTime);
            return Vector3.Distance(_transform.position, targetPos) < 0.01f;
        });

        _remainingMoves--;
        _isMoving = false;
    }

    /// <summary>
    /// 全てのハイライトを削除する
    /// </summary>
    private void ClearAllHighlights()
    {
        MoveHighlight[] highlights = Object.FindObjectsByType<MoveHighlight>(FindObjectsSortMode.None);

        foreach (MoveHighlight highlight in highlights)
        {
            Destroy(highlight.gameObject);
        }
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

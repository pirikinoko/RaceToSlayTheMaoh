using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BossSlayingTourney.Core;
using BossSlayingTourney.Game.Controllers;
using Cysharp.Threading.Tasks;
using Fusion;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

namespace BossSlayingTourney.Game.Field
{

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

        private Animator _animator;

        private void Start()
        {
            _transform = GetComponent<Transform>();
            _animator = GetComponent<Animator>();
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
            allEntity.AddRange(_playerController.SyncedPlayerList);
            allEntity.AddRange(_enemyController.EnemyList);


            foreach (var entity in allEntity)
            {
                if (entity == this.gameObject.GetComponent<Entity>() || !entity.IsAlive)
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

            if (GetComponent<Entity>().IsNpc)
            {
                moveHighlight.GetComponent<BoxCollider2D>().enabled = false;
            }
        }

        /// <summary>
        /// 指定された経路を順にたどって移動する
        /// </summary>
        [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
        public async UniTask Rpc_TracePathAsync(Queue<Vector2> path)
        {
            // 強調表示を全て削除
            ClearAllHighlights();

            var hasEncounted = false;
            while (path.Count > 0 && _remainingMoves > 0)
            {
                Vector2 nextPos = path.Dequeue();
                // 負けた場合棺桶を配置するので、移動前に位置を保存
                var previousPos = _transform.position;
                await MoveAsync(nextPos);
                if (_fieldController.Rpc_CheckEncount(gameObject.GetComponent<Entity>(), previousPos))
                {
                    hasEncounted = true;
                    break;
                }
            }

            // 移動完了後、残りがあればまた移動可能マスを表示
            if (!hasEncounted)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(Constants.DelayBeforeNewTurnSeconds));
                _mainController.NewTurnProcess();
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

            _animator.SetBool("IsMoving", true);
            // 移動
            await UniTask.WaitUntil(() =>
            {
                _transform.position = Vector3.MoveTowards(_transform.position, targetPos, Constants.PlayerMoveSpeed * Time.deltaTime);
                return Vector3.Distance(_transform.position, targetPos) < 0.01f;
            });
            _animator.SetBool("IsMoving", false);

            _remainingMoves--;
            _isMoving = false;
        }

        /// <summary>
        /// 全てのハイライトを削除する
        /// </summary>
        private void ClearAllHighlights()
        {
            MoveHighlight[] highlights = UnityEngine.Object.FindObjectsByType<MoveHighlight>(FindObjectsSortMode.None);

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

        public void MoveNpc(bool includePlayersAsTarget)
        {
            (Vector2? nearestGoal, Queue<Vector2> bestPath) = FindNearestEntityPath(includePlayersAsTarget);
            if (nearestGoal != null && bestPath != null && bestPath.Count > 0)
            {
                Rpc_TracePathAsync(bestPath).Forget();
            }
        }

        /// <summary>
        /// /// 一番少ない移動数で済むEntityに向かって移動する
        /// </summary>
        /// <returns></returns>
        /// /// <param name="includePlayersAsTarget">プレイヤーもターゲットに含めるか,プレイヤー同士の戦いになりすぎるため</param>
        public (Vector2? nearestGoal, Queue<Vector2> bestPath) FindNearestEntityPath(bool includePlayersAsTarget)
        {
            Vector2 startPos = _transform.position;

            // ゴール候補（Entityの位置）をHashSetで取得
            var entityPositions = new HashSet<Vector2>();

            foreach (var enemy in _enemyController.EnemyList)
            {
                if (enemy != this.GetComponent<Entity>() && Vector2.Distance(enemy.transform.position, _transform.position) > 0.1f && enemy.IsAlive)
                    entityPositions.Add((Vector2)enemy.transform.position);
            }

            foreach (var player in _playerController.SyncedPlayerList)
            {
                if (includePlayersAsTarget && player != this.GetComponent<Entity>() && Vector2.Distance(player.transform.position, _transform.position) > 0.1f && player.IsAlive)
                    entityPositions.Add((Vector2)player.transform.position);
            }


            // BFS用キュー: (現在地, 経路)
            var queue = new Queue<(Vector2 pos, Queue<Vector2> path)>();
            var visited = new HashSet<Vector2>();
            queue.Enqueue((startPos, new Queue<Vector2>()));
            visited.Add(startPos);

            Vector2? nearestGoal = null;
            Queue<Vector2> bestPath = null;
            int minStep = int.MaxValue;

            while (queue.Count > 0)
            {
                var (current, path) = queue.Dequeue();

                // すでに最短経路より長い場合はスキップ
                if (path.Count >= minStep)
                {
                    continue;
                }

                // ゴール判定: Entityの位置に到達したら
                if (entityPositions.Any(pos => Vector2.Distance(current, pos) < 0.1f))
                {
                    if (path.Count < minStep)
                    {
                        minStep = path.Count;
                        nearestGoal = current;
                        bestPath = new Queue<Vector2>(path);
                    }
                }

                // 4方向探索
                Vector2[] directions = { Vector2.up, Vector2.right, Vector2.down, Vector2.left };
                foreach (var dir in directions)
                {
                    Vector2 next = current + dir;
                    if (visited.Contains(next)) continue;

                    // 壁やマップ外チェック：Raycastで壁コライダーがあるか
                    RaycastHit2D hit = Physics2D.Raycast(current, dir, 1f);
                    if (hit.collider != null)
                    {
                        continue;
                    }

                    var newPath = new Queue<Vector2>(path);
                    newPath.Enqueue(next);
                    queue.Enqueue((next, newPath));
                    visited.Add(next);
                }
            }
            return (nearestGoal, bestPath);
        }
    }
}
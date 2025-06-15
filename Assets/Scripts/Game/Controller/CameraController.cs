using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Camera _camera;

    private float fixedZ = -10f;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        fixedZ = _camera.transform.position.z;
    }

    /// <summary>
    /// 指定された2D位置にカメラを移動します。
    /// </summary>
    /// <param name="targetPosition">移動先の2D位置(x, y)</param>
    public async UniTask MoveCameraAsync(Vector2 targetPosition)
    {
        Vector3 endPosition = new Vector3(targetPosition.x, targetPosition.y, fixedZ);
        await _camera.transform.DOMove(endPosition, Constants.CameraMoveDuration).AsyncWaitForCompletion();
    }

    public async UniTask ZoomInAsync(Vector2 targetPosition)
    {
        Vector3 endPosition = new Vector3(targetPosition.x, targetPosition.y, fixedZ);
        await _camera.transform.DOMove(endPosition, Constants.CameraZoomDuration).AsyncWaitForCompletion();
        await _camera.DOOrthoSize(_camera.orthographicSize * Constants.CameraZoomFactor, Constants.CameraZoomDuration).AsyncWaitForCompletion();
    }
}

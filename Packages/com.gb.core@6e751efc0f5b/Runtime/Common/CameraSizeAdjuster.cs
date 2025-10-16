using UnityEngine;

namespace GB
{
    public class CameraSizeAdjuster : MonoBehaviour
    {
        Camera _camera;
        Vector2Int _screenSz;
        void Start()
        {
            _camera = GetComponent<Camera>();
            AdjustCameraSize();
            
        }

        void Update()
        {
            // 해상도 또는 월드 크기가 변경되었는지 확인
            if (Screen.width != _screenSz.x || Screen.height != _screenSz.y)
            {
                AdjustCameraSize();
            }
        }

        void AdjustCameraSize()
        {
            if(_camera == null) return;

            _screenSz = new Vector2Int(Screen.width,Screen.height);

            // 현재 해상도 비율 계산
            float aspectRatio = (float)Screen.width / Screen.height;

            // 목표 월드 크기
            float targetWidth = worldSize.x;
            float targetHeight = worldSize.y;

            // 현재 카메라가 보여주는 월드 영역 계산
            float currentWidth = _camera.orthographicSize * 2 * aspectRatio;
            float currentHeight = _camera.orthographicSize * 2;

            // Width 또는 Height 중 더 큰 비율에 맞춰 카메라 size 조절
            if (currentWidth / targetWidth > currentHeight / targetHeight)
            {
                _camera.orthographicSize = targetHeight / 2;
            }
            else
            {
                _camera.orthographicSize = targetWidth / 2 / aspectRatio;
            }
        }

        // 현재 카메라가 보여주는 월드 크기 반환
        // Vector2 GetCurrentWorldSize()
        // {
        //     float currentWidth = _camera.orthographicSize * 2 * (float)Screen.width / Screen.height;
        //     float currentHeight = _camera.orthographicSize * 2;
        //     return new Vector2(currentWidth, currentHeight);
        // }

        public Vector2 worldSize = new Vector2(7.07f, 12.49f); // 사각형 크기

        void OnDrawGizmosSelected()
        {
            // Gizmos 색상 설정
            Gizmos.color = Color.red;

            // 사각형 중심점 계산 (Transform의 위치)
            Vector3 center = transform.position;

            // 사각형의 네 모서리 좌표 계산
            Vector3 corner1 = center + new Vector3(-worldSize.x / 2, -worldSize.y / 2, 0); // 왼쪽 아래
            Vector3 corner2 = center + new Vector3(worldSize.x / 2, -worldSize.y / 2, 0); // 오른쪽 아래
            Vector3 corner3 = center + new Vector3(worldSize.x / 2, worldSize.y / 2, 0); // 오른쪽 위
            Vector3 corner4 = center + new Vector3(-worldSize.x / 2, worldSize.y / 2, 0); // 왼쪽 위
                                                                                          // Gizmos를 사용하여 사각형 그리기
            Gizmos.DrawLine(corner1, corner2);
            Gizmos.DrawLine(corner2, corner3);
            Gizmos.DrawLine(corner3, corner4);
            Gizmos.DrawLine(corner4, corner1);

        }

    }

}
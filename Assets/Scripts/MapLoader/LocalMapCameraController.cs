using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalMapCameraController : MonoBehaviour
{
    Vector3 firstTouchPosi;

    private void Start()
    {
        SetCameraInit();
    }

    public void SetCameraInit()
    {
        //카메라를 맵 중앙으로 배치
        float maxX = GameManager.instance.maxBlockX;
        float maxY = GameManager.instance.maxBlockY;

        Vector3 newPosi = new Vector3(maxX / 2, -maxY / 2, -10);
        gameObject.transform.position = newPosi;
    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //최초 터치 위치 확인
            firstTouchPosi = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        else if(Input.GetMouseButton(0))
        {
            Vector3 touchPosi = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 dPosi = touchPosi - firstTouchPosi;
            dPosi.z = 0;

            //이동방향 제한
            Vector2 cameraPosi = gameObject.transform.position;
            Vector2 cameraPosiTo = gameObject.transform.position - dPosi;

            float maxX = GameManager.instance.maxBlockX;
            float maxY = GameManager.instance.maxBlockY;

            //변경될 카메라 시점이 바깥을 벗어나지 못하게 막음
            if (cameraPosiTo.x < 0)
            {
                dPosi.x = 0;
                Vector3 newPosi = new Vector3(0, cameraPosi.y, -10);
                gameObject.transform.position = newPosi;
            }
            if (cameraPosiTo.y > 0)
            {
                dPosi.y = 0;
                Vector3 newPosi = new Vector3(cameraPosi.x, 0, -10);
                gameObject.transform.position = newPosi;
            }
            if (cameraPosiTo.x > maxX)
            {
                dPosi.x = 0;
                Vector3 newPosi = new Vector3(maxX, cameraPosi.y, -10);
                gameObject.transform.position = newPosi;
            }
            if (cameraPosiTo.y < -maxY)
            {
                dPosi.y = 0;
                Vector3 newPosi = new Vector3(cameraPosi.x, -maxY, -10);
                gameObject.transform.position = newPosi;
            }

            //움직이기 전에 체크 필요
            gameObject.transform.Translate(-dPosi);

            firstTouchPosi = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        
    }
}

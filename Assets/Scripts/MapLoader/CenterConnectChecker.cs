using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CenterConnectChecker : MonoBehaviour
{
    public Vector3Int thisTileVector;
    public string thisTileType;

    public GameObject originTile;
    public bool isConnected;
    public bool nowChecking;
    public bool isChecked;

    int mapSize = 5;
    int xValue;
    int yValue;

    int thisChildCount;
    int passDirection = 100;

    public void CheckNearWEType()
    {
        mapSize = MainBattleManager.instance.battleMapSize;
        int mapSizeSqr = mapSize * mapSize;


        //현재 오브젝트가 몇 번째 자식인지 확인
        thisChildCount = gameObject.transform.GetSiblingIndex();

        //연결 탐색을 건너뛸 방향. 왼쪽 끝 타일은 왼쪽과 연결 확인 x, 오른쪽 끝 타일은 오른쪽과 연결 확인x

        //Debug.Log(gameObject + " " + thisChildCount + " " + mapSize + " " + thisChildCount % mapSize);
        if (thisChildCount % mapSize == 0)
        {
            passDirection = 3;
        }
        else if (thisChildCount % mapSize == mapSize - 1)
        {
            passDirection = 1;
        }

        //자기 자신은 체크
        isChecked = true;

        int[] plusSubNum = new int[4] { -mapSize, 1, mapSize, -1 };
        
        for (int i = 0; i < 4; i++)
        {
            //Debug.Log(thisChildCount + " " + plusSubNum[i]);
            int nearObjChildNum = thisChildCount + plusSubNum[i];

            //i가 passdirection이 아니고 child의 범위 안에 있을 경우 루프
            if (i != passDirection && nearObjChildNum >= 0 && nearObjChildNum < mapSizeSqr)
            {
                //사방의 tiletype 확인
                GameObject nearTileObj = transform.parent.GetChild(nearObjChildNum).gameObject;
                bool nearTileChecked = nearTileObj.GetComponent<CenterConnectChecker>().isChecked;

                string nearTileType = nearTileObj.GetComponent<CenterConnectChecker>().thisTileType;
 
                //체크를 하지 않았으면 주변 타일 체크
                if(!nearTileChecked)
                {
                    nearTileObj.GetComponent<CenterConnectChecker>().isChecked = true;

                    //Debug.Log(gameObject + " " + gameObject + " " + nearTileType);
                    //근처 타일 타입이 w이거나 e이면 해당 타일 연결 true
                    if (nearTileType == "w" || nearTileType == "e")
                    {
                        NearTileCheckConnected(nearTileObj);
                    }
                }

            }
        }
    }

    public void NearTileCheckConnected(GameObject _nearTileObj)
    {
        //해당 근접타일은 중앙 타일과 연결되어 있으므로 isConnected 을 true로 반환
        _nearTileObj.GetComponent<CenterConnectChecker>().isConnected = true;

        //근접타일에서 다른 근접 타일을 탐색
        _nearTileObj.GetComponent<CenterConnectChecker>().CheckNearWEType();
    }
}

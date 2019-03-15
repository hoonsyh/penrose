using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EventLoader : MonoBehaviour
{
    public Vector3Int eventPosition;

    
    public void CallEvent()
    {
        //키 정보 저장
        BattleMapSizeDataSetting();
        SaveTileKey();


        //이벤트 위치를 바탕으로 전투맵 호출   
        string battleScene = "Scenes/MainBattle";
        MainBattleManager.instance.battleLocationVector = eventPosition;

        //맵 데이터 임시 저장
        LocalMapLoader lml = transform.parent.GetComponent<LocalMapLoader>();
        lml.TempSaveMapData();

        SceneManager.LoadSceneAsync(battleScene);

        //Debug.Log(eventPosition);
        //Debug.Log(GameManager.instance.tileObject[eventPosition.x, -eventPosition.y].GetComponent<GeneralTileScript>().myPosition);
    }


    void BattleMapSizeDataSetting()
    {
        int mapSize = MainBattleManager.instance.battleMapSize;
        MainBattleManager.instance._keyTileClass = new string[mapSize, mapSize];
        MainBattleManager.instance._keyTileClassDetail = new string[mapSize, mapSize];
        MainBattleManager.instance._keyTileClassVariation = new string[mapSize, mapSize];
    }


    //임시 코드
    void SaveTileKey()
    {
        //전투 중심 위치 확인
        Vector3Int battleLocation = new Vector3Int(eventPosition.x, -eventPosition.y, 0);

        //전투하는 곳의 지형 데이터 로드
        //배틀맵 사이즈에 따라 범위 변경
        int mapSize = MainBattleManager.instance.battleMapSize;
        int range = (int)(mapSize / 2f);

        //원래 battleLocation.x - 2;

        int xMin = battleLocation.x - range;
        int xMax = battleLocation.x + range;
        int yMin = battleLocation.y - range;
        int yMax = battleLocation.y + range;


        //Debug.Log(xMin + " " + xMax + " " + yMin + " " + yMax);
        //Debug.Log(mapDataList[16][16]);
        //맵 빌드
        for (int i = yMin; i < yMax + 1; i++)
        {
            for (int j = xMin; j < xMax + 1; j++)
            {
                GameObject tileObj = GameManager.instance.tileObject[i, j].gameObject;

                string keyTileClass = tileObj.GetComponent<GeneralTileScript>().tileClass;
                string keyTileClassDetail = tileObj.GetComponent<GeneralTileScript>().tileClassDetail;
                string keyTileClassVariation = tileObj.GetComponent<GeneralTileScript>().tileClassVariation;
                /*
                Debug.Log(battleLocation);
                Debug.Log((j - xMin) + ", " + (i - yMin));
                */
                MainBattleManager.instance._keyTileClass[i - yMin, j - xMin] = keyTileClass;
                MainBattleManager.instance._keyTileClassDetail[i - yMin, j - xMin] = keyTileClassDetail;
                MainBattleManager.instance._keyTileClassVariation[i - yMin, j - xMin] = keyTileClassVariation;
            }
        }
    }
}

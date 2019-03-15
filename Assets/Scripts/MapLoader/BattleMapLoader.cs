using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using RotaryHeart.Lib.SerializableDictionary;
using System;
using System.Linq;

[System.Serializable]
public class LargeTileDictionary : SerializableDictionaryBase<string, GameObject> { }

[System.Serializable]
public class EnemyObjectDictionary : SerializableDictionaryBase<string, GameObject> { }

public class BattleMapLoader : MonoBehaviour
{



    public PolygonCollider2D cameraBlocker;
    public GameObject boundaryObj;
    public UnitDataSet unitDataSet;

    public LargeTileDictionary largeTileDic;
    public EnemyObjectDictionary enemyObjDic;
    public GameObject mapTiles;
    public GameObject enemyGroup;
    int xOffset;
    int yOffset;

    List<ArrayList> mapDataList;
    GridLayout m_gridLayout;
    GameObject grid;

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1.4f;
        //Debug.Log(MainBattleManager.instance.enemyCountPairs.ElementAt(0));
        //BattleMapSizeDataSetting();

        SetCameraBlocker();
        LoadData();
        BattleMapBuild();
        
        try
        {
            CheckConnectCenter();
        }
        catch(DivideByZeroException e)
        {
            Debug.LogError(e);
            Debug.Log("중앙과 연결된 도로 없음");
        }
        LoadDefaultEnemy();

        StartCoroutine(StartMission());

        mapDataList = GameManager.instance.tempSaveMap;
    }

    void SetCameraBlocker()
    {
        int mapSize = MainBattleManager.instance.battleMapSize;
        float mapHalf = mapSize / 2f;
        float mapEnd = 7 * mapHalf;

        Vector2[] boundaryVector = new Vector2[5];
        boundaryVector[0] = new Vector2(-mapEnd, mapEnd);
        boundaryVector[1] = new Vector2(-mapEnd, -mapEnd);
        boundaryVector[2] = new Vector2(mapEnd, -mapEnd);
        boundaryVector[3] = new Vector2(mapEnd, mapEnd);
        boundaryVector[4] = new Vector2(-mapEnd, mapEnd);

        cameraBlocker.SetPath(0, boundaryVector);

        //바깥 경계 조정
        boundaryObj.transform.GetChild(0).GetComponent<BoxCollider2D>().offset = new Vector2(0, mapEnd + 2);
        boundaryObj.transform.GetChild(1).GetComponent<BoxCollider2D>().offset = new Vector2(0, -(mapEnd + 2));
        boundaryObj.transform.GetChild(2).GetComponent<BoxCollider2D>().offset = new Vector2(-(mapEnd + 2), 0);
        boundaryObj.transform.GetChild(3).GetComponent<BoxCollider2D>().offset = new Vector2(mapEnd + 2, 0);

    }

    IEnumerator StartMission()
    {
        yield return new WaitForSeconds(3f);
        //모든 적들의 detector enable
        int enemyCount = enemyGroup.transform.childCount;

        for (int i = 0; i < enemyCount; i++)
        {
            enemyGroup.transform.GetChild(i).GetChild(3).GetComponent<CircleCollider2D>().enabled = true;
        }
    }


    void LoadData()
    {
        //mapDataList = MainBattleManager.instance.mapDataList;
        //임시. 실제로는 위의 코드를 사용해야 한다.
        mapDataList = SaveDataController.instance.LoadCsvData("LocalMapCSV/testMap");

        //gridLayout 호출
        grid = GameObject.Find("Grid").gameObject;
        m_gridLayout = grid.GetComponent<GridLayout>();
    }

    void BattleMapBuild()
    {
        //전투 중심 위치 확인
        Vector3Int eventPosition = MainBattleManager.instance.battleLocationVector;
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
                Vector3Int tileLocation = new Vector3Int(i - battleLocation.y, -j + battleLocation.x, 0);

                //string tileKey = mapDataList[i][j].ToString();
                //string tileKey = GameManager.instance.tileObject[j, i].GetComponent<GeneralTileScript>().tileType;

                //GameObject tileObj = GameManager.instance.tileObject[j, i].gameObject;
                

                string keyTileClass = MainBattleManager.instance._keyTileClass[j - xMin, i - yMin];
                string keyTileClassDetail = MainBattleManager.instance._keyTileClassDetail[j - xMin, i - yMin];
                string keyTileClassVariation = MainBattleManager.instance._keyTileClassVariation[j - xMin, i - yMin];

                try
                {
                    SetPrefabTile(keyTileClass, keyTileClassDetail, keyTileClassVariation, tileLocation);
                }
                catch(ArgumentException e)
                {
                    Debug.LogError(e);
                    Debug.Log(tileLocation);
                }
            }
        }
    }

    //프리팹은 잔디
    //빌딩 스크립트 부착
    //key에 맞는 스프라이트 삽입

    void SetPrefabTile(string keyClass, string keyDetail, string keyVariation, Vector3Int location)
    {
        //해당 타일의 정보를 통해 Dictionary에서 PrefabTile 불러오기 및 instantiate
        GameObject tile = Instantiate(largeTileDic["e"]);
        tile.name = location.y + "_" + location.x;
        GameObject changeSprTile = tile;

        CenterConnectChecker ccc = tile.GetComponent<CenterConnectChecker>();
        //타일에 타일타입 정보 저장
        ccc.thisTileType = keyClass;

        //keyclass가 공터나 도로가 아니면 해당 오브젝트에 BuildingSetPosition 스크립트 추가
        if (keyClass != "e" && keyClass != "w")
        {
            tile.AddComponent<BuildingSetPosition>();
            tile.GetComponent<BuildingSetPosition>().relativeVector = location;

            //box collider 활성화
            tile.transform.GetChild(0).GetComponent<BoxCollider2D>().enabled = true;
            tile.transform.GetChild(0).GetComponent<RangeColliderScript>().enabled = true;

            changeSprTile = tile.transform.GetChild(0).gameObject;
        }
        else if (keyClass == "e" && keyDetail != "0") //파괴된 곳일 경우 스프라이트 바꿀 대상만 변경
        {
            changeSprTile = tile.transform.GetChild(0).gameObject;
        }

        //도로 타일이 아닌 경우에만
        if (keyClass != "w" && keyClass != "e")
        {
            //keyDetail이 7 이상이면 적절하게 공사중 스프라이트
            if (System.Convert.ToInt32(keyDetail) == 7)
            {
                keyClass = "e";
                keyDetail = "const";
            }
            else if (System.Convert.ToInt32(keyDetail) > 7)
            {
                keyClass = "e";
                keyDetail = "upgrade";
                //건설 진행 상황이 3이상, 6이상이면 variation 변경
            }

            //건설 진행 상황이 3이상, 6이상이면 variation 변경
            if (System.Convert.ToInt32(keyVariation) <= 3)
            {
                keyVariation = "0";
            }
            else if (System.Convert.ToInt32(keyVariation) > 3 && System.Convert.ToInt32(keyVariation) <= 6)
            {
                keyVariation = "1";
            }
            else if (System.Convert.ToInt32(keyVariation) > 6)
            {
                keyVariation = "2";
            }
        }


        //스프라이트 변경
        //Debug.Log(tile.name + "      " + keyClass + " " + keyDetail + " " + keyVarition);
        Sprite largeSpr = SpriteDictionary.instance.largeSprDic[keyClass][keyDetail][keyVariation];
        changeSprTile.GetComponent<SpriteRenderer>().sprite = largeSpr;

        tile.transform.SetParent(mapTiles.transform);

        //행, 열에 맞게 해당 Tile을 Grid 위에 배치
        tile.transform.position = m_gridLayout.LocalToWorld(m_gridLayout.CellToLocalInterpolated(location) + new Vector3(3.5f, 3.5f, 0));
        

    }


    //중앙과 연결되어 있는 도로 확인
    void CheckConnectCenter()
    {
        int mapSize = MainBattleManager.instance.battleMapSize;
        //중앙 타일 childnum
        int centerChildNum = (mapSize * mapSize - 1) / 2;

        //중앙 타일에서 checkCenter 작동
        try
        {
            grid.transform.GetChild(2).GetChild(centerChildNum).GetComponent<CenterConnectChecker>().CheckNearWEType();
        }
        catch(UnityException e)
        {
            Debug.LogError(e);
            Debug.Log(centerChildNum);
        }
        //grid.transform.GetChild(2).GetChild(centerChildNum).GetComponent<CenterConnectChecker>().isConnected = true;
    }


    void LoadDefaultEnemy()
    {
        //중앙과 이어진 도로가 몇 개 있는지 확인
        int mapSize = MainBattleManager.instance.battleMapSize;
        int totalTile = mapSize * mapSize;
        int connectedRoadCount = 0;
        for (int i = 0; i < totalTile; i++)
        {
            bool isConnected = grid.transform.GetChild(2).GetChild(i).GetComponent<CenterConnectChecker>().isConnected;
            string tileType = grid.transform.GetChild(2).GetChild(i).GetComponent<CenterConnectChecker>().thisTileType;
            //중앙과 연결된 w 타입 카운트
            if(isConnected && tileType == "w")
            {
                connectedRoadCount++;
            }
        }

        //이번 이벤트에 등장할 적 타입, 종류 확인
        int enemyTypeCount = MainBattleManager.instance.enemyCountPairs.Count;
        //Debug.Log(enemyTypeCount);
        for (int i = 0; i < enemyTypeCount; i++)
        {
            string enemyString = MainBattleManager.instance.enemyCountPairs.ElementAt(i).Key;
            int enemyCount = MainBattleManager.instance.enemyCountPairs.ElementAt(i).Value;

            //한 타일 당 배치할 적 숫자
            int totalEnemy = enemyCount;
            int perEnemy = Mathf.CeilToInt(totalEnemy / connectedRoadCount);
            //Debug.Log(enemyCount + " " + perEnemy);
            //도로 타일 위에 적 배치
            for (int j = 0; j < totalTile; j++)
            {
                GameObject tile = grid.transform.GetChild(2).GetChild(j).gameObject;
                bool isConnected = tile.GetComponent<CenterConnectChecker>().isConnected;
                string tileType = tile.GetComponent<CenterConnectChecker>().thisTileType;
                if (isConnected && tileType == "w")
                {
                    for (int k = 0; k < perEnemy; k++)
                    {
                        CreateEnemy(tile, enemyString);
                        //적 카운트
                        MainBattleManager.instance.netTotalEnemy = MainBattleManager.instance.netTotalEnemy + 1;
                    }
                }
            }



        }


        

    }

    void CreateEnemy(GameObject _tile, string enemyType)
    {
        GameObject newEnemyObj = Instantiate(enemyObjDic[enemyType]);
        //타일 내 임의의 위치에 배치
        Vector3 tilePosi = _tile.transform.position;

        float minX = tilePosi.x - 3;
        float maxX = tilePosi.x + 3;
        float minY = tilePosi.y - 3;
        float maxY = tilePosi.y + 3;

        //구역 내부의 임의의 위치에서 적 생성
        float randX = UnityEngine.Random.Range(minX, maxX);
        float randY = UnityEngine.Random.Range(minY, maxY);
        Vector3 newPosi = new Vector3(randX, randY, randY);

        newEnemyObj.transform.SetParent(GameObject.Find("MainBattleScript").transform.GetChild(2));
        newEnemyObj.transform.position = newPosi;

        //SetEnemyUnitStat(newEnemyObj, _enemyTypeInt);
    }

    void SetEnemyUnitStat(GameObject enemyUnit, int enemyTypeInt)
    {
        //저장되어 있는 데이터 불러오기
        enemyUnit.GetComponent<EnemyAction>().unitData.power = unitDataSet.GetComponent<UnitDataSet>().enemyData[enemyTypeInt].power;
        enemyUnit.GetComponent<EnemyAction>().unitData.name = unitDataSet.GetComponent<UnitDataSet>().enemyData[enemyTypeInt].name;
        enemyUnit.GetComponent<EnemyAction>().unitData.type = unitDataSet.GetComponent<UnitDataSet>().enemyData[enemyTypeInt].type;
        enemyUnit.GetComponent<EnemyAction>().unitData.unitObject = unitDataSet.GetComponent<UnitDataSet>().enemyData[enemyTypeInt].unitObject;
        enemyUnit.GetComponent<EnemyAction>().unitData.health = unitDataSet.GetComponent<UnitDataSet>().enemyData[enemyTypeInt].health;
        enemyUnit.GetComponent<EnemyAction>().unitData.health100 = unitDataSet.GetComponent<UnitDataSet>().enemyData[enemyTypeInt].health100;
        enemyUnit.GetComponent<EnemyAction>().unitData.defense = unitDataSet.GetComponent<UnitDataSet>().enemyData[enemyTypeInt].defense;
        enemyUnit.GetComponent<EnemyAction>().unitData.speed = unitDataSet.GetComponent<UnitDataSet>().enemyData[enemyTypeInt].speed;
        enemyUnit.GetComponent<EnemyAction>().unitData.range = unitDataSet.GetComponent<UnitDataSet>().enemyData[enemyTypeInt].range;

        //스프라이트 변경, attackrange active, 
        GameObject spineObj = Instantiate(unitDataSet.GetComponent<UnitDataSet>().enemyData[enemyTypeInt].unitObject);
        spineObj.transform.SetParent(enemyUnit.transform);

        //attackRange 조정

        if (enemyUnit.GetComponent<EnemyAction>().unitData.type == "range")
        {
            //bullet 활성화

        }

    }


}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

[System.Serializable]
public class TileDictionary : SerializableDictionaryBase<string, GameObject> {}

public class LocalMapLoader : MonoBehaviour
{
    int maxX = 50;
    int maxY = 30;
    bool blockTouch;

    public bool firstStart;
    public TileDictionary m_tileDictionary;
    public GameObject testPrefab;
    List<ArrayList> mapDataList;
    Tile[] testTile;

    public GameObject mapTiles;
    GridLayout m_gridLayout;

    public GameObject eventbutton;
    

    // Start is called before the first frame update
    void Start()
    {
        QualitySettings.resolutionScalingFixedDPIFactor = 2f;

        StartCoroutine(CityTime());
        LoadMapData();

        if (firstStart)
        {
            FirstStart();
            firstStart = false;
        }

        
        MapBuild();
        CameraSet();
        //Test();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && blockTouch == false)
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

            if (hit.collider != null && hit.collider.tag == "UIBtn") // UI 버튼 터치
            {
                hit.collider.GetComponent<EventLoader>().CallEvent();            
            }
        }
    }

    
    void Test()
    {
        string test = "131_123123";
        string[] test2 = test.Split('_');
        Debug.Log(test2[1]);
    }
    

    void CameraSet()
    {
        float blockX = ((GameManager.instance.maxMapX * 50f) - 1920) / 100;
        float blockY = ((GameManager.instance.maxMapY * 50f) - 1080) / 100;

        GameManager.instance.maxBlockX = blockX;
        GameManager.instance.maxBlockY = blockY;

    }

    void FirstStart()
    {
        //맵 데이터 로드
        mapDataList = SaveDataController.instance.LoadCsvData("LocalMapCSV/testMap2");

        MainBattleManager.instance.mapDataList = mapDataList;
        SaveDataController.instance.SaveMapData(mapDataList);
    }

    public void SaveMap()
    {
        SaveDataController.instance.SaveMapData(TempSaveMapData());

        //인구, 수요 등의 데이터 저장
        ArrayList cityData = new ArrayList();
        cityData.Add(GameManager.instance.cityPopulation);
        cityData.Add(GameManager.instance.residenceCapa);
        cityData.Add(GameManager.instance.commercialCapa);
        cityData.Add(GameManager.instance.industrialCapa);
        cityData.Add(GameManager.instance.jobCapa);

        SaveDataController.instance.SaveArrayList(cityData, "cityData");

    }

    //전투맵 전환 성능을 위한 데이터 임시 저장
    public List<ArrayList> TempSaveMapData()
    {
        GameObject[,] nowMap = GameManager.instance.tileObject;
        string[,] nowMapData = new string[30, 50];
        //타일 정보를 불러와서 저장

        for (int i = 0; i < GameManager.instance.maxMapY; i++)
        {
            for (int j = 0; j < GameManager.instance.maxMapX; j++)
            {
                GameObject tileObj = GameManager.instance.tileObject[i, j];
                string tileType = tileObj.GetComponent<GeneralTileScript>().tileType;
                nowMapData[i, j] = tileType;
            }
        }
        List<ArrayList> data = SaveDataController.instance.ArrayToList(nowMapData);

        GameManager.instance.tempSaveMap = data;
        return data;
    }


    void LoadMapData()
    {
        //처음 시작하는 것이 아니라면 저장된 경로에서 데이터 추출
        //전투를 진행하고 난 뒤에는 로드 x
        if(!firstStart && MainBattleManager.instance.isInit)
        {
            ArrayList cityData = SaveDataController.instance.LoadArrayList("cityData");

            GameManager.instance.cityPopulation = System.Convert.ToInt32(cityData[0]);
            GameManager.instance.residenceCapa = System.Convert.ToInt32(cityData[1]);
            GameManager.instance.commercialCapa = System.Convert.ToInt32(cityData[2]);
            GameManager.instance.industrialCapa = System.Convert.ToInt32(cityData[3]);
            GameManager.instance.jobCapa = System.Convert.ToInt32(cityData[4]);
        }
    }



    void MapBuild()
    {
        //성장률 계산
        GameManager.instance.ReCalculateGrowthRate();

        GameManager.instance.tileObject = new GameObject[maxY, maxX];

        

        //저장된 맵 데이터 로드. 처음 기동 시 1회만 실행
        if (MainBattleManager.instance.isInit)
        {
            mapDataList = SaveDataController.instance.LoadMapData();
            MainBattleManager.instance.mapDataList = mapDataList;

            MainBattleManager.instance.isInit = false;
        }
        //전투 진행 후 
        else
        {          
            mapDataList = GameManager.instance.tempSaveMap;
        }

        GameManager.instance.maxMapX = mapDataList[0].Count;
        GameManager.instance.maxMapY = mapDataList.Count;

        //GameManager.instance.maxMapX = GameManager.instance.tileObject.GetLength(0);
        //GameManager.instance.maxMapY = GameManager.instance.tileObject.GetLength(1);

        //gridLayout 호출
        GameObject grid = GameObject.Find("Grid").gameObject;
        m_gridLayout = grid.GetComponent<GridLayout>();

        

        //맵의 타일에 셀 배치
        for (int i = 0; i < mapDataList.Count; i++)
        {
            for (int j = 0; j < mapDataList[0].Count; j++)
            {
                Vector3Int tileLocation = new Vector3Int(j, -i, 0);

                string tileKey = mapDataList[i][j].ToString();
                //try
                //{
                SetPrefabTile(tileKey, tileLocation);
                //}
                /*
                catch (System.Exception e)
                {
                    Debug.LogError(e);
                    Debug.Log(tileKey);
                    Debug.Log("i = " + i + "  j = " + j);
                }*/
            }
        }
    }

    IEnumerator CityTime()
    {
        while (true)
        {
            CalculatePopulation();
            GameManager.instance.ReCalculateGrowthRate();

            yield return new WaitForSeconds(GameManager.instance.cityGrowthTime * 10);
        }
    }


    void CalculatePopulation()
    {
        float populGrowth = GameManager.instance.populGrowthRate;
        int population = GameManager.instance.cityPopulation;

        int newPopul = population + Mathf.CeilToInt(population * populGrowth);
        GameManager.instance.cityPopulation = newPopul;
    }



    void ChoiceBattleLocation()
    {
        //임의의 그리드 좌표 지정 
        
        int xRandMax = mapDataList[0].Count - 1;
        int yRandMax = mapDataList.Count;

        int xRand = Random.Range(16, 27);
        int yRand = Random.Range(11, 21);

        Vector3Int randPosition = new Vector3Int(xRand, yRand, 0);
        MainBattleManager.instance.battleLocationVector = randPosition;

        //버튼 출력을 위한 y좌표 조정
        randPosition = new Vector3Int(xRand, -yRand, 0);

        //해당 좌표로 버튼 출력
        //Debug.Log(randPosition);
        eventbutton.transform.position = m_gridLayout.LocalToWorld(m_gridLayout.CellToLocalInterpolated(randPosition) + new Vector3(.25f, -.25f, 0));

    }

    public void BtnBattleStart()
    {
        //새로운 Scene 로드 <- battleMapScene
        TempSaveMapData();

        SceneManager.LoadSceneAsync(1);

    }

    void SetPrefabTile(string key, Vector3Int location)
    {
        //해당 타일의 정보를 통해 Dictionary에서 PrefabTile 불러오기 및 instantiate
        //저장된 csv 값의 맨 앞 글자가 타일의 타입 (ex. r1010 <- r)
        string tileType = key[0].ToString();
        GameObject tile = null;

        //key에 맞게 스크립트 저장 필요
        if (tileType == "e")
        {
            tile = Instantiate(m_tileDictionary[tileType]);
        }
        else if (tileType == "w")
        {
            tile = Instantiate(m_tileDictionary[tileType]);
        }
        //건물들
        else
        {
            tile = Instantiate(m_tileDictionary["e"]);

            tile.AddComponent<ConstructionTileScript>();

            //공터 스크립트 삭제
            Destroy(GetComponent<EmptyTileScript>());
            tile.AddComponent<BuildingTileScript>();

            //공터가 아니라는 불리언
            tile.GetComponent<BuildingTileScript>().isGrow = true;
        }

        //자신의 상태 업데이트
        tile.GetComponent<GeneralTileScript>().tileType = key;
        tile.GetComponent<GeneralTileScript>().UpdateThisObject();
        tile.GetComponent<GeneralTileScript>().ChangeSprite();

        //건물 타입, 공사중이라면 건설 메소드 실행
        if (tileType != "e" && tileType != "w" && System.Convert.ToInt32(key[1].ToString()) > 6)
        {
            tile.GetComponent<ConstructionTileScript>().StartConstruction();
        }



        /*
        //키에 맞게 스프라이트 변경
        string keyClass = tile.GetComponent<GeneralTileScript>().tileClass;
        string keyDetail = tile.GetComponent<GeneralTileScript>().tileClassDetail;
        string keyVariation = tile.GetComponent<GeneralTileScript>().tileClassVariation;
        try
        {
            tile.GetComponent<SpriteRenderer>().sprite = SpriteDictionary.instance.smallSprDic[keyClass][keyDetail][keyVariation];
        }
        catch(KeyNotFoundException e)
        {
            Debug.LogError(e);
            Debug.Log(location);
            Debug.Log(keyClass + " " + keyDetail + " " + keyVariation);
        }
        */
        tile.name = location.ToString();
        tile.transform.SetParent(mapTiles.transform);
        
        int xAxis = location.x;
        int yAxis = location.y;

        //행, 열에 맞게 해당 Tile을 Grid 위에 배치
        tile.transform.position = m_gridLayout.LocalToWorld(m_gridLayout.CellToLocalInterpolated(location) + new Vector3(.25f, -.25f, 0));

        //Tile 좌표 정보를 array에 저장
        GameManager.instance.tileObject[-yAxis, xAxis] = tile;

        //타일의 위치 정보를 게임오브젝트 스크립트에 저장
        tile.GetComponent<GeneralTileScript>().tileType = key;

        //tile의 자식class에서 SetTileData 메소드 실행 <- general 하게 만들 필요가 있다
        if(tileType == "w")
        {
            tile.GetComponent<GeneralTileScript>().tileType = key;
            tile.GetComponent<GeneralTileScript>().myPosition = new Vector2(xAxis, -yAxis);
            tile.GetComponent<RoadTileScript>().SetTileData();
            
        }
        
        //타일의 오브젝트
        tile.GetComponent<GeneralTileScript>().myPosition = new Vector2(xAxis, -yAxis);

    }


    /*
    void TileTest()
    {
        GameObject grid = GameObject.Find("Grid").gameObject;
        Tilemap tileMap = grid.transform.GetChild(0).GetComponent<Tilemap>();
        int tileboundxMin = tileMap.cellBounds.xMin;
        int tileboundxMax = tileMap.cellBounds.xMax;
        int tileboundyMin = tileMap.cellBounds.yMin;
        int tileboundyMax = tileMap.cellBounds.yMax;

        GridLayout gridLayout = grid.GetComponent<GridLayout>();
        //Debug.Log(gridLayout);

        Vector3Int test0 = new Vector3Int(-2, -2, 0);
        GameObject testBuilding = Instantiate(testPrefab);
        testBuilding.transform.SetParent(grid.transform);

        testBuilding.transform.position = gridLayout.LocalToWorld(gridLayout.CellToLocalInterpolated(test0) + new Vector3(.5f, .5f, 0));

        
        Vector3Int test = new Vector3Int(1, 1, 0);
        tileMap.SetTile(test, testTile[0]);
    }
    */

    public void SetEvent()
    {
        ///////퀘스트 지정

        int randEnemy0Num = Random.Range(10, 30);
        int randEnemy1Num = Random.Range(10, 20);

        MainBattleManager.instance.enemyCountPairs["enemy0"] = randEnemy0Num;
        MainBattleManager.instance.enemyCountPairs["enemy1"] = randEnemy1Num;

        ///////위치 지정

        int randX = 0;
        int randY = 0;

        GameObject randGameObject = null;
        string randGameObjectType = null;
     
        int roop = 0;

        //해당 좌표 Tile의 type이 w가 아니면 다시 랜덤 좌표   
        do
        {
            //가로 세로 랜덤좌표 (구석에 이벤트 발생 시, 맵 로딩에 문제가 생기기 때문에 구석은 제외)
            randX = Random.Range(2, maxX - 1);
            randY = Random.Range(2, maxY - 1);

            randGameObject = GameManager.instance.tileObject[randY, randX];
            randGameObjectType = randGameObject.GetComponent<GeneralTileScript>().tileType[0].ToString();

            roop++;

            if(roop > 100)
            {
                Debug.Log("못 찾음");
                break;
            }
        }
        while (randGameObjectType != "w");
        
        //해당 좌표 근처에 다른 이벤트가 있으면 다시 랜덤 좌표
        //정해진 위치에 이벤트 등록
        Vector2 eventPosition = randGameObject.GetComponent<GeneralTileScript>().myPosition;
        //Debug.Log(eventPosition);

        Vector3Int location = new Vector3Int((int)eventPosition.x, -(int)eventPosition.y, 0);
        //Vector3Int location = new Vector3Int(0, 0, 0);
        GameObject eventBtn = Instantiate(eventbutton);
        eventBtn.transform.localScale = Vector3.zero;
        eventBtn.transform.


        eventBtn.transform.SetParent(gameObject.transform);

        //행, 열에 맞게 해당 object를 Grid 위에 배치
        eventBtn.transform.position = m_gridLayout.LocalToWorld(m_gridLayout.CellToLocalInterpolated(location) + new Vector3(.25f, .15f, 0));

        //해당 버튼에 위치정보 저장
        eventBtn.GetComponent<EventLoader>().eventPosition = location;

    }

}




using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using RotaryHeart.Lib.SerializableDictionary;

[System.Serializable]
public class EnemyCountDic : SerializableDictionaryBase<string, int> { }

public class MainBattleManager : MonoBehaviour{

    static public MainBattleManager instance = null;

    public string test;

    //전투 진행 여부 확인
    public bool isInit;

    //전투맵 빌드를 위한 변수
    public List<ArrayList> mapDataList;
    public Vector3Int battleLocationVector;
    public int battleMapSize;


    //전투맵 빌드를 위한 키 어레이
    public string[,] _keyTileClass = new string[5, 5];
    public string[,] _keyTileClassDetail = new string[5, 5];
    public string[,] _keyTileClassVariation = new string[5, 5];

    //전투맵에 배치할 적 타입 및 수
    public string enemyType;
    public int enemyCount;

    public EnemyCountDic enemyCountPairs;
    public int netTotalEnemy;

    public string bossType;


    //전투 임시 스탯
    //0:보병 1:대전차보병 2:전차 3:자주포 4:지원 ----- 임시
    public int[] userUnitType;
    public int[] enemyUnitType;

    //임시 데이터
    public float[] tempInfantryStat;
    public float[] tempTankStat;

    //유저 행동 정보
    public Vector3 userUnitVector;
    public bool isUserAttacking;
    public bool isUserCharging;
    public bool isUserDashing;
    public bool isUserDefensing;

    public int vectorAffi = 1;

    //건물을 얼마나 부쉈는지 체크
    public int userDestroyBuilding;
    public int enemyDestroyBuilding;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if(mapDataList != null)
        {
            test = mapDataList[9][22].ToString();
        }
        
    }

}



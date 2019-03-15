using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using UnityEngine.Tilemaps;
using Spine;
using Spine.Unity;

public class MainBattleController : MonoBehaviour {

    public Tile[] testTile;
    public GameObject testPrefab;

    //UI 
    public GameObject hpUIMask;
    public Text hpUIText;
    public GameObject mpUIMask;
    public Text mpUIText;
    public GameObject itemGroup;
    public Text waveTime;
    public Text gotGold;

    public Sprite[] userUnitSprite;
    public Sprite[] enemyUnitSprite;
    public GameObject userUnitPrefab;
    public GameObject userUnit;
    public GameObject enemyUnit;

    public GameObject movePositionMark;

    public GameObject itemBox;

    bool placeUnit;
    bool unitSelected;

    int userUnitCount;
    float itemRegenTime;
    float enemyRegenTime;

    // Use this for initialization
    void Start () {

        //옵저버 패턴 실행
        EventManager.Instance.Listen("CollectGold", this, EventReceived);
        EventManager.Instance.Listen("EatHeal", this, EventReceived);

        //유저 유닛 탐색
        userUnit = transform.GetChild(1).GetChild(0).gameObject;

        //Time.timeScale = 1.5f;
        InitGame();

        TileTest();

        StartCoroutine(AppearUser());
    }

    void TileTest()
    {
        GameObject grid = GameObject.Find("Grid").gameObject;
        Tilemap tileMap = grid.transform.GetChild(0).GetComponent<Tilemap>();
        int tileboundxMin = tileMap.cellBounds.xMin;
        int tileboundxMax = tileMap.cellBounds.xMax;
        int tileboundyMin = tileMap.cellBounds.yMin;
        int tileboundyMax = tileMap.cellBounds.yMax;

        GridLayout gridLayout = grid.GetComponent <GridLayout>();
        //Debug.Log(gridLayout);

        Vector3Int test0 = new Vector3Int(-2, -2, 0);
        GameObject testBuilding = Instantiate(testPrefab);
        testBuilding.transform.SetParent(grid.transform);

        testBuilding.transform.position = gridLayout.LocalToWorld(gridLayout.CellToLocalInterpolated(test0) + new Vector3(3.5f, 3.5f, 0));

        Vector3Int test = new Vector3Int(1, 1, 0);
        tileMap.SetTile(test, testTile[0]);
    }


    private void Update()
    {
        /*
        if(Input.GetMouseButton(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            Vector2 touchPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            //터치한 곳이 이동 가능한 영역이면 마크 표시
            if(hit.collider == null || hit.collider.tag != "touchableArea")
            {
                movePositionMark.transform.position = touchPoint;
                movePositionMark.SetActive(true);
            }
            
        }
        else if(Input.GetMouseButtonUp(0))
        {
            
        }
        */
        //필드에 아이템 생성
        CreateItemBox();

        //웨이브 남은 시간 체크
        //CheckEnemyWave();
    }


    IEnumerator AppearUser()
    {
        yield return new WaitForSeconds(2);
        userUnit.SetActive(true);
    }


    void InitGame()
    {
        Physics2D.IgnoreLayerCollision(8, 9);

        //GameManager에 게이지 할당
        GameManager.instance.gaugeHpObject = hpUIMask;
        GameManager.instance.gaugeHpText = hpUIText;
        GameManager.instance.gaugeMpObject = mpUIMask;
        GameManager.instance.gaugeMpText = mpUIText;

        //수집 골드 초기화
        GameManager.instance.CollectGold = 0;

        //유저 스탯 삽입
        SetUserUnitStat();

        //적 임시 데이터 삽입
        SetEnemyTempStat();

        //적 웨이브 대기 시간 임시
        enemyRegenTime = 30;
    }

    void CreateItemBox()
    {
        //필드에 아이템이 3개 미만으로 있으면 10초 뒤에 아이템 생성
        itemRegenTime = itemRegenTime + Time.deltaTime;

        int itemCount = itemGroup.transform.childCount;
        if(itemCount < 3 && itemRegenTime > 10)
        {
            itemRegenTime = 0;
            //임시 위치 지정
            float randX = UnityEngine.Random.Range(-4f, 4f);
            float randY = UnityEngine.Random.Range(-1f, 8f);

            //아이템 박스 생성 및 부모 설정, 위치 지정
            GameObject newBox = Instantiate(itemBox);
            newBox.transform.SetParent(itemGroup.transform);
            newBox.transform.position = new Vector3(randX, randY, 0);

        }
    }
    /*
    void CheckEnemyWave()
    {
        enemyRegenTime = enemyRegenTime - Time.deltaTime;
        TimeSpan time = TimeSpan.FromSeconds(enemyRegenTime);

        int minutes = time.Minutes;
        int seconds = time.Seconds;
        //string remainTime = time.Minutes + " : " + time.Seconds;
        //string remainTime = time.ToString("ss");
        string remainTime = string.Format("{0:0}:{1:00}", minutes, seconds);
        waveTime.text = remainTime;

        //웨이브가 남아있을 경우, 일정 시간이 되면 적 웨이브 출몰(밖에서부터 등장)
        if(enemyRegenTime <= 0)
        {
            enemyRegenTime = 30;

            int enemyCount = 5; //임시
            //적 숫자에 맞게 적 생성
            for (int i = 0; i < enemyCount; i++)
            {
                CreateEnemy();
            }         
        }

    }
    */
    public void CollectGoldUI()
    {

    }

    void CreateEnemy()
    {

        //생성할 적 선택

        //0~3번 구역 중 랜덤하게 하나의 구역 선택
        int rand = UnityEngine.Random.Range(0, 4);
        float minX = 0;
        float maxX = 0;
        float minY = 0;
        float maxY = 0;

        if(rand == 0)
        {
            minX = -7f;
            maxX = -6f;
            minY = -2f;
            maxY = 9f;
        }
        else if(rand == 1)
        {
            minX = -5f;
            maxX = 5f;
            minY = 10.5f;
            maxY = 11.5f;
        }
        else if (rand == 2)
        {
            minX = 6f;
            maxX = 7f;
            minY = -2f;
            maxY = 9f;
        }
        else if (rand == 3)
        {
            minX = -5f;
            maxX = 5f;
            minY = -5f;
            maxY = -4f;
        }

        //구역 내부의 임의의 위치에서 적 생성
        float randX = UnityEngine.Random.Range(minX, maxX);
        float randY = UnityEngine.Random.Range(minY, maxY);
        Vector3 newPosi = new Vector3(randX, randY, randY);

        //적 유닛 생성
        GameObject newEnemy = Instantiate(enemyUnit);
        newEnemy.transform.SetParent(transform.GetChild(2));
        newEnemy.transform.position = newPosi;

        //적 유닛 능력치 할당
        SetEnemyUnitStat(newEnemy);

    }

    void SetEnemyUnitStat(GameObject enemyUnit)
    {
        //임시 데이터. 완성본에서는 타입에 맞는 데이터를 로드해야 한다
        float[] stat = new float[7];
        stat = MainBattleManager.instance.tempInfantryStat;

        enemyUnit.GetComponent<EnemyAction>().unitData.power = stat[0];
        enemyUnit.GetComponent<EnemyAction>().unitData.health = stat[1];
        enemyUnit.GetComponent<EnemyAction>().unitData.health100 = stat[1];
        enemyUnit.GetComponent<EnemyAction>().unitData.defense = stat[2];
        enemyUnit.GetComponent<EnemyAction>().unitData.speed = stat[3] / 2;
        enemyUnit.GetComponent<EnemyAction>().unitData.range = stat[4];
        enemyUnit.GetComponent<EnemyAction>().unitData.number = stat[5];
        enemyUnit.GetComponent<EnemyAction>().unitData.morale = stat[6];
    }

    void SetEnemyTempStat()
    {
        GameObject enemyGroup = transform.GetChild(2).gameObject;
        int enemyCount = enemyGroup.transform.childCount;

        float[] stat = new float[7];
        stat = MainBattleManager.instance.tempInfantryStat;

        for (int i = 0; i < enemyCount; i++)
        {
            enemyGroup.transform.GetChild(i).GetComponent<EnemyAction>().unitData.power = stat[0];
            enemyGroup.transform.GetChild(i).GetComponent<EnemyAction>().unitData.health = stat[1];
            enemyGroup.transform.GetChild(i).GetComponent<EnemyAction>().unitData.health100 = stat[1];
            enemyGroup.transform.GetChild(i).GetComponent<EnemyAction>().unitData.defense = stat[2];
            enemyGroup.transform.GetChild(i).GetComponent<EnemyAction>().unitData.speed = stat[3]/2;
            enemyGroup.transform.GetChild(i).GetComponent<EnemyAction>().unitData.range = stat[4];
            enemyGroup.transform.GetChild(i).GetComponent<EnemyAction>().unitData.number = stat[5];
            enemyGroup.transform.GetChild(i).GetComponent<EnemyAction>().unitData.morale = stat[6];
        }
    }


    void SetUserUnitStat()
    {
        GameObject userUnit = transform.GetChild(1).GetChild(0).gameObject;
        //임시 스탯 부여
        float[] stat = new float[7];

        stat = MainBattleManager.instance.tempTankStat;

        //타입에 맞게 AI 스크립트 붙이기
        //userUnit.AddComponent<UserAction>();
        //Debug.Log(stat[0]);

        userUnit.GetComponent<UserAction>().unitData.power = stat[0];
        userUnit.GetComponent<UserAction>().unitData.health = stat[1];
        userUnit.GetComponent<UserAction>().unitData.health100 = stat[1];
        userUnit.GetComponent<UserAction>().unitData.mana = 200;
        userUnit.GetComponent<UserAction>().unitData.mana100 = 200;
        userUnit.GetComponent<UserAction>().unitData.defense = stat[2];
        userUnit.GetComponent<UserAction>().unitData.speed = stat[3];
        userUnit.GetComponent<UserAction>().unitData.range = stat[4];
        userUnit.GetComponent<UserAction>().unitData.number = stat[5];
        userUnit.GetComponent<UserAction>().unitData.morale = stat[6];

        float hp = userUnit.GetComponent<UserAction>().unitData.health;
        float hp100 = userUnit.GetComponent<UserAction>().unitData.health100;
        float mp = userUnit.GetComponent<UserAction>().unitData.mana;
        float mp100 = userUnit.GetComponent<UserAction>().unitData.mana100;

        //HP MP에 맞게 게이지 설정
        GameManager.instance.SetGauge("hp" , hp, hp100, true);
        GameManager.instance.SetGauge("mp", mp, mp100, true);
    }


    void SetPlaceUnit(Vector2 touchPosition)
    {
        //새로운 프리팹 생성 및 위치 조정
        GameObject newUnit;
        newUnit = Instantiate(userUnitPrefab);
        newUnit.transform.position = touchPosition;
        newUnit.transform.SetParent(transform.GetChild(1)); // parent 설정

        newUnit.name = "userUnit" + transform.GetChild(1).childCount;

        //타입에 맞게 스프라이트 변경
        int newUnitType = GameManager.instance.userUnitType[userUnitCount - 1];
        Sprite newUnitSprite = userUnitSprite[newUnitType];
        newUnit.GetComponent<SpriteRenderer>().sprite = newUnitSprite;



        //임시 스탯 부여
        float[] stat = new float[7];
        if(newUnitType == 0)
        {
            stat = GameManager.instance.tempInfantryStat;
        }
        else if(newUnitType == 2)
        {
            stat = GameManager.instance.tempTankStat;
        }

        //타입에 맞게 AI 스크립트 붙이기
        newUnit.AddComponent<AIAction>();

        newUnit.GetComponent<AIAction>().unitData.power = stat[0];
        newUnit.GetComponent<AIAction>().unitData.health = stat[1];
        newUnit.GetComponent<AIAction>().unitData.health100 = stat[1];
        newUnit.GetComponent<AIAction>().unitData.defense = stat[2];
        newUnit.GetComponent<AIAction>().unitData.speed = stat[3];
        newUnit.GetComponent<AIAction>().unitData.range = stat[4];
        newUnit.GetComponent<AIAction>().unitData.number = stat[5];
        newUnit.GetComponent<AIAction>().unitData.morale = stat[6];


        //다음 유닛 선택
        userUnitCount--;
        if (userUnitCount == 0)
        {
            placeUnit = false;
            
        }
    }
    /*
    void UnitTouch(GameObject unitObj)
    {
        //이전에 선택한 유닛 선택해제
        if(oldSelectUnit != null)
        {
            UnitUntouch(oldSelectUnit);
        }

        //선택 효과
        unitObj.transform.GetChild(0).gameObject.SetActive(true);
        //사거리
        unitObj.transform.GetChild(1).gameObject.SetActive(true);
        oldSelectUnit = unitObj;
    }

    void UnitUntouch(GameObject unitObj)
    {
        //선택 효과
        unitObj.transform.GetChild(0).gameObject.SetActive(false);
        //사거리
        unitObj.transform.GetChild(1).gameObject.SetActive(false);
        oldSelectUnit = null;
    }
    */
    void EnemySet()
    {

    }

    void MoveUnit(GameObject unit, Vector2 targetPosi)
    {
        //이동할 위치 표시

        //공격 중이면 공격 중단
        //unit.GetComponent<AIAction>().EndEngage();

        //이동
        //이동거리와 유닛 속력으로 소요 시간 계산
        Vector2 originPosi = unit.transform.position;

        UnitAction m_UnitAction = unit.GetComponent<UnitAction>();
        float speed = m_UnitAction.unitData.speed;

        float moveTime = CalcDistance(originPosi, targetPosi) / speed; //유닛 속력은 자유롭게 변해야 한다

        unit.transform.DOKill();
        unit.transform.DOMove(targetPosi, moveTime).SetEase(Ease.Linear);

        //Debug.Log(CalcDistance(originPosi, targetPosi));

        //
    }

    public void ReturnWorldMap()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    float CalcDistance(Vector2 origin, Vector2 target)
    {    
        float dx = origin.x - target.x;
        float dy = origin.y - target.y;

        float distance = Mathf.Sqrt(dx * dx + dy * dy);

        return distance;
    }

    public void battleStart()
    {
        GameManager.instance.battleStartTrigger = true;
    }

    void EventReceived(string eventId, UnityEngine.Object target, object param)
    {
        switch (eventId)
        {
            case "CollectGold":

                int collectGolld = (int)param;
                int currentGold = GameManager.instance.CollectGold;
                GameManager.instance.CollectGold = currentGold + collectGolld;

                gotGold.text = GameManager.instance.CollectGold.ToString();
               
                break;

            case "EatHeal":

                //유저의 HP와 MP 20% 증가
                //데이터 호출
                //Debug.Log(userUnit);
                float hp = userUnit.GetComponent<UserAction>().unitData.health;
                float hp100 = userUnit.GetComponent<UserAction>().unitData.health100;
                float mp = userUnit.GetComponent<UserAction>().unitData.mana;
                float mp100 = userUnit.GetComponent<UserAction>().unitData.mana100;

                //증가량 계산
                float hpUp = Mathf.Round(hp100 * 0.2f);
                float mpUp = Mathf.Round(mp100 * 0.2f);

                //hp mp 증가
                hp = hp + hpUp;
                mp = mp + mpUp;

                //초과 제한
                if (hp > hp100)
                {
                    hp = hp100;
                }
                if (mp > mp100)
                {
                    mp = mp100;
                }

                //게이지 및 데이터 반영
                userUnit.GetComponent<UserAction>().unitData.health = hp;
                userUnit.GetComponent<UserAction>().unitData.mana = mp;

                GameManager.instance.SetGauge("hp", hp, hp100, false);
                GameManager.instance.SetGauge("mp", mp, mp100, false);

                //유저 유닛 체력 게이지 증가
                float ratio = hp / hp100;
                GameObject healthGauge = userUnit.transform.GetChild(0).GetChild(1).gameObject;
                healthGauge.transform.DOScaleX(ratio, 0.5f);


                break;
        }
    }
}

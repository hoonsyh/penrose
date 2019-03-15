using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GameManager : MonoBehaviour {

    static public GameManager instance = null;

    //전투맵 관련
    //public int battleMapSize = 5;
    public bool centerConnectCheck;

    //맵
    public List<ArrayList> tempSaveMap;

    public GameObject[,] tileObject;
    public string[,] tileTypeArr;
    public int cityPopulation;

    public int maxMapX;
    public int maxMapY;
    public float maxBlockX;
    public float maxBlockY;

    public float cityGrowthTime = 0.5f;

    //구역 성장률
    public float populGrowthRate;
    public float cityGrowthRate;
    public float residenceGrowthRate;
    public float commercialGrowthRate;
    public float industrialGrowthRate;
    public float jobGrowthRate;

    public float totalRate;

    //구역 캐파
    public int residenceCapa;
    public int commercialCapa;
    public int industrialCapa;
    public int jobCapa;

    //구역 수요
    public int residenceDemand;
    public int commercialDemand;
    public int industrialDemand;
    public int jobDemand;

    //치안
    public int securityRate;


    //적 출몰 조건




    public GameObject gaugeHpObject;
    public Text gaugeHpText;
    public GameObject gaugeMpObject;
    public Text gaugeMpText;
    public int CollectGold;

    //0:보병 1:대전차보병 2:전차 3:자주포 4:지원 ----- 임시
    public int[] userUnitType;
    public int[] enemyUnitType;

    //임시 데이터
    public float[] tempInfantryStat;
    public float[] tempTankStat;

    public bool battleStartTrigger;


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


    public void SetGauge(string type, float currentValue, float fullValue, bool immedi)
    {
        GameObject scalingObject = null;
        Text gaugeText = null;

        if (type == "hp")
        {
            scalingObject = gaugeHpObject;
            gaugeText = gaugeHpText;
        }
        else if(type == "mp")
        {
            scalingObject = gaugeMpObject;
            gaugeText = gaugeMpText;
        }

        //남은 게이지 비율 계산
        float duration = 0.5f;
        if(immedi)
        {
            duration = 0;
        }

        float ratio = currentValue / fullValue;
        scalingObject.transform.DOScaleX(ratio, duration);

        gaugeText.text = currentValue + " / " + fullValue;

    }

    public void ReCalculateGrowthRate()
    {
        //수요 조정
        commercialDemand = (int)Mathf.CeilToInt(cityPopulation / 10);
        industrialDemand = (int)Mathf.CeilToInt(commercialDemand / 2);
        jobDemand = (int)Mathf.CeilToInt(cityPopulation / 4);

        //성장률 조정
        residenceGrowthRate = CalculateGrowthRate(cityPopulation, residenceCapa);
        commercialGrowthRate = CalculateGrowthRate(commercialDemand, commercialCapa);
        industrialGrowthRate = CalculateGrowthRate(industrialDemand, industrialCapa);
        jobGrowthRate = CalculateGrowthRate(jobDemand, jobCapa);

        //모든 성장률의 합
        totalRate = residenceGrowthRate + commercialGrowthRate + industrialGrowthRate + jobGrowthRate;
        //모든 성장률의 평균
        cityGrowthRate = totalRate;

        //UI 반영
    }

    float CalculateGrowthRate(int demand, int capa)
    {
        int gap = demand - capa;

        if (gap < 0)
        {
            gap = 0;
        }

        float growthRate = ((float)gap / capa);

        return growthRate;
    }


    public string ChoiceBuildingType()
    {
        string type = "";
        float rand = Random.Range(0, totalRate);

        float interval_0r = residenceGrowthRate;
        float interval_rc = interval_0r + commercialGrowthRate;
        float interval_ci = interval_rc + industrialGrowthRate;
        float interval_ij = interval_ci + jobGrowthRate;
        /*
        Debug.Log(rand);
        Debug.Log(interval_0r);
        Debug.Log(interval_rc);
        */
        if (rand < interval_0r)
        {
            type = "resi";
        }
        else if (rand < interval_rc && rand >= interval_0r)
        {
            type = "comm";
        }
        else if (rand < interval_ci && rand >= interval_rc)
        {
            type = "indu";
        }
        else if (rand <= interval_ij && rand >= interval_ci)
        {
            type = "job";
        }
        else
        {
            Debug.LogError("건물 타입 설정 오류");
            Debug.Log(rand);
            Debug.Log(interval_0r);
            Debug.Log(interval_rc);
            Debug.Log(interval_ci);
            Debug.Log(interval_ij);
        }

        return type;
    }

}


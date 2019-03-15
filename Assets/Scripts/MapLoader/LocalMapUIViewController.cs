using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class LocalMapUIViewController : MonoBehaviour
{
    //인구
    public Text textPopul;
    public GameObject growthIcon;
    public Sprite[] growthIconSpr; // 0:크게감소 1:감소 2:증가 3:크게증가

    //도시 성장
    public GameObject resiParent;
    public GameObject commParent;
    public GameObject induParent;
    public GameObject jobParent;

    //우호도
    public GameObject friendlyDot;

    //시간
    float passtime;

    private void Start()
    {
        SetPopulation();
        SetDemand(true);
    }

    private void Update()
    {
        //5초 간격으로 UI 갱신
        passtime = passtime + Time.deltaTime;
        if(passtime > 5)
        {
            passtime = 0;
            SetPopulation();
            SetDemand(false);

        }

    }

    void ListenEventList()
    {
        EventManager.Instance.Listen("CollectGold", this, EventReceived);
    }


    public void SetPopulation()
    {
        //인구 숫자 표시
        int popul = GameManager.instance.cityPopulation;
        textPopul.text = popul.ToString("N0");

        //성장률에 따른 아이콘 변경
        float growthRate = GameManager.instance.populGrowthRate;
        if(growthRate < -0.05f) // 인구 급감
        {
            growthIcon.GetComponent<SpriteRenderer>().sprite = growthIconSpr[0];
        }
        else if (growthRate >= -0.05f && growthRate < 0f) // 인구 감소
        {
            growthIcon.GetComponent<SpriteRenderer>().sprite = growthIconSpr[1];
        }
        else if (growthRate > 0f && growthRate <= 0.05f) //인구 증가
        {
            growthIcon.GetComponent<SpriteRenderer>().sprite = growthIconSpr[2];
        }
        else if (growthRate > 0.05f) //인구 급증
        {
            growthIcon.GetComponent<SpriteRenderer>().sprite = growthIconSpr[3];
        }
        else if (growthRate == 0) //인구 변동 없음
        {
            growthIcon.GetComponent<SpriteRenderer>().sprite = null;
        }
    }

    public void SetDemand(bool immedi)
    {
        float resiGrowthRate = GameManager.instance.residenceGrowthRate;
        float commGrowthRate = GameManager.instance.commercialGrowthRate;
        float induGrowthRate = GameManager.instance.industrialGrowthRate;
        float jobGrowthRate = GameManager.instance.jobGrowthRate;

        float sqrResi = resiGrowthRate * resiGrowthRate;
        float sqrComm = commGrowthRate * commGrowthRate;
        float sqrIndu = induGrowthRate * induGrowthRate;
        float sqrJob = jobGrowthRate * jobGrowthRate;

        float normalizeFactor = 1 / Mathf.Sqrt(sqrResi + sqrComm + sqrIndu + sqrJob);

        float totalGrowth = GameManager.instance.totalRate;

        //수요 성장률의 합이 1을 넘어가면 모든 성장률 normalize
        if (totalGrowth >= 1)
        {
            resiGrowthRate = resiGrowthRate / totalGrowth;
            commGrowthRate = commGrowthRate / totalGrowth;
            induGrowthRate = induGrowthRate / totalGrowth;
            jobGrowthRate = jobGrowthRate / totalGrowth;
        }

        //실제로 움직일 게이지 비율 계산

        commGrowthRate = resiGrowthRate + commGrowthRate;
        induGrowthRate = commGrowthRate + induGrowthRate;
        jobGrowthRate = induGrowthRate + jobGrowthRate;

        float gaugeDuration = 1;

        if(immedi)
        {
            gaugeDuration = 0;
        }

        //게이지 변동
        resiParent.transform.DOScaleX(resiGrowthRate, gaugeDuration);
        commParent.transform.DOScaleX(commGrowthRate, gaugeDuration);
        induParent.transform.DOScaleX(induGrowthRate, gaugeDuration);
        jobParent.transform.DOScaleX(jobGrowthRate, gaugeDuration);

    }

    public void EventReceived(string eventId, UnityEngine.Object target, object param)
    {
        string newsText = "";
        switch (eventId)
        {
            case "StartGame":
                newsText = "히어로가 ";
                break;
        }

        ShowNews(newsText);

    }

    void ShowNews(string newsText)
    {

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionTileScript : MonoBehaviour
{

    public string beforeTileType;
    int constStepInt;
    string _tileDetail;
    int i_tileDetail;
    int newTileDetail;

    //어떤 종류의 건물을 얼마나 지었는지 저장
    //ex) R2_50; <- r 타입의 2번째 크기 건물을 50% 만큼 건설

    public void StartConstruction()
    {
        //신규 건설과 증축 구분(스프라이트로 구분하면 된다)
        string constStep = GetComponent<GeneralTileScript>().tileClassVariation;
        string _tileDetail = GetComponent<GeneralTileScript>().tileClassDetail;
        i_tileDetail = System.Convert.ToInt32(_tileDetail);
        newTileDetail = i_tileDetail;

        constStepInt = System.Convert.ToInt32(constStep);
        StartCoroutine(ConstructionStep());
    }

    IEnumerator ConstructionStep()
    {
        for (int i = constStepInt; i < 11; i++)
        {
            if (i == 0 && newTileDetail < 7)// 공사 초기에 저장되어 있는 경우, 두 번 더해지는 문제가 있으므로 해당 문제 방지용
            {
                newTileDetail = i_tileDetail + 6; //공사중이므로 detailvalue + 6
            }

            //10번째 스텝에서 건설 완료
            if (i > 9)
            {
                FinishConstruction();
            }
            else //그 외의 경우
            {
                string _tileClass = GetComponent<GeneralTileScript>().tileClass;
                //string _tileDetail = GetComponent<GeneralTileScript>().tileClassDetail;
                string _newTileVariation = i.ToString();

                string newType = _tileClass + newTileDetail.ToString() + "_" + _newTileVariation;
                GetComponent<GeneralTileScript>().tileType = newType;

                //Debug.Log()
                GetComponent<GeneralTileScript>().UpdateThisObject();
                GetComponent<GeneralTileScript>().ChangeSprite();
                //현재 타일 상태를 업데이트

            }
            yield return new WaitForSeconds(1);
        }
    }

    void FinishConstruction()
    {
        constStepInt = 0;


        //건물 스크립트가 없으면 건물 부착
        if (GetComponent<BuildingTileScript>() == null)
        {
            gameObject.AddComponent<BuildingTileScript>();
        }
        
        //emptyTileScript가 남아있으면 제거
        if(GetComponent<EmptyTileScript>() != null)
        {
            Destroy(GetComponent<EmptyTileScript>());
        }

        //스프라이트 변경
        string _tileClass = GetComponent<GeneralTileScript>().tileClass;
        string _tileDetail = GetComponent<GeneralTileScript>().tileClassDetail;
        int newtileDetail = System.Convert.ToInt32(_tileDetail) - 6;
        string _newTileVariation = "0";

        string newType = _tileClass + newtileDetail.ToString() + "_" + _newTileVariation;

        GetComponent<GeneralTileScript>().tileType = newType;
        GetComponent<GeneralTileScript>().UpdateThisObject();
        GetComponent<GeneralTileScript>().ChangeSprite();

        //Debug.Log(_tileDetail + " " + newtileDetail);

        //타입에 맞게 공급 변경
        CapaUpgrade(_tileClass, newtileDetail);

        //건설 종료 알림
        GetComponent<BuildingTileScript>().FinishUpgrade();
    }

    void CapaUpgrade(string buildingType, int _buildingSize)
    {
        

        //상가와 공장도 일자리 제공
        if (buildingType == "r")
        {
            GameManager.instance.residenceCapa = GameManager.instance.residenceCapa + 5 * System.Convert.ToInt32(Mathf.Pow(2, 3*(_buildingSize - 1)));
        }
        else if (buildingType == "i")
        {
            //GameManager.instance.industrialCapa = GameManager.instance.industrialCapa + (300 * _buildingSize - 400);
            GameManager.instance.industrialCapa = GameManager.instance.industrialCapa + 10 * System.Convert.ToInt32(Mathf.Pow(2, 3 * (_buildingSize - 1)));
            GameManager.instance.jobCapa = GameManager.instance.jobCapa + 3 * System.Convert.ToInt32(Mathf.Pow(2, 3 * (_buildingSize - 1)));
        }
        else if (buildingType == "c")
        {
            //GameManager.instance.commercialCapa = GameManager.instance.commercialCapa + (300 * _buildingSize - 400);
            GameManager.instance.commercialCapa = GameManager.instance.commercialCapa + 10 * System.Convert.ToInt32(Mathf.Pow(2, 3 * (_buildingSize - 1)));
            GameManager.instance.jobCapa = GameManager.instance.jobCapa + 3 * System.Convert.ToInt32(Mathf.Pow(2, 3 * (_buildingSize - 1)));
        }
        else if (buildingType == "j")
        {
            //GameManager.instance.jobCapa = GameManager.instance.jobCapa + (300 * _buildingSize - 400);
            GameManager.instance.jobCapa = GameManager.instance.jobCapa + 10 * System.Convert.ToInt32(Mathf.Pow(2, 3 * (_buildingSize - 1)));
        }
    }
}

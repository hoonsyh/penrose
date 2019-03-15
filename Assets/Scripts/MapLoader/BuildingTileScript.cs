using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingTileScript : MonoBehaviour
{
    public bool isGrow;
    string a_tileType;
    string buildingType;
    int buildingSize;
    string[] nearTileType = new string[4]; //차례대로 0시, 3시, 6시, 9시 방향

    int passDirectionX = 100;
    int passDirectionY = 100;


    private void Start()
    {
        //자신의 y위치와 z위치를 일치 시킴
        //z포지션을 -y포지션과 같게 <- 위에 있으면 뒤로 숨겨진다
        float zPosition = (transform.position.y - 4) / 10;
        Vector3 newPosition = new Vector3(transform.position.x, transform.position.y, zPosition);
        this.gameObject.transform.position = newPosition;

        //현재 타일의 종류
        try
        {
            a_tileType = GetComponent<GeneralTileScript>().tileType;
        }
        catch(System.NullReferenceException e)
        {
            Debug.LogError(e);
            Debug.Log(gameObject);
        }

        buildingType = a_tileType[0].ToString();
        buildingSize = (int)System.Char.GetNumericValue(a_tileType[1]) ;
        StartCoroutine(BuildingGrowth());
    }


    //성장률에 따라 건물의 성장 여부 판단
    IEnumerator BuildingGrowth()
    {
        string _tileDetail = GetComponent<GeneralTileScript>().tileClassDetail;
        buildingSize = System.Convert.ToInt32(_tileDetail);

        while (!isGrow && buildingSize != 3)
        {
            //임의의 수와 성장률을 비교해서 공사 여부 판단
            float rand = Random.Range(0.0f, 100.0f);
            float growthRate = GameManager.instance.cityGrowthRate;

            if(buildingType == "r")
            {
                growthRate = GameManager.instance.residenceGrowthRate;
            }
            else if(buildingType == "i")
            {
                growthRate = GameManager.instance.industrialGrowthRate;
            }
            else if (buildingType == "c")
            {
                growthRate = GameManager.instance.commercialGrowthRate;
            }
            else if (buildingType == "j")
            {
                growthRate = GameManager.instance.jobGrowthRate;
            }

            if (rand < growthRate)
            {
                //건물 타입 결정(거주지, 은행, 상점 등등)

                //공사 진행
                StartCoroutine(StartUpgrade());

                //공터가 아니라는 불리언
                isGrow = true;
            }
            yield return new WaitForSeconds(GameManager.instance.cityGrowthTime);
        }
        yield return new WaitForSeconds(0);
    }

    IEnumerator StartUpgrade()
    {
        //현재 건물 크기에 따라 다음 크기의 건물로 업그레이드
        string _tileDetail = GetComponent<GeneralTileScript>().tileClassDetail;
        int buildingSize = System.Convert.ToInt32(_tileDetail);
        string buildingType = GetComponent<GeneralTileScript>().tileClass;

        //Debug.Log("공사시작?");
        int nextSize = buildingSize + 1;
        buildingSize = nextSize;

        //건설 규모에 따라 공급 증가
        //CapaUpgrade(buildingSize);

        GameManager.instance.ReCalculateGrowthRate();

        string _tileType = buildingType + nextSize;

        gameObject.GetComponent<GeneralTileScript>().tileType = _tileType;

        //타일 정보 업데이트
        GetComponent<GeneralTileScript>().UpdateThisObject();

        //타일의 공사시작 메소드 실행
        GetComponent<ConstructionTileScript>().StartConstruction();


        yield return new WaitForSeconds(1);
        //FinishUpgrade(_tileType);

    }

    void CapaUpgrade(int _buildingSize)
    {
        //Debug.Log(buildingType);

        //상가와 공장도 일자리 제공
        if (buildingType == "r")
        {
            GameManager.instance.residenceCapa = GameManager.instance.residenceCapa + (100 * _buildingSize - 150);
        }
        else if (buildingType == "i")
        {
            GameManager.instance.industrialCapa = GameManager.instance.industrialCapa + (300 * _buildingSize - 400);
            GameManager.instance.jobCapa = GameManager.instance.jobCapa + (30 * _buildingSize - 40);
        }
        else if (buildingType == "c")
        {
            GameManager.instance.commercialCapa = GameManager.instance.commercialCapa + (300 * _buildingSize - 400);
            GameManager.instance.jobCapa = GameManager.instance.jobCapa + (30 * _buildingSize - 40);
        }
        else if (buildingType == "j")
        {
            GameManager.instance.jobCapa = GameManager.instance.jobCapa + (300 * _buildingSize - 400);
        }
    }



    public void FinishUpgrade()
    {
        //타일 타입 변경
        /*
        GetComponent<GeneralTileScript>().tileType = _tileType;

        //타일 정보 저장
        GetComponent<GeneralTileScript>().UpdateThisObject();

        GetComponent<GeneralTileScript>().ChangeSprite();*/
        /*
        //키에 맞게 스프라이트 변경
        string keyClass = GetComponent<GeneralTileScript>().tileClass;
        string keyDetail = GetComponent<GeneralTileScript>().tileClassDetail;
        string keyVariation = GetComponent<GeneralTileScript>().tileClassVariation;
        try
        {
            GetComponent<SpriteRenderer>().sprite = SpriteDictionary.instance.smallSprDic[keyClass][keyDetail][keyVariation];
        }
        catch (KeyNotFoundException e)
        {
            Debug.LogError(e);
            Debug.Log(keyClass + " " + keyDetail + " " + keyVariation);
        }
        */
        //건물이 빌라 이상이면 주변에 집 건설 시작

        if (buildingSize >= 2)
        {
            BuildNewHouse(buildingType);
        }

        isGrow = false;
        StartCoroutine(BuildingGrowth());
    }


    //근처에 공터 타일이 있으면 집 건설

    //사방에 건물이 건설되어 있는지 확인 후 정보 저장
    void BuildNewHouse(string buildingType)
    {
        int xValue = (int)GetComponent<GeneralTileScript>().myPosition.x;
        int yValue = (int)GetComponent<GeneralTileScript>().myPosition.y;

        int[] xOffset = new int[4] { 0, 1, 0, -1 };
        int[] yOffset = new int[4] { -1, 0, 1, 0 };

        //int[,] checkTileValue = new int[4, 2] { { xValue, yValue - 1 }, { xValue + 1, yValue }, { xValue, yValue + 1 }, { xValue - 1, yValue } };
        int[,] checkTileValue = new int[4, 2] { { yValue - 1, xValue }, { yValue, xValue + 1 }, { yValue + 1, xValue }, { yValue, xValue - 1 } };

        for (int i = 0; i < 4; i++)
        {
            //최대값, 최소값을 벗어나면 스크립트 호출 x
            if (checkTileValue[i, 0] >= 0 && checkTileValue[i, 0] < GameManager.instance.maxMapY && checkTileValue[i, 1] >= 0 && checkTileValue[i, 1] < GameManager.instance.maxMapX)
            {
                nearTileType[i] = GameManager.instance.tileObject[checkTileValue[i, 0], checkTileValue[i, 1]].GetComponent<GeneralTileScript>().tileType;
            }
            else
            {
                if(!(checkTileValue[i, 0] >= 0 && checkTileValue[i, 0] < GameManager.instance.maxMapY))
                {
                    passDirectionY = i;
                }
                if (!(checkTileValue[i, 1] >= 0 && checkTileValue[i, 1] < GameManager.instance.maxMapX))
                {
                    passDirectionX = i;
                }

            }


        }


        for (int i = 0; i < 4; i++)
        {
            //가장자리가 아닌 곳에서만 건설 메소드 실행
            if(i == passDirectionX || i == passDirectionY)
            {

            }
            else
            {
                nearTileType[i] = GameManager.instance.tileObject[yValue + yOffset[i], xValue + xOffset[i]].GetComponent<GeneralTileScript>().tileType;
                //공터일 경우 해당 게임오브젝트의 건설메소드 실행
                if (nearTileType[i][0] == 'e')
                {
                    Vector2 offsetVector = new Vector2(-xOffset[i], -yOffset[i]);

                    GameManager.instance.tileObject[yValue + yOffset[i], xValue + xOffset[i]].GetComponent<EmptyTileScript>().StartConstructionFromHouse(buildingType);

                }
            }

            
        }
    }
}

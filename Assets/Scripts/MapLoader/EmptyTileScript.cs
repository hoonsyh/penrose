using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmptyTileScript : MonoBehaviour
{
    bool[] openDirectionBool = new bool[4] { false, false, false, false }; //차례대로 0시, 3시, 6시, 9시 방향
    char[] roadOpenChar = new char[4];
    string roadType;
    bool isEmpty = true;
    bool roadConstComplete;

    //도로타일로부터 건설 요청을 받으면 도로 건설
    public void StartConstructionFromRoad(Vector2 originRoadDirection, bool originRoadOpen)
    {
        if(originRoadOpen)
        {
            StartCoroutine(ConstructionRoad(originRoadDirection));
        }
        //닫혀있는 방향은 건물 건설
        else if(!originRoadOpen)
        {
            //성장률에 따라 확률적으로 건설
            string building = GameManager.instance.ChoiceBuildingType();
            float probability = 0;

            if(building == "resi")
            {
                probability = GameManager.instance.residenceGrowthRate;
            }
            else if (building == "comm")
            {
                probability = GameManager.instance.commercialGrowthRate;
            }
            else if (building == "indu")
            {
                probability = GameManager.instance.industrialGrowthRate;
            }
            else if (building == "job")
            {
                probability = GameManager.instance.jobGrowthRate;
            }

            StartCoroutine(TryBuildHouse(building, probability));
        }
    }

    IEnumerator ConstructionRoad(Vector2 originRoadDirection)
    {
        while(!roadConstComplete)
        {
            float rand = Random.Range(0f, 100f);

            if(rand < GameManager.instance.cityGrowthRate * 2)
            {
                roadConstComplete = true;

                //도로 건설
                DirectionToBinary(originRoadDirection);
                NewRoadType();
                //ChangeSprToRoad();
                AddRoadScript();
                RemoveThisScript();
            }
            yield return new WaitForSeconds(GameManager.instance.cityGrowthTime);
        }
    }


    //집 타일로부터 건설 요청 <- 단지 형성을 위해 근처에 건물 건설 확률 10배 증가
    public void StartConstructionFromHouse(string buildingType)
    {
        string _buildingType = "";
        float growthRate = 0;


        if (buildingType == "r")
        {
            _buildingType = "resi";
            growthRate = GameManager.instance.residenceGrowthRate;
        }
        else if (buildingType == "i")
        {
            _buildingType = "indu";
            growthRate = GameManager.instance.industrialGrowthRate * 100;
        }
        else if (buildingType == "comm")
        {
            _buildingType = "resi";
            growthRate = GameManager.instance.commercialGrowthRate * 100;
        }
        else if (buildingType == "job")
        {
            _buildingType = "resi";
            growthRate = GameManager.instance.jobGrowthRate * 100;
        }


        StartCoroutine(TryBuildHouse(_buildingType, growthRate));
    }

    //////////////////////////도로 건설/////////////////////////

    void DirectionToBinary(Vector2 originRoadDirection)
    {
        //건설 요청을 받은 타일의 방향 필요
        int xValue = (int)originRoadDirection.x;
        int yValue = (int)originRoadDirection.y;

        if(xValue == 1)
        {
            openDirectionBool[1] = true;
            RandomOpen(1);
            //openDirectionBool[3] = true; //임시
        }
        else if (xValue == -1)
        {
            openDirectionBool[3] = true;
            RandomOpen(3);
            //openDirectionBool[1] = true; //임시
        }
        else if (yValue == 1)
        {
            openDirectionBool[2] = true;
            RandomOpen(2);
            //openDirectionBool[0] = true; //임시
        }
        else if (yValue == -1)
        {
            openDirectionBool[0] = true;
            RandomOpen(0);
            //openDirectionBool[2] = true; //임시
        }

    }
    
    void RandomOpen(int skipIndex)
    {
        int straightAffi = 1;
        if(skipIndex >= 2)
        {
            straightAffi = -1;
        }

        //i방향으로 road를 open할지 확인하는 for문
        //for (int i = 0; i < 4; i++)
        //{
            //i가 길이 있던 방향이라면 스킵, 그 외의 경우에만 open 여부 결정
            //if(i != skipIndex)
            //{
        float rand = Random.Range(0, 100f);

        //T 모양, 십자 교차로
        if(rand > 5)
        {
            //직진 방향 open
            openDirectionBool[skipIndex + straightAffi * 2] = true;
            //왼쪽 방향
            if(rand > 90)
            {
                openDirectionBool[LimitInt(skipIndex + 1)] = true;
            }
            //오른쪽 방향
            if (rand > 93)
            {
                openDirectionBool[LimitInt(skipIndex - 1)] = true;
                openDirectionBool[LimitInt(skipIndex + 1)] = false;
            }
            //십자 교차로
            if (rand > 96)
            {
                openDirectionBool[LimitInt(skipIndex + 1)] = true;
            }


        }
        //커브만
        else if(rand > 2 && rand <= 5)
        {
            openDirectionBool[LimitInt(skipIndex + 1)] = true;
        }
        else
        {
            openDirectionBool[LimitInt(skipIndex - 1)] = true;
        }

        /*
        for (int i = 0; i < 4; i++)
        {
            int rand = Random.Range(0, 100);
            if(rand < 10)
            {
                openDirectionBool[i] = true;
            }
        }*/
    }
    
    int LimitInt(int openInt)
    {
        int _openInt = openInt;

        if (_openInt > 3)
        {
            _openInt = 0;
        }

        if (_openInt < 0)
        {
            _openInt = 3;
        }

        return _openInt;
    }


    //어떤 방향으로 타일을 만들 것인지
    void NewRoadType()
    {
        //4방향 중 어디가 오픈되어 있는지 char 형태로 추출
        for (int i = 0; i < 4; i++)
        {
            //0이면 닫혀있음, 1이면 열려있음 데이터 저장
            if (openDirectionBool[i])
            {
                roadOpenChar[i] = '1';
            }
            else if (!openDirectionBool[i])
            {
                roadOpenChar[i] = '0';
            }
        }

        roadType = roadOpenChar[0].ToString() + roadOpenChar[1].ToString() + roadOpenChar[2].ToString() + roadOpenChar[3].ToString();
    }

    //현재 타일의 스프라이트를 도로 스프라이트로 변경
    /*
    void ChangeSprToRoad()
    {
        //Sprite roadSpr = SpriteDictionary.instance.roadSprDic[roadType];
        gameObject.GetComponent<SpriteRenderer>().sprite = roadSpr;
    }
    */
    //도로 스크립트 추가
    void AddRoadScript()
    {
        string tileType = "w" + roadType;

        gameObject.AddComponent<RoadTileScript>();
        RoadTileScript rts = gameObject.GetComponent<RoadTileScript>();
        GetComponent<GeneralTileScript>().tileType = tileType;
        rts.SetTileData();

        GetComponent<GeneralTileScript>().tileType = tileType;

    }



    ////////////////////////////////거주지 건설///////////////////////////

    IEnumerator TryBuildHouse(string type, float probability)
    {
        string building = GameManager.instance.ChoiceBuildingType();

        while (isEmpty)
        {
            //임의의 수와 성장률을 비교해서 공사 여부 판단
            int rand = Random.Range(0, 100);
            //float growthRate = GameManager.instance.cityGrowthRate;
            //float typeRate = GameManager.instance.residenceGrowthRate;

            if (rand < probability)
            {
                //공사중 스프라이트로 변경
                //GetComponent<SpriteRenderer>().sprite = SpriteDictionary.instance.emptySprDic["const"];

                //공사 스크립트 부착
                gameObject.AddComponent<ConstructionTileScript>();

                //건물 타입 결정(거주지, 은행, 상점 등등)
                if (type == "resi")
                {
                    GetComponent<GeneralTileScript>().tileType = "r1";
                    
                    //공사 진행

                    //StartCoroutine(ResidenceBuild());
                }
                else if(type == "indu")
                {
                    GetComponent<GeneralTileScript>().tileType = "i1";
                    //StartCoroutine(IndustyBuild());
                }
                else if (type == "comm")
                {
                    GetComponent<GeneralTileScript>().tileType = "c1";
                    //StartCoroutine(CommercialBuild());
                }
                else if (type == "job")
                {
                    GetComponent<GeneralTileScript>().tileType = "j1";
                    //StartCoroutine(JobBuild());
                }
                GetComponent<GeneralTileScript>().UpdateThisObject();

                //타일의 공사시작 메소드 실행
                GetComponent<ConstructionTileScript>().StartConstruction();


                //공터가 아니라는 불리언
                isEmpty = false;
            }


            yield return new WaitForSeconds(GameManager.instance.cityGrowthTime);
        }       
    }


    IEnumerator ResidenceBuild()
    {
        //건설 규모에 맞게 공급 증가(+4)
        GameManager.instance.residenceCapa = GameManager.instance.residenceCapa + 4;
        GameManager.instance.ReCalculateGrowthRate();

        //공사 시작 5초 후 작은 집으로 변경
        yield return new WaitForSeconds(5);

        //스프라이트 변경

        GetComponent<GeneralTileScript>().tileType = "r1"; 

        //건물 스크립트 추가
        gameObject.AddComponent<BuildingTileScript>();
        RemoveThisScript();
    }

    IEnumerator IndustyBuild()
    {
        //건설 규모에 맞게 공급 증가(+4)
        GameManager.instance.industrialCapa = GameManager.instance.industrialCapa + 50;
        GameManager.instance.ReCalculateGrowthRate();

        //공사 시작 5초 후 작은 집으로 변경
        yield return new WaitForSeconds(5);
        //스프라이트 변경

        GetComponent<GeneralTileScript>().tileType = "i1";

        //건물 스크립트 추가
        gameObject.AddComponent<BuildingTileScript>();
        RemoveThisScript();
    }

    IEnumerator CommercialBuild()
    {
        //건설 규모에 맞게 공급 증가(+4)
        GameManager.instance.commercialCapa = GameManager.instance.commercialCapa + 50;
        GameManager.instance.ReCalculateGrowthRate();

        //공사 시작 5초 후 작은 집으로 변경
        yield return new WaitForSeconds(5);
        //스프라이트 변경

        GetComponent<GeneralTileScript>().tileType = "c1";

        //건물 스크립트 추가
        gameObject.AddComponent<BuildingTileScript>();
        RemoveThisScript();
    }

    IEnumerator JobBuild()
    {
        //건설 규모에 맞게 공급 증가(+4)
        GameManager.instance.jobCapa = GameManager.instance.jobCapa + 50;
        GameManager.instance.ReCalculateGrowthRate();

        //공사 시작 5초 후 작은 집으로 변경
        yield return new WaitForSeconds(5);

        GetComponent<GeneralTileScript>().tileType = "j1";  

        //건물 스크립트 추가
        gameObject.AddComponent<BuildingTileScript>();
        RemoveThisScript();
    }

    //emptyTileScript 제거, 현재 타일 정보 저장
    void RemoveThisScript()
    {
        Destroy(GetComponent<EmptyTileScript>());
        GetComponent<GeneralTileScript>().UpdateThisObject();
        gameObject.GetComponent<GeneralTileScript>().ChangeSprite();
    }
    /*
    void ChangeSprite()
    {
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
    }*/
}

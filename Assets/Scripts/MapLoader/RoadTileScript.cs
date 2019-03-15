using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadTileScript : MonoBehaviour, IKeyLoad
{
    bool[] openDirectionBool = new bool[4] { false, false, false, false }; //차례대로 0시, 3시, 6시, 9시 방향
    bool[] constructionDirectionBool = new bool[4] { false, false, false, false }; //차례대로 0시, 3시, 6시, 9시 방향
    string[] nearTileType = new string[4]; //차례대로 0시, 3시, 6시, 9시 방향

    string key;

    int mapMaxXsize = 50;
    int mapMaxYsize = 30;
    int passDirectionX = 100;
    int passDirectionY = 100;

    public void SetTileData()
    {
        GetKey();
        CheckOpenDirection();

        Invoke("CheckNearConstruct",5f);      
    }

    public void GetKey()
    {
        key = GetComponent<GeneralTileScript>().tileType;
    }

    //어느 방향으로 열려있는지 정보 저장
    void CheckOpenDirection()
    {
        //Debug.Log(tileType);
        string intBool = GetComponent<GeneralTileScript>().tileType.Substring(1);
        //Debug.Log(intBool);
        char[] intBoolSet = new char[4];

        //4방향 중 어디가 오픈되어 있는지 char 형태로 추출
        for (int i = 0; i < 4; i++)
        {
            intBoolSet[i] = intBool[i];

            //0이면 닫혀있음, 1이면 열려있음 데이터 저장
            if(intBoolSet[i] == '0')
            {
                openDirectionBool[i] = false;
            }
            else if(intBoolSet[i] == '1')
            {
                openDirectionBool[i] = true;
            }
        }

        //스프라이트 호출

        gameObject.GetComponent<GeneralTileScript>().ChangeSprite();
    }

    //사방에 건물이 건설되어 있는지 확인 후 정보 저장
    void CheckNearConstruct()
    {
        int xValue = (int)GetComponent<GeneralTileScript>().myPosition.x;
        int yValue = (int)GetComponent<GeneralTileScript>().myPosition.y;

        int[] xOffset = new int[4] { 0, 1, 0, -1 };
        int[] yOffset = new int[4] { -1, 0, 1, 0 };

        //int[,] checkTileValue = new int[4, 2] { { xValue, yValue - 1 }, { xValue + 1, yValue }, { xValue, yValue + 1 }, { xValue - 1, yValue } };
        int[,] checkTileValue = new int[4, 2] { { yValue - 1, xValue}, { yValue, xValue + 1}, { yValue + 1, xValue}, { yValue, xValue - 1} };

        for (int i = 0; i < 4; i++)
        {
            //최대값, 최소값을 벗어나면 스크립트 호출 x
            if(checkTileValue[i, 0] >= 0 && checkTileValue[i, 0] < GameManager.instance.maxMapY && checkTileValue[i, 1] >= 0 && checkTileValue[i, 1] < GameManager.instance.maxMapX)
            {
                //Debug.Log(GameManager.instance.maxMapX);
                //Debug.Log(checkTileValue[i, 0] + " " + checkTileValue[i, 1]);
                nearTileType[i] = GameManager.instance.tileObject[checkTileValue[i, 0], checkTileValue[i, 1]].GetComponent<GeneralTileScript>().tileType;
            }
            else
            {
                if (!(checkTileValue[i, 0] >= 0 && checkTileValue[i, 0] < GameManager.instance.maxMapY))
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

                    GameManager.instance.tileObject[yValue + yOffset[i], xValue + xOffset[i]].GetComponent<EmptyTileScript>().StartConstructionFromRoad(offsetVector, openDirectionBool[i]);

                }
                //도로일 경우 현재 도로의 진행방향을 추가하여 합침
                else if (nearTileType[i][0] == 'w' && openDirectionBool[i])
                {
                    bool targetRoadOpen = GameManager.instance.tileObject[yValue + yOffset[i], xValue + xOffset[i]].GetComponent<RoadTileScript>().openDirectionBool[LimitInt(i + 2)];

                    //기존에 건설되어 있는 도로에 연결
                    if (!targetRoadOpen)
                    {
                        GameManager.instance.tileObject[yValue + yOffset[i], xValue + xOffset[i]].GetComponent<RoadTileScript>().MergeRoad(i + 2);
                        //Debug.Log("xOffset[i] : " + xOffset[i] + "  yOffset[i] : " + yOffset[i]  +  "  i : " + i);
                        //Debug.Log("호출하는 타일 : " + intBool + " " + GetComponent<GeneralTileScript>().myPosition);
                    }
                }
                else if (openDirectionBool[i]) // 그 외의 경우 도로가 열려있는데 건물에 막혀있을 경우
                {
                    //현재의 도로를 교차로로 변경
                    /*
                    for (int j = 0; j < 4; j++)
                    {
                        openDirectionBool[j] = true;
                        Vector2 offsetVector = new Vector2(-xOffset[j], -yOffset[j]);
                        GameManager.instance.tileObject[(xValue + xOffset[j]), (yValue + yOffset[j])].GetComponent<EmptyTileScript>().StartConstructionFromRoad(offsetVector, openDirectionBool[j]);
                    }

                    Sprite crossSpr = SpriteDictionary.instance.roadSprDic["1111"];
                    gameObject.GetComponent<SpriteRenderer>().sprite = crossSpr;*/
                }
            }
            
        } 
    }
    
    //
    public void MergeRoad(int directionInt)
    {
        //연결할 방향
        int connectDirectionInt = LimitInt(directionInt);
        try
        {
            openDirectionBool[connectDirectionInt] = true;
        }
        catch(System.IndexOutOfRangeException e)
        {
            Debug.LogError(e);
            Debug.Log(connectDirectionInt);
        }
        

        char[] openDirectionChar = new char[4];
        for (int i = 0; i < 4; i++)
        {
            if(openDirectionBool[i])
            {
                openDirectionChar[i] = '1';
            }
            else if(!openDirectionBool[i])
            {
                openDirectionChar[i] = '0';
            }
        }

        string intBool = openDirectionChar[0].ToString() + openDirectionChar[1].ToString() + openDirectionChar[2].ToString() + openDirectionChar[3].ToString();

        //타일 타입 변경
        gameObject.GetComponent<GeneralTileScript>().tileType = "w" + intBool;
        gameObject.GetComponent<GeneralTileScript>().UpdateThisObject();
        gameObject.GetComponent<GeneralTileScript>().ChangeSprite();
    }

    int LimitInt(int openInt)
    {
        int _openInt = openInt;

        if (_openInt >= 4)
        {
            _openInt = _openInt - 4;
        }
        

        return _openInt;
    }

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
    }
}

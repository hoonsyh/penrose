using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IKeyLoad
{
    void SetTileData();
}

public class GeneralTileScript : MonoBehaviour
{
    public Vector2 myPosition;
    public string tileType;

    public string tileClass; //타일의 종류
    public string tileClassDetail; //타일의 세부종류
    public string tileClassVariation; //타일의 바리에이션

    public Sprite largeSpr;

    private void Start()
    {
        UpdateThisObject();
    }

    //전체맵 데이터에 현재 타일 정보 업데이트
    public void UpdateThisObject()
    {
        myPosition = gameObject.GetComponent<GeneralTileScript>().myPosition; // 자기 자신에게 위치를 할당해야 버그 x
        tileType = gameObject.GetComponent<GeneralTileScript>().tileType;

        GameManager.instance.tileObject[(int)myPosition.y, (int)myPosition.x] = this.gameObject;

        string[] tileClassMember = tileType.Split('_'); // _ 기준으로 앞뒤를 나눈다
        //Debug.Log(tileType);
        //Debug.Log()
        //바리에이션이 없는 경우
        if (tileClassMember.Length == 1)
        {
            tileClass = tileType[0].ToString();
            tileClassDetail = tileType.Substring(1);
            //csv 마지막 열 데이터에 공백이 쌓이는 것을 삭제
            tileClassDetail = tileClassDetail.Trim();
 
            tileClassVariation = "0";
        }
        else //바리에이션이 있는 경우
        {
            tileClass = tileClassMember[0][0].ToString(); // 맨 앞 한 글자만
            tileClassDetail = tileClassMember[0].Substring(1); // 앞 글자 제외 뒤 글자만
            //csv 마지막 열 데이터에 공백이 쌓이는 것을 삭제
            tileClassDetail = tileClassDetail.Trim();

            if (tileClassMember.Length > 1)
            {
                tileClassVariation = tileClassMember[1]; //_ 기준으로 뒤
            }
        }
    }

    public void ChangeSprite()
    {
        //키에 맞게 스프라이트 변경
        string keyClass = GetComponent<GeneralTileScript>().tileClass;
        string keyDetail = GetComponent<GeneralTileScript>().tileClassDetail;
        string keyVariation = GetComponent<GeneralTileScript>().tileClassVariation;
        try
        {
            //도로 타일이 아닌 경우에만
            if(keyClass != "w" && keyClass != "e")
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

            GetComponent<SpriteRenderer>().sprite = SpriteDictionary.instance.smallSprDic[keyClass][keyDetail][keyVariation];
        }
        catch (KeyNotFoundException e)
        {
            Debug.LogError(e);
            Debug.Log(gameObject.name);
            Debug.Log(keyClass + " " + keyDetail + " " + keyVariation);
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildingSetPosition : MonoBehaviour
{
    //건물 체력
    public float hp = 100;
    public Vector3Int relativeVector;


    void Start()
    {
        //자신의 y위치와 z위치를 일치 시킴
        //z포지션을 -y포지션과 같게 <- 위에 있으면 뒤로 숨겨진다
        float zPosition = transform.position.y - 2.3f;
        Vector3 newPosition = new Vector3(transform.position.x, transform.position.y, zPosition);
        this.gameObject.transform.position = newPosition;

        
    }

    public void DestroyBuilding()
    {
        //hp가 0이면 빌딩 스프라이트 변경, collider 삭제
        GameObject childObj = transform.GetChild(0).gameObject;
        BoxCollider2D coll = childObj.GetComponent<BoxCollider2D>();
        RangeColliderScript rcs = childObj.GetComponent<RangeColliderScript>();

        coll.enabled = false;
        rcs.enabled = false;

        //스프라이트 변경
        childObj.GetComponent<SpriteRenderer>().sprite = SpriteDictionary.instance.largeSprDic["e"]["destroy"]["0"];
        //소팅 오더 변경(ground로)
        childObj.GetComponent<SpriteRenderer>().sortingLayerName = "TileGround";
        childObj.GetComponent<SpriteRenderer>().sortingOrder = 1;

        //파괴된 데이터 저장
        SaveDestroyedData();


    }

    void SaveDestroyedData()
    {
        //this 오브젝트의 실제 벡터
        Vector3Int battleVector = MainBattleManager.instance.battleLocationVector;
        Vector3Int absoluteVector = battleVector + relativeVector;

        //
        //Debug.Log(absoluteVector);
        GameManager.instance.tempSaveMap[-absoluteVector.y][absoluteVector.x] = "edestroy";
        //gameObject.GetComponent<GeneralTileScript>().tileType = _tileType;
    }
}

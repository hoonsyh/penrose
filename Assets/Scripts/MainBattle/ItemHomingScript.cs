using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class ItemHomingScript : MonoBehaviour {

    //아이템 데이터
    int goldCount;
    public Text uiGold;

    bool collect;
    GameObject userUnit;
    float speed = 0.5f;

    private void Start()
    {

        userUnit = transform.parent.parent.GetChild(1).GetChild(0).gameObject;
        
    }

    // Use this for initialization
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "itemCollect")
        {
            collect = true;
        }
    }
    // Update is called once per frame
    void Update () {
		
        if(collect)
        {
            MoveItem();
        }

	}


    void MoveItem()
    {
        Vector2 originPosi = gameObject.transform.position;
        Vector2 userPosi = userUnit.transform.position;

        speed = speed + 0.2f;
        float moveTime = CalcDistance(originPosi, userPosi) / speed;
        gameObject.transform.DOKill();
        gameObject.transform.DOMove(userPosi, moveTime).SetEase(Ease.Linear);  

        //아이템이 유저와 일정 거리 이하가 되면 아이템 수집
        if(CalcDistance(originPosi, userPosi) <= 0.05f)
        {
            //아이템 종류에 따라 아이템 수집
            if(gameObject.tag == "itemHeal")
            {
                EventManager.Instance.Broadcast("EatHeal", null);


            }

            if(gameObject.tag == "itemGold")
            {
                //적에게 할당되어있는 골드 정보 로드(나중에 추가)
                int getGold = 2;
  
                //UI에 반영
                EventManager.Instance.Broadcast("CollectGold", getGold);
            }


            Destroy(gameObject);
        }
       
    }

    float CalcDistance(Vector2 origin, Vector2 target)
    {
        float dx = origin.x - target.x;
        float dy = origin.y - target.y;

        float distance = Mathf.Sqrt(dx * dx + dy * dy);

        return distance;
    }
}

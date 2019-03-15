using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using cakeslice;

public class BoxScript : MonoBehaviour {

    GameObject userUnit;
    public GameObject itemHeal;


    private void Start()
    {
        //Debug.Log(transform.parent.parent.gameObject);
        userUnit = transform.parent.parent.GetChild(1).GetChild(0).gameObject;

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //유저 유닛에게 공격 받을 경우
        //유저 유닛은 inEngage == true 이어야만 한다
        //bool userInEngage = userUnit.GetComponent<UserAction>().isEngage;
        
        if (collision.transform.parent.tag == "userUnit")
        {
            userUnit = collision.transform.parent.gameObject;
            BreakBox();
        }
    }

    void BreakBox()
    {
        //targetLock, forceMoving 해제
        //userUnit.GetComponent<UserAction>().targetLock = false;
        //userUnit.GetComponent<UserAction>().isForceMoving = false;

        //아이템 드롭
        GameObject createdItem = Instantiate(itemHeal);
        createdItem.transform.SetParent(gameObject.transform.parent);
        createdItem.transform.position = gameObject.transform.position;

        Destroy(gameObject);
    }

    


    // Update is called once per frame
    void Update () {

        //z포지션을 y포지션과 같게 <- 위에 있으면 뒤로 숨겨진다
        float zPosition = transform.position.y;
        Vector3 newPosition = new Vector3(transform.position.x, transform.position.y, zPosition);
        this.gameObject.transform.position = newPosition;
        /*
        //자신이 타겟이라면 아웃라인 표시
        if(userUnit != null)
        {
            if (userUnit.GetComponent<UserAction>().target != gameObject)
            {
                GetComponent<Outline>().eraseRenderer = true;
            }
            else if (userUnit.GetComponent<UserAction>().target == gameObject)
            {
                GetComponent<Outline>().eraseRenderer = false;
            }
        }
        */
    }
}

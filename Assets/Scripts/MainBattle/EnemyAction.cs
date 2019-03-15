using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using cakeslice;

public class EnemyAction : AIAction {

    GameObject userUnit;
    public GameObject goldItem;
    

	// Use this for initialization
	void Start () {

        oppositeGroup = transform.parent.parent.GetChild(1).gameObject;
        userUnit = oppositeGroup.transform.GetChild(0).gameObject;
    }
	
	// Update is called once per frame
	void Update () {

        base.Update();

        if(target != null)
        {
            Debug.DrawLine(gameObject.transform.position, target.transform.position, Color.red);
        }

        //터치 입력이 들어오면 타겟 설정 해제
        if (Input.GetMouseButton(0))
        {
            gameObject.GetComponent<AIAction>().targeted = false;
        }

        //hp가 0이면 destroy
        float hp = unitData.health;
        if (hp <= 0)
        {
            MainBattleManager.instance.netTotalEnemy = MainBattleManager.instance.netTotalEnemy - 1;

            Destroy(this.gameObject);
            //랜덤 확률로 골드 드랍
            GameObject gold = Instantiate(goldItem);
            //위치 지정
            gold.transform.SetParent(transform.parent.parent.GetChild(4));
            gold.transform.position = gameObject.transform.position;
            //gold.transform.localPosition = Vector3.zero;

        }

        //자신이 타겟이 아니라면 아웃라인 제거
        /*
        if(userUnit.GetComponent<UserAction>().target != gameObject)
        {
            GetComponent<Outline>().eraseRenderer = true;
        }
        else if(userUnit.GetComponent<UserAction>().target == gameObject)
        {
            GetComponent<Outline>().eraseRenderer = false;
        }*/
    }

    public override void FindTarget()
    {
        base.FindTarget();
        if(detectUser)
        {
            try
            {
                FindTargetNearest(false);
            }
            catch(UnassignedReferenceException e)
            {
                Debug.LogError(e);
                Debug.Log(transform.GetSiblingIndex());
            }
        }
        
    }
}

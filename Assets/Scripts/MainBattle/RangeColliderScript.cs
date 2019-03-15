using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using UnityEngine.UI;
using Cinemachine;

public class RangeColliderScript : MonoBehaviour {

    public GameObject hpUIMask;
    public Text hpUIText;
    public GameObject mainCameraObj;
    public bool overlapPreventor;

    Coroutine corou;

    private void Start()
    {
        Physics2D.IgnoreLayerCollision(8, 9);

        //카메라 오브젝트 찾기
        //mainCameraObj = GameObject.FindGameObjectWithTag("MainCamera").gameObject;
        mainCameraObj = GameObject.FindGameObjectWithTag("Cinematic").gameObject;
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {

        //collision : 충돌이 인식된 대상
        //히트 소스를 가지고 있는 유닛

        
            string collisionTag = null;

            try
            {

                collisionTag = collision.transform.parent.tag;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                Debug.Log("gameObject = " + gameObject + "    collision = " + collision);
            }

            //Debug.Log(gameObject.transform.parent + "  " + collisionTag);
            //맞는 유닛
            string thisObjTag = tag;

            //충돌 대상이 상대 유닛의 hitsource일 경우
            if (collisionTag != thisObjTag && collision.tag == "hitSource")
            {
                if (collisionTag == "enemyUnit")
                {
                    //Debug.Log(collision.transform.position);
                }

                HitProcess(collision.gameObject);
            }
//}

        

        //넉백 효과 적용
        //넉백 효과 미적용  
    }

    IEnumerator CameraAction()
    {
        mainCameraObj.GetComponent<CinemachineVirtualCamera>().m_Lens.OrthographicSize = 4.67f;

        Time.timeScale = 1.3f;
        //mainCameraObj.GetComponent<Camera>().DOShakePosition(0.1f, 0.02f, 2, 90, true);
        yield return new WaitForSeconds(0.1f);
        Time.timeScale = 1.4f;
        //시네마틱 on
        mainCameraObj.GetComponent<CinemachineVirtualCamera>().m_Lens.OrthographicSize = 4.7f;
        //mainCameraObj.GetComponent<CinemachineBrain>().enabled = true;
    }


    void HitProcess(GameObject collision)
    {       

        //공격하는 대상의 공격력
        //맞는 대상의 방어력, 체력 필요

        GameObject attacker = collision.transform.parent.gameObject;
        GameObject receiver = transform.parent.gameObject;

        //공격이 성공하면 hitsource의 collider를 바로 비활성화
        //attacker.transform.GetChild(1).GetComponent<Collider2D>().enabled = false;

        if (attacker.tag == "userUnit")
        {
            //유저가 때릴 때만 카메라 효과 
            StartCoroutine(CameraAction());

            //차징 게이지 증가
            attacker.GetComponent<UserAction>().totalchargingTime = attacker.GetComponent<UserAction>().totalchargingTime + 0.2f;
            attacker.GetComponent<UserAction>().AttackEnergyCharging();
        }

        float dmg = attacker.GetComponent<AIAction>().unitData.power;
        float defense = 0;
        float hp = 0;
        //Debug.Log(receiver);
        //receiver의 태그가 건물일 경우에는 BuildingSetPosition에서 체력 데이터를 가져온다
        if (receiver.transform.GetChild(0).tag == "Building")
        {
            //receiver = receiver.transform.GetChild(0).gameObject;
            hp = receiver.GetComponent<BuildingSetPosition>().hp;
            //defense = receiver.GetComponent<BuildingSetPosition>().unitData.defense;
        }
        else
        {
            hp = receiver.GetComponent<AIAction>().unitData.health;
            defense = receiver.GetComponent<AIAction>().unitData.defense;

            //방어 중이면 해당 캐릭터의 방어 액션 실행
            if (MainBattleManager.instance.isUserDefensing)
            {
                receiver.GetComponent<SpineController>().DefenseAction();

                float chargingEnergy = receiver.GetComponent<UserAction>().totalchargingTime;
                //방어한 데미지에 비례해서 차징 에너지 감소
                dmg = receiver.GetComponent<UserAction>().DefenseAction(dmg);
                defense = 10000;
            }
            else //방어중이 아닐 경우, 밀려나는 액션
            {
                if (corou != null)
                {
                    StopCoroutine(corou);
                }

                //피니시 어택을 맞을 경우 밀려남
                float distance = 0;
                float time = 0;
                float rearrangeTime = 0; // 맞은 뒤 재정비 하는 시간

                receiver.GetComponent<AIAction>().isHitting = true;
                //

                if (attacker.tag == "userUnit" && !attacker.GetComponent<AIAction>().isFinishAttack)
                {
                    receiver.transform.DOKill(); // 적 유닛이 맞으면서 이동하는 것을 방지

                    //공격자가 유저일 경우에만 밀려남
                    distance = 0.1f;
                    time = 0.1f;
                    rearrangeTime = 0.5f;
                }
                else if (attacker.GetComponent<AIAction>().isFinishAttack)
                {
                    receiver.transform.DOKill(); // 피니시 어택을 맞으면 모든 행동 취소

                    distance = 3f;
                    time = 0.6f;
                    rearrangeTime = 1f;
                }
                else
                {

                }

                //밀려나는 거리
                //밀려나는 방향
                MoveAfterAction(receiver, attacker.transform.position, receiver.transform.position, distance, time, Ease.OutCubic);
                corou = StartCoroutine(ResetHitStatus(receiver, time + rearrangeTime));
            }
        }


        float effDmg = dmg - defense;
        if (effDmg < 1)
        {
            effDmg = 1;
        }
        hp = hp - effDmg;

        //공격이 성공하면 hitsource의 collider를 바로 비활성화
        attacker.transform.GetChild(1).GetComponent<Collider2D>().enabled = false;

        //receiver의 태그가 건물일 경우에는 BuildingSetPosition에서 체력 데이터를 가져온다
        if (receiver.transform.GetChild(0).tag == "Building")
        {
            //건물의 체력이 0일 때 건물 파괴
            if(hp <= 0)
            {
                hp = 0;
                receiver.GetComponent<BuildingSetPosition>().DestroyBuilding();

                //누가 부쉈는지 확인
                if(attacker.tag == "userUnit")
                {
                    MainBattleManager.instance.userDestroyBuilding = MainBattleManager.instance.userDestroyBuilding + 1;
                }
                else
                {
                    MainBattleManager.instance.enemyDestroyBuilding = MainBattleManager.instance.enemyDestroyBuilding + 1;
                }

            }

            receiver.GetComponent<BuildingSetPosition>().hp = hp;
        }
        else
        {
            receiver.GetComponent<AIAction>().unitData.health = hp;
            //체력 게이지 감소
            float fullHp = receiver.GetComponent<AIAction>().unitData.health100;
            float ratio = hp / fullHp;

            //float xValue = -0.8f * (1 - (hp / fullHp));
            GameObject healthGauge = receiver.transform.GetChild(0).GetChild(1).gameObject;
            healthGauge.transform.DOScaleX(ratio, 0.5f);

            //유저가 맞는 경우에는 메인UI에서 체력 감소 필요
            if (receiver.tag == "userUnit")
            {
                /*
                GameObject hpUIMask = transform.parent.parent.parent.parent.GetChild(3).GetChild(1).GetChild(1).gameObject;
                Text hpUIText = transform.parent.parent.parent.parent.GetChild(3).GetChild(3).*/
                GameManager.instance.SetGauge("hp", hp, fullHp, false);

            }

            if (hp <= 0 && attacker.tag == "userUnit")
            {
                hp = 0;
                //attacker.GetComponent<UserAction>().EndEngage();
            }
        }


    }

    IEnumerator ResetHitStatus(GameObject receiver, float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        
        receiver.GetComponent<AIAction>().isHitting = false;
    }

    void MoveAfterAction(GameObject receiver, Vector3 attackerVector, Vector3 receiverVector,  float distance, float time, Ease ease)
    {
        float xPosi = gameObject.transform.position.x;
        int affi = MainBattleManager.instance.vectorAffi;

        //공격을 할 때 고정되었던 벡터로 이동
        //벡터 normalize 필요
        
        float oriX = receiverVector.x - attackerVector.x; //<-moveVector로 하면 정지했을때 콤보 불가
        float oriY = receiverVector.y - attackerVector.y;
        
        
        oriX = (MainBattleManager.instance.userUnitVector.x + oriX) / 3;
        oriY = (MainBattleManager.instance.userUnitVector.y + oriY) / 3;
        


        float c = Mathf.Sqrt(oriX * oriX + oriY * oriY);
        float newXVector = oriX / c;
        float newYVector = oriY / c;

        float newX = gameObject.transform.position.x + distance * newXVector;
        float newY = gameObject.transform.position.y + distance * newYVector;

        Vector3 comboPosition = new Vector3(newX, newY, receiverVector.z);
        Vector3 comboDir = new Vector3(newXVector, newYVector, 0);

        //해당 벡터로 ray를 쏜 뒤, 건물이나 외곽이 있으면 ray가 부딪힌 곳으로 newX, newY 변경
        int layerMask = 1 << 14;
        RaycastHit2D hit = Physics2D.Raycast(this.gameObject.transform.position, comboDir, distance, layerMask);
        if (hit.collider != null)
        {
            comboPosition = new Vector3(hit.point.x, hit.point.y, receiverVector.z);
        }
        else
        {
        }

        //Debug.DrawRay(this.gameObject.transform.position, comboDir, Color.green, 10);
        receiver.transform.DOMove(comboPosition, time).SetEase(ease);
    }

}

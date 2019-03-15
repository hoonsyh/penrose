using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using cakeslice;

//유닛 AI 대전제
public class AIAction : UnitAction {

    public GameObject target;
    public GameObject oppositeGroup;
    //int enemyGroupChildCount;
    public bool targetLock;

    public bool isForceMoving;
    public bool isEngage;
    bool isAutoFind;
    bool isAutoFindExecuted;
    public bool isHitting;

    public bool resetCheck;

    GameObject oldTarget;
    GameObject newTarget;

    public Coroutine engageCorou;

    float passTime;
    float attackLimit;

    public bool detectUser;

    public bool isFinishAttack;

    //적이 교전거리 밖에 있으면 우선순위에 따라 타겟 지정
    protected void Start()
    {
        

        resetCheck = true;
        isForceMoving = false;
        passTime = 0;

        //EnemyGroupCount();
    }

    virtual protected void Update()
    {
        //z포지션을 -y포지션과 같게 <- 위에 있으면 뒤로 숨겨진다
        float zPosition = transform.position.y;
        Vector3 newPosition = new Vector3(transform.position.x, transform.position.y, zPosition);
        this.gameObject.transform.position = newPosition;

        if (gameObject.tag == "enemyUnit" && !isHitting) // 맞고 있는 중이 아닐 때만 액션
        {
 
            if (target != null)
            {

                if (!isEngage && !isForceMoving && !isHitting)
                {
                    MoveUnit(gameObject, target.transform.position);

                    CheckEngage();
                }
                else if (!isEngage && isForceMoving) // 교전 중은 아니지만 타겟을 향해 강제로 이동하고 있을 때
                {


                    CheckEngage(); // 1초 뒤에 다시 체크 가능

                }
                else // 교전 중에 적을 추격
                {

                    InEngage();
                }

            }
            else // 타겟이 없으면타겟 고정 해제 -> 타겟 찾는다
            {
                targetLock = false;
                isEngage = false;
                //CancelInvoke();
                //Invoke("DeselectTarget", 1f);
                //EndEngage();
            }

            //detectUser <= 적이 유저를 발견
            if (detectUser && !targetLock && GameManager.instance.battleStartTrigger && target == null) // 타겟 고정 해제, 트리거 true이면 타겟을 찾는다
            {
                FindTarget();
            }


            //디버깅용. 공격하지 않는 시간이 1초가 넘으면 coroutine = null로 강제 설정
            CheckIdleState(Time.deltaTime);
        }
    }

    void CheckIdleState(float _passTime)
    {
        
        //hitsource 위치 확인 후 기준점에서 벗어나면 초기화
        GameObject hitSource = transform.GetChild(1).gameObject;
        Vector3 hitSourcePosition = hitSource.transform.localPosition;

        if(hitSourcePosition == Vector3.zero)
        {
            passTime += _passTime;
            //히트소스가 1초 이상 움직이지 않고 타겟자동찾기가 비활성화 되어있는 경우에 자동찾기 활성화
            if(passTime > 1 && !isAutoFind)
            {
                isAutoFind = true;
                //isAutoFindExecuted = false;
            }

        }
        //히트소스가 움직이고 있으면 자동찾기 비활성화
        else
        {
            isAutoFind = false;
            isAutoFindExecuted = false;
            passTime = 0;
        }

        

        //자동찾기가 활성화되어 있는 경우에 자동찾기 1회 실행
        if (isAutoFind && !isAutoFindExecuted)
        {
            //전투 중이었다면 강제로 전투를 시작하게 만들어야 한다.
            isAutoFind = false;
            targetLock = false;
            isEngage = false;
            engageCorou = null;

            isAutoFindExecuted = true;
            //Debug.Log("자동찾기");
            FindTarget();
        }

    }

   
    //교전 거리 안에 적이 있는지 확인
    void CheckEngage()
    {
        //임시
        float engageRangeDefault = 0.80f;
        float iconRadius = 0.55f; 
        float engageRange = unitData.range * engageRangeDefault + iconRadius;

        //근처 타겟을 스캔하다가 교전 거리 미만으로 들어오는 적을 새로운 타겟으로 설정
        //Debug.Log("적 체크");
        int objectLength = oppositeGroup.transform.childCount;
        float distance = 100;

        for (int i = 0; i < objectLength; i++)
        {
            distance = CalcDistance(gameObject.transform.position, oppositeGroup.transform.GetChild(i).transform.position);
            if (distance <= engageRange)
            {
                //targetLock = true;
                target = oppositeGroup.transform.GetChild(i).gameObject;
                isForceMoving = false;
                //engageCorou = null;
            }
        }

        //교전 거리 안에 있는 적만 교전
        //오브젝트와 타겟 사이에 장애물이 있으면 교전x

        distance = CalcDistance(gameObject.transform.position, target.transform.position);
        if (distance <= engageRange)
        {
            if(gameObject.tag == "userUnit")
            {
                //Debug.Log(target.name + " 과 교전");
            }
            
            Engage();
        
        }          
    }

    
    void InEngage()
    {
        //사기 상태에 따라 판단

        //임시
        float engageRangeDefault = 0.80f;
        float iconRadius = 0.55f;
        float engageRange = unitData.range * engageRangeDefault + iconRadius;

        float distance = CalcDistance(gameObject.transform.position, target.transform.position);

        //타겟과의 거리가 교전 가능 거리보다 멀면 타겟 방향으로 이동
        if (distance > engageRange)
        {
            isEngage = false;
            MoveUnit(gameObject, target.transform.position);
        }     

    }



    public virtual void FindTarget()
    {
        //HideMovePositionMark();
        //타겟팅 그룹 설정
        //가장 가까운 적을 찾는다.

        //타겟 아웃라인 활성화
        //Debug.Log("타겟 추적");
        //OutlineEnable(FindTargetNearest(false), false);
    }

    public GameObject FindTargetNearest(bool isOverlap) // 타겟 중복 x
    {
        List<GameObject> oppositeObj = new List<GameObject>();
        int objectLength = oppositeGroup.transform.childCount;

        for (int i = 0; i < objectLength; i++)
        {
            oppositeObj.Add(oppositeGroup.transform.GetChild(i).gameObject);
        }

        //제일 가까운 적을 타겟으로 설정
        float[] oppositeDistance = new float[objectLength];
        float minDistance = 100;
        Vector2 originPosi = transform.position;
        
        for (int i = 0; i < objectLength; i++)
        {
            oppositeDistance[i] = CalcDistance(originPosi, oppositeObj[i].transform.position);

            //최단 거리에 있는 타겟 설정
            //if (!oppositeObj[i].GetComponent<AIAction>().targeted)
            //{
                
                if ((i > 0) && (minDistance > oppositeDistance[i]))
                {
                    target = oppositeObj[i];
                    minDistance = oppositeDistance[i];
                }
                else if (i == 0)//i==0일 경우 타겟으로 지정
                {
                    target = oppositeObj[0];
                    minDistance = oppositeDistance[0];
                }
                //
            //} 

            //마지막 루프까지 끝나고 타겟 지정
            if(i == objectLength - 1)
            {
                //모든 타겟이 이미 지정되어 있을 경우, 가장 가까운 타겟을 지정
                if(target == null)
                {
                    
                    for (int j = 0; j < objectLength; j++)
                    {
                        oppositeDistance[j] = CalcDistance(originPosi, oppositeObj[j].transform.position);
                        //이전 오브젝트보다 거리가 가까우면 타겟으로 지정
                        //현재 오브젝트가 타겟으로 지정되었는지 확인

                        //oppositeObj[i].GetComponent<UnitAction>().targeted = false; // 타겟 설정 초기화
                        if ((j > 0) && (minDistance > oppositeDistance[j]))
                        {
                            target = oppositeObj[j];
                            minDistance = oppositeDistance[j];
                        }
                        else if (j == 0)//i==0일 경우 타겟으로 지정
                        {
                            target = oppositeObj[0];
                            minDistance = oppositeDistance[0];
                        }       
                    }
                    //Debug.Log(target.name);
                }

                //지정된 타겟의 타겟bool을 true
                

                if(gameObject.tag == "userUnit")
                {
                    target.GetComponent<AIAction>().targeted = true;
                    //Debug.Log(target.name);
                }

                targetLock = true;
                if(gameObject.tag == "userUnit")
                {
                    //Debug.Log("확정된 타겟 " + target);
                }
                //isEngage = true;
                return target;
                
            }
        }
        return target;
    }


    //지정된 타겟으로 이동

    public void MoveUnit(GameObject unit, Vector2 targetPosi)
    {
        //이동거리와 유닛 속력으로 소요 시간 계산
        Vector2 originPosi = unit.transform.position;

        AIAction m_AIAction = unit.GetComponent<AIAction>();
        float speed = m_AIAction.unitData.speed;

        float moveTime = CalcDistance(originPosi, targetPosi) / speed; //유닛 속력은 자유롭게 변해야 한다
        unit.transform.DOKill();
        unit.transform.DOMove(targetPosi, moveTime).SetEase(Ease.Linear);
    }

    float CalcDistance(Vector2 origin, Vector2 target)
    {
        float dx = origin.x - target.x;
        float dy = origin.y - target.y;

        float distance = Mathf.Sqrt(dx * dx + dy * dy);

        return distance;
    }

    //공격 범위 안에 적이 있으면 정지
    //이동을 멈추고 타겟과 교전
    public void Engage()
    {
        //이동 중단
        isEngage = true;

        gameObject.transform.DOKill();
        /*
        if (gameObject.tag == "userUnit")
        {
            Debug.Log(target + "    " + engageCorou);
        }*/
        

        if (target != null && engageCorou == null)
        {
            engageCorou = StartCoroutine(EngageInternal());
        }
        else
        {
            StopCoroutine(engageCorou);
            //StopAllCoroutines();
            engageCorou = null;
        }
        
    }

    virtual protected IEnumerator EngageInternal()
    {
        GameObject hitsourceCollider = transform.GetChild(1).gameObject;

        //1초 간격으로 공격력 - 방어력만큼 상대 체력을 깎는다
        float dmg = unitData.power;
        float defense = 0;
        float hp = 1;
 
        //box가 아닌 경우에는 유닛 데이터 삽입
        if(target.tag != "box")
        {
            defense = target.GetComponent<UserAction>().unitData.defense;
            hp = target.GetComponent<UserAction>().unitData.health;
        }
 

        //적이 보스일 경우에는 보스 패턴 삽입


        //일반적의 공격 AI
        //타겟이 있고 체력이 0 이상이라면 계속 공격
        while (target != null && hp >= 0 && isEngage)
        {
            hitsourceCollider.GetComponent<Collider2D>().enabled = true;

            NormalAttackAI(); // 일반 공격         
            yield return new WaitForSeconds(1);          
        }       
        //EndEngage();
    }


    protected void NormalAttackAI()
    {
        //게임오브젝트의  hitsource를 찾는다
        GameObject hitsourceCollider = transform.GetChild(1).gameObject;
        //hitSource를 target 위치로 이동
        Vector3 targetPosition = target.transform.position;

        hitsourceCollider.transform.localPosition = targetPosition - gameObject.transform.position;//현재 위치 보정
        
        if (gameObject.tag == "userUnit")
        {
            //Debug.Log(target.name + "를 공격");
            //Debug.Log(hitsourceCollider.transform.localPosition + " 방향으로 hitsource 이동");
        }
        
        //이동 후에는 다시 원점으로 복귀

        Invoke("ResetHitSource", 0.1f);
    }

    void ResetHitSource()
    {
        /*
        if (gameObject.tag == "userUnit")
        {
            Debug.Log("hitsource 원점 복귀");
        }
        */
        GameObject hitsourceCollider = transform.GetChild(1).gameObject;
        hitsourceCollider.transform.localPosition = Vector3.zero;
    }


    


}

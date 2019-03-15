using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using cakeslice;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using System;
using Cinemachine;
using Spine;
using Spine.Unity;

public class UserAction : AIAction {

    //public UnitData unitData = new UnitData();
    //public 
   

    Vector2 debugcomboPoint;
    public GameObject pointObj;
    public GameObject mainCameraObj;

    //효과 관련
    public GameObject blackObj;



    int enemyGroupChildCount;
    public Sprite moveCurser;
    public Sprite attackCurser;
    public GameObject directionObj;

    GameObject hitsourceCollider;
    GameObject hitboxCollider;
    Vector3 attackDestination;

    //에너지 차징 관련
    public GameObject chargingObj;
    public GameObject chargingGauge;

    public GameObject chargingTextUI;
    public GameObject useSkillTextUI;
    public GameObject chargingGaugeUI;
    public Text chargingText;

    public float chargingIdleTime;
    public int chargingLevel;

    public float totalchargingTime;
    public float netChargingTime;

    float needChargingTime = 1f;
    public float afterAttackTime;
    public float idleTime;
    public float defenseTime;
    public float afterChargingTime; // 에너지 차징 후 자동 감소를 위한 시간 체크

    int useSkillNum = 0;

    float nextCheckTime = 1;
    int nextDefenseCheckTime = 1;

    bool nowCharging;
    float chargingReadyTime = 0.5f;

    SpineController spineController;
    UserController userController;

    //유저 행동에 대한 변수
    public int actionCombo;
    public int[] actionNum = new int[4] { 0, 0, 1, 2 };
    public float[] attackDelayArr = new float[4] { 0.3f, 0.3f, 0.3f, 1.2f };
    //public float[] attackDelay = new float[4] { 0.3f, 0.3f, 0.3f, 1.2f };
    bool isAttackEnd;

    //public bool isFinishAttack;

    float[] comboOKTime = new float[4] { 0.6f, 0.6f, 0.6f, 1.2f };

    //액션에 필요한 스태미나
    int staminaDash = 10;
    int staminaSkill0 = 20;

    // Use this for initialization
    void Start () {

        hitsourceCollider = transform.GetChild(1).gameObject;
        hitboxCollider = transform.GetChild(2).gameObject;
        spineController = gameObject.GetComponent<SpineController>();
        userController = gameObject.GetComponent<UserController>();

        //attackDelay = 0.3f;
    }
	
	// Update is called once per frame
	void Update () {

        //z포지션을 -y포지션과 같게 <- 위에 있으면 뒤로 숨겨진다
        float zPosition = transform.position.y;
        Vector3 newPosition = new Vector3(transform.position.x, transform.position.y, zPosition);
        this.gameObject.transform.position = newPosition;

        //방향을 가리키는 스프라이트(각도에 따라 방향을 정한다)
        //float xVector = MainBattleManager.instance.userUnitVector.x;
        //float yVector = MainBattleManager.instance.userUnitVector.y;

        float xVector = userController._moveVector.x;
        float yVector = userController._moveVector.y;

        //0벡터가 아니면 이동 각도 읽고 화살표 회전
        if(userController._moveVector != Vector3.zero)
        {
            float angle = Mathf.Atan2(yVector, xVector) * 180 / Mathf.PI;
            directionObj.transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        //버튼을 누르고 있으면 에너지를 모은다
        if (Input.GetKey(KeyCode.A) && 
            !MainBattleManager.instance.isUserAttacking &&
            !MainBattleManager.instance.isUserDashing &&
            !MainBattleManager.instance.isUserDefensing)
        {
            netChargingTime = netChargingTime + Time.deltaTime;
            EnergyChargingGaugeActive();

            //사용하는 스킬 = 최대 차징 에너지
            if(netChargingTime > chargingReadyTime)
            {
                useSkillNum = chargingLevel;
            }
        }
        //버튼을 떼면 에너지 모으기를 중단하고 공격
        else if (Input.GetKeyUp(KeyCode.A) && !MainBattleManager.instance.isUserAttacking && !MainBattleManager.instance.isUserDashing)
        {
            //차징 애니메이션 실행 중단 체크
            spineController.isPlayed = false;
            
            //공격 딜레이 확인
            if (afterAttackTime > attackDelayArr[actionCombo])
            {
                AttackAction();

                afterAttackTime = 0; // 공격 시 공격 시간 간격 초기화

                
                totalchargingTime = totalchargingTime - useSkillNum * needChargingTime; //사용 스킬에 따라 다르게 초기화
                netChargingTime = 0;

                EnergyChargingGaugeDeactive();
            }
            SetChargingTimeToChargingLevel();
        }
        //중립 s키를 누르고 있으면 방어 자세
        else if(Input.GetKey(KeyCode.S) && userController._moveVector == Vector3.zero &&
            !MainBattleManager.instance.isUserDashing &&
            !MainBattleManager.instance.isUserAttacking &&
            totalchargingTime > 0)
        {
            Defense();
        }
        //방어 중일 때 s키를 떼거나 방어 중에 스태미나가 0이 되면 방어 중단
        else if((Input.GetKeyUp(KeyCode.S) && MainBattleManager.instance.isUserDefensing) ||
            (MainBattleManager.instance.isUserDefensing && totalchargingTime <= 0))
        {
            DefenseEnd();
        }

        //s키를 누르면 대시, 정지 중이거나 공격 중이거나 대시 중에는 대시 x, 스태미나가 충분해야 한다      
        else if (Input.GetKeyUp(KeyCode.S) && 
            !MainBattleManager.instance.isUserAttacking && 
            !MainBattleManager.instance.isUserDashing && 
            !MainBattleManager.instance.isUserDefensing &&
            userController._moveVector != Vector3.zero)
        {
            Dash();
        }
        //D키를 누르고 있으면 차징
        else if (Input.GetKey(KeyCode.D) &&
            !MainBattleManager.instance.isUserAttacking &&
            !MainBattleManager.instance.isUserDashing &&
            !MainBattleManager.instance.isUserDefensing)
        {
            
            netChargingTime = netChargingTime + Time.deltaTime;

            EnergyChargingGaugeActive();
        }
        else if (Input.GetKeyUp(KeyCode.D) &&
            !MainBattleManager.instance.isUserAttacking &&
            !MainBattleManager.instance.isUserDashing &&
            !MainBattleManager.instance.isUserDefensing)
        {
            //차징키를 짧게 누른 거라면 사용스킬 변경
            if (netChargingTime < chargingReadyTime)
             {
                useSkillNum++;

                //Debug.Log(useSkillNum + " " + chargingLevel);

                //현재 모은 에너지보다 에너지 소모량이 많은 스킬로 전환 x
                if(useSkillNum > chargingLevel)
                {
                    useSkillNum = 0;
                }
                
             }
             else
             {
                //그 외 경우에는 차징 종료 
                ChargingEnd();
             }

            netChargingTime = 0;
        }


        //콤보 가능 시간을 넘기면 자동으로 1번 콤보로 복귀

        if (afterAttackTime > comboOKTime[actionCombo])
        {
            actionCombo = 0;
        }

        //콤보 작동을 위한 공격 후 시간 체크
        afterAttackTime = afterAttackTime + Time.deltaTime;

        //idle 상태가 1초 이상 지속되면 1초에 1씩 스태미나 회복
        if(userController._moveVector == Vector3.zero && !MainBattleManager.instance.isUserDefensing)
        {
            idleTime = idleTime + Time.deltaTime;
            if(idleTime > nextCheckTime)
            {
                //UseStamina(-1);
                nextCheckTime = nextCheckTime + 0.5f;
            }
        }
        else
        {
            nextCheckTime = 1;
            idleTime = 0;
        }

        //방어 상태가 1초 이상 지속되면 1초에 1씩 마나 소모
        if(MainBattleManager.instance.isUserDefensing)
        {
            defenseTime = defenseTime + Time.deltaTime / 10;
            if (defenseTime > nextDefenseCheckTime)
            {
                //UseChargingEnergy(1);
                nextDefenseCheckTime = nextDefenseCheckTime + 2;
            }
        }

        //차징 중이 아니라면 1초에 0.1씩 차징 카운터 감소
        if(!MainBattleManager.instance.isUserCharging && afterChargingTime > 2)
        {
            totalchargingTime = totalchargingTime - Time.deltaTime / 10;

            if (totalchargingTime < 0)
            {
                totalchargingTime = 0;
            }
            SetChargingTimeToChargingLevel(); 
            
        }
        else
        {
            //nextCheckTime = 1;
            //chargingIdleTime = 0;
        }

        //항상 업데이트를 해야하는 것
        afterChargingTime = afterChargingTime + Time.deltaTime;
        useSkillTextUI.GetComponent<Text>().text = useSkillNum.ToString();

    }

    public void AttackEnergyCharging()
    {
        afterChargingTime = 0;
        SetChargingTimeToChargingLevel();
    }


    //방어 액션
    void Defense()
    {     
        //처음에 방어 중이 아니었을 때만 방어 액션 1회 실행
        if(!MainBattleManager.instance.isUserDefensing)
        {
            MainBattleManager.instance.isUserDefensing = true;
            spineController.DefenseReady();
        }
    }

    public int DefenseAction(float damage)
    {
        //방어하는 데미지에 비례해서 차징 에너지 감소
        totalchargingTime = totalchargingTime - damage / 100f;
        SetChargingTimeToChargingLevel();

        //막지 못하는 데미지 반환
        if (totalchargingTime < 0)
        {
            return (int)Math.Truncate(totalchargingTime * 100);
        }
        else
        {
            return 1;
        }
    }

    void DefenseEnd()
    {
        //방어 종료 시, 캐릭터가 움직이는 중이라면 이동 애니메이션
        if(userController._moveVector == Vector3.zero)
        {
            spineController.IdleCharacter();
        }
        else
        {
            spineController.MoveCharacter();
        }

        MainBattleManager.instance.isUserDefensing = false;
        nextDefenseCheckTime = 1;
        defenseTime = 0;
    }


    //대시 애니메이션
    void Dash()
    {
        //UseStamina(staminaDash);

        MainBattleManager.instance.isUserDashing = true;
        //대시 애니메이션 실행
        spineController.DashCharacter();

        //이동 중 공격, 피격 차단 => hitBox, hitSource 콜라이더 비활성화
        hitsourceCollider.GetComponent<Collider2D>().enabled = false;
        hitboxCollider.GetComponent<Collider2D>().enabled = false;

    }

    public void DashAction()
    {
        //대시 후 이동할 위치
        float dashDistance = 1.5f;

        float newX = gameObject.transform.position.x + dashDistance * userController._moveVector.x;
        float newY = gameObject.transform.position.y + dashDistance * userController._moveVector.y;
       
        Vector3 dashPosition = new Vector3(newX, newY, 0);
        Vector3 dashDirVector = new Vector3(userController._moveVector.x, userController._moveVector.y);

        //해당 벡터로 ray를 쏜 뒤, 건물이나 외곽이 있으면 ray가 부딪힌 곳으로 newX, newY 변경
        int layerMask = 1 << 14;
        RaycastHit2D hit = Physics2D.Raycast(this.gameObject.transform.position, dashDirVector, dashDistance, layerMask);
        if(hit.collider != null)
        {
            dashPosition = new Vector3(hit.point.x, hit.point.y, 0); 
        }
        else
        {
        }

        //해당 위치로 캐릭터 이동
        float moveTime = 0.3f;
        gameObject.transform.DOMove(dashPosition, moveTime).SetEase(Ease.OutQuad);
        Invoke("DashEnd", moveTime);
    }

    void DashEnd()
    {
        //이동 중 공격, 피격 차단 => hitBox, hitSource 콜라이더 활성화
        //hitsourceCollider.GetComponent<Collider2D>().enabled = true;
        hitboxCollider.GetComponent<Collider2D>().enabled = true;

        spineController.DashEndCharacter();
    }

    //공격이 종료될 때 애니메이션. 공격이 종료된 뒤 한 번만 실행
    void AttackActionEnd()
    {
        if(afterAttackTime > attackDelayArr[actionCombo]) // attackDelay는 콤보 공격 별로 다르게
        {
            Debug.Log("여기");    
            spineController.AfterAction();
        }
    }


    void CheckCombo(float time)
    {
        if (time < comboOKTime[actionCombo])
        {
            actionCombo++;
            //isAttackEnd = false; //콤보 중에는 idle x
        }
        else
        {
            actionCombo = 0;
            //isAttackEnd = true; //콤보 종료 (경과 시간이 콤보 시간보다 길면 계속 true -> idle을 실행하게 된다)
        }

        if(actionCombo >= 4)
        {
            actionCombo = 3;
            //isAttackEnd = true; //콤보 종료
        }
    }

    //남아있는 차징 시간을 통해 차징 레벨 계산 및 게이지 반영
    void SetChargingTimeToChargingLevel()
    {
        //레벨 계산
        //float remainChargingTime = totalchargingTime - chargingReadyTime;
        chargingLevel = (int)Math.Truncate(totalchargingTime / needChargingTime);

        if (chargingLevel > 4)
        {
            chargingLevel = 4;
        }

        float ratio = (totalchargingTime - chargingLevel * needChargingTime) / needChargingTime;

        //장착 스킬 변경
        //현재 모은 에너지만큼의 스킬로 전환
        if (useSkillNum > chargingLevel)
        {
            useSkillNum = chargingLevel;
        }
        //UI 반영
        chargingTextUI.GetComponent<Text>().text = chargingLevel.ToString();
        chargingGaugeUI.transform.localScale = new Vector3(ratio, 1, 1);


    }

    void ChargingEnd()
    {
        MainBattleManager.instance.isUserCharging = false;

        //차징 애니메이션 실행 중단 체크
        spineController.isPlayed = false;

        //캐릭터 위 차징 게이지x
        EnergyChargingGaugeDeactive();

        //차징 종료 시, 캐릭터가 움직이는 중이라면 이동 애니메이션
        if (userController._moveVector == Vector3.zero)
        {
            spineController.IdleCharacter();
        }
        else
        {
            spineController.MoveCharacter();
        }
     
    }


    void EnergyChargingGaugeActive()
    {
        //차징 시간이 대기 시간을 넘어가면 차징 시작
        if(netChargingTime > chargingReadyTime)
        {
            afterChargingTime = 0;
            totalchargingTime = totalchargingTime + Time.deltaTime;
            if(totalchargingTime > needChargingTime * 4) //차징 에너지 상한
            {
                totalchargingTime = needChargingTime * 4;
            }


            //차징 애니메이션         
            MainBattleManager.instance.isUserCharging = true;
            spineController.ChargingReady();

            chargingLevel = (int)Math.Truncate(totalchargingTime / needChargingTime);
            //Debug.Log(chargingLevel + " " + needChargingTime);

            float ratio = 0;

            //차징 레벨에 따라 차징되는 속도 변경 및 게이지 스케일에 맞게 변경
            if (chargingLevel >= 0 && chargingLevel < 4)
            {
                chargingObj.SetActive(true);

                chargingText.text = chargingLevel.ToString();
                ratio = (totalchargingTime - chargingLevel * needChargingTime) / needChargingTime;
            }
            else if (chargingLevel >= 4)
            {
                chargingText.text = "4";
                ratio = 1;
            }
            else
            {

            }

            chargingGauge.transform.localScale = new Vector3(ratio, 1, 1);
            SetChargingTimeToChargingLevel();
        }

    }

    void EnergyChargingGaugeDeactive()
    {
        chargingObj.SetActive(false);
    }

    public void AttackAction()
    {
        //콜라이더 활성화
        

        //Debug.Log(afterAttackTime + " " + actionCombo);

        //콤보 체크
        CheckCombo(afterAttackTime);

        MainBattleManager.instance.isUserCharging = false;

        //버튼을 누른 시간을 체크
        //버튼 누른 시간에 맞게 기술 발동(히트 소스 이동)

        if (MainBattleManager.instance.isUserAttacking == false)
        {
            //차징 시간과 스태미나가 충분하면 스킬 사용
            //여기서는 스킬 사용 시 애니메이션만 컨트롤, 스킬 효과는 각각의 스킬 메소드에서 조정
            if (useSkillNum == 0)
            {
                NormalAttack();

                //일반 공격 스파인 실행
                spineController.AttackActionCharacter(actionNum[actionCombo]);
            }
            else if(useSkillNum == 1 && chargingLevel >= 1)
            {
                NormalSkill();

                //스파인, 벡터 방향으로 캐릭터 약간 이동
                spineController.SkillNormalAction();
                MoveAfterAction(1f, 0.3f, Ease.OutCubic);

            }
            else if (useSkillNum == 2 && chargingLevel >= 2)
            {
                MiddleSkill();

                //스파인, 벡터 방향으로 캐릭터 약간 이동
                //spineController.SkillNormalAction();
                //MoveAfterAction(1f, 0.3f, Ease.OutCubic);
            }
            else if (useSkillNum == 3 && chargingLevel >= 3)
            {
                HardSkill();

                //스파인, 벡터 방향으로 캐릭터 약간 이동(임시)
                spineController.SkillNormalAction();
                MoveAfterAction(1f, 0.3f, Ease.OutCubic);
            }
            else if (useSkillNum == 4 && chargingLevel >= 4)
            {
                ExpertSkill();

                //스파인, 벡터 방향으로 캐릭터 약간 이동(임시)
                spineController.SkillNormalAction();
                MoveAfterAction(1f, 0.3f, Ease.OutCubic);
            }
            else // 스태미나가 충분하지 않을때는 일반공격
            {
                NormalAttack();
            }
        }

        //3번째 공격에서는 앞으로 이동
        if (actionCombo == 3)
        {
            MoveAfterAction(1, 0.5f, Ease.OutQuad);

            isFinishAttack = true; // 피니시 어택;

            //조이스틱이 drag하는 방향으로 벡터 조정
            //MainBattleManager.instance.userUnitVector = userController._moveVector;
        }

        hitsourceCollider.GetComponent<Collider2D>().enabled = true;
    }

    void MoveAfterAction(float distance, float time, Ease ease)
    {
        float xPosi = gameObject.transform.position.x;
        int affi = MainBattleManager.instance.vectorAffi;

        //공격을 할 때 고정되었던 벡터로 이동
        //벡터 normalize 필요
        float oriX = userController._moveVector.x; //<-moveVector로 하면 정지했을때 콤보 불가
        float oriY = userController._moveVector.y;

        if(oriX == 0 && oriY == 0)
        {
            oriX = MainBattleManager.instance.userUnitVector.x;
            oriY = MainBattleManager.instance.userUnitVector.y;
        }


        float c = Mathf.Sqrt(oriX * oriX + oriY * oriY);
        float newXVector = oriX / c;
        float newYVector = oriY / c;

        float newX = gameObject.transform.position.x + distance * newXVector;
        float newY = gameObject.transform.position.y + distance * newYVector;

        Vector3 comboPosition = new Vector3(newX, newY, gameObject.transform.position.z);
        Vector3 comboDir = new Vector3(newXVector, newYVector, gameObject.transform.position.z);

        //해당 벡터로 ray를 쏜 뒤, 건물이나 외곽이 있으면 ray가 부딪힌 곳으로 newX, newY 변경
        int layerMask = 1 << 31;
        RaycastHit2D hit = Physics2D.Raycast(this.gameObject.transform.position, comboDir, distance, layerMask);
        if (hit.collider != null)
        {
            //Debug.Log("변경");
            comboPosition = new Vector3(hit.point.x, hit.point.y, gameObject.transform.position.z);
        }
        else
        {
        }
        //pointObj.transform.position = comboPosition;
        //Debug.Log(comboPosition);
        gameObject.transform.DOMove(comboPosition, time).SetEase(ease);
    }
    /*
    void OnDrawGizmosSelected()
    {
        // Display the explosion radius when selected
        Gizmos.color = Color.white;
        Gizmos.DrawSphere(debugcomboPoint, 1f);
    }
    */

    Vector2 AttackDestination(float attackRange)
    {
        //유저가 바라보고 있는 방향
        Vector3 attackPosition = userController._moveVector;
        if (attackPosition.x == 0 && attackPosition.y == 0)
        {
            attackPosition = MainBattleManager.instance.userUnitVector;
        }


        float originX = attackPosition.x;
        float originY = attackPosition.y;
        float normalize = Mathf.Pow(originX * originX + originY * originY, 0.5f);
  
        //hitSource를 target 위치로 이동    

        float xAxis = attackRange * (originX / normalize);
        float yAxis = attackRange * (originY / normalize);
        Vector2 attackDestination = new Vector2(xAxis, yAxis);

        return attackDestination;
    }

    void NormalAttack()
    {
        //일반 공격 : 히트 박스를 기본으로 키우고 적 방향으로 최단거리 공격
        hitsourceCollider.transform.DOLocalMove(AttackDestination(1), 0.1f); 

        //불리언 true => 공격 중에는 이동x
        MainBattleManager.instance.isUserAttacking = true;

        //Invoke("ResetHitSource", attackDelayArr[actionCombo]);
        Invoke("ResetHitSource", attackDelayArr[actionCombo]);
    }

    void NormalSkill()
    {
        isFinishAttack = true; // 스킬은 전부 피니시 어택

        hitsourceCollider.transform.DOLocalMove(AttackDestination(1), 0.1f);
        GetComponent<UserAction>().unitData.power = GetComponent<UserAction>().unitData.power * 2f;
        MainBattleManager.instance.isUserAttacking = true;
    }

    void MiddleSkill()
    {
        isFinishAttack = true; // 스킬은 전부 피니시 어택

        StartCoroutine(CameraEffect());
        /*
        hitsourceCollider.transform.DOLocalMove(AttackDestination(1), 0.1f);
        GetComponent<UserAction>().unitData.power = GetComponent<UserAction>().unitData.power * 3f;
        MainBattleManager.instance.isUserAttacking = true;*/
    }

    void HardSkill()
    {
        isFinishAttack = true; // 스킬은 전부 피니시 어택

        hitsourceCollider.transform.DOLocalMove(AttackDestination(1), 0.1f);
        GetComponent<UserAction>().unitData.power = GetComponent<UserAction>().unitData.power * 4f;
        MainBattleManager.instance.isUserAttacking = true;
    }

    void ExpertSkill()
    {
        isFinishAttack = true; // 스킬은 전부 피니시 어택

        StartCoroutine(CameraEffect());
    }

    IEnumerator CameraEffect()
    {
        MainBattleManager.instance.isUserAttacking = true;
        //mainCameraObj.GetComponent<CinemachineVirtualCamera>().m_Lens.OrthographicSize.
        //GetComponent<Camera>().DOOrthoSize

        //검은색 효과 등장
        blackObj.SetActive(true);

        //주인공 spine의 소팅오더 변경
        gameObject.transform.GetChild(7).GetComponent<MeshRenderer>().sortingOrder = 2;

        //cinematic disable -> 캐릭터 중앙으로 카메라 이동 및 확대 -> 확대한 값을 다시 원래대로
        mainCameraObj.GetComponent<CinemachineBrain>().enabled = false;
        Vector3 unitPosi = gameObject.transform.position;

        Camera camera = mainCameraObj.GetComponent<Camera>();

        mainCameraObj.transform.DOMove(new Vector3(unitPosi.x, unitPosi.y, -60), 0.3f);
        camera.DOOrthoSize(3.5f, 0.301f);
        blackObj.GetComponent<SpriteRenderer>().DOFade(0.6f, 0.3f);
        yield return new WaitForSeconds(0.3f);

        //임시 스파인
        spineController.SkillNormalAction();

        Time.timeScale = 0.01f;
        Debug.Log("카메라효과");
        yield return new WaitForSeconds(0.01f);

        Time.timeScale = 1.4f;
        blackObj.GetComponent<SpriteRenderer>().DOFade(0, 0.1f);
        blackObj.SetActive(false);
        //주인공 spine의 소팅오더 변경
        gameObject.transform.GetChild(7).GetComponent<MeshRenderer>().sortingOrder = 0;


        //카메라 효과 원상복구
        camera.DOOrthoSize(4.7f, 0.3f);
        mainCameraObj.GetComponent<CinemachineBrain>().enabled = true;

        
        MoveAfterAction(1f, 0.3f, Ease.OutCubic);


        hitsourceCollider.transform.DOLocalMove(AttackDestination(1), 0.1f);
        GetComponent<UserAction>().unitData.power = GetComponent<UserAction>().unitData.power * 5f;
        MainBattleManager.instance.isUserAttacking = true;
    }



    void UseChargingEnergy(float useValue)
    {
        //스태미나 감소
        //float mp = unitData.mana;
        totalchargingTime = totalchargingTime - useValue;
        if (totalchargingTime < 0)
        {
            totalchargingTime = 0;
        }
        else if (totalchargingTime > needChargingTime * 4)
        {
            totalchargingTime = needChargingTime * 4;
        }
        //unitData.mana = mp;

        SetChargingTimeToChargingLevel();
        //GameManager.instance.SetGauge("mp", mp, unitData.mana100, false);
    }

    public void ResetHitSource()
    {
        //변경된 스탯 초기화
        ResetUnitSetting();

        MainBattleManager.instance.isUserAttacking = false;

        GameObject hitsourceCollider = transform.GetChild(1).gameObject;
        //hitsourceCollider.GetComponent<CircleCollider2D>().radius = 0.1f;   

        hitsourceCollider.GetComponent<Collider2D>().enabled = false;
        hitsourceCollider.transform.localPosition = Vector3.zero;

        isFinishAttack = false;

        //스파인
        spineController.AfterAction();
    }

    void ResetUnitSetting()
    {
        float[] stat = new float[7];
        stat = GameManager.instance.tempTankStat;
        GetComponent<UserAction>().unitData.power = stat[0];
    }

}

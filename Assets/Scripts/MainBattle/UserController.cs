using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserController : MonoBehaviour
{
    public JoystickScript joystickScript;
    public float moveSpeed;
    public Vector3 _moveVector;
    private Transform _transform;
    public bool isIdle;
    bool oldIsIdle;
    bool isStartAnimation;

    SpineController spineController;
    UserAction userAction;

    float afterAttackTime;
    float comboDelay;
    float vectorZeroTime;

    bool isIdleAfterAction;

    // Start is called before the first frame update
    void Start()
    {
        _transform = transform;
        _moveVector = Vector3.zero;
        spineController = gameObject.GetComponent<SpineController>();
        userAction = gameObject.GetComponent<UserAction>();

        afterAttackTime = userAction.afterAttackTime;
        comboDelay = userAction.attackDelayArr[userAction.actionCombo];
    }

    // Update is called once per frame
    void Update()
    {
        HandleInput();  
    }

    private void FixedUpdate()
    {
        afterAttackTime = userAction.afterAttackTime;
        comboDelay = userAction.attackDelayArr[userAction.actionCombo];

        //공격 후 타이머가 콤보 딜레이 타이머보다 커야 이동
        if (afterAttackTime > comboDelay + 0.1f && 
            !MainBattleManager.instance.isUserCharging && 
            !MainBattleManager.instance.isUserDashing &&
            !MainBattleManager.instance.isUserDefensing &&
            !MainBattleManager.instance.isUserAttacking)
        {
            Move();           
        }
        
    }

    public void HandleInput()
    {
        //공격 중, 대시 중에는 input 변화x
        if(userAction.actionCombo == 0 && 
            !MainBattleManager.instance.isUserDashing &&
            !MainBattleManager.instance.isUserAttacking)
        {
            _moveVector = PoolInput();
        }  
    }

    public Vector3 PoolInput()
    {
        float h = joystickScript.GetHorizontalValue();
        float v = joystickScript.GetVerticalValue();
        Vector3 moveDir = new Vector3(h, v, 0).normalized;
        
        //캐릭터 방향 확인, 공격 중에는 방향 전환x, 대시 중에는 방향 전환 x
        if(h > 0)
        {
            MainBattleManager.instance.vectorAffi = 1;
            spineController.controlCharacter.transform.localScale = new Vector3(1, 1, 1);
        }
        else if(h < 0)
        {
            MainBattleManager.instance.vectorAffi = -1;
            spineController.controlCharacter.transform.localScale = new Vector3(-1, 1, 1);
        }


        //idle 상태 확인
        //공격 중, 대시 중이 아니거나 h=0, v=0이면 idle
        if(h == 0 && v == 0 && !MainBattleManager.instance.isUserDashing)
        {
            isIdle = true;
        }
        else
        {
            isIdle = false;
        }

        return moveDir;
    }

    public bool ChangeStat()
    {
        bool status;

        //이전 상태와 동일하면 false 반환, 상태가 변하면 true 반환
        if(isIdle == oldIsIdle)
        {
            status = false;
        }
        else
        {
            status = true;
        }

        oldIsIdle = isIdle;

        return status;
    }


    public void Move()
    {
        
        _transform.Translate(_moveVector * moveSpeed * Time.deltaTime);

        //캐릭터가 움직이지 않으면 idle animation 실행
        if(ChangeStat())
        {
            if (isIdle)
            {
                spineController.IdleCharacter();
                isStartAnimation = true;
            }
            else
            {
                //Debug.Log("이동 애니");
                spineController.MoveCharacter();
                isStartAnimation = true;
            }
        }      
    }
}

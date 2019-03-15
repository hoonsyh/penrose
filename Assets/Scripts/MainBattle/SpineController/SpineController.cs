using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using Spine;

public class SpineController : MonoBehaviour
{
    public GameObject controlCharacter;
    SkeletonAnimation skelAni;
    UserAction userAction;
    UserController userController;
    public bool isIdleAfterAction;

    public bool isPlayed;

    private void Start()
    {
        skelAni = controlCharacter.GetComponent<SkeletonAnimation>();
        userAction = gameObject.GetComponent<UserAction>();
        userController = gameObject.GetComponent<UserController>();

        //스파인 오브젝트에 이벤트 리스너 추가
        AddEventListener(controlCharacter);
    }

    void AddEventListener(GameObject characterObj)
    {
        SkeletonAnimation skelAni = characterObj.GetComponent<SkeletonAnimation>();
        skelAni.AnimationState.Event += OnEvent;
    }

    void OnEvent(TrackEntry trackIndex, Spine.Event e)
    {
        //dashGo일 때는 dash 메소드 실행
        if(e.Data.Name == "dashGo")
        {
            userAction.DashAction();
        }

        //charging_ready_end일 때는 chargingAction 메소드 실행
        if (e.Data.Name == "charging_ready_end")
        {
            ChargingAction();
        }

        //스킬 애니메이션이 종료될 때
        if (e.Data.Name == "skillEnd")
        {
            userAction.ResetHitSource();
        }

    }


    //idle 상태 애니메이션
    public void IdleCharacter()
    {
        //SkeletonAnimation skelAni = controlCharacter.GetComponent<SkeletonAnimation>();
        skelAni.AnimationState.SetAnimation(0, "attack_idle", true);
    }

    //캐릭터 이동 시 애니메이션
    public void MoveCharacter()
    {
        //SkeletonAnimation skelAni = controlCharacter.GetComponent<SkeletonAnimation>();
        skelAni.AnimationState.SetAnimation(0, "run", true);
        
    }

    //캐릭터 공격 시 애니메이션
    public void AttackActionCharacter(int comboNum)
    {
        //SkeletonAnimation skelAni = controlCharacter.GetComponent<SkeletonAnimation>();
        CancelInvoke();

        TrackEntry entry = skelAni.AnimationState.SetAnimation(0, "attack_normal", false);

        float delay = 0;
        string aniName = "";

        //공격 콤보 단계에 맞게 공격 애니메이션 실행
        if(comboNum == 0)
        {
            aniName = "attack_normal";
        }
        else if(comboNum == 1)
        {
            aniName = "attack_normal2";
        }
        else if (comboNum == 2)
        {
            aniName = "attack_normal3";
        }

        entry = skelAni.AnimationState.SetAnimation(0, aniName, false);
        delay = skelAni.skeleton.Data.FindAnimation(aniName).Duration;
        
        //mixDuration이 있는 경우에 동작의 역동성이 떨어지는 경우에는 mixduration = 0
        if(comboNum == 0)
        {
            entry.MixDuration = 0.05f;
        }
        else
        {
            entry.MixDuration = 0.1f;
        }
        
        entry.AnimationStart = 0;

        //Invoke("AfterAction", delay);

    }

    //대시
    public void DashCharacter()
    {
        skelAni.AnimationState.SetAnimation(0, "dashStart", false);
    }

    //대시 종료
    public void DashEndCharacter()
    {
        //대시 종료 애니메이션 실행이 끝나면 afterAction 실행
        skelAni.AnimationState.SetAnimation(0, "dashEnd", false);
        float delay = skelAni.skeleton.Data.FindAnimation("dashEnd").Duration;

        Invoke("AfterAction", delay);
    }

    //방어 준비
    public void DefenseReady()
    {
        skelAni.AnimationState.SetAnimation(0, "defense_idle", false);
    }

    //방어 동작
    public void DefenseAction()
    {
        skelAni.AnimationState.SetAnimation(0, "defense_action", false);
    }

    //차징 준비
    public void ChargingReady()
    {
        if (!isPlayed)
        {
            skelAni.AnimationState.SetAnimation(0, "charging_ready", false);
            isPlayed = true;
        }
    }

    //차징
    public void ChargingAction()
    {
 
        skelAni.AnimationState.SetAnimation(0, "charging_action", true);
        
            
    }

    //기본 스킬
    public void SkillNormalAction()
    {
        TrackEntry entry = skelAni.AnimationState.SetAnimation(0, "attack_skill", false);
        entry.MixDuration = 0f;
        entry.AnimationStart = 0;
    }


    public void AfterAction()
    {
        //이동 중이면 run, 이동 중이 아니면 idle     
        if (userController.isIdle)
        {
            IdleCharacter();
        }
        else
        {
            MoveCharacter();
        }
        MainBattleManager.instance.isUserDashing = false; //어떤 액션이든 종료 후에는 대시를 하면 안된다.
    }

}

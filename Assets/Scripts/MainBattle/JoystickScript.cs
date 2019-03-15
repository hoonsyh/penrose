using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class JoystickScript : MonoBehaviour, IPointerUpHandler, IDragHandler, IPointerDownHandler
{
    Image joystickBackImage;
    Image joystickImage;
    Vector3 inputVector;
    Vector3 inputVectorSaved;

    bool touchJoystick;
    public UserAction userAction;
    public UserController userController;

	// Use this for initialization
	void Start () {

        joystickBackImage = transform.GetChild(0).GetComponent<Image>();
        joystickImage = transform.GetChild(0).GetChild(0).GetComponent<Image>();

	}
	
	// Update is called once per frame
	void Update () {


        if(Input.GetKey(KeyCode.LeftArrow))
        {
            inputVector = new Vector3 (-122, 0 ,0);
        }
        else if(Input.GetKey(KeyCode.RightArrow))
        {
            inputVector = new Vector3(122, 0, 0);
        }
        else if (Input.GetKey(KeyCode.UpArrow))
        {
            inputVector = new Vector3(0, 122, 0);
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            inputVector = new Vector3(0, -122, 0);
        }
        else
        {
            if(!touchJoystick)
            {
                inputVector = Vector3.zero;
            }
            
        }
		
	}

    public virtual void OnDrag(PointerEventData ped)
    {
        Vector2 pos;
        if(RectTransformUtility.ScreenPointToLocalPointInRectangle(joystickBackImage.rectTransform, ped.position, ped.pressEventCamera, out pos))
        {
            pos.x = (pos.x / joystickBackImage.rectTransform.sizeDelta.x);
            pos.y = (pos.y / joystickBackImage.rectTransform.sizeDelta.y);

            inputVector = new Vector3(pos.x, pos.y, 0);

            //조이스틱의 조작 거리 제한을 두고 싶으면 magnitude를 조절. inputVector.magnitude 까지 조이스틱이 이동할 수 있고, inputVector.magnitude를 넘어서면 inputVector.normalized/2에 조이스틱이 위치하게 됨
            inputVector = (inputVector.magnitude > 0.5f) ? inputVector.normalized/2 : inputVector;

            //공격 중이거나 대시 중이면 input 변경 x
            if(userAction.actionCombo == 0)
            {
                MainBattleManager.instance.userUnitVector = inputVector; // 드래그가 되는 경우에만 유저 유닛 벡터 설정
            }

            joystickImage.rectTransform.anchoredPosition = new Vector3(inputVector.x * (joystickBackImage.rectTransform.sizeDelta.x), inputVector.y * (joystickBackImage.rectTransform.sizeDelta.y));
        }
    }

    public virtual void OnPointerDown(PointerEventData ped)
    {
        touchJoystick = true;
        OnDrag(ped);
    }

    public virtual void OnPointerUp(PointerEventData ped)
    {
        
        touchJoystick = false;
        inputVector = Vector3.zero;
        //userController._moveVector = Vector3.zero;
        joystickImage.rectTransform.anchoredPosition = Vector3.zero;
    }

    public float GetHorizontalValue()
    {
        return inputVector.x;
    }

    public float GetVerticalValue()
    {
        return inputVector.y;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDetectUserScript : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.Log(transform.parent.gameObject.transform.GetSiblingIndex() + " " + collision.tag);
        if (collision.tag == "userUnit")
        {
            
            gameObject.transform.parent.GetComponent<EnemyAction>().detectUser = true;
            gameObject.transform.parent.GetComponent<EnemyAction>().FindTarget();

        }
        
    }
}

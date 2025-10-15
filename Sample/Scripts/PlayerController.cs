using System;
using System.Collections.Generic;
using ActionBuilder.Runtime;
using UnityEngine;
using UnityEngine.Assertions;


[Serializable]
public struct KeyActionNamePair
{
    public KeyCode keyCode;
    public string useActionName;
}


public class PlayerController : MonoBehaviour
{
    [Min(0)]
    public float moveSpeed;
    
    public List<KeyActionNamePair> triggerList;
    public ActionController actionController;


    private void Update()
    {
        for (int i = 0; i < triggerList.Count; ++i)
        {
            if (Input.GetKeyDown(triggerList[i].keyCode))
            {
                KeyActionNamePair pair = triggerList[i];
                //Assert.IsTrue(actionController.HasAction(pair.useActionName));
                
                //actionController.TriggerAction(pair.useActionName);
            }
        }


        if (Input.GetKey(KeyCode.A))
        {
            transform.position += Vector3.left * (moveSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.D))
        {
            transform.position += Vector3.right * (moveSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.W))
        {
            transform.position += Vector3.up * (moveSpeed * Time.deltaTime);
        }
        
        if (Input.GetKey(KeyCode.S))
        {
            transform.position += Vector3.down * (moveSpeed * Time.deltaTime);
        }
    }
}

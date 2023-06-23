using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public LayerMask RaycastLayerMax;
    public CharacterController characterController;
    public GameObject ball;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (Camera.main == null)
            {
                return;
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 1000, RaycastLayerMax, QueryTriggerInteraction.Ignore))
            {
                characterController.ReachPosition(hit.point);
            }
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            characterController.GetComponent<CharacterAI>().GiveCommand("pick ball");
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            characterController.GetComponent<CharacterAI>().GiveCommand("leave ball");
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            characterController.GetComponent<CharacterAI>().GiveCommand("inspect bush");
        }
    }
}

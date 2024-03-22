using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    Camera mainCamera;

    (bool isSuccess, RaycastHit target) GetTile()
    {
        Ray m_rayFromMouse = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitAtGameObject;

        LayerMask myMask = (1 << 8);

        if (Physics.Raycast(
            m_rayFromMouse.origin,
            m_rayFromMouse.direction,
            out hitAtGameObject,
            maxDistance: 200.0f,
            myMask
            ))
        {
            return (true, hitAtGameObject);
        }
        else return (false, default);
    }


    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;


    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            (bool isSuccess, RaycastHit target) = GetTile();
            if (isSuccess)
            {
                target.collider.gameObject.GetComponent<TileButtonController>().ActionLeftMouse();
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            (bool isSuccess, RaycastHit target) = GetTile();
            if (isSuccess)
            {
                target.collider.gameObject.GetComponent<TileButtonController>().ActionRightMouse();
            }
        }
        else if (Input.GetMouseButtonDown(2))
        {
            (bool isSuccess, RaycastHit target) = GetTile();
            if (isSuccess)
            {
                target.collider.gameObject.GetComponent<TileButtonController>().ActionMiddleMouse();
            }
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            GetComponent<GameManager>().Init();
        }
    }

    
}

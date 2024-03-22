using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileButtonController : MonoBehaviour
{
    GameManager gameManager;
    public int x;
    public int y;

    public void ActionLeftMouse()
    {
        gameManager.Explore(x, y);
    }
    public void ActionMiddleMouse()
    {
        gameManager.AutoExplore(x, y);
    }
    public void ActionRightMouse()
    {
        gameManager.SwichFlagStat(x, y);
    }

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }
}

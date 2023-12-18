using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Wallrun-Default", menuName = "Player Logic/Wallrun Logic/Default")]
public class PlayerWallrunDefault : PlayerWallrunSOBase
{
    public override void CheckTransitions()
    {
        base.CheckTransitions();
    }

    public override void DoEnterLogic()
    {
        base.DoEnterLogic();
        //StartWallrun();
    }

    public override void DoExitLogic()
    {
        base.DoExitLogic();
    }

    public override void DoFixedUpdateState()
    {
        base.DoFixedUpdateState();
        //WallrunMovement();

        //if(NotOnWall)
        //{
        //    StopWallrun();
        //}
    }

    public override void DoUpdateState()
    {
        base.DoUpdateState();
    }

    public override void ResetValues()
    {
        base.ResetValues();
    }
}

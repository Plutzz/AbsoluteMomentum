using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DebugMenu : Singleton<DebugMenu>
{
    public TextMeshProUGUI PlayerStateText;
    public TextMeshProUGUI GroundedCheckText;
    public TextMeshProUGUI WallrunCheckText;
    public TextMeshProUGUI VelocityText;
    public TextMeshProUGUI SpeedText;
}

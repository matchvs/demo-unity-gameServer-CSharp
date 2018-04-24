/*******************************************************************
** 文件名:	Gvalue
** 版  权:	(C)  2017 - 掌玩
** 创建人:	ZJ
** 日  期:	2017/09/06
** 版  本:	1.0
** 描  述:	
** 应  用:  全局定义常量

**************************** 修改记录 ******************************
** 修改人: ZJ
** 日  期: 2018/01/29
** 描  述: 删除无用信息
********************************************************************/
using System;
using System.Collections.Generic;

public class Gvalue
{
    // CmdHeartbeat 心跳
    public static uint CmdHeartbeat = 99999999;

    public static uint MaxPlayerNum = 3;
}
/// <summary>
/// gs配置文件
/// </summary>
public class Gsconfig
{
    public int LogLevel { get; set; }
    public string HostIp { get; set; }
    public string HostPort { get; set; }
}

public enum GameStatus {
    None = 0,
    GameLoadStatus = 1,
    GameReadyStatus = 2,
    GameStartStatus = 3,
    GameUploadStatus = 4,
    GameOverStatus = 5,
}

public enum PlayerStatus {
    None = 0,
    PlayerGameLoadStatus = 1,
    PlayerReadyStatus = 2,
    PlayerBattleStatus = 3,
    PlayerEscapeStatus = 4,
    PlayerUploadStatus = 5,
    PlayerDead = 6,
}

public enum RoomStatus {
    None = 0,
    Full = 1,
    ExsitSamePlayer = 2,
    AddPlayerOk = 3,
    JoinOver = 4,
}

public enum RewardStatus {
    None = 0,
    Disappear = 1,
}




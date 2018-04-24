/*******************************************************************
** 文件名:	Player
** 版  权:	(C)  2017 - 掌玩
** 创建人:	ZJ
** 日  期:	2017/09/07
** 版  本:	1.0
** 描  述:	
** 应  用:  玩家抽象

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/
using System;
using System.Collections.Generic;

public class Player
{
    public UInt32 Uid { get; private set; }
    public int roolID = 2;
    public int isRobot = 3;
    public int typeId = 4;  //飞机类型
    public float xpos = 5;
    public float ypos = 6;
    public float xrot = 7;
    public float yrot = 8;
    public bool robot = false;
    /// <summary>
    /// 用户跑完全程时间
    /// </summary>
    public UInt32 UseTime { get; set; }
    public UInt32 Score { get; set; }
    /// <summary>
    /// 用户状态(1-游戏加载完毕 2-准备 3-游戏中 4-逃跑 5-游戏上报)
    /// </summary>
    public PlayerStatus Status { get; set; }
    public UInt32 Nickname { get; private set; }
    public UInt32 Attr_1 { get;  set; }
    public UInt32 Attr_2 { get; private set; }
    public UInt32 Attr_3 { get; private set; }
    public UInt32 Attr_4 { get; private set; }
    public UInt32 PlayerId { get; private set; }
    public UInt32 PlayerLevel { get; private set; }
    public UInt32 PlayerPromote { get; private set; }
    /// <summary>
    /// 用户是否已经提前离开（上报完分数后离开）
    /// </summary>
    public UInt32 IsLeftInAdvance { get; private set; }
    /// <summary>
    /// 上报用户超时时间
    /// </summary>
    public UInt32 UploadPlayerTimeoutId { get; private set; }
    public List<byte> Data;
    public Dictionary<UInt32, Player> killDic = new Dictionary<uint, Player>();
    public int Hp
    {
        get;
        private set;
    }
    public Player(UInt32 userId)
    {
        Uid = userId;
    }
   
    public void UpdateHp(bool add, int value)
    {
        if (add)
        {
            Hp += value;
        }
        else
        {
            Hp -= value;
            if (Hp <= 0)
            {
                SetStatus(PlayerStatus.PlayerDead);
            }
        }
    }
    public void AddKill(Player player)
    {
        killDic.Add(player.Uid, player);
    }
    public int GetKillNum()
    {
        return killDic.Count;             
    }
    public void SetStatus(PlayerStatus status)
    {
        this.Status = status;
    }
}

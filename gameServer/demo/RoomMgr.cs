/*******************************************************************
** 文件名:	RoomMgr
** 版  权:	(C)  2017 - 掌玩
** 创建人:	ZJ
** 日  期:	2017/09/07
** 版  本:	1.0
** 描  述:	
** 应  用:  

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/
using System;
using System.Collections.Generic;

public class RoomMgr
{
    private Dictionary<UInt64, Room> rooms;
    public RoomMgr()
    {
        rooms = new Dictionary<UInt64, Room>();
    }
    public Room CreateRoom(UInt64 roomId, UInt32 gameId)
    {
        Room room = new Room()
        {
            RoomId = roomId,
            GameId = gameId,
            Status = GameStatus.GameLoadStatus,
            MaxPlayerNum = Gvalue.MaxPlayerNum,
        };
        //create房间时，mvs先创建，创建返回后，再join就不需判断。出现的BUG是，mvs创建，不等返回，直接再join, 多线程就会出现如下
        //e=System.ArgumentException: An item with the same key has already been added. Key: 1589340729308614723
        //at System.ThrowHelper.ThrowAddingDuplicateWithKeyArgumentException(Object key)
        //at System.Collections.Generic.Dictionary`2.TryInsert(TKey key, TValue value, InsertionBehavior behavior)
        //at RoomMgr.CreateRoom(UInt64 roomId, UInt32 gameId, UInt32 fieldId)
        //mvs报错message:stream recv error: rpc error: code = Unknown desc = Exception was thrown by handler.
        //mvs已经改为等创建返回后再join
        if (rooms.TryGetValue(roomId, out Room r))
        {
            Logger.Info("RoomMgr, already exist roomId:{0}", roomId);
            return r;
        }

        rooms.Add(roomId, room);
        return room;
    }

    public void ExitRoom(UInt64 roomId, UInt32 userId)
    {
        Room room = GetRoom(roomId);
        if (room == null)
        {
            Logger.Error("not exist roomId:{0}", roomId);
            return;
        }

        room.KickPlayer(userId);
    }

    public int GetPlayerKillNum(UInt64 roomId, UInt32 userId)
    {
        Room room = GetRoom(roomId);
        if (room == null)
        {
            Logger.Error("room id:{0} is not exist!", roomId);
            return -1;
        }
        return room.GetPlayerKillNum(userId);
    }
    public void SetRoomStart(UInt64 roomId)
    {
        Room room = GetRoom(roomId);
        if (room == null)
        {
            Logger.Error("room id:{0} is not exist!", roomId);
            return;
        }
        room.GameSart();
    }
    public void DeleteRoom(UInt64 id)
    {
        if (rooms.ContainsKey(id))
        {
            Logger.Info("delete room id:{0}", id);
            rooms.Remove(id);
        }
        else
        {
            Logger.Error("room id:{0} is not exist!", id);
        }
    }
    private bool IsGameOver(UInt64 roomId)
    {
        Room room = GetRoom(roomId);
        if (room != null)
        {
            return room.IsGameOver();
        }

        Logger.Error("roomId:{0} not exist!", roomId);
        return false;
    }
    /// <summary>
    /// 房间内所有人都结束了就删掉房间
    /// </summary>
    /// <param name="id"></param>
    public void EntityOver(UInt64 roomId, UInt32 userId)
    {
        if (rooms.TryGetValue(roomId, out Room room))
        {
            room.SomebodyDead(userId);
        }
        else
        {
            Logger.Error("room id:{0} is not exist!", roomId);
        }
    }
    public Player GetLastPlayer(UInt64 roomId)
    {
        Room room = GetRoom(roomId);
        if (room != null)
        {
            return room.GetLastPlayer();    
        }

        return null;
    }
    private void PlayerEscape(UInt64 roomId, UInt32 userId)
    {
        Room room = GetRoom(roomId);
        if (room == null)
        {
            Logger.Error("room id:{0} is not exist!", roomId);
            return;
        }
        room.Escape(userId);
    }
    public void SomebodyLeave(UInt64 roomId, UInt32 userId)
    {
        PlayerEscape(roomId, userId);
    }
    public bool IsAllPlayerFinishUpload(UInt64 roomId)
    {
        Room room = GetRoom(roomId);
        if (room == null)
        {
            Logger.Error("room id:{0} is not exist!", roomId);
            return false;
        }

        return room.IsAllUpload();
    }
    public Room GetRoom(UInt64 roomId)
    {
        if (rooms.TryGetValue(roomId, out Room room) == false)
        {
            Logger.Info("room id:{0} is not exist!", roomId);
            return null;
        }

        return room;
    }
}

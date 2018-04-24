/*******************************************************************
** 文件名:	FightHandler
** 版  权:	(C)  2018 - 掌玩
** 创建人:	ZJ
** 日  期:	2018/01/30
** 版  本:	1.0
** 描  述:	
** 应  用:  战斗示例

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/
using System;
using Stream;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf;
using Newtonsoft.Json.Linq;

public class FightHandler : BaseHandler
{
    private RoomMgr roomMgr;

    public RoomMgr RoomManager
    {
        get { return roomMgr; }
    }

    private BaseServer baseServer;

    public FightHandler(BaseServer server)
    {
        baseServer = server;

        roomMgr = new RoomMgr();
    }
    /// <summary>
    /// 创建房间
    /// </summary>
    /// <param name="msg"></param>
    public override IMessage OnCreateRoom(ByteString msg)
    {
        Request request = new Request();
        ByteUtils.ByteStringToObject(request, msg);

        Reply reply = new Reply()
        {
            UserID = request.UserID,
            GameID = request.GameID,
            RoomID = request.RoomID,
            Errno = ErrorCode.Ok,
            ErrMsg = "OnCreateRoom success"
        };

        ulong roomID = request.RoomID;
        Room room = roomMgr.GetRoom(roomID);
        if (room == null) {
            Logger.Info("DoCreateRoomV32, create Room, roomId={0}", request.RoomID);

            room = roomMgr.CreateRoom(request.RoomID, request.GameID);
        }

        Logger.Info("OnCreateRoom start, userId={0}, gameId={1}, roomId={2}", request.UserID, request.GameID, request.RoomID);
        
        CreateExtInfo createEx = new CreateExtInfo();
        ByteUtils.ByteStringToObject(createEx, request.CpProto);
        Logger.Info("OnCreateRoom CreateExtInfo, userId={0}, roomId={1}, state={2}, CreateTime={3}", createEx.UserID, createEx.RoomID, createEx.State, createEx.CreateTime);

        return reply;
    }
    /// <summary>
    /// 加入房间
    /// </summary>
    /// <param name="msg"></param>
    public override IMessage OnJoinRoom(ByteString msg)
    {
        Request request = new Request();
        ByteUtils.ByteStringToObject(request, msg);

        Reply reply = new Reply()
        {
            UserID = request.UserID,
            GameID = request.GameID,
            RoomID = request.RoomID,
            Errno = ErrorCode.Ok,
            ErrMsg = "OnJoinRoom success"
        };

        Room room = roomMgr.GetRoom(request.RoomID);
        if (room == null) {
            room = roomMgr.CreateRoom(request.RoomID, request.GameID);
        }
        RoomStatus status = room.AddPlayer(request.UserID);
        if (status != RoomStatus.AddPlayerOk) {
            reply.Errno = ErrorCode.InternalServerError;
            reply.ErrMsg = "DoJoinRoomV32 failed, add player failed";
        }

        Logger.Info("OnJoinRoom start, userId={0}, gameId={1}, roomId={2}", request.UserID, request.GameID, request.RoomID);

        JoinExtInfo joinEx = new JoinExtInfo();
        ByteUtils.ByteStringToObject(joinEx, request.CpProto);
        Logger.Info("OnJoinRoom JoinExtInfo, userId={0}, roomId={1}, JoinType ={2}", joinEx.UserID, joinEx.RoomID, joinEx.JoinType);

        return reply;
    }
    /// <summary>
    /// 加入房间Over
    /// </summary>
    /// <param name="msg"></param>
    public override IMessage OnJoinOver(ByteString msg)
    {
        Request request = new Request();
        ByteUtils.ByteStringToObject(request, msg);

        Reply reply = new Reply()
        {
            UserID = request.UserID,
            GameID = request.GameID,
            RoomID = request.RoomID,
            Errno = ErrorCode.Ok,
            ErrMsg = "OnJoinOver success"
        };

        Room room = roomMgr.GetRoom(request.RoomID);
        if (room == null) {
            string str = string.Format("DoJoinOverV32 failed, not exist room, roomId={0}", request.RoomID);
            Logger.Warn(str);

            reply.Errno = ErrorCode.InternalServerError;
            reply.ErrMsg = str;
        } else {
            Logger.Info("DoJoinOverV32, JoinOver");
            room.JoinOver();
        }

        Logger.Info("OnJoinOver start, userId={0}, gameId={1}, roomId={2}", request.UserID, request.GameID, request.RoomID);

        if (room.Count == room.MaxPlayerNum) {
            JObject data = new JObject();
            data["action"] = "gsReady";
            ByteString cpProto = JsonUtils.EncodetoByteString(data);

            PushToHotelMsg pushMsg = new PushToHotelMsg() {
                PushType = PushMsgType.UserTypeExclude,
                GameID = request.GameID,
                RoomID = request.RoomID,
                CpProto = cpProto,
            };
            PushToHotel(request.RoomID, pushMsg);
        } else {
            CreateRobot(room);
        }

        return reply;
    }
    /// <summary>
    /// 离开房间
    /// </summary>
    /// <param name="msg"></param>
    public override IMessage OnLeaveRoom(ByteString msg)
    {
        Request request = new Request();
        ByteUtils.ByteStringToObject(request, msg);

        Reply reply = new Reply()
        {
            UserID = request.UserID,
            GameID = request.GameID,
            RoomID = request.RoomID,
            Errno = ErrorCode.Ok,
            ErrMsg = "OnLeaveRoom success"
        };

        Room room = roomMgr.GetRoom(request.RoomID);
        if (room == null) {
            string str = string.Format("DoLeaveRoomV32 failed, not exist room, roomId={0}", request.RoomID);
            Logger.Warn(str);

            reply.Errno = ErrorCode.InternalServerError;
            reply.ErrMsg = str;
        } else {
            Logger.Info("DoLeaveRoomV32, Leave Room, RoomId={0}, UserId={1}", request.RoomID, request.UserID);
            roomMgr.SomebodyLeave(request.RoomID, request.UserID);
        }

        Logger.Info("OnLeaveRoom start, userId={0}, gameId={1}, roomId={2}", request.UserID, request.GameID, request.RoomID);

        return reply;
    }

    /// <summary>
    /// 踢人
    /// </summary>
    /// <param name="msg"></param>
    public override IMessage OnKickPlayer(ByteString msg)
    {
        Request request = new Request();
        ByteUtils.ByteStringToObject(request, msg);

        Reply reply = new Reply()
        {
            UserID = request.UserID,
            GameID = request.GameID,
            RoomID = request.RoomID,
            Errno = ErrorCode.Ok,
            ErrMsg = "OnKickPlayer success"
        };

        Room room = roomMgr.GetRoom(request.RoomID);
        if (room == null) {
            string str = string.Format("DoKickPlayerV32 failed, not exist room, roomId={0}", request.RoomID);
            Logger.Warn(str);

            reply.Errno = ErrorCode.InternalServerError;
            reply.ErrMsg = str;
        } else {
            Logger.Info("DoKickPlayerV32, roomId={0}, kickPlayerId={1}", request.RoomID, request.UserID);

            room.KickPlayer(request.UserID);
        }

        Logger.Info("OnKickPlayer start, userId={0}, gameId={1}, roomId={2}", request.UserID, request.GameID, request.RoomID);

        return reply;
    }
    /// <summary>
    /// 连接状态
    /// </summary>
    /// <param name="msg"></param>
    public override IMessage OnConnectStatus(ByteString msg)
    {
        Request request = new Request();
        ByteUtils.ByteStringToObject(request, msg);

        Reply reply = new Reply()
        {
            UserID = request.UserID,
            GameID = request.GameID,
            RoomID = request.RoomID,
            Errno = ErrorCode.Ok,
            ErrMsg = "OnConnectStatus success"
        };
        string status = request.CpProto.ToStringUtf8();

        Logger.Info("OnConnectStatus start, userId={0}, gameId={1}, roomId={2}, status = {3}", request.UserID, request.GameID, request.RoomID, status);

        //1.掉线了  2.重连成功  3.重连失败
        if (status == "3")
        {
            Logger.Info("OnConnectStatus leaveroom, userId={0}, gameId={1}, roomId={2}, status = {3}", request.UserID, request.GameID, request.RoomID, status);
            return OnLeaveRoom(msg);
        }

        Logger.Info("OnConnectStatus end, userId={0}, gameId={1}, roomId={2}, status = {3}", request.UserID, request.GameID, request.RoomID, status);

        return reply;
    }

    /// <summary>
    /// 房间详情
    /// </summary>
    /// <param name="msg"></param>
    public override void OnRoomDetail(ByteString msg)
    {
        Request request = new Request();
        ByteUtils.ByteStringToObject(request, msg);

        RoomDetail roomDetail = new RoomDetail();
        ByteUtils.ByteStringToObject(roomDetail, request.CpProto);

        Logger.Info("OnRoomDetail, roomId={0}, state={1}, maxPlayer={2}, mode={3}, canWatch={4}, owner={5}",
            roomDetail.RoomID, roomDetail.State, roomDetail.MaxPlayer, roomDetail.Mode, roomDetail.CanWatch, roomDetail.Owner);
        foreach (PlayerInfo player in roomDetail.PlayerInfos)
        {
            Logger.Info("player userId={0}", player.UserID);
        }
    }

    public void CreateRobot(Room room) {
        Random random = new Random();
        uint MaxPlayerNum = room.MaxPlayerNum;
        uint Count = room.Count;
        uint remainCount = MaxPlayerNum - Count;

        Logger.Info("房间最大人数： " + MaxPlayerNum + " 当前人数: " + Count + " 剩余人数: " + remainCount);
        for (uint i = 0; i < remainCount; i++) {
            int userid = random.Next(100000, 1000000);

            room.AddPlayer((uint)userid, true);

            JObject data = new JObject();
            data["action"] = "gsRobot";
            data["userid"] = userid;
            ByteString cpProtoRobot = JsonUtils.EncodetoByteString(data);
            PushToHotelMsg pushMsgRobot = new PushToHotelMsg() {
                PushType = PushMsgType.UserTypeExclude,
                GameID = room.GameId,
                RoomID = room.RoomId,
                CpProto = cpProtoRobot,
            };

            PushToHotel(room.RoomId, pushMsgRobot);
        }

        //发送准备请求
        JObject readyData = new JObject();
        readyData["action"] = "gsReady";
        ByteString cpProto = JsonUtils.EncodetoByteString(readyData);

        PushToHotelMsg pushMsg = new PushToHotelMsg() {
            PushType = PushMsgType.UserTypeExclude,
            GameID = room.GameId,
            RoomID = room.RoomId,
            CpProto = cpProto,
        };

        PushToHotel(room.RoomId, pushMsg);
    }


    public override IMessage OnHotelConnect(ByteString msg)
    {
        Connect connect = new Connect();
        ByteUtils.ByteStringToObject(connect, msg);
        Logger.Info("OnHotelConnect, gameID:{0}, roomID:{1}", connect.GameID, connect.RoomID);

        return new ConnectAck() { Status = (UInt32)ErrorCode.Ok };
    }
    public override IMessage OnHotelBroadCast(ByteString msg)
    {
        HotelBroadcast broadcast = new HotelBroadcast();
        ByteUtils.ByteStringToObject(broadcast, msg);
        Logger.Info("HotelBroadcast start, userID:{0} gameID:{1} roomID:{2} cpProto:{3}", broadcast.UserID, broadcast.GameID, broadcast.RoomID, broadcast.CpProto.ToStringUtf8());

        HotelBroadcastAck broadcastAck = new HotelBroadcastAck() { UserID = broadcast.UserID, Status = (UInt32)ErrorCode.Ok };

        string cpValue = broadcast.CpProto.ToStringUtf8();
        var obj = JObject.Parse(cpValue);
        String action = obj["action"].ToString();
        Logger.Info("请求数据: {0}", cpValue);
        Logger.Info("请求的action: {0}", action);
        if (action.Equals("gsReadyRsp")) {
            Room room = roomMgr.GetRoom(broadcast.RoomID);
            if (room.Ready(broadcast.UserID)) {
                JObject data = new JObject();
                data["action"] = "gsStart";
                JArray array = new JArray();
                for (int i = 0; i < 3; i++) {
                    RewardItem item = room.CreateRrewardItem();
                    JObject rewardObj = new JObject();
                    rewardObj["x"] = item.x;
                    rewardObj["y"] = item.y;
                    array.Add(rewardObj);
                }
                data["rewards"] = array;
                ByteString cpProto = JsonUtils.EncodetoByteString(data);
                PushToHotelMsg pushMsg = new PushToHotelMsg() {
                    PushType = PushMsgType.UserTypeExclude,
                    GameID = broadcast.GameID,
                    RoomID = broadcast.RoomID,
                    CpProto = cpProto,
                };

                PushToHotel(broadcast.RoomID, pushMsg);
            }
        }

        if (action.Equals("gsReward")) {
            int rewardID = (int)obj["rewardID"];
            int userID = (int)obj["userID"];
            Room room = roomMgr.GetRoom(broadcast.RoomID);
            bool eated = room.GetReward(rewardID);
            if (eated) {
                JObject data = new JObject();
                data["action"] = "gsRewardRsp";
                data["rewardID"] = rewardID;
                data["userID"] = userID;
                ByteString cpProto = JsonUtils.EncodetoByteString(data);
                PushToHotelMsg pushMsg = new PushToHotelMsg() {
                    PushType = PushMsgType.UserTypeExclude,
                    GameID = broadcast.GameID,
                    RoomID = broadcast.RoomID,
                    CpProto = cpProto,
                };

                PushToHotel(broadcast.RoomID, pushMsg);
            }
        }

        if (action.Equals("gsScore")) {
            int score = (int)obj["score"];
            int rewardNum = (int)obj["rewardNum"];
            bool roomOwner = (bool)obj["roomOwner"];
            Room room = roomMgr.GetRoom(broadcast.RoomID);
            if (roomOwner) {
                JArray robotScore = (JArray)obj["robotScore"];
                for (int i = 0; i < robotScore.Count; i++) {
                    JObject item = (JObject)robotScore[i];
                    int robotuserid = (int)item["userid"];
                    int robotscore = (int)item["score"];
                    int robotrewardNum = (int)item["rewardNum"];
                    room.ReportScore((uint)robotuserid, robotscore, robotrewardNum);
                }
            }

            bool flag = room.ReportScore(broadcast.UserID, score, rewardNum);
            if (flag) {
                JObject data = new JObject();
                data["action"] = "gsResult"; ;
                JArray resultList = new JArray();
                for (int i = 0; i < room.playerResults.Count; i++) {
                    Player player = room.playerResults[i];
                    JObject playerResult = new JObject();
                    playerResult["userid"] = player.Uid;
                    playerResult["rewardNum"] = player.Attr_1;
                    resultList.Add(playerResult);
                }
                data["resultList"] = resultList;
                ByteString cpProto = JsonUtils.EncodetoByteString(data);
                PushToHotelMsg pushMsg = new PushToHotelMsg() {
                    PushType = PushMsgType.UserTypeExclude,
                    GameID = broadcast.GameID,
                    RoomID = broadcast.RoomID,
                    CpProto = cpProto,
                };

                PushToHotel(broadcast.RoomID, pushMsg);
            }
        } else {
            PushToHotelMsg pushMsg = new PushToHotelMsg() {
                PushType = PushMsgType.UserTypeExclude,
                GameID = broadcast.GameID,
                RoomID = broadcast.RoomID,
                CpProto = broadcast.CpProto,
            };
            pushMsg.DstUids.Add(broadcast.UserID);

            PushToHotel(broadcast.RoomID, pushMsg);
        }

        //PushToHotel(broadcast.RoomID, pushMsg);

        //测试主动推送给MVS的两个消息
        string str = broadcast.CpProto.ToStringUtf8();
        Logger.Info("HotelBroadcast, str = {0}", str);

        String[] result = str.Split("|");
        if (result.Length > 1)
        {
            if (result[0] == "joinover")
            {
                String[] param = result[1].Split(",");
                if (param.Length > 1)
                {
                    UInt64 roomID = UInt64.Parse(param[0]);
                    UInt32 gameID = UInt32.Parse(param[1]);
                    UInt32 userID = UInt32.Parse(param[2]);
                    PushJoinOver(roomID, gameID, userID);
                }
            }
            else if (result[0] == "kickplayer")
            {
                String[] param = result[1].Split(",");
                if (param.Length > 2)
                {
                    UInt64 roomID = UInt64.Parse(param[0]);
                    UInt32 srcID = UInt32.Parse(param[1]);
                    UInt32 destID = UInt32.Parse(param[2]);

                    PushKickPlayer(roomID, srcID, destID);
                }
            }
            else if (result[0] == "getRoomDetail")
            {
                String[] param = result[1].Split(",");
                if (param.Length > 1)
                {
                    UInt32 gameID = UInt32.Parse(param[0]);
                    UInt64 roomID = UInt64.Parse(param[1]);
                    PushGetRoomDetail(roomID, gameID);
                }
            }
        }

        Logger.Info("HotelBroadcast end, userID:{0} gameID:{1} roomID:{2} cpProto:{3}", broadcast.UserID, broadcast.GameID, broadcast.RoomID, broadcast.CpProto.ToStringUtf8());

        return broadcastAck;
    }
    public override IMessage OnHotelCloseConnect(ByteString msg)
    {
        CloseConnect close = new CloseConnect();
        ByteUtils.ByteStringToObject(close, msg);
        Logger.Info("CloseConnect, gameID:{0} roomID:{1}", close.GameID, close.RoomID);

        baseServer.DeleteStreamMap(close.RoomID);

        return new CloseConnectAck() { Status = (UInt32)ErrorCode.Ok };
    }
    /// <summary>
    /// 主动推送给MVS，房间不可以再加人
    /// </summary>
    public void PushJoinOver(UInt64 roomId, UInt32 gameId, UInt32 userId = 0, UInt32 version = 2)
    {
        Logger.Info("PushJoinOver, roomID:{0}, gameID:{1}", roomId, gameId);

        JoinOverReq joinReq = new JoinOverReq()
        {
            RoomID = roomId,
            GameID = gameId,
            UserID = userId
        };
        baseServer.PushToMvs(userId, version, (UInt32)MvsGsCmdID.MvsJoinOverReq, joinReq);
    }
    /// <summary>
    /// 主动推送给MVS，踢掉某人
    /// </summary>
    /// <param name="roomId"></param>
    /// <param name="srcId"></param>
    /// <param name="destId"></param>
    public void PushKickPlayer(UInt64 roomId, UInt32 srcId, UInt32 destId, UInt32 userId = 0, UInt32 version = 2)
    {
        Logger.Info("PushKickPlayer, roomID:{0}, srcId:{1}, destId:{2}", roomId, srcId, destId);

        KickPlayer kick = new KickPlayer()
        {
            RoomID = roomId,
            SrcUserID = srcId,
            UserID = destId
        };
        baseServer.PushToMvs(userId, version, (UInt32)MvsGsCmdID.MvsKickPlayerReq, kick);
    }
    /// <summary>
    /// 获取房间详情
    /// </summary>
    /// <param name="roomId"></param>
    /// <param name="gameId"></param>
    public void PushGetRoomDetail(UInt64 roomId, UInt32 gameId, UInt32 userId = 1, UInt32 version = 2)
    {
        Logger.Info("PushGetRoomDetail, roomID:{0}, gameId:{1}", roomId, gameId);
        GetRoomDetailReq roomDetail = new GetRoomDetailReq()
        {
            RoomID = roomId,
            GameID = gameId
        };
        baseServer.PushToMvs(userId, version, (UInt32)MvsGsCmdID.MvsGetRoomDetailReq, roomDetail);
    }
    /// <summary>
    /// 推送给Hotel，根据roomID来区分是哪个Hotel
    /// </summary>
    /// <param name="roomID"></param>
    /// <param name="msg"></param>
    public void PushToHotel(UInt64 roomID, IMessage msg, UInt32 userId = 1, UInt32 version = 2)
    {
        baseServer.PushToHotel(userId, version, roomID, (UInt32)HotelGsCmdID.HotelPushCmdid, msg);
    }
}


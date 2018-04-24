/*******************************************************************
** 文件名:	Room
** 版  权:	(C)  2017 - 掌玩
** 创建人:	ZJ
** 日  期:	2017/09/07
** 版  本:	1.0
** 描  述:	
** 应  用:  room抽象

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/
using System;
using System.Collections.Generic;
using System.Timers;
using Google.Protobuf;
using Newtonsoft.Json.Linq;

public class Room {
	public UInt64 RoomId { get; set; }
	public UInt32 GameId { get; set; }
	public UInt32 FieldId { get; set; }
	/// <summary>
	/// 1-游戏加载 2-游戏准备 3-开始(房主调用) 4-游戏上报 5-游戏结束
	/// </summary>
	public GameStatus Status { get; set; }
	public RoomStatus RoomStatus { get; set; }
	public UInt32 MaxPlayerNum { get; set; }
	public UInt32 MapId { get; private set; }
	/// <summary>
	/// 游戏加载超时id
	/// </summary>
	public UInt32 LoadTimeoutId { get; private set; }
	/// <summary>
	/// 游戏准备超时id
	/// </summary>
	public UInt32 ReadyTimeoutId { get; private set; }
	/// <summary>
	/// 游戏结束超时id
	/// </summary>
	public UInt32 OverTimeoutId { get; private set; }
	/// <summary>
	/// 游戏开始时间
	/// </summary>
	public UInt32 StartTime { get; private set; }
	public uint Count
	{
		get { return (uint)players.Count; }
	}
	public Dictionary<UInt32, Player> Players { get { return players; } }  //所有玩家

	private Dictionary<UInt32, Player> players;
	private List<Player> remainPlayer;  //未死玩家
	private List<RewardItem> rewards;
	public List<Player> playerResults;
    private Timer t;
	public Room() {
		players = new Dictionary<UInt32, Player>();
		remainPlayer = new List<Player>();
		rewards = new List<RewardItem>();
		playerResults = new List<Player>();

        //创建定时器，发送机器人
        //        t = new Timer(10000);
        //        t.Elapsed += new ElapsedEventHandler(CreateRobot);
        //        t.AutoReset = false;
        //        t.Enabled = true;
        //        t.Start();
    }

   

    public RoomStatus AddPlayer(UInt32 userId,bool robot = false) {
		if (players.Count + 1 > MaxPlayerNum) {
			Logger.Error("roomId:{0} is full, don't contain userId:{1}", RoomId, userId);
			return RoomStatus.Full;
		}

		if (players.ContainsKey(userId)) {
			Logger.Error("roomId:{0} already exist userId:{1}", RoomId, userId);
			return RoomStatus.ExsitSamePlayer;
		}

		Player player = new Player(userId);
        player.robot = robot;
		players.Add(userId, player);
		remainPlayer.Add(player);

		return RoomStatus.AddPlayerOk;
	}
	public int GetPlayerKillNum(UInt32 userId) {
		Player player = GetPlayer(userId);
		if (player == null) {
			return -1;
		}

		return player.killDic.Count;
	}
	public Player GetPlayer(UInt32 userId) {
		if (players.TryGetValue(userId, out Player player)) {
			return player;
		} else {
			Logger.Error("not exist userId:{0}", userId);
			return null;
		}
	}
	public void KickPlayer(UInt32 userId) {
		if (players.TryGetValue(userId, out Player player)) {
			if (player.Status < PlayerStatus.PlayerBattleStatus) {
				players.Remove(userId);
				remainPlayer.Remove(player);
			} else {
				player.SetStatus(PlayerStatus.PlayerEscapeStatus);
			}
		} else {
			Logger.Error("not exist userId:{0}", userId);
		}
	}
	public void GameSart() {
		Status = GameStatus.GameStartStatus;
		foreach (KeyValuePair<UInt32, Player> player in players) {
			player.Value.SetStatus(PlayerStatus.PlayerBattleStatus);
		}
	}

	int minX = -240;
	int maxX = 240;
	int minY = -260;
	int maxY = 260;
	public RewardItem CreateRrewardItem() {
		Random random = new Random();
		
		int x = random.Next(minX, maxX);
		int y = random.Next(minY, maxY);
		RewardItem item = new RewardItem(x, y);
		rewards.Add(item);
		return item;
	}

	public bool GetReward(int rewardID)
	{
		RewardItem reward = rewards[rewardID];
		RewardStatus status = reward.Status;
		if (status == RewardStatus.None)
		{
			reward.Status = RewardStatus.Disappear;
			return true;
		}
		return false;
	}

	public bool Ready(UInt32 userid) {
		Status = GameStatus.GameReadyStatus;
		foreach (KeyValuePair<UInt32, Player> player in players) {
            if(player.Value.robot)
                player.Value.SetStatus(PlayerStatus.PlayerReadyStatus);
            
            if (player.Value.Uid == userid)
				player.Value.SetStatus(PlayerStatus.PlayerReadyStatus);
		}

		foreach (KeyValuePair<UInt32, Player> player in players)
		{
			if (player.Value.Status != PlayerStatus.PlayerReadyStatus)
			{
                Logger.Info("有玩家没准备: " + player.Value.PlayerId);
				return false;
			}
		}

		return true;
	}

	public bool ReportScore(uint userid,int score,int rewardNum)
	{
		Player player = new Player((uint)userid);
		player.Score = (uint)score;
		player.Attr_1 = (uint)rewardNum;

		playerResults.Add(player);

		if (playerResults.Count == MaxPlayerNum)
		{
			playerResults.Sort((Player a, Player b) => { return -a.Score.CompareTo(b.Score);});
			return true;
		}


		return false;
	}


	public void SomebodyDead(UInt32 userId) {
		if (players.TryGetValue(userId, out Player player)) {
			player.SetStatus(PlayerStatus.PlayerDead);
			remainPlayer.Remove(player);
		} else {
			Logger.Error("userId:{0} is not exist!", userId);
		}
	}
	public Player GetLastPlayer() {
		if (remainPlayer.Count == 1) {
			return remainPlayer[0];
		}

		return null;
	}
	public bool IsGameOver() {
		foreach (KeyValuePair<UInt32, Player> player in players) {
			if (player.Value.Status != PlayerStatus.PlayerDead) {
				return false;
			}
		}

		return true;
	}
	public void Escape(UInt32 userId) {
		if (players.TryGetValue(userId, out Player player) == false) {
			Logger.Error("Escape userId:{0} is not exist!");
			return;
		}
		Logger.Info("userId:{0} escaped!", userId);

		player.SetStatus(PlayerStatus.PlayerEscapeStatus);

		players.Remove(userId);
		remainPlayer.Remove(player);
	}
	public bool IsAllUpload() {
		foreach (KeyValuePair<UInt32, Player> player in players) {
			if (player.Value.Status != PlayerStatus.PlayerUploadStatus && player.Value.Status != PlayerStatus.PlayerEscapeStatus) {
				return false;
			}
		}
		return true;
	}
	/// <summary>
	/// 房间JoinOver
	/// </summary>
	public void JoinOver() {
		RoomStatus = RoomStatus.JoinOver;
	}

	public void Leave() {

	}
}

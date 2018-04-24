

using System;

public class RewardItem
{
    public RewardStatus Status { get; set; }
    public int x;
    public int y;
    public RewardItem(int x, int y) {
        this.x = x;
        this.y = y;
    }
    public void SetRewardStatus(RewardStatus status) {
        this.Status = status;
    }
}


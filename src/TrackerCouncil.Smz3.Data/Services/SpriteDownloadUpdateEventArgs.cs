using System;

namespace TrackerCouncil.Smz3.Data.Services;

public class SpriteDownloadUpdateEventArgs(int completed, int total) : EventArgs
{
    public int Completed => completed;
    public int Total => total;
}

namespace Seecool.VideoAR
{
    public interface IVideoARInfo
    {
        string VideoId { get; }
        ITargetInfo[] Targets { get; }
    }
}
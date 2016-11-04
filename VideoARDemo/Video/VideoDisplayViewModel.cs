using AopUtil.WpfBinding;
using CCTVModels;
using Common.Util;
using Seecool.VideoAR;
using System;
using System.Linq;
using System.Threading;
using System.Windows.Media;
using VideoRender;
using VideoStreamClient.Events;

namespace VideoARDemo
{
    public class VideoDisplayViewModel: ObservableObject, IDisposable
    {
        [AutoNotify]
        public string VideoId { get; private set; }
        [AutoNotify]
        public ImageSource ImageSrc { get; private set; }
        [AutoNotify]
        public Stretch StretchMode { get; set; } = Stretch.Fill;
        [AutoNotify]
        public string VideoName { get; set; } = "未知视频";
        [AutoNotify]
        public int Width { get; private set; }
        [AutoNotify]
        public int Height { get; private set; }

        [AutoNotify]
        public DateTime LastImageTime { get; private set; } = DateTime.Now;

        VideoFrameBuffer _frameBuffer = new VideoFrameBuffer();
        IRenderSource _renderSource;
        public VideoDisplayViewModel()
        {
            _frameBuffer.VideoFrameEvent += _frameBuffer_VideoFrameEvent;
            _frameBuffer.Start();

            _renderSource = new D3DImageSource();
            _renderSource.ImageSourceChanged += () => updateImageSource(_renderSource.ImageSource);
        }

        #region 视频播放
        VideoData _videoData;
        object _videoSourceLockObj = new object();  
        public void PlayVideo(string videoId, int streamIndex = -1)
        {
            StopVideo();

            VideoId = videoId;

            _tryTimes = 0;
            playAndRetry(videoId, streamIndex);
        }
        Timer _timer;
        int _tryTimes = 0;
        void playAndRetry(string videoId, int streamIndex = -1)
        {
            if (play(videoId, streamIndex))
            {
                _fluentAverageCalculator = new FluentAverageCalculator();
            }
            else
            {
                if (videoId == VideoId)
                {
                    TimerCallback callback = x =>
                    {
                        WindowUtil.BeginInvoke(() => playAndRetry(videoId, streamIndex));
                    };
                    if (_tryTimes++ >= 10)
                    {
                        string msg = "============已尝试很多次，仍无法播放视频:{0} ========";
                        Console.WriteLine(msg, videoId);
                        Common.Log.Logger.Default.Error(msg, videoId);
                    }
                    else
                        _timer = new Timer(callback, null, 500, Timeout.Infinite);
                }
            }
        }
        
        bool play(string videoId, int streamIndex = -1)
        {
            LastImageTime = DateTime.Now;

            if (videoId == VideoId)
            {
                CCTVStaticInfo videoInfo = CCTVInfoManager.Instance.GetStaticInfo(videoId);
                if (videoInfo != null && videoInfo.Streams != null && videoInfo.Streams.Length > 0)
                {
                    VideoName = CCTVInfoManager.Instance.GetVideoReadableName(VideoId);
                    //Console.WriteLine(VideoId + "___" + VideoName);
                    if (streamIndex < 0)
                        streamIndex = videoInfo.Streams[0].Index;
                    StreamInfo streamInfo = videoInfo.Streams.First(x => x.Index == streamIndex);
                    if (streamInfo != null)
                    {
                        lock (_videoSourceLockObj)
                        {
                            _videoData = VideoDataManager.Instance.GetVideoData(videoId, streamInfo.Url);
                            _videoData.VideoSource.VideoFrameReceived += VideoSource_VideoFrameReceived;
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        private void VideoSource_VideoFrameReceived(object sender, VideoFrameEventArgs e)
        {
            //TODO:有可能导致数据溢出的强制数据转换。
            _frameBuffer.InputVideoFrame(e.Frame.Width, e.Frame.Height, e.Frame.Data, (int)e.Frame.Timestamp);
        }

        public void StopVideo()
        {
            VideoName = "未知视频";
            VideoId = string.Empty;
            _timer = null;
            releaseVideoSource();
            _frameBuffer.Clear();
            Width = 0;
            Height = 0;
        }
        
        void releaseVideoSource()
        {
            lock (_videoSourceLockObj)
            {
                if (_videoData != null)
                {
                    _videoData.VideoSource.VideoFrameReceived -= VideoSource_VideoFrameReceived;
                }
                _videoData = null;
            }
        }
        #endregion 视频播放

        FluentAverageCalculator _fluentAverageCalculator = new FluentAverageCalculator();
        private void _frameBuffer_VideoFrameEvent(int width, int height, byte[] frameData, int timeStamp)
        {
            updateFrame(frameData, width, height);
        }

        void updateFrame(byte[] frame, int width, int height)
        {
            if (width != Width || height != Height)
            {
                Width = width;
                Height = height;
                _renderSource.SetupSurface(width, height);
                updateStretch();
            }
            renderFrame(frame, width, height);
        }

        void renderFrame(byte[] frame, int width, int height)
        {
            _renderSource.Render(frame);
            LastImageTime = DateTime.Now;
        }

        void updateImageSource(ImageSource imgSrc)
        {
            WindowUtil.BeginInvoke(() =>
            {
                this.ImageSrc = imgSrc;
                //Console.WriteLine(imgSrc.Width + " " + imgSrc.Height);
            });
        }

        void updateStretch()
        {
            WindowUtil.BeginInvoke(() =>
            {
                Stretch stretch = this.StretchMode;
                this.StretchMode = Stretch.None;
                this.StretchMode = stretch;
            });
        }

        public void Dispose()
        {
            StopVideo();

            _frameBuffer.VideoFrameEvent -= _frameBuffer_VideoFrameEvent;
            _frameBuffer.Stop();
        }

        ~VideoDisplayViewModel()
        {
            Dispose();
        }
    }
}
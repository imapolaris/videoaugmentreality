using Adapter.Proto;
using Seecool.DataBus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Seecool.VideoAR.DataBus
{
    class DataBusDataReceiver : IDisposable
    {
        private Consumer _consumer;
        private string _endpoint;
        private string[] _topics;
        private CancellationTokenSource _cts;
        public Action<ScUnion> DynamicEvent;
        ManualResetEvent _disposeEvent = new ManualResetEvent(false);
        //"tcp://127.0.0.1:62626",ScUnion
        public DataBusDataReceiver(string endpoint, string[] topics)
        {
            _endpoint = endpoint;
            _topics = topics;
            _disposeEvent.Reset();
            startup();
        }
        private void startup()
        {
            try
            {
                _cts = new CancellationTokenSource();
                _consumer = new Consumer(_endpoint, _topics);
                //Task.Factory.StartNew(onReceive, _cts.Token);
                new Thread(onReceive) { IsBackground = true }.Start();
            }
            catch (Exception ex)
            {
                throw new Exception("DataBus连接错误！", ex);
            }
        }

        private void onReceive()
        {
            //while (!_cts.IsCancellationRequested)
            while (!_disposeEvent.WaitOne(0))
            {
                try
                {
                    Seecool.DataBus.Message msg;
                    _consumer.TryReceiveMessage(out msg);
                    handleData(msg);
                }
                catch (ZmqException err)
                {
                    Console.WriteLine(err.ToString());
                    Thread.Sleep(10 * 1000);
                }
                catch (Exception err)
                {
                    Console.WriteLine(err.ToString());
                }
            }
        }

        private void handleData(Seecool.DataBus.Message msg)
        {
            if (msg.Data == null || msg.Data.Length == 0)
                return;
            using (MemoryStream ms = new MemoryStream(msg.Data))
            {
                ScUnion union = ProtoBuf.Serializer.Deserialize<ScUnion>(ms);
                //Console.WriteLine($"{union.ID} ： MMSI: {union.MMSI} Lon: {union.Longitude}, Lat: {union.Latitude} SOG: {union.SOG} COG: {union.COG}");
                onData(union);
            }
        }

        private void onData(ScUnion union)
        {
            var handler = DynamicEvent;
            if (handler != null)
                handler(union);
        }
        
        private void shutdown()
        {
            _cts?.Cancel();
            if (_consumer != null)
            {
                _consumer.Dispose();
                _consumer = null;
            }
        }

        public void Dispose()
        {
            _disposeEvent.Set();
            shutdown();
        }
    }
}

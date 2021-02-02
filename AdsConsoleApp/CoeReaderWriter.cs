using System;
using System.Text;
using TwinCAT.Ads;

namespace AdsConsoleApp
{
    internal class CoeReaderWriter : IDisposable
    {
        private const int SdoIndexGroup = 0xF302;
        private readonly AmsAddress _address;
        private AdsClient _client;
        private bool disposedValue;

        public CoeReaderWriter(AmsAddress address)
        {
            _address = address;
        }

        public T Read<T>(ushort index, byte subIndex, ushort stringLength = 0)
        {
            if (_client?.IsConnected != true)
                Connect();

            // https://infosys.beckhoff.de/index.php?content=../content/1031/eap/9007200776467979.html&id=

            uint indexOffset = ((uint)index) << 16 | subIndex;
            if (typeof(T) == typeof(string))
            {
                var m = new Memory<byte>(new byte[stringLength]);
                _client.Read(SdoIndexGroup, indexOffset, m);
                return (T)(object)Encoding.ASCII.GetString(m.ToArray());
            }
            else
            {
                return _client.ReadAny<T>(SdoIndexGroup, indexOffset);
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _client?.Dispose();
                }

                disposedValue = true;
            }
        }

        private void Connect()
        {
            _client = new AdsClient();
            _client.Connect(_address);
        }
    }
}
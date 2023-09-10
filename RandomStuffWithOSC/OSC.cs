using System.Net;
using System.Net.Sockets;

namespace VRCSoundpad
{
    internal class OSC
    {
        private const string ipAddress = "127.0.0.1";
        private const int port = 9001;

        private static CancellationTokenSource recvThreadCts;
        private static Socket receiver;
        public static void Init()
        {
            receiver = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            receiver.Bind(new IPEndPoint(IPAddress.Parse(ipAddress), port));
        }

        public static void Listen()
        {
            while (true)
            {
                recvThreadCts?.Cancel();

                var newToken = new CancellationTokenSource();
                ThreadPool.QueueUserWorkItem(new WaitCallback(ct =>
                {
                    var token = (CancellationToken)ct;
                    while (!token.IsCancellationRequested)
                        Receive();
                }), newToken.Token);

                recvThreadCts = newToken;

                Thread.Sleep(1);
            }
        }

        private static byte[] buffer = new byte[4096];
        private const string soundpadAvatarAddress = "/avatar/parameters/SNDP_";
        private static void Receive()
        {
            try
            {
                var bytesReceived = receiver.Receive(buffer, buffer.Length, SocketFlags.None);

                Msg msg = ParseOSC(buffer, bytesReceived);
                if (msg.success && msg.address.StartsWith(soundpadAvatarAddress) && msg.value is bool && (bool)msg.value == true)
                {
                    Program.ReceiveSoundpadCommand(msg.address.Replace(soundpadAvatarAddress, ""));
                }

                Array.Clear(buffer, 0, bytesReceived);
            }
            catch { }
        }

        struct Msg
        {
            public string address;
            public object value;
            public bool success;
        }
        private static Msg ParseOSC(byte[] buffer, int length)
        {
            Msg msg = new Msg();
            msg.success = false;

            if (length < 4)
                return msg;

            int bufferPosition = 0;
            string address = ParseString(buffer, length, ref bufferPosition);
            if (address == "")
                return msg;

            msg.address = address;

            // checking for ',' char
            if (buffer[bufferPosition] != 44)
                return msg;
            bufferPosition++; // skipping ',' character

            char valueType = (char)buffer[bufferPosition];
            bufferPosition++;

            object value = null;
            switch (valueType)
            {
                case 'f':
                    value = ParesFloat(buffer, length, bufferPosition);

                    break;

                case 'i':
                    value = ParseInt(buffer, length, bufferPosition);

                    break;

                case 'F':
                    value = false;

                    break;

                case 'T':
                    value = true;

                    break;

                default:
                    break;
            }

            msg.value = value ?? 0;
            msg.success = true;

            return msg;
        }

        private static string ParseString(byte[] buffer, int length, ref int bufferPosition)
        {
            string address = "";

            // first character must be '/'
            if (buffer[0] != 47)
                return address;

            for (int i = 0; i < length; i++)
            {
                if (buffer[i] == 0)
                {
                    bufferPosition = i + 1;

                    if (bufferPosition % 4 != 0)
                    {
                        bufferPosition += 4 - (bufferPosition % 4);
                    }

                    break;
                }

                address += (char)buffer[i];
            }

            return address;
        }

        private static float ParesFloat(byte[] buffer, int length, int bufferPosition)
        {
            var valueBuffer = new byte[length - bufferPosition];

            int j = 0;
            for (int i = bufferPosition; i < length; i++)
            {
                valueBuffer[j] = buffer[i];

                j++;
            }

            float value = bytesToFLoat(valueBuffer);
            return value;
        }

        private static float bytesToFLoat(byte[] bytes)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes); // Convert big endian to little endian
            }

            float val = BitConverter.ToSingle(bytes, 0);
            return val;
        }

        private static int ParseInt(byte[] buffer, int length, int bufferPosition)
        {
            var valueBuffer = new byte[length - bufferPosition];

            int j = 0;
            for (int i = bufferPosition; i < length; i++)
            {
                valueBuffer[j] = buffer[i];

                j++;
            }

            int value = bytesToInt(valueBuffer);
            return value;
        }

        private static int bytesToInt(byte[] bytes)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes); // Convert big endian to little endian
            }

            int val = BitConverter.ToInt32(bytes, 0);
            return val;
        }
    }
}

using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace VRCSoundpad
{
    internal class OSC
    {
        private const string soundpadAvatarAddress = "/avatar/parameters/SNDP_";

        private const string ipAddress = "127.0.0.1";
        private const int port = 9001;

        private static UdpClient udp;
        public static void Init()
        {
            try
            {
                udp = new UdpClient(port);
            }
            catch
            {
                Console.WriteLine($"Failed to create listen socket! (Another app listening on {ipAddress}:{port} already?)");
            }
        }

        public static void StartListening()
        {
            Listen();
        }

        private static async void Listen()
        {
            while (true)
            {
                Receive();

                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }
        }

        private static async void Receive()
        {
            try
            {
                var data = await udp.ReceiveAsync();

                Msg msg = ParseOSC(data.Buffer, data.Buffer.Length);
                if (msg.success && msg.address.StartsWith(soundpadAvatarAddress) && msg.value is bool && (bool)msg.value == true)
                {
                    Program.ReceiveSoundpadCommand(msg.address.Replace(soundpadAvatarAddress, ""));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }
        }

        private static void AlignStringBytes(ref string str)
        {
            int strLen = str.Length;
            if (strLen % 4 != 0)
            {
                strLen += 4 - (strLen % 4);
            }

            for (int i = str.Length; i < strLen; i++)
            {
                str += '\0';
            }
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

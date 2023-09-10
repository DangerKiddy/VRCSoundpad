using SoundpadConnector;
namespace VRCSoundpad
{
    internal class Program
    {
        private static Soundpad soundpad;

        private static int totalCountOfSounds = 0;
        private static Random random = new Random();
        static void Main(string[] args)
        {
            InitSoundpad();
            OSC.Init();
            OSC.Listen();
        }

        private static void InitSoundpad()
        {
            soundpad = new Soundpad();
            soundpad.ConnectAsync();
            Thread.Sleep(1000);

            totalCountOfSounds = (int)soundpad.GetSoundFileCount().Result.Value;
        }

        private static void SoundpadOnStatusChanged(object sender, EventArgs e)
        {
            Console.WriteLine(soundpad.ConnectionStatus);

            if (soundpad.ConnectionStatus == ConnectionStatus.Connected)
            {
                soundpad.PlaySound(1);
            }
        }

        public static Dictionary<int, bool> playedRandom = new(); 
        public static void ReceiveSoundpadCommand(string command)
        {
            switch (command)
            {

                case "Random":
                    soundpad.PlaySound(random.Next(1, totalCountOfSounds));

                    break;

                case "SmartRandom":
                    if (playedRandom.Count >= totalCountOfSounds)
                        playedRandom.Clear();

                    int randId;
                    do
                    {
                        randId = random.Next(1, totalCountOfSounds);
                    }
                    while (playedRandom.ContainsKey(randId));

                    playedRandom[randId] = true;
                    soundpad.PlaySound(randId);

                    break;

                case "Stop":
                    soundpad.StopSound();

                    break;

                case "TogglePause":
                    soundpad.TogglePause();

                    break;

                case "PlayLastPlayed":
                    soundpad.PlayPreviouslyPlayedSound();

                    break;

                case "PlaySelectedSound":
                    soundpad.PlaySelectedSound();

                    break;

                case "Forward1Sec":
                    soundpad.Jump(1000);

                    break;

                case "Forward3Sec":
                    soundpad.Jump(3000);
                
                    break;

                default:
                    try
                    {
                        soundpad.PlaySound(int.Parse(command));
                    }
                    catch { }

                    break;
            }
            /*
            if (command == "Random")
                soundpad.PlaySound(random.Next(1, totalCountOfSounds));
            else if (command == "SmartRandom")
            {
                if (playedRandom.Count >= totalCountOfSounds)
                    playedRandom.Clear();

                int randId;
                do
                {
                    randId = random.Next(1, totalCountOfSounds);
                }
                while (playedRandom.ContainsKey(randId));

                playedRandom[randId] = true;
                soundpad.PlaySound(randId);
            }
            else if (command == "Stop")
            {
                soundpad.StopSound();
            }
            else if (command == "TogglePause")
            {
                soundpad.TogglePause();
            }
            else if (command == "PlayLastPlayed")
            {
                soundpad.PlayPreviouslyPlayedSound();
            }
            else if (command == "PlaySelectedSound")
            {
                soundpad.PlaySelectedSound();
            }
            else if (command == "Forward1Sec")
            {
                soundpad.Jump(1000);
            }
            else if (command == "Forward3Sec")
            {
                soundpad.Jump(3000);
            }
            else
            {
                try
                {
                    soundpad.PlaySound(int.Parse(command));
                }
                catch { }
            }
            */
        }
    }
}
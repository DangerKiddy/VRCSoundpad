using SoundpadConnector;
namespace VRCSoundpad
{
    internal class Program
    {
        private static Soundpad soundpad;

        private static int totalCountOfSounds = 0;
        private static Random random = new Random();
        private delegate void OnSoundpadInit();
        private static OnSoundpadInit onSoundpadInit;
        static void Main(string[] args)
        {
            InitSoundpad();
            onSoundpadInit += InitOSC;

            Console.ReadKey();
        }

        private static void InitSoundpad()
        {
            soundpad = new Soundpad();
            soundpad.StatusChanged += SoundpadOnStatusChanged;

            soundpad.ConnectAsync();
        }

        private static void InitOSC()
        {
            OSC.Init();
            OSC.Listen();
        }

        private static void SoundpadOnStatusChanged(object sender, EventArgs e)
        {
            Console.WriteLine(soundpad.ConnectionStatus);

            if (soundpad.ConnectionStatus == ConnectionStatus.Connected)
            {
                totalCountOfSounds = (int)soundpad.GetSoundFileCount().Result.Value;

                onSoundpadInit();
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
                    catch
                    {
                        Console.WriteLine($"Failed to parse sound id! (Non-numeric id? {command})");
                    }

                    break;
            }
        }
    }
}
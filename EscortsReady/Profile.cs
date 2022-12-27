namespace EscortsReady
{
    public class Profile
    {
        public Guid uuid = Guid.NewGuid();
        public string name = Utils.Unset;

        public string vrcName = Utils.Unset;
        public GType gender;
        public SType serviceType;
        public EType equipmentType;
        public RType roleType;
        public GType bodyTypes;
        public string timesAvailable = Utils.Unset;
        public string kinksAndFetishes = Utils.Unset;
        public string doAndDonts = Utils.Unset;
        public string ReasonForJoining = Utils.Unset; 
        public string AboutMe = Utils.Unset;
        public string AvatarUrl = Utils.Unset;
        public string tipLink = Utils.Unset;

        public ulong serverID;
        public ulong channelID;
        public ulong messageID;
        public ulong memberID;
        public DateTime dateCreated;
        public DateTime TimeSinceLastRequested;
        public int likes = 0;
        public int dislikes = 0;
        public bool isHead;

    }
}

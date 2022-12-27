namespace EscortsReady
{
    public class DUser
    {
        public ulong id { get; internal set; }
        public string? username { get; internal set; }
        public string? discriminator { get; internal set; }
        public string? email { get; internal set; }
        public string? vrchatUsername { get; internal set; }

        public string Username() => $"{username}#{discriminator}";
        public string VRChatname() => $"{vrchatUsername}";
    }
}
namespace Wolfteam.Client.Config
{
    /// <summary>
    ///     General Wolfteam stuff.
    /// </summary>
    internal static class WolfteamConfig
    {
        /// <summary>
        ///     The login server ip address of http://wolfteam.aeriagames.com/.
        /// </summary>
        public const string LoginIp = "78.138.122.21";

        /// <summary>
        ///     The login server port of http://wolfteam.aeriagames.com/.
        /// </summary>
        public const int LoginPort = 8444;
        
        /// <summary>
        ///     The client version of http://wolfteam.aeriagames.com/.
        /// </summary>
        public const int ClientVersion = 448;

        /// <summary>
        ///     A magic number required for packet checksum.
        ///     Source: https://github.com/CarlosX/GunBoundWC
        /// </summary>
        public const int MagicChecksum = -1463622207; // (uint)2831345089
    }
}

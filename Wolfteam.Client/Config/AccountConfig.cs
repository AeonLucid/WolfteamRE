namespace Wolfteam.Client.Config
{
    /// <summary>
    ///     An account configuration for the <see cref="Net.Wolfteam.Session"/>.
    /// </summary>
    internal class AccountConfig
    {
        /// <summary>
        ///     An username valid for http://wolfteam.aeriagames.com/.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        ///     A password valid for http://wolfteam.aeriagames.com/.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        ///     The AES key that belongs to the <see cref="Username"/> and <see cref="AuthRandom"/> combination.
        ///     See the README.md for more information.
        /// </summary>
        public string AesKey { get; set; }

        /// <summary>
        ///     See the README.md for more information.
        /// </summary>
        public uint AuthRandom { get; set; }
    }
}

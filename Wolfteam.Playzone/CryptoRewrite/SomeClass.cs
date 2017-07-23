// Horrible attempt at reversing the current aes key generation.

namespace Wolfteam.Playzone.CryptoRewrite
{
    class SomeClass
    {
        /// <summary>
        ///     *(_DWORD*)a1
        /// </summary>
        public int One { get; set; }

        /// <summary>
        ///     *(_DWORD*)(a1 + 4)
        /// </summary>
        public int Two { get; set; }

        /// <summary>
        ///     *(_DWORD*)(a1 + 8)
        /// </summary>
        public int Three { get; set; }

        /// <summary>
        ///     *(_DWORD*)(a1 + 12)
        /// </summary>
        public int Four { get; set; }

        /// <summary>
        ///     *(_DWORD*)(a1 + 16)
        /// </summary>
        public int Five { get; set; }
    }
}

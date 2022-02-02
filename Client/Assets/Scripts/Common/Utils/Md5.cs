using System;
using System.IO;
using System.Text;

namespace Common.Utils
{
    /// <summary>
    /// The MD5 custom implementation for WP8.
    /// </summary>
    public sealed class Md5 : IDisposable
    {
        public static string GetMd5String(byte[] _Bytes)
        {
            Md5 md = Create();
            byte[] hash = md.ComputeHash(_Bytes);

            StringBuilder sb = new StringBuilder();
            foreach (byte b in hash)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
        
		private static string GetMd5String(Stream _Stream)
		{
			Md5 md = Create();
			byte[] hash = md.ComputeHash(_Stream);
			
			StringBuilder sb = new StringBuilder();
			foreach (byte b in hash)
				sb.Append(b.ToString("x2"));
			return sb.ToString();
		}     

		
		public static Md5 Create()
		{
			return new Md5();
		}

        #region base implementation of the MD5
        #region constants
        private const byte S11 = 7;
        private const byte S12 = 12;
        private const byte S13 = 17;
        private const byte S14 = 22;
        private const byte S21 = 5;
        private const byte S22 = 9;
        private const byte S23 = 14;
        private const byte S24 = 20;
        private const byte S31 = 4;
        private const byte S32 = 11;
        private const byte S33 = 16;
        private const byte S34 = 23;
        private const byte S41 = 6;
        private const byte S42 = 10;
        private const byte S43 = 15;
        private const byte S44 = 21;
        private static byte[] _padding = {
              0x80, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
              0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
              0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
             };
        #endregion

        #region F, G, H and I are basic MD5 functions.
        private static uint F(uint _X, uint _Y, uint _Z)
        {
            return (_X & _Y) | (~_X & _Z);
        }
        private static uint G(uint _X, uint _Y, uint _Z)
        {
            return (_X & _Z) | (_Y & ~_Z);
        }
        private static uint H(uint _X, uint _Y, uint _Z)
        {
            return _X ^ _Y ^ _Z;
        }
        private static uint I(uint _X, uint _Y, uint _Z)
        {
            return _Y ^ (_X | ~_Z);
        }
        #endregion

        #region rotates x left n bits.
        /// <summary>
        /// rotates x left n bits.
        /// </summary>
        /// <param name="_X"></param>
        /// <param name="_N"></param>
        /// <returns></returns>
        private static uint ROTATE_LEFT(uint _X, byte _N)
        {
            return (_X << _N) | (_X >> (32 - _N));
        }
        #endregion

        #region FF, GG, HH, and II transformations
        /// FF, GG, HH, and II transformations
        /// for rounds 1, 2, 3, and 4.
        /// Rotation is separate from addition to prevent recomputation.
        private static void Ff(ref uint _A, uint _B, uint _C, uint _D, uint _X, byte _S, uint _Ac)
        {
            _A += F(_B, _C, _D) + _X + _Ac;
            _A = ROTATE_LEFT(_A, _S);
            _A += _B;
        }
        private static void Gg(ref uint _A, uint _B, uint _C, uint _D, uint _X, byte _S, uint _Ac)
        {
            _A += G(_B, _C, _D) + _X + _Ac;
            _A = ROTATE_LEFT(_A, _S);
            _A += _B;
        }
        private static void Hh(ref uint _A, uint _B, uint _C, uint _D, uint _X, byte _S, uint _Ac)
        {
            _A += H(_B, _C, _D) + _X + _Ac;
            _A = ROTATE_LEFT(_A, _S);
            _A += _B;
        }
        private static void Ii(ref uint _A, uint _B, uint _C, uint _D, uint _X, byte _S, uint _Ac)
        {
            _A += I(_B, _C, _D) + _X + _Ac;
            _A = ROTATE_LEFT(_A, _S);
            _A += _B;
        }
        #endregion

        #region context info
        /// <summary>
        /// state (ABCD)
        /// </summary>
        uint[] m_State = new uint[4];

        /// <summary>
        /// number of bits, modulo 2^64 (lsb first)
        /// </summary>
        uint[] m_Count = new uint[2];

        /// <summary>
        /// input buffer
        /// </summary>
        byte[] m_Buffer = new byte[64];
        #endregion

        internal Md5()
        {
            Initialize();
        }

        /// <summary>
        /// MD5 initialization. Begins an MD5 operation, writing a new context.
        /// </summary>
        /// <remarks>
        /// The RFC named it "MD5Init"
        /// </remarks>
        public void Initialize()
        {
            m_Count[0] = m_Count[1] = 0;

            // Load magic initialization constants.
            m_State[0] = 0x67452301;
            m_State[1] = 0xefcdab89;
            m_State[2] = 0x98badcfe;
            m_State[3] = 0x10325476;
        }

        /// <summary>
        /// MD5 block update operation. Continues an MD5 message-digest
        /// operation, processing another message block, and updating the
        /// context.
        /// </summary>
        /// <param name="_Input"></param>
        /// <param name="_Offset"></param>
        /// <param name="_Count"></param>
        /// <remarks>The RFC Named it MD5Update</remarks>
        private void HashCore(byte[] _Input, int _Offset, int _Count)
        {
            int i;
            int index;
            int partLen;

            // Compute number of bytes mod 64
            index = (int)((this.m_Count[0] >> 3) & 0x3F);

            // Update number of bits
            if ((this.m_Count[0] += (uint)_Count << 3) < (uint)_Count << 3)
                this.m_Count[1]++;
            this.m_Count[1] += (uint)_Count >> 29;

            partLen = 64 - index;

            // Transform as many times as possible.
            if (_Count >= partLen)
            {
                Buffer.BlockCopy(_Input, _Offset, m_Buffer, index, partLen);
                Transform(m_Buffer, 0);

                for (i = partLen; i + 63 < _Count; i += 64)
                    Transform(_Input, _Offset + i);

                index = 0;
            }
            else
                i = 0;

            // Buffer remaining input
            Buffer.BlockCopy(_Input, _Offset + i, m_Buffer, index, _Count - i);
        }

        /// <summary>
        /// MD5 finalization. Ends an MD5 message-digest operation, writing the
        /// the message digest and zeroizing the context.
        /// </summary>
        /// <returns>message digest</returns>
        /// <remarks>The RFC named it MD5Final</remarks>
        private byte[] HashFinal()
        {
            byte[] digest = new byte[16];
            byte[] bits = new byte[8];
            int index, padLen;

            // Save number of bits
            Encode(bits, 0, m_Count, 0, 8);

            // Pad out to 56 mod 64.
            index = (int)(m_Count[0] >> 3 & 0x3f);
            padLen = index < 56 ? 56 - index : 120 - index;
            HashCore(_padding, 0, padLen);

            // Append length (before padding)
            HashCore(bits, 0, 8);

            // Store state in digest
            Encode(digest, 0, m_State, 0, 16);

            // Zeroize sensitive information.
            m_Count[0] = m_Count[1] = 0;
            m_State[0] = 0;
            m_State[1] = 0;
            m_State[2] = 0;
            m_State[3] = 0;

            // initialize again, to be ready to use
            Initialize();

            return digest;
        }

        /// <summary>
        /// MD5 basic transformation. Transforms state based on 64 bytes block.
        /// </summary>
        /// <param name="_Block"></param>
        /// <param name="_Offset"></param>
        private void Transform(byte[] _Block, int _Offset)
        {
            uint a = m_State[0], b = m_State[1], c = m_State[2], d = m_State[3];
            uint[] x = new uint[16];
            Decode(x, 0, _Block, _Offset, 64);

            // Round 1
            Ff(ref a, b, c, d, x[0], S11, 0xd76aa478); /* 1 */
            Ff(ref d, a, b, c, x[1], S12, 0xe8c7b756); /* 2 */
            Ff(ref c, d, a, b, x[2], S13, 0x242070db); /* 3 */
            Ff(ref b, c, d, a, x[3], S14, 0xc1bdceee); /* 4 */
            Ff(ref a, b, c, d, x[4], S11, 0xf57c0faf); /* 5 */
            Ff(ref d, a, b, c, x[5], S12, 0x4787c62a); /* 6 */
            Ff(ref c, d, a, b, x[6], S13, 0xa8304613); /* 7 */
            Ff(ref b, c, d, a, x[7], S14, 0xfd469501); /* 8 */
            Ff(ref a, b, c, d, x[8], S11, 0x698098d8); /* 9 */
            Ff(ref d, a, b, c, x[9], S12, 0x8b44f7af); /* 10 */
            Ff(ref c, d, a, b, x[10], S13, 0xffff5bb1); /* 11 */
            Ff(ref b, c, d, a, x[11], S14, 0x895cd7be); /* 12 */
            Ff(ref a, b, c, d, x[12], S11, 0x6b901122); /* 13 */
            Ff(ref d, a, b, c, x[13], S12, 0xfd987193); /* 14 */
            Ff(ref c, d, a, b, x[14], S13, 0xa679438e); /* 15 */
            Ff(ref b, c, d, a, x[15], S14, 0x49b40821); /* 16 */

            // Round 2
            Gg(ref a, b, c, d, x[1], S21, 0xf61e2562); /* 17 */
            Gg(ref d, a, b, c, x[6], S22, 0xc040b340); /* 18 */
            Gg(ref c, d, a, b, x[11], S23, 0x265e5a51); /* 19 */
            Gg(ref b, c, d, a, x[0], S24, 0xe9b6c7aa); /* 20 */
            Gg(ref a, b, c, d, x[5], S21, 0xd62f105d); /* 21 */
            Gg(ref d, a, b, c, x[10], S22, 0x2441453); /* 22 */
            Gg(ref c, d, a, b, x[15], S23, 0xd8a1e681); /* 23 */
            Gg(ref b, c, d, a, x[4], S24, 0xe7d3fbc8); /* 24 */
            Gg(ref a, b, c, d, x[9], S21, 0x21e1cde6); /* 25 */
            Gg(ref d, a, b, c, x[14], S22, 0xc33707d6); /* 26 */
            Gg(ref c, d, a, b, x[3], S23, 0xf4d50d87); /* 27 */
            Gg(ref b, c, d, a, x[8], S24, 0x455a14ed); /* 28 */
            Gg(ref a, b, c, d, x[13], S21, 0xa9e3e905); /* 29 */
            Gg(ref d, a, b, c, x[2], S22, 0xfcefa3f8); /* 30 */
            Gg(ref c, d, a, b, x[7], S23, 0x676f02d9); /* 31 */
            Gg(ref b, c, d, a, x[12], S24, 0x8d2a4c8a); /* 32 */

            // Round 3
            Hh(ref a, b, c, d, x[5], S31, 0xfffa3942); /* 33 */
            Hh(ref d, a, b, c, x[8], S32, 0x8771f681); /* 34 */
            Hh(ref c, d, a, b, x[11], S33, 0x6d9d6122); /* 35 */
            Hh(ref b, c, d, a, x[14], S34, 0xfde5380c); /* 36 */
            Hh(ref a, b, c, d, x[1], S31, 0xa4beea44); /* 37 */
            Hh(ref d, a, b, c, x[4], S32, 0x4bdecfa9); /* 38 */
            Hh(ref c, d, a, b, x[7], S33, 0xf6bb4b60); /* 39 */
            Hh(ref b, c, d, a, x[10], S34, 0xbebfbc70); /* 40 */
            Hh(ref a, b, c, d, x[13], S31, 0x289b7ec6); /* 41 */
            Hh(ref d, a, b, c, x[0], S32, 0xeaa127fa); /* 42 */
            Hh(ref c, d, a, b, x[3], S33, 0xd4ef3085); /* 43 */
            Hh(ref b, c, d, a, x[6], S34, 0x4881d05); /* 44 */
            Hh(ref a, b, c, d, x[9], S31, 0xd9d4d039); /* 45 */
            Hh(ref d, a, b, c, x[12], S32, 0xe6db99e5); /* 46 */
            Hh(ref c, d, a, b, x[15], S33, 0x1fa27cf8); /* 47 */
            Hh(ref b, c, d, a, x[2], S34, 0xc4ac5665); /* 48 */

            // Round 4
            Ii(ref a, b, c, d, x[0], S41, 0xf4292244); /* 49 */
            Ii(ref d, a, b, c, x[7], S42, 0x432aff97); /* 50 */
            Ii(ref c, d, a, b, x[14], S43, 0xab9423a7); /* 51 */
            Ii(ref b, c, d, a, x[5], S44, 0xfc93a039); /* 52 */
            Ii(ref a, b, c, d, x[12], S41, 0x655b59c3); /* 53 */
            Ii(ref d, a, b, c, x[3], S42, 0x8f0ccc92); /* 54 */
            Ii(ref c, d, a, b, x[10], S43, 0xffeff47d); /* 55 */
            Ii(ref b, c, d, a, x[1], S44, 0x85845dd1); /* 56 */
            Ii(ref a, b, c, d, x[8], S41, 0x6fa87e4f); /* 57 */
            Ii(ref d, a, b, c, x[15], S42, 0xfe2ce6e0); /* 58 */
            Ii(ref c, d, a, b, x[6], S43, 0xa3014314); /* 59 */
            Ii(ref b, c, d, a, x[13], S44, 0x4e0811a1); /* 60 */
            Ii(ref a, b, c, d, x[4], S41, 0xf7537e82); /* 61 */
            Ii(ref d, a, b, c, x[11], S42, 0xbd3af235); /* 62 */
            Ii(ref c, d, a, b, x[2], S43, 0x2ad7d2bb); /* 63 */
            Ii(ref b, c, d, a, x[9], S44, 0xeb86d391); /* 64 */

            m_State[0] += a;
            m_State[1] += b;
            m_State[2] += c;
            m_State[3] += d;

            // Zeroize sensitive information.
            for (int i = 0; i < x.Length; i++)
                x[i] = 0;
        }

        /// <summary>
        /// Encodes input (uint) into output (byte). Assumes len is
        ///  multiple of 4.
        /// </summary>
        /// <param name="_Output"></param>
        /// <param name="_OutputOffset"></param>
        /// <param name="_Input"></param>
        /// <param name="_InputOffset"></param>
        /// <param name="_Count"></param>
        private static void Encode(byte[] _Output, int _OutputOffset, uint[] _Input, int _InputOffset, int _Count)
        {
            int i, j;
            int end = _OutputOffset + _Count;
            for (i = _InputOffset, j = _OutputOffset; j < end; i++, j += 4)
            {
                _Output[j] = (byte)(_Input[i] & 0xff);
                _Output[j + 1] = (byte)((_Input[i] >> 8) & 0xff);
                _Output[j + 2] = (byte)((_Input[i] >> 16) & 0xff);
                _Output[j + 3] = (byte)((_Input[i] >> 24) & 0xff);
            }
        }

        /// <summary>
        /// Decodes input (byte) into output (uint). Assumes len is
        /// a multiple of 4.
        /// </summary>
        /// <param name="_Output"></param>
        /// <param name="_OutputOffset"></param>
        /// <param name="_Input"></param>
        /// <param name="_InputOffset"></param>
        /// <param name="_Count"></param>
        private static void Decode(uint[] _Output, int _OutputOffset, byte[] _Input, int _InputOffset, int _Count)
        {
            int i, j;
            int end = _InputOffset + _Count;
            for (i = _OutputOffset, j = _InputOffset; j < end; i++, j += 4)
                _Output[i] = _Input[j] | ((uint)_Input[j + 1] << 8) | ((uint)_Input[j + 2] << 16) | ((uint)_Input[j + 3] << 24);
        }
        #endregion

        #region expose the same interface as the regular MD5 object

        private byte[] m_HashValue;
        private int m_State1 = 0;
        public bool CanReuseTransform
        {
            get
            {
                return true;
            }
        }

        public bool CanTransformMultipleBlocks
        {
            get
            {
                return true;
            }
        }
        public byte[] Hash
        {
            get
            {
                if (m_State1 != 0)
                    throw new InvalidOperationException();
                return (byte[])m_HashValue.Clone();
            }
        }
        public int HashSize
        {
            get
            {
                return m_HashSizeValue;
            }
        }

        private int m_HashSizeValue = 128;

        public int InputBlockSize
        {
            get
            {
                return 1;
            }
        }
        public int OutputBlockSize
        {
            get
            {
                return 1;
            }
        }

        public void Clear()
        {
            Dispose(true);
        }

        public byte[] ComputeHash(byte[] _Buffer)
        {
            return ComputeHash(_Buffer, 0, _Buffer.Length);
        }
        public byte[] ComputeHash(byte[] _Buffer, int _Offset, int _Count)
        {
            Initialize();
            HashCore(_Buffer, _Offset, _Count);
            m_HashValue = HashFinal();
            return (byte[])m_HashValue.Clone();
        }

        public byte[] ComputeHash(Stream _InputStream)
        {
            Initialize();
            int count;
            byte[] buffer = new byte[4096];
            while (0 < (count = _InputStream.Read(buffer, 0, 4096)))
            {
                HashCore(buffer, 0, count);
            }
            m_HashValue = HashFinal();
            return (byte[])m_HashValue.Clone();
        }
        
        #endregion

        private void Dispose(bool _Disposing)
        {
            if (!_Disposing)
                Initialize();
        }
        public void Dispose()
        {
            Dispose(true);
        }
    }
}

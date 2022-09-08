using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace SharpHostInfo.Lib
{
    public class NTLMSSPExtract
    {

        #region Challenge 结构体 FromBytes  
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        private struct NTLM_CHALLENGE_MESSAGE
        {
            public Int64 Signature;
            public Int32 MessageType;
            //public long TargetNameFields;
            public Int16 TargetNameLen;
            public Int16 TargetNameMaxLen;
            public Int32 TargetNameBufferOffset;

            public Int32 NegotiateFlags;
            public Int64 ServerChallenge;
            public Int64 Reserved;
            //public long TargetInfoFields;
            public Int16 TargetInfoLen;
            public Int16 TargetInfoMaxLen;
            public Int32 TargetInfoBufferOffset;

            //public long Version;
            public Byte Major;
            public Byte Minor;
            public Int16 Build;
            public Int32 NTLM_Current_Revision;
        }

        /// <summary>
        /// 通过 Challenge 结构体进行数据匹配
        /// </summary>
        private static NTLM_CHALLENGE_MESSAGE ChallengeFromBytes(byte[] buffer)
        {
            NTLM_CHALLENGE_MESSAGE str = new NTLM_CHALLENGE_MESSAGE();
            int size = Marshal.SizeOf(str);
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(buffer, 0, ptr, size);
            str = (NTLM_CHALLENGE_MESSAGE)Marshal.PtrToStructure(ptr, str.GetType());
            Marshal.FreeHGlobal(ptr);
            return str;
        }
        #endregion

        private static int ReadInt2(byte[] src, int srcIndex)
        {
            return unchecked(src[srcIndex] & 0xFF) + ((src[srcIndex + 1] & 0xFF) << 8);
        }

        private static void ParseTargetInfo(byte[] records, ref SSPKey _SSPKey)
        {
            int pos = 0;
            while (pos + 4 < records.Length)
            {
                int recordType = ReadInt2(records, pos);
                int recordLength = ReadInt2(records, pos + 2);
                pos += 4;

                switch (recordType)
                {
                    case 1:
                        _SSPKey.NbtComputerName = Encoding.Unicode.GetString(records, pos, recordLength);
                        break;
                    case 2:
                        _SSPKey.NbtDomainName = Encoding.Unicode.GetString(records, pos, recordLength);
                        break;
                    case 3:
                        _SSPKey.DnsComputerName = Encoding.Unicode.GetString(records, pos, recordLength);
                        break;
                    case 4:
                        _SSPKey.DnsDomainName = Encoding.Unicode.GetString(records, pos, recordLength);
                        break;
                    case 7:
                        _SSPKey.TimeStamp = DateTime.FromFileTime(BitConverter.ToInt64(records, pos));
                        break;
                }
                pos += recordLength;
            }
        }

        /// <summary>
        /// 解析收到的 Response 数据
        /// </summary>
        public static SSPKey ParsingSocketStremResponse(ref byte[] responseBuffer, ref SSPKey _SSPKey)
        {
            try
            {
                var tmpBuffer = responseBuffer;
                var responseBuffer_String = BitConverter.ToString(tmpBuffer).Replace("-", "");
                var NTLMSSP_Bytes_Index = responseBuffer_String.IndexOf("4E544C4D53535000") / 2;

                var len = tmpBuffer.Length - NTLMSSP_Bytes_Index;
                var challengeResult = new Byte[len];
                Array.Copy(tmpBuffer, NTLMSSP_Bytes_Index, challengeResult, 0, len);

                NTLM_CHALLENGE_MESSAGE typeMessage = ChallengeFromBytes(challengeResult);

                _SSPKey.OsBuildNumber = typeMessage.Build;
                _SSPKey.OsMajor = typeMessage.Major;
                _SSPKey.OsMinor = typeMessage.Minor;

                var TargetInfo = challengeResult.Skip(typeMessage.TargetInfoBufferOffset).ToArray().Take(typeMessage.TargetInfoLen).ToArray();
                ParseTargetInfo(TargetInfo, ref _SSPKey);

                var otherOffset = typeMessage.TargetInfoBufferOffset + typeMessage.TargetInfoLen;
                len = len - otherOffset;
                var otherByteResult = new Byte[len];
                Array.Copy(challengeResult, otherOffset, otherByteResult, 0, len);
                responseBuffer = otherByteResult;
            }
            catch { }
            return _SSPKey;
        }
    }
}

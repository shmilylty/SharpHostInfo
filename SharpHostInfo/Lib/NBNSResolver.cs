using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;


namespace SharpHostInfo.Lib
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct nbname
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public char[] ascii_name;
        public UInt16 rr_flags;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct nbname_response_header
    {
        public UInt16 transaction_id;
        public UInt16 flags;
        public UInt16 question_count;
        public UInt16 answer_count;
        public UInt16 name_service_count;
        public UInt16 additional_record_count;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 34)]
        public char[] question_name;
        public UInt16 question_type;
        public UInt16 question_class;
        public UInt32 ttl;
        public UInt16 rdata_length;
        public byte number_of_names;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct nbname_response_footer
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public byte[] adapter_address;
        public byte version_major;
        public byte version_minor;
        public UInt16 duration;
        public UInt16 frmps_received;
        public UInt16 frmps_transmitted;
        public UInt16 iframe_receive_errors;
        public UInt16 transmit_aborts;
        public UInt32 transmitted;
        public UInt32 received;
        public UInt16 iframe_transmit_errors;
        public UInt16 no_receive_buffer;
        public UInt16 tl_timeouts;
        public UInt16 ti_timeouts;
        public UInt16 free_ncbs;
        public UInt16 ncbs;
        public UInt16 max_ncbs;
        public UInt16 no_transmit_buffers;
        public UInt16 max_datagram;
        public UInt16 pending_sessions;
        public UInt16 max_sessions;
        public UInt16 packet_sessions;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct nb_host_info
    {
        public nbname_response_header header;
        public nbname[] names;
        public nbname_response_footer footer;
        public int is_broken;
    }

    class NBNSResolver
    {
        public static Exception BrokenPacket = new Exception("Broken Packet");
        public static byte get8(byte[] buff, int offset)
        {
            return buff[offset];
        }

        public static UInt16 get16(byte[] buff, int offset)
        {
            short x = BitConverter.ToInt16(buff, offset);
            return ((UInt16)IPAddress.NetworkToHostOrder(x));
        }

        public static UInt32 get32(byte[] buff, int offset)
        {
            int x = BitConverter.ToInt32(buff, offset);
            return ((UInt32)IPAddress.NetworkToHostOrder(x));
        }

        public static byte[] getBytes(byte[] buff, int offset, int length)
        {
            byte[] _buff = new byte[length];
            Buffer.BlockCopy(buff, offset, _buff, 0, length);
            return _buff;
        }

        public static int getSize(Type clsType, String FieldName)
        {
            FieldInfo f = clsType.GetField(FieldName);
            MarshalAsAttribute ma = (MarshalAsAttribute)Attribute.GetCustomAttribute(f, typeof(MarshalAsAttribute));
            return ma.SizeConst;
        }

        public static char[] getCharArray(byte[] buff, int offset, int length)
        {
            return Encoding.UTF8.GetString(buff, offset, length).ToCharArray();
        }

        public static nb_host_info NBNSParser(byte[] buff, int buffsize)
        {
            int offset = 0;
            int size = 0;
            nb_host_info HostInfo = new nb_host_info();
            nbname_response_header Header = new nbname_response_header();
            nbname name = new nbname();
            nbname_response_footer Footer = new nbname_response_footer();

            #region dumm head parser
            //transaction_id
            size = Marshal.SizeOf(Header.transaction_id);
            if (offset + size >= buffsize) throw BrokenPacket;
            Header.transaction_id = get16(buff, offset);
            offset += size;
            //flags
            size = Marshal.SizeOf(Header.flags);
            if (offset + size >= buffsize) throw BrokenPacket;
            Header.flags = get16(buff, offset);
            offset += size;
            //question_count
            size = Marshal.SizeOf(Header.question_count);
            if (offset + size >= buffsize) throw BrokenPacket;
            Header.question_count = get16(buff, offset);
            offset += size;
            //answer_count
            size = Marshal.SizeOf(Header.answer_count);
            if (offset + size >= buffsize) throw BrokenPacket;
            Header.answer_count = get16(buff, offset);
            offset += size;
            //name_service_count
            size = Marshal.SizeOf(Header.name_service_count);
            if (offset + size >= buffsize) throw BrokenPacket;
            Header.name_service_count = get16(buff, offset);
            offset += size;
            //additional_record_count
            size = Marshal.SizeOf(Header.additional_record_count);
            if (offset + size >= buffsize) throw BrokenPacket;
            Header.additional_record_count = get16(buff, offset);
            offset += size;
            // question_name
            size = getSize(typeof(nbname_response_header), "question_name");
            if (offset + size >= buffsize) throw BrokenPacket;
            Header.question_name = getCharArray(buff, offset, size);
            offset += size;
            //question_type
            size = Marshal.SizeOf(Header.question_type);
            if (offset + size >= buffsize) throw BrokenPacket;
            Header.question_type = get16(buff, offset);
            offset += size;
            //question_class
            size = Marshal.SizeOf(Header.question_class);
            if (offset + size >= buffsize) throw BrokenPacket;
            Header.question_class = get16(buff, offset);
            offset += size;
            //ttl
            size = Marshal.SizeOf(Header.ttl);
            if (offset + size >= buffsize) throw BrokenPacket;
            Header.ttl = get32(buff, offset);
            offset += size;
            //rdata_length
            size = Marshal.SizeOf(Header.rdata_length);
            if (offset + size >= buffsize) throw BrokenPacket;
            Header.rdata_length = get16(buff, offset);
            offset += size;
            //number_of_names
            size = Marshal.SizeOf(Header.number_of_names);
            if (offset + size >= buffsize) throw BrokenPacket;
            Header.number_of_names = get8(buff, offset);
            offset += size;
            //
            HostInfo.header = Header;
            #endregion

            #region dumm name_table parser
            size = Marshal.SizeOf(name) * Header.number_of_names;
            if (offset + size >= buffsize) throw BrokenPacket;
            HostInfo.names = new nbname[Header.number_of_names];
            for (int i = 0; i < HostInfo.names.Length; i++)
            {
                nbname _name = new nbname();
                size = getSize(typeof(nbname), "ascii_name");
                _name.ascii_name = getCharArray(buff, offset, size);
                offset += size;
                size = Marshal.SizeOf(name.rr_flags);
                _name.rr_flags = get16(buff, offset);
                offset += size;
                HostInfo.names[i] = _name;
            }

            #endregion

            #region dumm foot parser
            //adapter_address
            size = getSize(typeof(nbname_response_footer), "adapter_address");
            if (offset + size >= buffsize) throw BrokenPacket;
            Footer.adapter_address = getBytes(buff, offset, size);
            offset += size;
            //version_major;
            size = Marshal.SizeOf(Footer.version_major);
            if (offset + size >= buffsize) throw BrokenPacket;
            Footer.version_major = get8(buff, offset);
            offset += size;
            //version_minor;
            size = Marshal.SizeOf(Footer.version_minor);
            if (offset + size >= buffsize) throw BrokenPacket;
            Footer.version_minor = get8(buff, offset);
            offset += size;
            //duration;
            size = Marshal.SizeOf(Footer.duration);
            if (offset + size >= buffsize) throw BrokenPacket;
            Footer.duration = get16(buff, offset);
            offset += size;
            //frmps_received;
            size = Marshal.SizeOf(Footer.frmps_received);
            if (offset + size >= buffsize) throw BrokenPacket;
            Footer.frmps_received = get16(buff, offset);
            offset += size;
            //frmps_transmitted;
            size = Marshal.SizeOf(Footer.frmps_transmitted);
            if (offset + size >= buffsize) throw BrokenPacket;
            Footer.frmps_transmitted = get16(buff, offset);
            offset += size;
            //iframe_receive_errors;
            size = Marshal.SizeOf(Footer.iframe_receive_errors);
            if (offset + size >= buffsize) throw BrokenPacket;
            Footer.iframe_receive_errors = get16(buff, offset);
            offset += size;
            //transmit_aborts;
            size = Marshal.SizeOf(Footer.transmit_aborts);
            if (offset + size >= buffsize) throw BrokenPacket;
            Footer.transmit_aborts = get16(buff, offset);
            offset += size;
            //transmitted;
            size = Marshal.SizeOf(Footer.transmitted);
            if (offset + size >= buffsize) throw BrokenPacket;
            Footer.transmitted = get32(buff, offset);
            offset += size;
            //received;
            size = Marshal.SizeOf(Footer.received);
            if (offset + size >= buffsize) throw BrokenPacket;
            Footer.received = get32(buff, offset);
            offset += size;
            //iframe_transmit_errors;
            size = Marshal.SizeOf(Footer.iframe_transmit_errors);
            if (offset + size >= buffsize) throw BrokenPacket;
            Footer.iframe_transmit_errors = get16(buff, offset);
            offset += size;
            //no_receive_buffer;
            size = Marshal.SizeOf(Footer.no_receive_buffer);
            if (offset + size >= buffsize) throw BrokenPacket;
            Footer.no_receive_buffer = get16(buff, offset);
            offset += size;
            //tl_timeouts;
            size = Marshal.SizeOf(Footer.tl_timeouts);
            if (offset + size >= buffsize) throw BrokenPacket;
            Footer.tl_timeouts = get16(buff, offset);
            offset += size;
            //ti_timeouts;
            size = Marshal.SizeOf(Footer.ti_timeouts);
            if (offset + size >= buffsize) throw BrokenPacket;
            Footer.ti_timeouts = get16(buff, offset);
            offset += size;
            //free_ncbs;
            size = Marshal.SizeOf(Footer.free_ncbs);
            if (offset + size >= buffsize) throw BrokenPacket;
            Footer.free_ncbs = get16(buff, offset);
            offset += size;
            //ncbs;
            size = Marshal.SizeOf(Footer.ncbs);
            if (offset + size >= buffsize) throw BrokenPacket;
            Footer.ncbs = get16(buff, offset);
            offset += size;
            //max_ncbs;
            size = Marshal.SizeOf(Footer.max_ncbs);
            if (offset + size >= buffsize) throw BrokenPacket;
            Footer.max_ncbs = get16(buff, offset);
            offset += size;
            //no_transmit_buffers;
            size = Marshal.SizeOf(Footer.no_transmit_buffers);
            if (offset + size >= buffsize) throw BrokenPacket;
            Footer.no_transmit_buffers = get16(buff, offset);
            offset += size;

            //seems always throw BrokenPacket if add codes below,havent find the reason yet.The struct may wrong?

            ////max_datagram;
            //size = Marshal.SizeOf(Footer.max_datagram);
            //if (offset + size >= buffsize) throw BrokenPacket;
            //Footer.max_datagram = get16(buff, offset);
            //offset += size;
            ////pending_sessions;
            //size = Marshal.SizeOf(Footer.pending_sessions);
            //if (offset + size >= buffsize) throw BrokenPacket;
            //Footer.pending_sessions = get16(buff, offset);
            //offset += size;
            ////max_sessions;
            //size = Marshal.SizeOf(Footer.max_sessions);
            //if (offset + size >= buffsize) throw BrokenPacket;
            //Footer.max_sessions = get16(buff, offset);
            //offset += size;
            ////packet_sessions;
            //size = Marshal.SizeOf(Footer.packet_sessions);
            //if (offset + size >= buffsize) throw BrokenPacket;
            //Footer.packet_sessions = get16(buff, offset);
            //offset += size;
            //
            HostInfo.footer = Footer;
            #endregion

            return HostInfo;
        }

        #region [INCOMPLETE] service identification
        ////when you want to identify service,put it in

        //static Dictionary<char, string> ServiceType = new Dictionary<char, string>()
        //{
        //    { '\x1c',"DC"},
        //};

        //public static string ServiceParser(char chrService)
        //{
        //    string ServiceName = "";
        //    if(ServiceType.ContainsKey(chrService))
        //        ServiceName=ServiceType[chrService];
        //    return ServiceName;
        //}

        #endregion


        #region mac to company


        public static string MACParser(String address)
        {
            string org = "";
            string mac = address.Replace("-", string.Empty).ToLower();
            string key = mac.Substring(0, 6);
            Dictionary<string, string> macs = GetMACDict();
            if (macs.ContainsKey(key))
                org = macs[key];
            return org;
        }

        public static Dictionary<string, string> GetMACDict()
        {
            Dictionary<string, string> MACDict = new Dictionary<string, string>()
                {
                    {"000c29","Vmware"},
                    {"005056","Vmware"},
                    {"000569","Vmware"},
                    {"001c14","Vmware"},
                    {"0242ac","Docker"},
                    {"0003ff","HyperV" },
                    {"000D3a","HyperV" },
                    {"00125a","HyperV" },
                    {"00155d","HyperV" },
                    {"0017fa","HyperV" },
                    {"001dd8","HyperV" },
                    {"002248","HyperV" },
                    {"0025ae","HyperV" },
                    {"0050f2","HyperV" },
                    {"444553","HyperV" },
                    {"7Ced8d","HyperV" },
                    {"0010e0","VirtualBox" },
                    {"00144f","VirtualBox" },
                    {"0020f2","VirtualBox" },
                    {"002128","VirtualBox" },
                    {"0021f6","VirtualBox" },
                    {"080027","VirtualBox" },
                    {"001c42","ParallelsVM" },
                    {"00163e","XensourceVM" },
                    {"080020","VirtualBox" },
                    {"0050c2","IEEE ReGi VM" },
                };
            // https://www.wireshark.org/assets/js/manuf.json
            string path = "manuf.json";
            if (!File.Exists(path))
            {
                return MACDict;
            }
            using (StreamReader reader = File.OpenText(path))
            {
                JObject o = (JObject)JToken.ReadFrom(new JsonTextReader(reader));
                string data = o["data"].ToString();
                MACDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);
            }
            return MACDict;
        }

        #endregion
    }
}

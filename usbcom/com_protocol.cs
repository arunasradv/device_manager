using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace usbcom
{
    /// <summary>
    /// C-Power5200 COM protocol
    /// </summary>
    public static class com_protocol
    {
        public struct com_packet_struct
        {
            public byte start; // The start of a packet 0xa5
            public byte type; // Recognition of this type of packet 0x68
            public byte card_type; // Fixed Type Code 0x32
            public byte card_id; // Control card ID 0x01~0xFE
            public byte protocol_code; // Protocol code 0X7B
            public byte additional; // Additional information 0xFF / confirmation mark;
                                    // The meaning of bytes in the packet is sent,
                                    // "Additional Information"
                                    // , is a packet plus instructions, and now only use the
                                    // lowest:
                                    // bit 0: whether to return a confirmation, 1 to return
                                    // and 0 not to return
                                    // bit1 ~bi7: reserved, set to 0
            public ushort length; //Packed data length
            public byte packet_number; //Packet number 0x00~0x255; When the packet
                                       //sequence number is equal to when the last packet
                                       //sequence number, indicating that this is the last
                                       //one package.
            public byte last_packet_index; //The total number of packages minus 1. 0x00~0x255

            public byte[] data;// Command sub-code and data 
            public ushort crc; // Packet data checksum SH SL;Two bytes, checksum。
                               // Lower byte in the former。
                               // The sum of each byte from " Packet type " to “ Packet data”
                               // content
            public byte end; // The end of a packet（Package tail）0xae
            public byte[] data_bytes;
        };
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cc">0x04</param>
        /// <param name="window"></param>
        /// <param name="aligment"></param>
        /// <param name="left_x"></param>
        /// <param name="left_y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="font_size"></param>
        /// <param name="font_style"></param>
        /// <param name="red"></param>
        /// <param name="green"></param>
        /// <param name="blue"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static byte[] make_cc_static_text(byte cc, byte window, byte aligment,
            ushort left_x, ushort left_y, ushort width,ushort height,byte font_size, byte font_style, byte red, byte green, byte blue, byte [] text )
        { 
            byte[] out_data = new byte[text.Length + 16];
            int index = 0;

            index= AddByte(cc,index,out_data);
            index=AddByte(window,index,out_data);
            index=AddByte(0x01,index,out_data); // data type const == 1
            index=AddByte(aligment,index,out_data);
            index=AddUShort(left_x,index,out_data,true);
            index=AddUShort(left_y,index,out_data,true);
            index=AddUShort(width,index,out_data,false);
            index=AddUShort(height,index,out_data,false);
            byte tb = (byte) (font_size | ((byte) (font_style << 4)));
            index=AddByte(tb,index,out_data);
            index=AddByte(red,index,out_data);
            index=AddByte(green,index,out_data);
            index=AddByte(blue,index,out_data);
            index=AddRange(text,index,out_data);
            return out_data; 
        }

        public static byte [] make_cc_text(byte cc,byte window,byte mode, byte aligment,byte speed,ushort stay_time, byte font_color, byte font_size,byte [] text)
        {
            byte [] out_data = new byte [text.Length*3+7];
            int index = 0;
            index=AddByte(cc,index,out_data);
            index=AddByte(window,index,out_data);
            index=AddByte(mode,index,out_data);
            index=AddByte(aligment,index,out_data);
            index=AddByte(speed,index,out_data);
            index=AddUShort(stay_time,index,out_data,true);

            for (int i = 0; i <text.Length - 1; i++)
            {
                byte tb = (byte)(font_size|((byte)(font_color<<4)));
                //byte tb = (byte)(font_color|((byte)(font_size<<4)));
                
                index=AddByte(tb,index,out_data);
                index=AddByte(text [i+1],index,out_data);
                index=AddByte(text [i],index,out_data);
                
                //index=AddByte(text [i],index,out_data);  
                //index=AddByte(text [i+1],index,out_data);
               



                i= i+1;
                
            }

            return out_data;
        }

        public static byte [] make_cc_puretext(byte cc,byte window,byte mode,byte aligment,byte speed,ushort stay_time,byte font_style,byte font_size, byte red, byte green, byte blue,byte [] text)
        {
            byte [] out_data = new byte [text.Length+11];
            int index = 0;
            index=AddByte(cc,index,out_data);
            index=AddByte(window,index,out_data);
            index=AddByte(mode,index,out_data);
            index=AddByte(aligment,index,out_data);
            index=AddByte(speed,index,out_data);
            index=AddUShort(stay_time,index,out_data,true);
            byte tb = (byte)(font_size|((byte)(font_style<<4)));
            index=AddByte(tb,index,out_data);
            index=AddByte(red,index,out_data);
            index=AddByte(green,index,out_data);
            index=AddByte(blue,index,out_data);
            AddRange(text,index,out_data);
          
            return out_data;
        }

        public static byte [] make_cc_play_programs(byte cc)
        {

            byte [] out_data = new byte [1];
            int index = 0;
            index=AddByte(cc,index,out_data);
            return out_data;
        }

        public static byte [] make_cc_play_program_n(byte cc, byte save_option, byte program_count, byte[] program_table)
        {
             
            byte [] out_data = new byte [1+1+1+program_table.Length];
            int index = 0;
            index=AddByte(cc,index,out_data);
            index=AddByte(save_option,index,out_data);
            index=AddByte(program_count,index,out_data);
            AddRange(program_table,index,out_data);
            return out_data;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="card_id"></param>
        /// <param name="return_flag"></param>
        /// <param name="packet_number"></param>
        /// <param name="max_packet_size"> max value is 512 </param>
        /// <param name="cc" > sub-code CC
        /// 0x01 Division of display window (area)
        /// 0x02 To send text data to a specified window
        /// 0x03 To send image data to the specified window
        /// 0x04 Static text data sent to the specified window
        /// 0x05 To send clock data to the specified window
        /// 0x06 Exit show to return to play within the program
        /// 0x07 Save / clear the data
        /// 0x08 Select play stored program(single-byte) 1~255
        /// 0x09 Select play stored program(double-byte) 1~512
        /// 0x0a Set variable value
        /// 0x0b Select play single stored program, and set the variable value
        /// 0x0c Set global display area
        /// 0x0d Push user variable data
        /// 0x0e Set timer control
        /// 0x0f Set the global display area and variable values
        /// 0x12 Send pure text to the specified window
        /// 0x81 Set program template command
        /// 0x82 In or out program template command
        /// 0x83 Query program template command
        /// 0x84 Delete program command
        /// 0x85 Send text to special window
        /// 0x86 Send picture to special window
        /// 0x87 Clock / temperature display in the specified window of the specified program
        /// 0x88 Send alone program
        /// 0x89 Query program information
        /// 0x8a Set program property
        /// 0x8b Set play plan
        /// 0x8c Delete play plan
        /// 0x8d Query play plan
        /// === Basic ===
        /// 0x2d Restart hardware
        /// 0xfe Restart APP
        /// 0x30 Write file(Open) 
        /// 0x32 Write file (Write)
        /// 0x33 Write file (Close)
        /// 0x50 Quick write file (Open)
        /// 0x51 Quick write file (Write) 
        /// 0x52 Quick write file (Close)
        /// 0x47 Time query and set
        /// 0x46 Brightness query and set
        /// 0x2e Query version info
        /// 0x45 Power ON/OFF info 
        /// 0x76 Power ON/OFF control 
        /// 0x75 Query temperature 
        /// 0x2c Remove file  
        /// 0x29 Query free disk space
        /// =============
        /// </param>
        /// <param name="cc_and_data"></param>
        /// <returns></returns>
        public static byte[] MakePackets(byte card_id, byte return_flag, byte packet_number, int max_packet_size, byte cc,byte [] cc_and_data)
        {
#warning file send not supported multi packet
            bool is_basic_protocol = (cc>0x12&&cc<0x81);

            com_packet_struct outPacket = new com_packet_struct();

            outPacket.start =(byte)0xA5;
            outPacket.type =(byte)0x68;
            outPacket.card_type =(byte)0x32;
            outPacket.card_id = card_id;
            outPacket.protocol_code = (is_basic_protocol==true ? cc: (byte)0x7B);
            outPacket.additional = return_flag;
            outPacket.length=(ushort)0x0000;
            outPacket.packet_number=packet_number;
            outPacket.last_packet_index=(byte)0x00;
            outPacket.crc=(ushort)0x0000;
            outPacket.end=(byte)0xAE;

            //int patch_count = get_patch_byte_count(data);
            //int patched_length = patch_count+data.Length;

            outPacket.length=Convert.ToUInt16(cc_and_data.Length);
            
            outPacket.data_bytes= new byte [(is_basic_protocol ? 6:10)+ outPacket.length + 3];

            List<byte> data_for_crc = new List<byte>();
           
            int index = 0;

            index=AddByte(outPacket.start,index,outPacket.data_bytes);
            int start_index = index;
            index=AddByte(outPacket.type,index,outPacket.data_bytes);
            index=AddByte(outPacket.card_type,index,outPacket.data_bytes);
            index=AddByte(outPacket.card_id,index,outPacket.data_bytes);
            index=AddByte(outPacket.protocol_code,index,outPacket.data_bytes);
            index=AddByte(outPacket.additional,index,outPacket.data_bytes);
           
            if(is_basic_protocol==false)
            {
                index=AddUShort(outPacket.length,index,outPacket.data_bytes,true);
                index=AddByte(outPacket.packet_number,index,outPacket.data_bytes);
                index=AddByte(outPacket.last_packet_index,index,outPacket.data_bytes);           
            }

            index=AddRange(cc_and_data,index,outPacket.data_bytes);
            int end_index = index - 1;

            outPacket.crc = crc(outPacket.data_bytes,start_index, end_index);

            index=AddUShort(outPacket.crc,index,outPacket.data_bytes,true); //?
            index=AddByte(outPacket.end,index,outPacket.data_bytes);

            apply_patch(outPacket.data_bytes,1,outPacket.data_bytes.Length - 2,out byte [] data_patched);

            outPacket.data_bytes=data_patched;

            return (outPacket.data_bytes);
        }

        private static int get_patch_byte_count(byte [] data)
        {
            int count = 0;

            foreach(byte b in data)
            {
                if(b==0xAE||b==0xA5||b==0xAA)
                {
                    count++;
                }
            }
            return count;
        }

        public static UInt16 crc(byte [] data, int start_index, int end_index)
        {
            UInt16 crc = 0x0000;

            for(int i = start_index; i <= end_index; i++)
            {
                crc+=Convert.ToUInt16(data [i]);
            }

            return crc;
        }


        private static void apply_patch(byte [] data,int start_index,int end_index,out byte [] data_patched)
        {
            List<byte> data_for_patch = new List<byte>();

            for(int i = 0; i <data.Length; i++)
            {
                
                if(data [i]==0xAE||data [i]==0xA5||data [i]==0xAA)
                {
                    if(i>=start_index&&i<=end_index)
                    {
                        data_for_patch.Add(0xAA);
                        data_for_patch.Add((byte)((byte)data [i]&(byte)0x0F));
                    }
                    else
                    { 
                        data_for_patch.Add(data [i]); 
                    }
                }
                else
                {
                    data_for_patch.Add (data [i]);
                }
            }
            data_patched=data_for_patch.ToArray();
        }

        public static void apply_receive_patch(byte [] data,int start_index,int end_index,out byte [] data_patched)
        {
            List<byte> data_for_patch = new List<byte>();

            for(int i = 0;i<end_index;i++)
            {
                if(data [i]==0xAA)
                {
                    if(i + 1 <= end_index)
                    {
                        i++;
                        if(data [i]==0x0E||data [i]==0x05||data [i]==0x0A)
                        {
                            if(i>=start_index&&i<=end_index)
                            {
                                byte patch = (byte)(0xA0|(byte)data [i]);
                                data_for_patch.Add(patch);
                            }
                            else
                            {
                                data_for_patch.Add(data [i]);
                            }
                        }
                        else
                        {
                            data_for_patch.Add(data [i]);
                        }
                    }
                    else
                    {
                        data_for_patch.Add(data [i]);
                    }
                }
                else
                {
                    data_for_patch.Add(data [i]);
                }
            }
            data_patched=data_for_patch.ToArray();
        }

        public static Int32 AddUShort(UInt16 value,int index,byte [] buff, bool low_byte_first)
        {
            if(low_byte_first) // (usual case)
            {
                buff [index++]=(byte)(value&0xff);
                buff [index++]=(byte)((value>>8)&0xff);               
            }
            else
            {
                buff [index++]=(byte)((value>>8)&0xff);
                buff [index++]=(byte)(value&0xff);
            }
            return index;
        }

        public static Int32 AddByte(byte value,int index,byte [] buff)
        {
            buff [index++]=(byte)(value&0xff);
            return index;
        }
        public static Int32 AddRange(byte [] value,int index,byte [] buff)
        {
            for(int i = 0;i<value.Length;i++)
                buff [index++]=value [i];
            return index;
        }
    }
}

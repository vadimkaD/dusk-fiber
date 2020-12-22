using System;
using System.Collections.Generic;
using System.Text;

namespace RootGenitor.utils
{
    class Util
    {
        public static Byte[] EncodeMessageToSend(String message)
        {
            Byte[] response;
            Byte[] bytesRaw = Encoding.UTF8.GetBytes(message);
            Byte[] frame = new Byte[10];

            Int32 indexStartRawData = -1;
            Int32 length = bytesRaw.Length;

            frame[0] = (Byte)129;
            if (length <= 125)
            {
                frame[1] = (Byte)length;
                indexStartRawData = 2;
            }
            else if (length >= 126 && length <= 65535)
            {
                frame[1] = (Byte)126;
                frame[2] = (Byte)((length >> 8) & 255);
                frame[3] = (Byte)(length & 255);
                indexStartRawData = 4;
            }
            else
            {
                frame[1] = (Byte)127;
                frame[2] = (Byte)((length >> 56) & 255);
                frame[3] = (Byte)((length >> 48) & 255);
                frame[4] = (Byte)((length >> 40) & 255);
                frame[5] = (Byte)((length >> 32) & 255);
                frame[6] = (Byte)((length >> 24) & 255);
                frame[7] = (Byte)((length >> 16) & 255);
                frame[8] = (Byte)((length >> 8) & 255);
                frame[9] = (Byte)(length & 255);

                indexStartRawData = 10;
            }

            response = new Byte[indexStartRawData + length];

            Int32 i, reponseIdx = 0;

            //Add the frame bytes to the reponse
            for (i = 0; i < indexStartRawData; i++)
            {
                response[reponseIdx] = frame[i];
                reponseIdx++;
            }

            //Add the data bytes to the response
            for (i = 0; i < length; i++)
            {
                response[reponseIdx] = bytesRaw[i];
                reponseIdx++;
            }

            return response;
        }


        public static string getTextFromData(byte[] data)
        {

            int msglen = data[1] & 0b01111111;
            int offsetToPayload = 2; //as default we skip |FIN,RSV1,RSV2,RSV3,OPCODE,OPCODE,OPCODE,OPCODE|MASK,LEN,LEN,LEN,LEN,LEN,LEN,LEN|
            bool mask = (data[1] & 0b10000000) != 0;
            if (msglen == 126)
            {
                // was ToUInt16(bytes, offset) but the result is incorrect
                var lenBytes = new byte[] { data[2], data[3] };
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(lenBytes);
                msglen = BitConverter.ToUInt16(lenBytes, 0);
                Console.WriteLine("msglen {0}", msglen.ToString());
                offsetToPayload = 4; //
            }
            else if (msglen == 127)
            {
                Console.WriteLine("TODO: msglen == 127, needs qword to store msglen");
                // i don't really know the byte order, please edit this
                // msglen = BitConverter.ToUInt64(new byte[] { bytes[5], bytes[4], bytes[3], bytes[2], bytes[9], bytes[8], bytes[7], bytes[6] }, 0);
                // offset = 10;
                throw new NotImplementedException();
            }

            if (msglen == 0)
                return "";
            else if (mask)
            {
                byte[] decoded = new byte[msglen];
                byte[] masks = new byte[4] { data[offsetToPayload], data[offsetToPayload + 1], data[offsetToPayload + 2], data[offsetToPayload + 3] };
                offsetToPayload += 4;

                for (int i = 0; i < msglen; ++i)
                    decoded[i] = (byte)(data[offsetToPayload + i] ^ masks[i % 4]);

                string text = Encoding.UTF8.GetString(decoded);
                return text;
            }

            return "";
        }
    }
}

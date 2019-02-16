using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace YikesInterpreter
{
    public class StringStream
    {
        Stream innerstream;
        int charpos;

        public bool DataLeft => innerstream.Position != innerstream.Length;

        public StringStream(Stream stream)
        {
            innerstream = stream;
        }

        public StringStream(string str)
        {
            innerstream = new MemoryStream(Encoding.UTF8.GetBytes(str));
        }

        public bool ReadNextChar(out char result)
        {
            if(innerstream.Position == innerstream.Length)
            {
                result = ' ';
                return false;
            }
            byte[] buffer = new byte[1];
            innerstream.Read(buffer, 0, 1);
            char[] chars = Encoding.UTF8.GetChars(buffer);
            result = chars[0];
            return true;
        }
    }
}

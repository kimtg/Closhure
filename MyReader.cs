using System.IO;

namespace Closhure
{
    // to support multi-line input and EOF handling
    internal class MyReader : TextReader
    {
        private string lineBuffer = "";
        private TextReader reader;

        public MyReader(TextReader r)
        {
            this.reader = r;
        }

        public override int Peek()
        {
            if (lineBuffer == "")
            {
                while (true)
                {
                    lineBuffer = reader.ReadLine();
                    if (lineBuffer == null) return -1;
                    break;
                }
                lineBuffer += '\n';
            }
            return lineBuffer[0];
        }

        public override int Read()
        {
            if (lineBuffer == "")
            {
                while (true)
                {
                    lineBuffer = reader.ReadLine();
                    if (lineBuffer == null) return -1;
                    break;
                }
                lineBuffer += '\n';
            }
            int v = lineBuffer[0];
            lineBuffer = lineBuffer.Substring(1);
            return v;
        }
    }
}

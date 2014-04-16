using System.IO;

namespace ObjLoader.Loader.Loaders
{
    public abstract class LoaderBase
    {
        private StreamReader _lineStreamReader;
        private static int _currentGroupSerialization = 0;

        protected void StartLoad(Stream lineStream)
        {
            _lineStreamReader = new StreamReader(lineStream);

            while (!_lineStreamReader.EndOfStream)
            {
                ParseLine();
            }
        }

        private void ParseLine()
        {
            var currentLine = _lineStreamReader.ReadLine();

            if (string.IsNullOrWhiteSpace(currentLine) || currentLine[0] == '#')
            {
                return;
            }

            var fields = currentLine.Trim().Split(null, 2);
            var keyword = fields[0].Trim();
            string data;
            if(fields.Length != 2)
            {
                data = "_sgroup" + ++_currentGroupSerialization;
            }
            else
            { 
                data = fields[1].Trim();
            }

            ParseLine(keyword, data);
        }

        protected abstract void ParseLine(string keyword, string data);
    }
}
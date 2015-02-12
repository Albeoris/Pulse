using System.IO;

namespace Pulse.Core
{
    public sealed class Wildcard
    {
        private readonly string _normalCase;
        private readonly string _alterCase;
        private readonly bool _ignoreDirectory;
        private readonly bool _caseSensitive;

        public Wildcard(string wildcard, bool ignoreDirectory = true, bool caseSensitive = false)
        {
            if (caseSensitive)
            {
                _normalCase = wildcard;
            }
            else
            {
                _normalCase = wildcard.ToLower();
                _alterCase = wildcard.ToUpper();
            }

            _ignoreDirectory = ignoreDirectory;
            _caseSensitive = caseSensitive;
        }

        public bool IsMatch(string path)
        {
            int index = -1;
            if (_ignoreDirectory)
            {
                for (index = path.Length - 1; index >= 0; index--)
                {
                    if (path[index] == Path.DirectorySeparatorChar || path[index] == Path.AltDirectorySeparatorChar)
                        break;
                }
            }
            index++;

            while (true)
            {
                index = IsMatch(path, index);
                if (index == -1 || index >= path.Length)
                    return false;
                if (index == 0)
                    return true;
            }
        }

        private int IsMatch(string path, int index)
        {
            bool asterisk = false;
            int nextSearch = -1;

            for (int maskIndex = 0; maskIndex < _normalCase.Length; maskIndex++)
            {
                bool finded = false;

                switch (_normalCase[maskIndex])
                {
                    case '*':
                    {
                        asterisk = true;
                        break;
                    }
                    case '?':
                    {
                        asterisk = false;
                        if (nextSearch < 0) nextSearch = index + 1;
                        if (++index > path.Length) return nextSearch;
                        break;
                    }
                    default:
                    {
                        for (; index < path.Length; index++)
                        {
                            if (path[index] == _normalCase[maskIndex] || (!_caseSensitive && path[index] == _alterCase[maskIndex]))
                            {
                                asterisk = false;
                                finded = true;
                                if (nextSearch < 0) nextSearch = index + 1;
                                index++;
                                break;
                            }

                            if (!asterisk)
                                return nextSearch;
                        }

                        if (!finded)
                            return nextSearch;

                        break;
                    }
                }
            }

            if (asterisk || index >= path.Length)
                nextSearch = 0;

            return nextSearch;
        }
    }
}
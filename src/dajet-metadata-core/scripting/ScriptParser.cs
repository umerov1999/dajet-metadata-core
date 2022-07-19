using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DaJet.Scripting
{
    public struct ScriptParser : IDisposable
    {
        private StringReader _reader;
        private int _line = 1;
        private int _start = 0;
        private int _position = 0;
        private char _char = char.MinValue;
        private readonly StringBuilder _text = new(256);
        private readonly List<ScriptToken> _tokens = new();
        public ScriptParser(string script)
        {
            _reader = new StringReader(script);
        }
        public List<ScriptToken> Parse()
        {
            Tokenize();

            return _tokens;
        }
        private void Tokenize()
        {
            while (Consume())
            {
                if (_char == '{') { } // start reading identifier !?
                else if (_char == '}') { } // stop reading identifier !?
                else if (_char == '[')
                {
                    AddToken(ScriptTokenType.OpenSquareBracket);
                }
                else if (_char == ']')
                {
                    AddToken(ScriptTokenType.CloseSquareBracket);
                }
                else if (_char == '>')
                {
                    AddToken(Consume('=') ? ScriptTokenType.GreateOrEqual : ScriptTokenType.Greater);
                }
                else if (_char == '-')
                {
                    if (Consume('-'))
                    {
                        while (PeekNext() != '\n' && Consume())
                        {
                            // read comment to the end of line
                        }
                        AddToken(ScriptTokenType.Comment);
                    }
                    else
                    {
                        AddToken(ScriptTokenType.Minus);
                    }
                }
                else if (_char == '\'')
                {
                    SingleQuotedString();
                }
                else if (_char == '"')
                {
                    DoubleQuotedString();
                }
                else if (char.IsDigit(_char))
                {
                    ReadNumber();
                }
            }
        }
        private bool Consume()
        {
            int consumed = _reader.Read();

            if (consumed == -1)
            {
                return false;
            }

            _char = (char)consumed;

            _position++;
            
            if (_char == '\n')
            {
                _line++;
            }
            else if (_char == ' ' || _char == '\r' || _char == '\t')
            {
                // ignore
            }
            else
            {
                _text.Append(_char);
            }
            
            return true;
        }
        private bool Consume(char expected)
        {
            char next = PeekNext();

            if (next == char.MinValue)
            {
                return false;
            }

            if (next == expected)
            {
                return Consume();
            }

            return false;
        }
        private char PeekNext()
        {
            int next = _reader.Peek();

            if (next == -1)
            {
                return char.MinValue;
            }

            return (char)next;
        }
        private void AddToken(ScriptTokenType token)
        {
            _tokens.Add(new ScriptToken(token)
            {
                StartPosition = _start,
                EndPosition = _position,
                Text = _text.ToString()
            });

            _text.Clear();
            _start = _position;
        }
        private void SingleQuotedString()
        {
            //TODO
        }
        private void DoubleQuotedString()
        {
            //TODO
        }
        private void ReadNumber()
        {
            //TODO
        }
        private void ReadIdentifier()
        {
            //TODO
        }
        private bool IsKeyword(string identifier)
        {
            //TODO
            return false;
        }
        public void Dispose()
        {
            _reader.Dispose();
        }
    }
}
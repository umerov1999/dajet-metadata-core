using System.Text;

namespace DaJet.Scripting
{
    public struct ScriptScanner : IDisposable
    {
        private readonly StringReader _reader;
        private readonly StringBuilder _lexeme = new(256);
        private readonly List<ScriptToken> _tokens = new();

        private int _line = 1;
        private int _start = 0;
        private int _position = 0;
        private char _char = char.MinValue;
        
        public ScriptScanner(string script)
        {
            _reader = new StringReader(script);
        }
        public List<ScriptToken> Scan()
        {
            Tokenize();

            return _tokens;
        }
        private void Tokenize()
        {
            while (Consume())
            {
                if (_char == '\n')
                {
                    _line++;
                }
                else if (_char == ' ' || _char == '\r' || _char == '\t')
                {
                    // ignore
                }
                else if (_char == '+')
                {
                    _start = _position;
                    _lexeme.Append(_char);
                    AddToken(ScriptTokenType.Plus);
                }
                else if (_char == '-')
                {
                    _start = _position;

                    _lexeme.Append(_char);

                    if (Consume('-'))
                    {
                        SingleLineComment();
                    }
                    else
                    {
                        AddToken(ScriptTokenType.Minus);
                    }
                }
                else if (_char == '*') // Multiply | SELECT *
                {
                    _start = _position;
                    _lexeme.Append(_char);
                    AddToken(ScriptTokenType.Star);
                }
                else if (_char == '/')
                {
                    _start = _position;

                    _lexeme.Append(_char);

                    if (Consume('*'))
                    {
                        MultiLineComment();
                    }
                    else
                    {
                        AddToken(ScriptTokenType.Divide);
                    }
                }
                else if (_char == '%')
                {
                    _start = _position;
                    _lexeme.Append(_char);
                    AddToken(ScriptTokenType.Modulo);
                }
                else if (_char == '=')
                {
                    _start = _position;
                    _lexeme.Append(_char);
                    AddToken(ScriptTokenType.Equals);
                }
                else if (_char == '!')
                {
                    _start = _position;
                    _lexeme.Append(_char);

                    if (Consume('='))
                    {
                        _lexeme.Append('=');
                        AddToken(ScriptTokenType.NotEquals);
                    }
                    else
                    {
                        _lexeme.Clear();
                    }
                }
                else if (_char == '>')
                {
                    _start = _position;

                    _lexeme.Append(_char);

                    if (Consume('='))
                    {
                        _lexeme.Append('=');
                        AddToken(ScriptTokenType.GreateOrEquals);
                    }
                    else
                    {
                        AddToken(ScriptTokenType.Greater);
                    }
                }
                else if (_char == '<')
                {
                    _start = _position;

                    _lexeme.Append(_char);

                    if (Consume('='))
                    {
                        _lexeme.Append('=');
                        AddToken(ScriptTokenType.LessOrEquals);
                    }
                    else if (Consume('>'))
                    {
                        _lexeme.Append('>');
                        AddToken(ScriptTokenType.NotEquals);
                    }
                    else
                    {
                        AddToken(ScriptTokenType.Less);
                    }
                }
                else if (_char == ',')
                {
                    _start = _position;
                    _lexeme.Append(_char);
                    AddToken(ScriptTokenType.Comma);
                }
                else if (_char == ';')
                {
                    _start = _position;
                    _lexeme.Append(_char);
                    AddToken(ScriptTokenType.EndOfStatement);
                }
                else if (_char == '[')
                {
                    _start = _position;
                    _lexeme.Append(_char);
                    AddToken(ScriptTokenType.OpenSquareBracket);
                }
                else if (_char == ']')
                {
                    _start = _position;
                    _lexeme.Append(_char);
                    AddToken(ScriptTokenType.CloseSquareBracket);
                }
                else if (_char == '(')
                {
                    _start = _position;
                    _lexeme.Append(_char);
                    AddToken(ScriptTokenType.OpenRoundBracket);
                }
                else if (_char == ')')
                {
                    _start = _position;
                    _lexeme.Append(_char);
                    AddToken(ScriptTokenType.CloseRoundBracket);
                }
                else if (_char == '{')
                {
                    _start = _position;
                    _lexeme.Append(_char);
                    AddToken(ScriptTokenType.OpenCurlyBracket);
                }
                else if (_char == '}')
                {
                    _start = _position;
                    _lexeme.Append(_char);
                    AddToken(ScriptTokenType.CloseCurlyBracket);
                }
                else if (_char == '\'')
                {
                    SingleQuotedString();
                }
                else if (_char == '"')
                {
                    DoubleQuotedString();
                }
                else if (ScriptHelper.IsNumeric(_char))
                {
                    Number();
                }
                else if (_char == '@')
                {
                    Variable();
                }
                else if (_char == '#')
                {
                    TemporaryTable();
                }
                else if (ScriptHelper.IsAlphaNumeric(_char))
                {
                    Identifier();
                }
                else
                {
                    throw new Exception(GetErrorText("Unexpected character"));
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

            _position++;

            _char = (char)consumed;
            
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
                Text = _lexeme.ToString()
            });

            _lexeme.Clear();
        }
        
        private void SingleLineComment()
        {
            //_start = _position;
            _lexeme.Append(_char);

            while (PeekNext() != '\n' && Consume())
            {
                // read comment to the end of line
                _lexeme.Append(_char);
            }

            AddToken(ScriptTokenType.Comment);
        }
        private void MultiLineComment()
        {
            //_start = _position;
            _lexeme.Append(_char);

            while (PeekNext() != '*' && Consume())
            {
                // read comment until * is met

                if (_char == '\n')
                {
                    // process new line
                    _line++;
                }
                else
                {
                    _lexeme.Append(_char);
                }
            }

            // the * is met
            _ = Consume();
            _lexeme.Append(_char);

            if (_char != '*') // end of script
            {
                throw new Exception(GetErrorText("Unterminated comment"));
            }

            if (!Consume('/'))
            {
                throw new Exception(GetErrorText("Unexpected character"));
            }
            else
            {
                _lexeme.Append(_char);
            }

            AddToken(ScriptTokenType.Comment);
        }
        private void SingleQuotedString()
        {
            _start = _position;
            _lexeme.Append(_char);

            while (PeekNext() != '\'' && Consume())
            {
                // read string literal until ' is met

                if (_char == '\n')
                {
                    // process new line
                    _line++;
                }
                else
                {
                    _lexeme.Append(_char);
                }
            }

            // the ' is met
            if (Consume())
            {
                _lexeme.Append(_char);
            }

            if (_char != '\'') // end of script
            {
                throw new Exception(GetErrorText("Unterminated string"));
            }

            AddToken(ScriptTokenType.String);
        }
        private void DoubleQuotedString()
        {
            _start = _position;
            _lexeme.Append(_char);

            while (PeekNext() != '\"' && Consume())
            {
                // read string literal until " is met

                if (_char == '\n')
                {
                    // process new line
                    _line++;
                }
                else
                {
                    _lexeme.Append(_char);
                }
            }

            // the " is met
            if (Consume())
            {
                _lexeme.Append(_char);
            }

            if (_char != '\"') // end of script
            {
                throw new Exception(GetErrorText("Unterminated string"));
            }

            AddToken(ScriptTokenType.String);
        }
        private void Number()
        {
            _start = _position;
            _lexeme.Append(_char);

            while (ScriptHelper.IsNumeric(PeekNext()) && Consume())
            {
                // read number literal
                _lexeme.Append(_char);
            }

            if (Consume('.'))
            {
                if (!ScriptHelper.IsNumeric(PeekNext()))
                {
                    throw new Exception(GetErrorText("Unexpected character"));
                }

                _lexeme.Append(_char);

                while (ScriptHelper.IsNumeric(PeekNext()) && Consume())
                {
                    // consume digits - fractional part
                    _lexeme.Append(_char);
                }
            }

            AddToken(ScriptTokenType.Number);
        }
        private void Identifier()
        {
            _start = _position;
            _lexeme.Append(_char);

            char next = PeekNext();

            while (ScriptHelper.IsAlphaNumeric(next) && Consume())
            {
                // read identifier
                _lexeme.Append(_char);
                next = PeekNext();
            }

            string test = _lexeme.ToString();

            if (ScriptHelper.IsNullLiteral(test))
            {
                AddToken(ScriptTokenType.NULL);
            }
            else if (ScriptHelper.IsBooleanLiteral(test))
            {
                AddToken(ScriptTokenType.Boolean);
            }
            else if (ScriptHelper.IsKeyword(test))
            {
                AddToken(ScriptTokenType.Keyword);
            }
            else
            {
                AddToken(ScriptTokenType.Identifier);
            }
        }
        private void Variable()
        {
            _start = _position;
            _lexeme.Append(_char);

            if (Consume('@')) // double @@
            {
                _lexeme.Append(_char);
            }

            char next = PeekNext();

            while (ScriptHelper.IsAlphaNumeric(next) && Consume())
            {
                // read variable
                _lexeme.Append(_char);
                next = PeekNext();
            }

            AddToken(ScriptTokenType.Variable);
        }
        private void TemporaryTable()
        {
            _start = _position;
            _lexeme.Append(_char);

            if (Consume('#')) // ## global temporary table
            {
                _lexeme.Append(_char);
            }

            char next = PeekNext();

            while (ScriptHelper.IsAlphaNumeric(next) && Consume())
            {
                // read variable
                _lexeme.Append(_char);
                next = PeekNext();
            }

            AddToken(ScriptTokenType.TemporaryTable);
        }

        private string GetErrorText(string reason)
        {
            return $"{reason}. [{_char}] Line: {_line}. Position: {_position}.";
        }

        public void Dispose()
        {
            _reader.Dispose();
        }
    }
}
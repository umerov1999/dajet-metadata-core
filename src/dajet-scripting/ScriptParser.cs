namespace DaJet.Scripting
{
    public struct ScriptParser : IDisposable
    {
        private readonly ScriptScanner _tokenizer;
        private readonly SyntaxTree _tree = new();
        private List<ScriptToken> _tokens = new();

        private int _current = 0;
        private ScriptToken? _token = null;

        public ScriptParser(string script)
        {
            _tokenizer = new ScriptScanner(script);
        }
        public List<ScriptToken> TokenStream { get { return _tokens; } }
        public SyntaxTree Parse()
        {
            _tokens = _tokenizer.Scan();

            ParseTokenStream();

            return _tree;
        }
        private void ParseTokenStream()
        {
            while (Consume())
            {
                if (_token!.TokenType == ScriptTokenType.Keyword)
                {
                    if (_token.Text == "FROM")
                    {
                        ParseFROM();
                    }
                    else if (_token.Text == "JOIN")
                    {
                        ParseFROM();
                    }
                }
            }
        }
        private bool Consume()
        {
            if (_tokens.Count == 0 ||
                _tokens.Count <= _current)
            {
                return false;
            }

            _token = _tokens[_current++];

            return (_token != null);
        }
        private bool Consume(string[] tokens)
        {
            if (_tokens.Count <= _current)
            {
                return false;
            }

            for (int i = 0; i < tokens.Length; i++)
            {
                if (_tokens[_current].Text == tokens[i])
                {
                    return Consume();
                }
            }

            return false;
        }
        private ScriptToken? PeekNext()
        {
            if (_tokens.Count <= _current)
            {
                return null;
            }

            return _tokens[_current];
        }
        private void ParseFROM()
        {
            SyntaxNode node = new()
            {
                Token = _token!
            };

            _tree.Nodes.Add(node);

            ParseIdentifier(node);
        }
        private void ParseIdentifier(SyntaxNode parent)
        {
            SyntaxNode node = null!;

            ScriptToken next = PeekNext()!;

            if (next != null && next.TokenType == ScriptTokenType.OpenSquareBracket && Consume())
            {
                next = PeekNext()!;
            }

            if (next != null && next.TokenType == ScriptTokenType.Identifier && Consume())
            {
                node = new SyntaxNode()
                {
                    Token = _token!
                };

                parent.Children.Add(node);

                next = PeekNext()!;
            }
            
            if (next != null && next.TokenType == ScriptTokenType.CloseSquareBracket && Consume())
            {
                next = PeekNext()!;
            }

            if (next != null && next.Text == "AS" && Consume())
            {
                ParseIdentifier(node);
            }
            else if (next != null && next.TokenType == ScriptTokenType.OpenSquareBracket)
            {
                ParseIdentifier(node);
            }
            else if (next != null && next.TokenType == ScriptTokenType.Identifier)
            {
                ParseIdentifier(node);
            }
        }

        public void Dispose()
        {
            _tokenizer.Dispose();
        }
    }
}
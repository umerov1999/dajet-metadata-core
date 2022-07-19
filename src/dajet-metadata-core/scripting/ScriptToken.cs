namespace DaJet.Scripting
{
    public sealed class ScriptToken
    {
        public ScriptToken(ScriptTokenType tokenType)
        {
            TokenType = tokenType;
        }
        public ScriptTokenType TokenType { get; }
        public string Text { get; set; }
        public int StartPosition { get; set; }
        public int EndPosition { get; set; }
        public override string ToString()
        {
            return $"{TokenType} [{StartPosition}-{EndPosition}] {Text}";
        }
    }
}
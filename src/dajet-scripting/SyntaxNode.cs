namespace DaJet.Scripting
{
    public class SyntaxNode
    {
        public ScriptToken Token { get; set; }
        public List<SyntaxNode> Children { get; } = new();
    }
}
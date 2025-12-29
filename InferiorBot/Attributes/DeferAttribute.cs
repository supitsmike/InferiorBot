namespace InferiorBot.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class DeferAttribute(bool ephemeral = false) : Attribute
    {
        public bool Ephemeral { get; } = ephemeral;
    }
}

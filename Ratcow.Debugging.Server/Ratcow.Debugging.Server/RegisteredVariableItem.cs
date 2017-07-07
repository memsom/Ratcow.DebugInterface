namespace Ratcow.Debugging.Server
{
    public class RegisteredVariableItem
    {
        public object Reference { get; set; }
        public string RegisteredName { get; set; }
        public string Name { get; set; }
        public bool IsDirectReference { get; set; }
    }
}

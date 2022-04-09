namespace AstroAdapters.Services
{
    public class Box<TStruct> where TStruct : struct
    {
        public TStruct Value { get; private set; }

        public Box(TStruct value) => Value = value;        
    }
}

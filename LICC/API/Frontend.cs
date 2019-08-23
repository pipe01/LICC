namespace LICC.API
{
    public abstract class Frontend
    {
        public abstract void Write(string str);
        public virtual void WriteLine(string str) => Write(str + "\n");
    }
}

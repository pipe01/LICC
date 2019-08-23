namespace LICC.API
{
    internal delegate void LineInputDelegate(string line);

    public abstract class Frontend
    {
        internal event LineInputDelegate LineInput;

        protected void OnLineInput(string line) => LineInput?.Invoke(line);

        public abstract void Write(string str);
        public virtual void WriteLine(string str) => Write(str + "\n");
    }
}

using System;

namespace LICC.API
{
    internal delegate void LineInputDelegate(string line);

    public abstract class Frontend
    {
        internal event LineInputDelegate LineInput;

        protected internal IHistory History { get; internal set; }

        protected void OnLineInput(string line) => LineInput?.Invoke(line);

        protected internal virtual void Init() { }

        protected internal virtual void PauseInput() { }
        protected internal virtual void ResumeInput() { }

        public virtual void Write(string str) => Write(str, Color.White);
        public abstract void Write(string str, Color color);


        public virtual void WriteLine(string str) => Write(str + Environment.NewLine);
        public virtual void WriteLine(string str, Color color) => Write(str + Environment.NewLine, color);
    }
}

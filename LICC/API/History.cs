using System.Collections.Generic;

namespace LICC.API
{
    public interface IHistory
    {
        string GetPrevious();
        string GetNext();
    }

    internal interface IWriteableHistory : IHistory
    {
        void AddNewItem(string item);
    }

    internal class History : IWriteableHistory
    {
        private readonly IList<string> Items = new List<string>();

        private int CurrentPosition = 0; //Starts from end of list

        public void AddNewItem(string item)
        {
            Items.Add(item);
            CurrentPosition = 0;
        }

        public string GetNext()
        {
            if (CurrentPosition == 1)
                return null;

            return Items[Items.Count - --CurrentPosition];
        }

        public string GetPrevious()
        {
            if (CurrentPosition == Items.Count)
                return null;

            return Items[Items.Count - 1 - CurrentPosition++];
        }
    }
}

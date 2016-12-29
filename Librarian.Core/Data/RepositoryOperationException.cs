using System;
namespace Librarian.Core.Data
{
    public class RepositoryOperationException : Exception
    {
        public string Key { get; private set; }

        public RepositoryOperationException(string message) : base(message)
        {
            Key = string.Empty;
        }
        public RepositoryOperationException(string key, string message) : base(message)
        {
            Key = key;
        }
    }
}

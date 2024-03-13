using System;

namespace Rowbot.Sources
{
    public class DynamicObjectsNotIdenticalException : Exception
    {
        public DynamicObjectsNotIdenticalException(string message) : base(message) { }
    }
}
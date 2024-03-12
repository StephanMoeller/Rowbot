using System;

namespace Rowbot.Targets
{
    public class TypeMismatchException : Exception
    {
        public TypeMismatchException(string msg) : base(msg) { }
    }
}
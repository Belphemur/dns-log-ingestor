using System;

namespace VictoriaMetrics.Exceptions
{
    public class FieldException : ArgumentException
    {
        public FieldException(string message) : base(message)
        {
        }
    }
}
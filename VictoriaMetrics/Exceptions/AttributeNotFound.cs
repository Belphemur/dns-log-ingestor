using System;

namespace VictoriaMetrics.Exceptions
{
    public class AttributeNotFound : ArgumentException
    {
        public AttributeNotFound(string message) : base(message)
        {
        }
    }
}
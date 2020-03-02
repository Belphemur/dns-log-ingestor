using System;

namespace VictoriaMetrics.Attributes
{
    public abstract class NamedAttribute : Attribute
    {
        public string Name { get; }

        protected NamedAttribute(string name)
        {
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class Measurement : NamedAttribute
    {
        public Measurement(string name) : base(name)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class Timestamp : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class Tag : NamedAttribute
    {
        public Tag(string name) : base(name)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class Field : NamedAttribute
    {
        public Field(string name) : base(name)
        {
        }
    }
}
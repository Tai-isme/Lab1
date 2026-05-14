namespace PRN232.LAB_1.API.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class ConditionalJsonPropertyAttribute : Attribute
{
    public string PropertyName { get; }

    public ConditionalJsonPropertyAttribute(string propertyName)
    {
        PropertyName = propertyName;
    }
}

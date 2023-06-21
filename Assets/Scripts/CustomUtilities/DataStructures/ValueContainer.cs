public class ValueContainer<T>
{
    public static implicit operator T(ValueContainer<T> v) => v.Value;

    public T Value { get; set; }

    public ValueContainer(T value)
    {
        Value = value;
    }
}

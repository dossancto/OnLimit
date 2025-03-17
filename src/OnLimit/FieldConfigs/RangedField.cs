namespace OnLimit.FieldConfigs;

public record RangedField
(
   long? MinValue,
   long MaxValue
)
{
    public RangedField() : this(null, 0) { }
}

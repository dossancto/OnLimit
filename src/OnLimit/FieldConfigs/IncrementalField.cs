namespace OnLimit.FieldConfigs;

public record IncrementalField
(
   long FallbackValue,
   long? MaxValue = null
)
{

}


namespace Jay.Extensions;

public static class ArrayExtensions
{
    public static TOut[] SelectToArray<TIn, TOut>(this TIn[] inputArray, Func<TIn, TOut> convertInput)
    {
        int len = inputArray.Length;
        var output = new TOut[len];
        for (var i = 0; i < len; i++)
        {
            output[i] = convertInput(inputArray[i]);
        }
        return output;
    }
}
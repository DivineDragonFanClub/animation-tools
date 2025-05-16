using UnityEngine;

public static class AnimationEventExtensions
{
    /// <summary>
    /// Creates a complete clone of an AnimationEvent
    /// </summary>
    /// <param name="source">The AnimationEvent to clone</param>
    /// <returns>A new AnimationEvent with identical values</returns>
    public static AnimationEvent Clone(this AnimationEvent source)
    {
        return new AnimationEvent
        {
            time = source.time,
            functionName = source.functionName,
            floatParameter = source.floatParameter,
            intParameter = source.intParameter,
            stringParameter = source.stringParameter,
            objectReferenceParameter = source.objectReferenceParameter,
            messageOptions = source.messageOptions
        };
    }
}
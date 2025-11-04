using UnityEngine;

public class Stopwatch
{
    private float startTime = 0f;
    private float elapsedTime = 0f;

    public float ElapsedTimeSec()
    {
        elapsedTime = Time.time - startTime;
        return elapsedTime;
    }

    public void Restart()
    {
        startTime = Time.time;
        elapsedTime = 0f;
    }
}

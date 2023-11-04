using System;

public abstract class AudioOutput
{
    abstract public string GetName();
    public string GetTruncateName(int maxLength)
    {
        return TruncateString(GetName(), maxLength);
    }
    public override string ToString()
    {
        return GetTruncateName(30);
    }
    abstract public void SetVolume(float volume);
    abstract public float GetVolume();
    private static string TruncateString(string input, int maxLength)
    {
        if (string.IsNullOrEmpty(input) || input.Length <= maxLength)
        {
            return input;
        }
        else
        {
            return "..." + input.Substring(input.Length - maxLength + 3);
        }
    }

}
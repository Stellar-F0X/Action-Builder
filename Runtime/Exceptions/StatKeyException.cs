using System;

[Serializable]
public class StatKeyCastException : InvalidCastException
{
    public StatKeyCastException(string message) : base(message) { }
    
    public StatKeyCastException() { }
}
namespace Wolfteam.Server.Login;

public class PacketReaderException : Exception
{
    public PacketReaderException(string? message) : base(message)
    {
    }

    public PacketReaderException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
namespace GitActDashNet.Models;

public sealed class ServerOptions
{
    public int Port { get; set; } = 5000;
    public bool UseHttps { get; set; }
    public string Host { get; set; } = "localhost";
}
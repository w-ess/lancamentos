using System.Net;
using System.Net.Sockets;

namespace FluxoDeCaixa.Testes.EndToEnd.Infraestrutura;

internal static class PortaLivreHelper
{
    public static int ObterPortaLivreTcp()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();

        try
        {
            return ((IPEndPoint)listener.LocalEndpoint).Port;
        }
        finally
        {
            listener.Stop();
        }
    }
}

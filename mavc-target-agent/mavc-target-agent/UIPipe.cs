using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mavc_target_agent
{

    /**
     * A interprocess communication to receive data from the ui process
     */
    internal class UIPipe
    {
        MAVCSave save = new MAVCSave();

        static async Task startUICom()
        {
            while (true)
            {
                using (NamedPipeServerStream serverPipe = new NamedPipeServerStream("UIPipe", PipeDirection.InOut))
                {
                    Console.WriteLine("Waiting for UI request...");
                    await serverPipe.WaitForConnectionAsync();
                    Console.WriteLine("Client connected.");

                    // Read data from the client
                    byte[] buffer = new byte[256];
                    int bytesRead = await serverPipe.ReadAsync(buffer, 0, buffer.Length);
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine("Received from client: " + message);

                    // Process the request and generate a response
                    //string response = "Hello from the server! Request: " + message;

                    //// Send a response back to the client
                    //byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                    //await serverPipe.WriteAsync(responseBytes, 0, responseBytes.Length);
                    //Console.WriteLine("Response sent to client: " + response);
                }
            }
        }
    }
}

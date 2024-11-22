//using System;
//using System.Net;
//using System.Text.Json;
//using Microsoft.Office.Interop.PowerPoint;
//using Mso = Microsoft.Office.Core;

//namespace PowerPointVoiceControl
//{
//    internal class Program
//    {
//        static void Main(string[] args)
//        {
//            // Configuração do PowerPoint


//            Application pptApp = new Application();
//            Presentation presentation = pptApp.Presentations.Open(
//                @"C:\Users\Asus\OneDrive - Universidade de Aveiro\LEI - Sara Almeida\4ºano\IM\IM_Projects\PowerPointVoiceApp\ConsoleApp\IM First Presentation.pptx",
//                Mso.MsoTriState.msoTrue,
//                Mso.MsoTriState.msoFalse,
//                Mso.MsoTriState.msoTrue);

//            presentation.SlideShowSettings.Run();

//            // Configuração do HttpListener
//            HttpListener listener = new HttpListener();
//            listener.Prefixes.Add("http://localhost:5000/api/voice-command/");
//            listener.Start();
//            Console.WriteLine("Servidor em execução...");
//            while (true)
//            {
//                try
//                {
//                    var context = listener.GetContext();
//                    var request = context.Request;

//                    if (request.HttpMethod == "OPTIONS")
//                    {
//                        // Respond to CORS preflight requests
//                        var response = context.Response;
//                        AddCorsHeaders(response);
//                        response.StatusCode = (int)HttpStatusCode.OK;
//                        response.Close();
//                        continue;
//                    }

//                    if (request.HttpMethod == "POST")
//                    {
//                        using (var reader = new System.IO.StreamReader(request.InputStream, request.ContentEncoding))
//                        {
//                            var body = reader.ReadToEnd();
//                            VoiceCommand command;

//                            try
//                            {
//                                command = JsonSerializer.Deserialize<VoiceCommand>(body);
//                            }
//                            catch (JsonException)
//                            {
//                                Console.WriteLine("Erro ao desserializar o comando.");
//                                SendResponse(context.Response, "Formato JSON inválido.", HttpStatusCode.BadRequest);
//                                continue;
//                            }

//                            if (command == null || string.IsNullOrEmpty(command.Intent))
//                            {
//                                Console.WriteLine("Comando vazio ou inválido recebido.");
//                                SendResponse(context.Response, "Comando inválido.", HttpStatusCode.BadRequest);
//                                continue;
//                            }

//                            string responseMessage = HandleCommand(command, presentation);
//                            SendResponse(context.Response, responseMessage, HttpStatusCode.OK);
//                        }
//                    }
//                    else
//                    {
//                        Console.WriteLine($"Método {request.HttpMethod} não suportado.");
//                        SendResponse(context.Response, "Método não suportado.", HttpStatusCode.MethodNotAllowed);
//                    }
//                }
//                catch (Exception ex)
//                {
//                    Console.WriteLine($"Erro no servidor: {ex.Message}");
//                }
//            }

//        }

//        private static void AddCorsHeaders(HttpListenerResponse response)
//        {
//            response.Headers.Add("Access-Control-Allow-Origin", "*");
//            response.Headers.Add("Access-Control-Allow-Methods", "POST, OPTIONS");
//            response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");
//        }

//        private static void SendResponse(HttpListenerResponse response, string message, HttpStatusCode statusCode)
//        {
//            AddCorsHeaders(response);

//            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(message);
//            response.StatusCode = (int)statusCode;
//            response.ContentLength64 = buffer.Length;

//            try
//            {
//                using (var output = response.OutputStream)
//                {
//                    output.Write(buffer, 0, buffer.Length);
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Erro ao enviar a resposta: {ex.Message}");
//            }
//        }



//        private static string HandleCommand(VoiceCommand command, Presentation presentation)
//        {
//            if (command == null || string.IsNullOrEmpty(command.Intent))
//            {
//                return "Comando inválido.";
//            }

//            switch (command.Intent.ToLower())
//            {
//                case "next_slide":
//                    presentation.SlideShowWindow.View.Next();
//                    return "Próximo slide.";

//                case "previous_slide":
//                    presentation.SlideShowWindow.View.Previous();
//                    return "Slide anterior.";

//                default:
//                    return "Comando não reconhecido.";
//            }
//        }
//    }

//    public class VoiceCommand
//    {
//        public string Intent { get; set; }
//    }
//}
using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Office.Interop.PowerPoint;
using Mso = Microsoft.Office.Core;

namespace PowerPointWebSocketControl
{
    internal class Program
    {
        private static Presentation _presentation;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting PowerPoint WebSocket Control...");

            // Initialize PowerPoint
            Application pptApp = new Application();
            _presentation = pptApp.Presentations.Open(
                @"C:\Users\maria\Downloads\IM First Presentation.pptx",
                Mso.MsoTriState.msoTrue,
                Mso.MsoTriState.msoFalse,
                Mso.MsoTriState.msoTrue);

            _presentation.SlideShowSettings.Run();

            // Start WebSocket Server
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:5000/");
            listener.Start();
            Console.WriteLine("WebSocket server started. Listening on ws://localhost:5000/");

            while (true)
            {
                var context = await listener.GetContextAsync();
                if (context.Request.IsWebSocketRequest)
                {
                    var webSocketContext = await context.AcceptWebSocketAsync(null);
                    Console.WriteLine("WebSocket client connected.");

                    await HandleWebSocketConnection(webSocketContext.WebSocket);
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }
        }


        private static void HandleCors(HttpListenerResponse response)
        {
            response.Headers.Add("Access-Control-Allow-Origin", "*");
            response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");
            response.StatusCode = (int)HttpStatusCode.OK;
            response.Close();
        }

        private static async Task HandleWebSocketConnection(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];

            while (webSocket.State == WebSocketState.Open)
            {
                try
                {
                    // Receive a message
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Console.WriteLine("WebSocket client disconnected.");
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                        break;
                    }

                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine($"Received message: {message}");

                    // Process the command and provide feedback
                    var response = HandleCommand(message);
                    var responseBytes = Encoding.UTF8.GetBytes(response);

                    // Send the response back to the client
                    await webSocket.SendAsync(new ArraySegment<byte>(responseBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error handling WebSocket message: {ex.Message}");
                }
            }
        }

        private static string HandleCommand(string message)
        {
            try
            {
                // Parse the received message (assume it's JSON)
                var command = JsonSerializer.Deserialize<VoiceCommand>(message);
                if (command == null || string.IsNullOrEmpty(command.Intent))
                {
                    return "Invalid command.";
                }

                switch (command.Intent.ToLower())
                {
                    case "next_slide":
                        _presentation.SlideShowWindow.View.Next();
                        return "Próximo slide.";

                    case "previous_slide":
                        _presentation.SlideShowWindow.View.Previous();
                        return "Slide anterior.";

                    case "jump_to_slide_by_title":
                        if (!string.IsNullOrEmpty(command.SlideTitle))
                        {
                            string normalizedTitle = command.SlideTitle.Trim();
                            Console.WriteLine($"Received SlideTitle: {normalizedTitle}"); 
                            foreach (Slide slide in _presentation.Slides)
                            {
                                if (slide.Shapes.HasTitle == Mso.MsoTriState.msoTrue &&
                                    slide.Shapes.Title.TextFrame.TextRange.Text.Equals(normalizedTitle, StringComparison.OrdinalIgnoreCase))
                                {
                                    _presentation.SlideShowWindow.View.GotoSlide(slide.SlideIndex);
                                    return $"Indo para o slide: {command.SlideTitle}.";
                                }
                            }
                            return $"Slide com o título '{command.SlideTitle}' não encontrado.";
                        }
                        return "Título do slide não fornecido.";

                    case "jump_to_slide_by_number":
                        if (!string.IsNullOrEmpty(command.SlideNumber))
                        {
                            Console.WriteLine($"Received SlideNumber: {command.SlideNumber}");
                            if (int.TryParse(command.SlideNumber, out int slideNumber) &&
                                slideNumber > 0 &&
                                slideNumber <= _presentation.Slides.Count)
                            {
                                _presentation.SlideShowWindow.View.GotoSlide(slideNumber);
                                return $"Indo para o slide número {slideNumber}.";
                            }
                            return "Número de slide inválido.";
                        }
                        return "Número de slide não fornecido.";


                    default:
                        return "Comando não reconhecido.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing command: {ex.Message}");
                return "Error processing command.";
            }
        }
    }

    public class VoiceCommand
    {
        public string Intent { get; set; }
        public string SlideTitle { get; set; } // For jump_to_slide_by_title
        public string SlideNumber { get; set; }  // For jump_to_slide_by_number
    }
}

using System;
using System.Net;
using System.Text.Json;
using Microsoft.Office.Interop.PowerPoint;
using Mso = Microsoft.Office.Core;

namespace PowerPointVoiceControl
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Configuração do PowerPoint
            Application pptApp = new Application();
            Presentation presentation = pptApp.Presentations.Open(
                @"C:\Users\maria\Downloads\IM First Presentation.pptx",
                Mso.MsoTriState.msoTrue,
                Mso.MsoTriState.msoFalse,
                Mso.MsoTriState.msoTrue);

            presentation.SlideShowSettings.Run();

            // Configuração do HttpListener
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:5000/api/voice-command/");
            listener.Start();
            Console.WriteLine("Servidor em execução...");

            //while (true)
            //{
            //    var context = listener.GetContext();
            //    var request = context.Request;

            //    if (request.HttpMethod == "POST")
            //    {
            //        using (var reader = new System.IO.StreamReader(request.InputStream, request.ContentEncoding))
            //        {
            //            var body = reader.ReadToEnd();
            //            var command = JsonSerializer.Deserialize<VoiceCommand>(body);
            //            string responseMessage = HandleCommand(command, presentation);

            //            var response = context.Response;
            //            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseMessage);
            //            response.ContentLength64 = buffer.Length;
            //            response.OutputStream.Write(buffer, 0, buffer.Length);
            //            response.OutputStream.Close();
            //        }
            //    }
            //}
            while (true)
            {
                try
                {
                    var context = listener.GetContext();
                    var request = context.Request;

                    if (request.HttpMethod == "OPTIONS")
                    {
                        // Respond to CORS preflight requests
                        var response = context.Response;
                        AddCorsHeaders(response);
                        response.StatusCode = (int)HttpStatusCode.OK;
                        response.Close();
                        continue;
                    }

                    if (request.HttpMethod == "POST")
                    {
                        using (var reader = new System.IO.StreamReader(request.InputStream, request.ContentEncoding))
                        {
                            var body = reader.ReadToEnd();
                            VoiceCommand command;

                            try
                            {
                                command = JsonSerializer.Deserialize<VoiceCommand>(body);
                            }
                            catch (JsonException)
                            {
                                Console.WriteLine("Erro ao desserializar o comando.");
                                SendResponse(context.Response, "Formato JSON inválido.", HttpStatusCode.BadRequest);
                                continue;
                            }

                            if (command == null || string.IsNullOrEmpty(command.Intent))
                            {
                                Console.WriteLine("Comando vazio ou inválido recebido.");
                                SendResponse(context.Response, "Comando inválido.", HttpStatusCode.BadRequest);
                                continue;
                            }

                            string responseMessage = HandleCommand(command, presentation);
                            SendResponse(context.Response, responseMessage, HttpStatusCode.OK);
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Método {request.HttpMethod} não suportado.");
                        SendResponse(context.Response, "Método não suportado.", HttpStatusCode.MethodNotAllowed);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro no servidor: {ex.Message}");
                }
            }

        }

        private static void AddCorsHeaders(HttpListenerResponse response)
        {
            response.Headers.Add("Access-Control-Allow-Origin", "*");
            response.Headers.Add("Access-Control-Allow-Methods", "POST, OPTIONS");
            response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");
        }

        private static void SendResponse(HttpListenerResponse response, string message, HttpStatusCode statusCode)
        {
            AddCorsHeaders(response);

            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(message);
            response.StatusCode = (int)statusCode;
            response.ContentLength64 = buffer.Length;

            try
            {
                using (var output = response.OutputStream)
                {
                    output.Write(buffer, 0, buffer.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao enviar a resposta: {ex.Message}");
            }
        }



        private static string HandleCommand(VoiceCommand command, Presentation presentation)
        {
            if (command == null || string.IsNullOrEmpty(command.Intent))
            {
                return "Comando inválido.";
            }

            switch (command.Intent.ToLower())
            {
                case "next_slide":
                    presentation.SlideShowWindow.View.Next();
                    return "Próximo slide.";

                case "previous_slide":
                    presentation.SlideShowWindow.View.Previous();
                    return "Slide anterior.";

                default:
                    return "Comando não reconhecido.";
            }
        }
    }

    public class VoiceCommand
    {
        public string Intent { get; set; }
    }
}

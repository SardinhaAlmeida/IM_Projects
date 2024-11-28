//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Net.WebSockets;
//using System.Text;
//using System.Text.Json;
//using System.Threading;
//using System.Threading.Tasks;
//using Newtonsoft.Json;
//using Microsoft.Office.Interop.PowerPoint;
//using Mso = Microsoft.Office.Core;
//using System.Xml.Linq;

//namespace PowerPointWebSocketControl
//{
//    internal class Program
//    {
//        private static Presentation _presentation;
//        private static Dictionary<string, (float Width, float Height, float Left, float Top)> _originalShapes = new Dictionary<string, (float, float, float, float)>();
//        private static DateTime _startTime;

//        static async Task Main(string[] args)
//        {
//            Console.WriteLine("Application starting...");
//            try
//            {
//                Console.WriteLine("Initializing PowerPoint...");
//                Application pptApp = new Application();
//                _presentation = pptApp.Presentations.Open(
//                    @"C:\Users\Asus\OneDrive - Universidade de Aveiro\LEI - Sara Almeida\4ºano\IM\IM_Projects\PowerPointVoiceApp\ConsoleApp\IM First Presentation.pptx",
//                    Mso.MsoTriState.msoTrue,
//                    Mso.MsoTriState.msoFalse,
//                    Mso.MsoTriState.msoTrue);

//                Console.WriteLine("Running slideshow...");
//                _presentation.SlideShowSettings.Run();
//                _startTime = DateTime.Now;

//                Console.WriteLine("Initializing WebSocket...");
//                ClientWebSocket client = await Init();

//                Console.WriteLine("Starting message processing...");
//                var cancellationTokenSource = new CancellationTokenSource();
//                CancellationToken cancellationToken = cancellationTokenSource.Token;

//                Start processing WebSocket messages
//                var messageProcessingTask = Task.Run(async () =>
//                {
//                    while (client.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
//                    {
//                        await ProcessMessages(client);
//                    }
//                });

//                Console.WriteLine("Application running.");
//                await messageProcessingTask;
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Application error: {ex.Message}");
//            }
//        }


//        public static async Task<ClientWebSocket> Init()
//        {

//            string host = "127.0.0.1"; // Replace with your actual host
//            string path = "/IM/USER1/APP"; // Replace with your WebSocket path
//            ClientWebSocket client = new ClientWebSocket();

//            try
//            {
//                Console.WriteLine("Starting WebSocket initialization...");
//                Uri uri = new Uri("wss://" + host + ":8005" + path);
//                Console.WriteLine($"Connecting to {uri}...");

//                await client.ConnectAsync(uri, CancellationToken.None);
//                Console.WriteLine("Connected to the WebSocket server.");
//                IMPORTANTE
//                Handle messages and other logic here
//               await ProcessMessages(client);

//                Close the WebSocket when done
//               await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);
//                if (client.State != WebSocketState.Open)
//                {
//                    Console.WriteLine("WebSocket connection closed. Attempting to reconnect...");
//                    client = await Init(); // Reconnect logic
//                }

//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"WebSocket connection error: {ex.Message}");
//            }

//            return client;
//        }
//        private static async Task ProcessMessages(ClientWebSocket client)
//        {
//            byte[] buffer = new byte[1024];
//            while (client.State == WebSocketState.Open)
//            {
//                WebSocketReceiveResult result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
//                string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
//                Console.WriteLine($"Received message: {message}");

//                dynamic messageJSON = JsonConvert.DeserializeObject(message);
//                if (messageJSON != null && messageJSON.nlu != null)
//                {
//                    string intent = messageJSON.nlu.intent;
//                    string response = HandleIntent(intent, messageJSON);
//                    await SendMessage(client, response);
//                }
//                else
//                {
//                    await SendMessage(client, CreateMMIMessage("Desculpe, não entendi o comando."));
//                }
//            }
//        }

//        private static string HandleIntent(string intent, dynamic messageJSON)
//        {
//            Console.WriteLine($"[INFO] Received intent: {intent}, Parameters: {JsonConvert.SerializeObject(messageJSON)}");

//            switch (intent)
//            {
//                case "next_slide":
//                    if (_presentation.SlideShowWindow != null)
//                    {
//                        Console.WriteLine("Presentation is in slideshow mode.");
//                        _presentation.SlideShowWindow.View.Next();
//                        return CreateMMIMessage("Avançando para o próximo slide.");
//                    }
//                    else
//                    {
//                        Console.WriteLine("No active slideshow window.");
//                        return "Nenhum slideshow ativo.";
//                    }

//                case "previous_slide":
//                    _presentation.SlideShowWindow.View.Previous();
//                    return CreateMMIMessage("Voltando ao slide anterior.");

//                case "highlight_text":
//                    {
//                        string phraseToHighlight = messageJSON.nlu.text;
//                        bool found = false;

//                        foreach (Slide slide in _presentation.Slides)
//                        {
//                            foreach (Shape shape in slide.Shapes)
//                            {
//                                if (shape.HasTextFrame == Mso.MsoTriState.msoTrue)
//                                {
//                                    string text = shape.TextFrame.TextRange.Text;
//                                    int startIndex = text.IndexOf(phraseToHighlight, StringComparison.OrdinalIgnoreCase);
//                                    if (startIndex >= 0)
//                                    {
//                                        TextRange foundText = shape.TextFrame.TextRange.Characters(startIndex + 1, phraseToHighlight.Length);
//                                        foundText.Font.Bold = Mso.MsoTriState.msoTrue;
//                                        foundText.Font.Underline = Mso.MsoTriState.msoTrue;
//                                        Console.WriteLine($"Phrase '{phraseToHighlight}' highlighted.");
//                                        found = true;
//                                        break;
//                                    }
//                                }
//                            }
//                            if (found) break;
//                        }

//                        if (found)
//                            return CreateMMIMessage($"Texto '{phraseToHighlight}' destacado.");
//                        else
//                            return CreateMMIMessage($"Não foi possível destacar o texto '{phraseToHighlight}'.");
//                    }

//                case "jump_to_slide_by_title":
//                    {
//                        string title = messageJSON.nlu.title;
//                        bool found = false;

//                        foreach (Slide slide in _presentation.Slides)
//                        {
//                            if (slide.Shapes.HasTitle == Mso.MsoTriState.msoTrue &&
//                                slide.Shapes.Title.TextFrame.TextRange.Text.Equals(title, StringComparison.OrdinalIgnoreCase))
//                            {
//                                _presentation.SlideShowWindow.View.GotoSlide(slide.SlideIndex);
//                                found = true;
//                                break;
//                            }
//                        }

//                        if (found)
//                            return CreateMMIMessage($"Indo para o slide com o título '{title}'.");
//                        else
//                            return CreateMMIMessage($"Slide com o título '{title}' não encontrado.");
//                    }

//                case "jump_to_slide_by_number":
//                    {
//                        int slideNumber = (int)messageJSON.nlu.number;

//                        if (slideNumber > 0 && slideNumber <= _presentation.Slides.Count)
//                        {
//                            _presentation.SlideShowWindow.View.GotoSlide(slideNumber);
//                            return CreateMMIMessage($"Indo para o slide número {slideNumber}.");
//                        }
//                        else
//                        {
//                            return CreateMMIMessage($"Slide número {slideNumber} não encontrado.");
//                        }
//                    }

//                default:
//                    return CreateMMIMessage("Comando não reconhecido.");
//            }
//        }


//        private static string GetElapsedTime()
//        {
//            TimeSpan elapsedTime = DateTime.Now - _startTime;
//            return $"Tempo decorrido: {elapsedTime.Hours} horas, {elapsedTime.Minutes} minutos e {elapsedTime.Seconds} segundos.";
//        }

//        private static async Task SendMessage(ClientWebSocket client, string message)
//        {
//            byte[] buffer = Encoding.UTF8.GetBytes(message);
//            await client.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
//            Console.WriteLine($"Sent message: {message}");
//        }

//        private static string CreateMMIMessage(string text)
//        {
//            return "<mmi:mmi xmlns:mmi=\"http://www.w3.org/2008/04/mmi-arch\" mmi:version=\"1.0\">" +
//                        "<mmi:startRequest mmi:context=\"ctx-1\" mmi:requestId=\"text-1\" mmi:source=\"APPSPEECH\" mmi:target=\"IM\">" +
//                            "<mmi:data>" +
//                                "<emma:emma xmlns:emma=\"http://www.w3.org/2003/04/emma\" emma:version=\"1.0\">" +
//                                    "<emma:interpretation emma:confidence=\"1\" emma:id=\"text-\" emma:medium=\"text\" emma:mode=\"command\" emma:start=\"0\">" +
//                                        "<command>\"&lt;speak version=\"1.0\" xmlns=\"http://www.w3.org/2001/10/synthesis\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.w3.org/2001/10/synthesis http://www.w3.org/TR/speech-synthesis/synthesis.xsd\" xml:lang=\"pt-PT\"&gt;&lt;p&gt;" + text + "&lt;/p&gt;&lt;/speak&gt;\"</command>" +
//                                    "</emma:interpretation>" +
//                                    "</emma:emma>" +
//                            "</mmi:data>" +
//                        "</mmi:startRequest>" +
//                    "</mmi:mmi>";
//        }
//    }
//}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Office.Interop.PowerPoint;
using Mso = Microsoft.Office.Core;
using System.Xml.Linq;

class Program

{
    private static Presentation _presentation;

    static async Task Main(string[] args)
    {
        string host = "127.0.0.1"; // Replace with your actual host
        string path = "/IM/USER1/APP"; // Replace with your WebSocket path

        // Receive Messages from rasa
        using (ClientWebSocket client = new ClientWebSocket())
        {
            Uri uri = new Uri("wss://" + host + ":8005" + path);

            try
            {
                await client.ConnectAsync(uri, CancellationToken.None);

                Console.WriteLine("Connected to the WebSocket server.");

                // Handle messages and other logic here
                await ProcessMessages(client);

                // Close the WebSocket when done
                await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WebSocket connection error: {ex.Message}");
            }
        }

        Console.WriteLine("Initializing PowerPoint...");
        Application pptApp = new Application();
        _presentation = pptApp.Presentations.Open(
            @"C:\Users\Asus\OneDrive - Universidade de Aveiro\LEI - Sara Almeida\4ºano\IM\IM_Projects\PowerPointVoiceApp\ConsoleApp\IM First Presentation.pptx",
            Mso.MsoTriState.msoTrue,
            Mso.MsoTriState.msoFalse,
            Mso.MsoTriState.msoTrue);

        Console.WriteLine("Running slideshow...");
        _presentation.SlideShowSettings.Run();
    }
    private static async Task SendMessage(ClientWebSocket client, string message)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(message);
        await client.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
        Console.WriteLine($"Sent message: {message}");
    }

    static async Task ProcessMessages(ClientWebSocket client)
    {

        byte[] buffer = new byte[1024];


        while (client.State == WebSocketState.Open)
        {
            WebSocketReceiveResult result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (result.MessageType == WebSocketMessageType.Text)
            {
                string message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                if (message == "OK")
                {
                    Console.WriteLine("Received message OK: " + message);
                }
                else if (message != null && message != "RENEW")
                {
                    //Console.WriteLine("Received message: " + message);

                    var doc = XDocument.Parse(message);
                    var com = doc.Descendants("command").FirstOrDefault().Value;
                    dynamic messageJSON = JsonConvert.DeserializeObject(com);

                    Console.WriteLine(messageJSON);

                    //Console.WriteLine(messageJSON["nlu"] == null ? "Sim" : "Nao");

                    // Only process the message if there is something in the nlu parameter 
                    // To resolve the runtime error 
                    if (messageJSON["nlu"] != null)
                    {
                        Console.WriteLine(messageJSON["nlu"]);

                        string intent = (string)messageJSON["nlu"]["intent"];

                        if (intent == "next_slide")
                        {
                            _presentation.SlideShowWindow.View.Next();
                            await SendMessage(client, messageMMI("Avançando para o próximo slide."));

                        }
                    }
                    else
                    {
                        await SendMessage(client, messageMMI("Não entendi. Repita por favor !"));
                    }
                }
            }
        }
    }

    public static string messageMMI(string msg)
    {
        return "<mmi:mmi xmlns:mmi=\"http://www.w3.org/2008/04/mmi-arch\" mmi:version=\"1.0\">" +
                    "<mmi:startRequest mmi:context=\"ctx-1\" mmi:requestId=\"text-1\" mmi:source=\"APPSPEECH\" mmi:target=\"IM\">" +
                        "<mmi:data>" +
                            "<emma:emma xmlns:emma=\"http://www.w3.org/2003/04/emma\" emma:version=\"1.0\">" +
                                "<emma:interpretation emma:confidence=\"1\" emma:id=\"text-\" emma:medium=\"text\" emma:mode=\"command\" emma:start=\"0\">" +
                                    "<command>\"&lt;speak version=\"1.0\" xmlns=\"http://www.w3.org/2001/10/synthesis\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.w3.org/2001/10/synthesis http://www.w3.org/TR/speech-synthesis/synthesis.xsd\" xml:lang=\"pt-PT\"&gt;&lt;p&gt;" + msg + "&lt;/p&gt;&lt;/speak&gt;\"</command>" +
                                "</emma:interpretation>" +
                                "</emma:emma>" +
                        "</mmi:data>" +
                    "</mmi:startRequest>" +
                "</mmi:mmi>";
    }
}
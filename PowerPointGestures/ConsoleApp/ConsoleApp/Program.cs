using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Newtonsoft.Json;
using Microsoft.Office.Interop.PowerPoint;
using Mso = Microsoft.Office.Core;
using System.Xml.Linq;
using System.Drawing;
using static System.Net.Mime.MediaTypeNames;

class Program

{
    private static Presentation _presentation;
    private static Dictionary<string, (float Width, float Height, float Left, float Top)> _originalShapes = new Dictionary<string, (float, float, float, float)>();
    private static DateTime _startTime;

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

                Console.WriteLine("Initializing PowerPoint...");
                Microsoft.Office.Interop.PowerPoint.Application pptApp = new Microsoft.Office.Interop.PowerPoint.Application();

                // Get the project root directory (ConsoleApp1)
                string projectDir = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;

                // Combine with the filename in the project directory
                string pptFilePath = Path.Combine(projectDir, "IM_Final_Gesture_108796-108067.pptx");

                // Ensure the file exists before trying to open it
                Console.WriteLine($"Looking for presentation at: {pptFilePath}");
                if (!File.Exists(pptFilePath))
                {
                    Console.WriteLine("Presentation file does not exist!");
                    return;
                }

                //_presentation = pptApp.Presentations.Open(
                //    pptFilePath,
                //    Mso.MsoTriState.msoTrue,
                //    Mso.MsoTriState.msoFalse,
                //    Mso.MsoTriState.msoTrue);

                //Console.WriteLine("Running slideshow...");
                //_presentation.SlideShowSettings.Run();
                //_startTime = DateTime.Now;


                // Open the presentation without starting slideshow mode
                _presentation = pptApp.Presentations.Open(
                    pptFilePath,
                    Mso.MsoTriState.msoTrue,
                    Mso.MsoTriState.msoFalse,
                    Mso.MsoTriState.msoFalse);

                Console.WriteLine("PowerPoint opened without slideshow mode.");
                _startTime = DateTime.Now;


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
            try
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
                        if (com != null)
                        {
                            dynamic messageJSON = JsonConvert.DeserializeObject(com);

                            // Check if the recognized key exists and has at least two elements
                            if (messageJSON != null && messageJSON["recognized"] != null && messageJSON["recognized"].Count > 1)
                            {
                                string gesture = messageJSON["recognized"][1];
                                string confidence = messageJSON["confidence"];
                                Console.WriteLine($"Gesture: {gesture}");


                                if (gesture == "NEXTSLIDE")
                                {
                                    Console.WriteLine("aqu2");
                                    _presentation.SlideShowWindow.View.Next();
                                    Console.WriteLine("aqui");
                                    //await SendMessage(client, messageMMI("Avançando para o próximo slide."));

                                }

                                if (gesture == "PREVIOUSSLIDE")
                                {
                                    _presentation.SlideShowWindow.View.Previous();
                                    //await SendMessage(client, messageMMI("Voltando ao slide anterior."));

                                }

                                if (gesture == "SILENCE")
                                {
                                        await SendMessage(client, messageMMI("Pedimos silêncio à audiência, por favor! "));
                                }

                                if (gesture == "TIMER")
                                {
                                    TimeSpan elapsedTime = DateTime.Now - _startTime;
                                    await SendMessage(client, messageMMI($" Tempo decorrido: {elapsedTime.Hours} horas, {elapsedTime.Minutes} minutos e {elapsedTime.Seconds} segundos."));
                                }

                                if (gesture == "QUESTIONS")
                                {
                                    string title = "Obrigada!";
                                    bool found = false;

                                    foreach (Slide slide in _presentation.Slides)
                                    {
                                        if (slide.Shapes.HasTitle == Mso.MsoTriState.msoTrue &&
                                            slide.Shapes.Title.TextFrame.TextRange.Text.Trim().Equals(title, StringComparison.OrdinalIgnoreCase))
                                        {
                                            _presentation.SlideShowWindow.View.GotoSlide(slide.SlideIndex);
                                            found = true;
                                            break;
                                        }
                                    }

                                    if (found)
                                        await SendMessage(client, messageMMI($"Alguém tem alguma dúvida?."));

                                    else
                                        await SendMessage(client, messageMMI($"Slide com o título '{title}' não encontrado."));
                                }

                                if (gesture == "SKIP")
                                {
                                    try
                                    {
                                        // Obtém o índice atual do slide
                                        int currentSlideIndex = _presentation.SlideShowWindow.View.Slide.SlideIndex;

                                        // Calcula o índice do próximo slide
                                        int nextSlideIndex = currentSlideIndex + 2;

                                        // Verifica se o índice do próximo slide está dentro do intervalo válido
                                        if (nextSlideIndex <= _presentation.Slides.Count)
                                        {
                                            // Avança para o slide calculado
                                            _presentation.SlideShowWindow.View.GotoSlide(nextSlideIndex);
                                            await SendMessage(client, messageMMI("Saltados 2 slides."));
                                        }
                                        else
                                        {
                                            // Caso não seja possível avançar dois slides
                                            await SendMessage(client, messageMMI("Não é possível saltar 2 slides. Já está no final da apresentação."));
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Erro ao avançar 2 slides: {ex.Message}");
                                        await SendMessage(client, messageMMI("Ocorreu um erro ao tentar avançar 2 slides."));
                                    }
                                }

                                if (gesture == "START")
                                {
                                    Console.WriteLine("Starting presentation...");
                                    _presentation.SlideShowSettings.StartingSlide = 1;
                                    _presentation.SlideShowSettings.EndingSlide = _presentation.Slides.Count;
                                    _presentation.SlideShowSettings.ShowWithNarration = Mso.MsoTriState.msoTrue;
                                    _presentation.SlideShowSettings.ShowWithAnimation = Mso.MsoTriState.msoTrue;

                                    _presentation.SlideShowSettings.AdvanceMode = PpSlideShowAdvanceMode.ppSlideShowUseSlideTimings;

                                    // Run the slideshow
                                    _presentation.SlideShowSettings.Run();
                                    await SendMessage(client, messageMMI("Apresentação iniciada."));

                                    foreach (Slide slide in _presentation.Slides)
                                    {
                                        Console.WriteLine("Slide index: " + slide.SlideIndex);
                                        foreach (Microsoft.Office.Interop.PowerPoint.Shape shape in slide.Shapes)
                                        {
                                            if (shape.HasTextFrame == Mso.MsoTriState.msoTrue && shape.TextFrame.TextRange != null)
                                            {
                                                Console.WriteLine($"Shape name: {shape.Name}, Text: '{shape.TextFrame.TextRange.Text}'");
                                            }
                                        }
                                    }


                                }

                                if (gesture == "STOP")
                                {
                                    if (_presentation != null)
                                    {
                                        _presentation.SlideShowWindow.View.Exit();
                                    }
                                    else
                                    {
                                        await SendMessage(client, messageMMI("Nenhuma apresentação está aberta."));
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine("No recognized gesture or invalid data format.");
                            }

                        }
                        else
                        {
                            Console.WriteLine("Invalid command format.");
                        }


                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
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

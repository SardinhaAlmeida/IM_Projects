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
    private static Presentation _presentation2;
    private static Presentation _activePresentation;
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
                string pptFilePath = Path.Combine(projectDir, "Last_IM_Presentation.pptx");

                // Ensure the file exists before trying to open it
                Console.WriteLine($"Looking for presentation at: {pptFilePath}");
                if (!File.Exists(pptFilePath))
                {
                    Console.WriteLine("Presentation file does not exist!");
                    return;
                }

                _presentation = pptApp.Presentations.Open(
                    pptFilePath,
                    Mso.MsoTriState.msoTrue,
                    Mso.MsoTriState.msoFalse,
                    Mso.MsoTriState.msoTrue);

                Console.WriteLine("Running slideshow...");
                _presentation.SlideShowSettings.Run();
                _activePresentation = _presentation;
                _startTime = DateTime.Now;


                // Open the presentation without starting slideshow mode
                //_presentation = pptApp.Presentations.Open(
                //    pptFilePath,
                //    Mso.MsoTriState.msoTrue,
                //    Mso.MsoTriState.msoFalse,
                //    Mso.MsoTriState.msoFalse);

                //Console.WriteLine("PowerPoint opened without slideshow mode.");
                //_startTime = DateTime.Now;


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
                var completeMessage = new List<byte>();
                WebSocketReceiveResult result;

                do
                {
                    result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    completeMessage.AddRange(buffer.Take(result.Count));
                }
                while (!result.EndOfMessage);


                if (result.MessageType == WebSocketMessageType.Text)
                {

                    // Now we have the entire XML in one string
                    string message = Encoding.UTF8.GetString(completeMessage.ToArray());

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
                            Console.WriteLine($"messageJSON HEREEEEEEE: {messageJSON}");

                            // Check if the recognized key exists and has at least two elements
                            if (messageJSON != null && messageJSON["recognized"] != null && messageJSON["recognized"].Count > 1)
                            {
                                Console.WriteLine($"messageJSON: {messageJSON}");
                                string modality = messageJSON["recognized"][0];
                                string confidence = messageJSON["confidence"];
                                Console.WriteLine($"Modality: {modality}");

                                if (modality == "SPEECH")
                                {
                                    //Console.WriteLine((string)messageJSON["nlu"]);
                                    //string intent = (string)messageJSON["nlu"]["intent"]["name"];
                                    //Console.WriteLine($"intent: {intent} ");
                                    string intent = messageJSON["recognized"][1];
                                    HandleSpeech(intent, messageJSON, client);
                                }
                                else if (modality == "GESTURES")
                                {
                                    string gesture = messageJSON["recognized"][1];
                                    HandleGesture(gesture, client);
                                }
                                else if (modality == "FUSION")
                                {
                                    // Get the "recognized" array starting from index 1
                                    var fusionData = new List<dynamic>();
                                    for (int i = 1; i < messageJSON["recognized"].Count; i++)
                                    {
                                        fusionData.Add(messageJSON["recognized"][i]);
                                    }

                                    // Handle fusion data
                                    HandleFusion(fusionData, messageJSON, client);

                                }

                            }
                            else
                            {
                                Console.WriteLine("No recognized modality or invalid data format.");
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

    static async Task HandleSpeech(string intent, dynamic messageJSON, ClientWebSocket client)
    {

        if (intent == "GO_TO_SLIDE_NUMBER")
        {
            //int slideNumber = (int)messageJSON.nlu.slide_number;
            int slideNumber = (int)messageJSON["recognized"][2];

            if (slideNumber > 0 && slideNumber <= _presentation.Slides.Count)
            {
                _presentation.SlideShowWindow.View.GotoSlide(slideNumber);
                await SendMessage(client, messageMMI($"Slide {slideNumber}."));
            }
            else
            {
                await SendMessage(client, messageMMI($"Slide número {slideNumber} não encontrado."));
            }

        }

        if (intent == "GO_TO_SLIDE_TITLE")
        {
            string title = messageJSON["recognized"][2];
            Console.WriteLine($"title {title}");
            bool found = false;

            foreach (Slide slide in _presentation.Slides)
            {
                if (slide.Shapes.HasTitle == Mso.MsoTriState.msoTrue &&
                    slide.Shapes.Title.TextFrame.TextRange.Text.Equals(title, StringComparison.OrdinalIgnoreCase))
                {
                    _presentation.SlideShowWindow.View.GotoSlide(slide.SlideIndex);
                    found = true;
                    break;
                }
            }

            if (found)
                await SendMessage(client, messageMMI($"Slide de '{title}'."));

            else
                await SendMessage(client, messageMMI($"Slide com o título '{title}' não encontrado."));
        }

        if (intent == "HIGHLIGHT_PHRASE")
        {
            string phraseToHighlight = messageJSON["recognized"][2];
            bool found = false;

            foreach (Slide slide in _presentation.Slides)
            {
                foreach (Shape shape in slide.Shapes)
                {
                    if (shape.HasTextFrame == Mso.MsoTriState.msoTrue)
                    {
                        string text = shape.TextFrame.TextRange.Text;
                        int startIndex = text.IndexOf(phraseToHighlight, StringComparison.OrdinalIgnoreCase);
                        if (startIndex >= 0)
                        {
                            TextRange foundText = shape.TextFrame.TextRange.Characters(startIndex + 1, phraseToHighlight.Length);
                            foundText.Font.Bold = Mso.MsoTriState.msoTrue;
                            foundText.Font.Underline = Mso.MsoTriState.msoTrue;
                            Console.WriteLine($"Phrase '{phraseToHighlight}' highlighted.");
                            found = true;
                            break;
                        }
                    }
                }
                if (found) break;
            }

            if (found)
                await SendMessage(client, messageMMI($"Destacado."));
            else
                await SendMessage(client, messageMMI($"Não foi possível destacar o texto '{phraseToHighlight}'."));
        }

        if (intent == "ZOOM_IN")
        {
            var slide = _presentation.SlideShowWindow.View.Slide;
            Shape focusShape = null;
            float maxArea = 0;

            foreach (Shape shape in slide.Shapes)
            {
                if (shape.Type == Mso.MsoShapeType.msoPicture || shape.Type == Mso.MsoShapeType.msoAutoShape)
                {
                    float area = shape.Width * shape.Height;
                    if (area > maxArea)
                    {
                        maxArea = area;
                        focusShape = shape;
                    }

                    // Armazena os tamanhos e posições originais, se ainda não estiverem salvos
                    if (!_originalShapes.ContainsKey(shape.Name))
                    {
                        _originalShapes[shape.Name] = (shape.Width, shape.Height, shape.Left, shape.Top);
                    }
                }
            }

            if (focusShape != null)
            {
                // Ampliar e centralizar
                focusShape.Width *= 1.5f;
                focusShape.Height *= 1.5f;
                focusShape.Left = (_presentation.PageSetup.SlideWidth - focusShape.Width) / 2;
                focusShape.Top = (_presentation.PageSetup.SlideHeight - focusShape.Height) / 2;

                await SendMessage(client, messageMMI($"Zoom aplicado."));

            }
            else
            {
                await SendMessage(client, messageMMI("Nenhuma área principal foi encontrada no slide para aplicar zoom."));
            }
        }
        if (intent == "ZOOM_OUT")
        {
            var slide = _presentation.SlideShowWindow.View.Slide;

            foreach (Shape shape in slide.Shapes)
            {
                if (_originalShapes.ContainsKey(shape.Name))
                {
                    // Restaurar tamanho e posição originais
                    var original = _originalShapes[shape.Name];
                    shape.Width = original.Width;
                    shape.Height = original.Height;
                    shape.Left = original.Left;
                    shape.Top = original.Top;
                }
            }

            await SendMessage(client, messageMMI("Zoom revertido"));
            //else
            //{
            //    await SendMessage(client, messageMMI("Nenhuma área principal foi encontrada no slide para aplicar zoom."));
            //}
        }


        if (intent == "CURRENT_SLIDE")
        {
            if (_presentation?.SlideShowWindow?.View != null)
            {
                int currentSlideIndex = _presentation.SlideShowWindow.View.Slide.SlideIndex;
                await SendMessage(client, messageMMI($"Estás no slide número {currentSlideIndex}."));
            }
            else
            {
                await SendMessage(client, messageMMI("Nenhuma apresentação está aberta ou ativa."));
            }
        }

        if (intent == "SLIDES_LEFT")
        {
            if (_presentation?.SlideShowWindow?.View != null)
            {
                int currentSlideIndex = _presentation.SlideShowWindow.View.Slide.SlideIndex;
                int totalSlides = _presentation.Slides.Count;
                int slidesLeft = totalSlides - currentSlideIndex;

                if (slidesLeft > 0)
                {
                    await SendMessage(client, messageMMI($"Ainda faltam {slidesLeft} slides para terminar a apresentação."));
                }
                else
                {
                    await SendMessage(client, messageMMI("Você está no último slide."));
                }
            }
            else
            {
                await SendMessage(client, messageMMI("Nenhuma apresentação está aberta ou ativa."));
            }
        }

        if (intent == "RESTART_PRESENTATION")
        {
            if (_presentation?.SlideShowWindow != null)
            {
                _presentation.SlideShowWindow.View.GotoSlide(1);
                await SendMessage(client, messageMMI("Reiniciada."));
            }
            else
            {
                await SendMessage(client, messageMMI("Nenhuma apresentação está em execução para reiniciar."));
            }
        }

        if (intent == "START_TIMER")
        {
            _startTime = DateTime.Now; // Reinicia o temporizador com o horário atual
            Console.WriteLine("Temporizador iniciado.");
            await SendMessage(client, messageMMI("Temporizador iniciado."));
        }
        if (intent == "STOP_TIMER")
        {
            if (_startTime == default(DateTime))
            {
                Console.WriteLine("Nenhum temporizador ativo.");
                await SendMessage(client, messageMMI("Nenhum temporizador está ativo."));
            }

            TimeSpan elapsed = DateTime.Now - _startTime; // Calcula o tempo decorrido
            _startTime = default(DateTime); // Reseta o temporizador
            Console.WriteLine("Temporizador parado.");
            await SendMessage(client, messageMMI($"Temporizador parado depois de {elapsed.Hours} horas, {elapsed.Minutes} minutos e {elapsed.Seconds} segundos."));
        }

        if (intent == "END_HELPER")
        {
            await SendMessage(client, messageMMI("De nada! Boa sorte!"));
            if (_presentation2 != null)
            {
                _presentation2.SlideShowWindow.View.Exit();
                _presentation2.Close();
                _activePresentation = _presentation;
                _presentation2 = null;
            }

            if (_presentation != null)
            {
                _activePresentation = _presentation;
            }

        }

        if (intent == "greet")
        {
            await SendMessage(client, messageMMI("Olá! Como posso ajudar hoje?"));
        }
        if (intent == "ask_how_are_you")
        {
            await SendMessage(client, messageMMI("Estou ótimo, obrigado por perguntar! Como está você?"));
        }
        if (intent == "respond_how_am_i")
        {
            await SendMessage(client, messageMMI("Estou aqui para o que precisar."));
        }

        if (intent == "close_presentation")
        {
            if (_presentation != null)
            {
                _presentation.SlideShowWindow.View.Exit();
                _presentation.Close();
                _presentation.Application.Quit();
                _presentation = null;
            }
            else
            {
                await SendMessage(client, messageMMI("Nenhuma apresentação está aberta."));
            }
        }

    }

    static async Task HandleGesture(string gesture, ClientWebSocket client)
    {
        if (gesture == "REQUEST_SILENCE")
        {
            await SendMessage(client, messageMMI("Pedimos silêncio à audiência, por favor!"));
        }

        if (gesture == "QUESTIONS")
        {
            string title = "Obrigada!";
            bool found = false;

            foreach (Slide slide in _activePresentation.Slides)
            {
                if (slide.Shapes.HasTitle == Mso.MsoTriState.msoTrue &&
                    slide.Shapes.Title.TextFrame.TextRange.Text.Trim().Equals(title, StringComparison.OrdinalIgnoreCase))
                {
                    _activePresentation.SlideShowWindow.View.GotoSlide(slide.SlideIndex);
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
                int currentSlideIndex = _activePresentation.SlideShowWindow.View.Slide.SlideIndex;

                // Calcula o índice do próximo slide
                int nextSlideIndex = currentSlideIndex + 2;

                // Verifica se o índice do próximo slide está dentro do intervalo válido
                if (nextSlideIndex <= _activePresentation.Slides.Count)
                {
                    // Avança para o slide calculado
                    _activePresentation.SlideShowWindow.View.GotoSlide(nextSlideIndex);
                    await SendMessage(client, messageMMI("Feito!"));
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
            _activePresentation.SlideShowSettings.StartingSlide = 1;
            _activePresentation.SlideShowSettings.EndingSlide = _presentation.Slides.Count;
            _activePresentation.SlideShowSettings.ShowWithNarration = Mso.MsoTriState.msoTrue;
            _activePresentation.SlideShowSettings.ShowWithAnimation = Mso.MsoTriState.msoTrue;

            _activePresentation.SlideShowSettings.AdvanceMode = PpSlideShowAdvanceMode.ppSlideShowUseSlideTimings;

            // Run the slideshow
            _activePresentation.SlideShowSettings.Run();
            await SendMessage(client, messageMMI("Apresentação iniciada."));

            foreach (Slide slide in _activePresentation.Slides)
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
                _activePresentation.SlideShowWindow.View.Exit();
            }
            else
            {
                await SendMessage(client, messageMMI("Nenhuma apresentação está aberta."));
            }
        }
    }


    static async Task HandleFusion(List<dynamic> fusion, dynamic messageJSON, ClientWebSocket client)
    {
        Console.WriteLine("Fusion detected:");
        Console.WriteLine($"Fusion Data: {string.Join(", ", fusion)}");

        var fusion_data = string.Join(",", fusion);

        if (fusion_data == "NEXT_SLIDE")
        {
            _activePresentation.SlideShowWindow.View.Next();
            await SendMessage(client, messageMMI("Ok!"));

        }

        if (fusion_data == "PREVIOUS_SLIDE")
        {
            _activePresentation.SlideShowWindow.View.Previous();
            await SendMessage(client, messageMMI("Feito!"));

        }

        if (fusion_data == "ELAPSED_TIME")
        {
            TimeSpan elapsedTime = DateTime.Now - _startTime;
            await SendMessage(client, messageMMI($" Passaram: {elapsedTime.Minutes} minutos e {elapsedTime.Seconds} segundos."));
        }

        if (fusion_data == "HELPER")
        {
            Microsoft.Office.Interop.PowerPoint.Application pptApp2 = new Microsoft.Office.Interop.PowerPoint.Application();

            await SendMessage(client, messageMMI("Olá! Estou aqui para ajudar! Aqui está um powerpoint onde podes ver tudo o que podes fazer para interagir com o sistema!"));
            string projectDir2 = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;

            string ppthelpFilePath = Path.Combine(projectDir2, "Help_Functionalities.pptx");


            Console.WriteLine($"Looking for presentation at: {ppthelpFilePath}");
            if (!File.Exists(ppthelpFilePath))
            {
                Console.WriteLine("Presentation file does not exist!");
                return;
            }

            _presentation2 = pptApp2.Presentations.Open(
                    ppthelpFilePath,
                    Mso.MsoTriState.msoTrue,
                    Mso.MsoTriState.msoFalse,
                    Mso.MsoTriState.msoTrue);

            Console.WriteLine("Running slideshow...");
            _presentation2.SlideShowSettings.Run();
            _activePresentation = _presentation2;
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

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
                string pptFilePath = Path.Combine(projectDir, "IM_Final_Voice_108796-108067.pptx");

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

                                //if (intent == "jump_to_slide_by_number")
                                //{
                                //    int slideNumber = (int)messageJSON.nlu.slide_number;

                                //    if (slideNumber > 0 && slideNumber <= _presentation.Slides.Count)
                                //    {
                                //        _presentation.SlideShowWindow.View.GotoSlide(slideNumber);
                                //        await SendMessage(client, messageMMI($"Indo para o slide número {slideNumber}."));
                                //    }
                                //    else
                                //    {
                                //        await SendMessage(client, messageMMI($"Slide número {slideNumber} não encontrado."));
                                //    }

                                //}

                                //if (intent == "jump_to_slide_by_title")
                                //{
                                //    string title = messageJSON.nlu.slide_title;
                                //    bool found = false;

                                //    foreach (Slide slide in _presentation.Slides)
                                //    {
                                //        if (slide.Shapes.HasTitle == Mso.MsoTriState.msoTrue &&
                                //            slide.Shapes.Title.TextFrame.TextRange.Text.Equals(title, StringComparison.OrdinalIgnoreCase))
                                //        {
                                //            _presentation.SlideShowWindow.View.GotoSlide(slide.SlideIndex);
                                //            found = true;
                                //            break;
                                //        }
                                //    }

                                //    if (found)
                                //        await SendMessage(client, messageMMI($"Indo para o slide com o título '{title}'."));

                                //    else
                                //        await SendMessage(client, messageMMI($"Slide com o título '{title}' não encontrado."));
                                //}

                                //if (intent == "highlight_phrase")
                                //{
                                //    string phraseToHighlight = messageJSON.nlu.phrase;
                                //    bool found = false;

                                //    foreach (Slide slide in _presentation.Slides)
                                //    {
                                //        foreach (Shape shape in slide.Shapes)
                                //        {
                                //            if (shape.HasTextFrame == Mso.MsoTriState.msoTrue)
                                //            {
                                //                string text = shape.TextFrame.TextRange.Text;
                                //                int startIndex = text.IndexOf(phraseToHighlight, StringComparison.OrdinalIgnoreCase);
                                //                if (startIndex >= 0)
                                //                {
                                //                    TextRange foundText = shape.TextFrame.TextRange.Characters(startIndex + 1, phraseToHighlight.Length);
                                //                    foundText.Font.Bold = Mso.MsoTriState.msoTrue;
                                //                    foundText.Font.Underline = Mso.MsoTriState.msoTrue;
                                //                    Console.WriteLine($"Phrase '{phraseToHighlight}' highlighted.");
                                //                    found = true;
                                //                    break;
                                //                }
                                //            }
                                //        }
                                //        if (found) break;
                                //    }

                                //    if (found)
                                //        await SendMessage(client, messageMMI($"Texto '{phraseToHighlight}' destacado."));
                                //    else
                                //        await SendMessage(client, messageMMI($"Não foi possível destacar o texto '{phraseToHighlight}'."));
                                //}

                                //if (intent == "show_elapsed_time")
                                //{
                                //    TimeSpan elapsedTime = DateTime.Now - _startTime;
                                //    await SendMessage(client, messageMMI($"Tempo decorrido: {elapsedTime.Hours} horas, {elapsedTime.Minutes} minutos e {elapsedTime.Seconds} segundos."));
                                //}

                                //if (intent == "zoom_in")
                                //{
                                //    var slide = _presentation.SlideShowWindow.View.Slide;
                                //    Shape focusShape = null;
                                //    float maxArea = 0;

                                //    foreach (Shape shape in slide.Shapes)
                                //    {
                                //        if (shape.Type == Mso.MsoShapeType.msoPicture || shape.Type == Mso.MsoShapeType.msoAutoShape)
                                //        {
                                //            float area = shape.Width * shape.Height;
                                //            if (area > maxArea)
                                //            {
                                //                maxArea = area;
                                //                focusShape = shape;
                                //            }

                                //            // Armazena os tamanhos e posições originais, se ainda não estiverem salvos
                                //            if (!_originalShapes.ContainsKey(shape.Name))
                                //            {
                                //                _originalShapes[shape.Name] = (shape.Width, shape.Height, shape.Left, shape.Top);
                                //            }
                                //        }
                                //    }

                                //    if (focusShape != null)
                                //    {
                                //        // Ampliar e centralizar
                                //        focusShape.Width *= 1.5f;
                                //        focusShape.Height *= 1.5f;
                                //        focusShape.Left = (_presentation.PageSetup.SlideWidth - focusShape.Width) / 2;
                                //        focusShape.Top = (_presentation.PageSetup.SlideHeight - focusShape.Height) / 2;

                                //        await SendMessage(client, messageMMI($"Simulando zoom na área principal: {focusShape.Name}."));

                                //    }
                                //    else
                                //    {
                                //        await SendMessage(client, messageMMI("Nenhuma área principal foi encontrada no slide para aplicar zoom."));
                                //    }
                                //}
                                //if (intent == "zoom_out")
                                //{
                                //    var slide = _presentation.SlideShowWindow.View.Slide;

                                //    foreach (Shape shape in slide.Shapes)
                                //    {
                                //        if (_originalShapes.ContainsKey(shape.Name))
                                //        {
                                //            // Restaurar tamanho e posição originais
                                //            var original = _originalShapes[shape.Name];
                                //            shape.Width = original.Width;
                                //            shape.Height = original.Height;
                                //            shape.Left = original.Left;
                                //            shape.Top = original.Top;
                                //        }
                                //    }

                                //    await SendMessage(client, messageMMI("Zoom revertido"));
                                //    //else
                                //    //{
                                //    //    await SendMessage(client, messageMMI("Nenhuma área principal foi encontrada no slide para aplicar zoom."));
                                //    //}
                                //}


                                //if (intent == "get_current_slide")
                                //{
                                //    if (_presentation?.SlideShowWindow?.View != null)
                                //    {
                                //        int currentSlideIndex = _presentation.SlideShowWindow.View.Slide.SlideIndex;
                                //        await SendMessage(client, messageMMI($"Você está no slide número {currentSlideIndex}."));
                                //    }
                                //    else
                                //    {
                                //        await SendMessage(client, messageMMI("Nenhuma apresentação está aberta ou ativa."));
                                //    }
                                //}

                                //if (intent == "slides_left")
                                //{
                                //    if (_presentation?.SlideShowWindow?.View != null)
                                //    {
                                //        int currentSlideIndex = _presentation.SlideShowWindow.View.Slide.SlideIndex;
                                //        int totalSlides = _presentation.Slides.Count;
                                //        int slidesLeft = totalSlides - currentSlideIndex;

                                //        if (slidesLeft > 0)
                                //        {
                                //            await SendMessage(client, messageMMI($"Ainda faltam {slidesLeft} slides para terminar a apresentação."));
                                //        }
                                //        else
                                //        {
                                //            await SendMessage(client, messageMMI("Você está no último slide."));
                                //        }
                                //    }
                                //    else
                                //    {
                                //        await SendMessage(client, messageMMI("Nenhuma apresentação está aberta ou ativa."));
                                //    }
                                //}

                                //if (intent == "restart_presentation")
                                //{
                                //    if (_presentation?.SlideShowWindow != null)
                                //    {
                                //        _presentation.SlideShowWindow.View.GotoSlide(1);
                                //        await SendMessage(client, messageMMI("Apresentação reiniciada no primeiro slide."));
                                //    }
                                //    await SendMessage(client, messageMMI("Nenhuma apresentação está em execução para reiniciar."));
                                //}

                                //if (intent == "start_timer")
                                //{
                                //    _startTime = DateTime.Now; // Reinicia o temporizador com o horário atual
                                //    Console.WriteLine("Temporizador iniciado.");
                                //    await SendMessage(client, messageMMI("Temporizador iniciado."));
                                //}
                                //if (intent == "stop_timer")
                                //{
                                //    if (_startTime == default(DateTime))
                                //    {
                                //        Console.WriteLine("Nenhum temporizador ativo.");
                                //        await SendMessage(client, messageMMI("Nenhum temporizador está ativo."));
                                //    }

                                //    TimeSpan elapsed = DateTime.Now - _startTime; // Calcula o tempo decorrido
                                //    _startTime = default(DateTime); // Reseta o temporizador
                                //    Console.WriteLine("Temporizador parado.");
                                //    await SendMessage(client, messageMMI($"Temporizador parado. Tempo decorrido: {elapsed.Hours} horas, {elapsed.Minutes} minutos e {elapsed.Seconds} segundos."));
                                //}
                                //if (intent == "helper")
                                //{
                                //    await SendMessage(client, messageMMI(
                                //    "Aqui estão os comandos que pode usar para começar: "
                                //                                            + "- Próximo slide para avançar para o próximo slide. "
                                //            + "- Slide anterior para voltar ao slide anterior. "));
                                //}

                                //if (intent == "greet")
                                //{
                                //    await SendMessage(client, messageMMI("Olá! Como posso ajudar hoje?"));
                                //}
                                //if (intent == "ask_how_are_you")
                                //{
                                //    await SendMessage(client, messageMMI("Estou ótimo, obrigado por perguntar! Como está você?"));
                                //}
                                //if (intent == "respond_how_am_i")
                                //{
                                //    await SendMessage(client, messageMMI("Estou aqui para o que precisar."));
                                //}

                                //if (intent == "close_presentation")
                                //{
                                //    if (_presentation != null)
                                //    {
                                //        _presentation.SlideShowWindow.View.Exit();
                                //        _presentation.Close();
                                //        _presentation.Application.Quit();
                                //        _presentation = null;
                                //        await SendMessage(client, messageMMI("Apresentação fechada com sucesso."));
                                //    }
                                //    else
                                //    {
                                //        await SendMessage(client, messageMMI("Nenhuma apresentação está aberta."));
                                //    }
                                //}
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

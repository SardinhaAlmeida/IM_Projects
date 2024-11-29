using System;
using System.Collections.Generic;
using System.Linq;
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
        private static Dictionary<string, (float Width, float Height, float Left, float Top)> _originalShapes = new Dictionary<string, (float, float, float, float)>();
        private static DateTime _startTime;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting PowerPoint WebSocket Control...");

            // Initialize PowerPoint
            Application pptApp = new Application();
            _presentation = pptApp.Presentations.Open(
                @"C:\Users\maria\Downloads\IM Second Presentation.pptx",
                Mso.MsoTriState.msoTrue,
                Mso.MsoTriState.msoFalse,
                Mso.MsoTriState.msoTrue);

            _presentation.SlideShowSettings.Run();
            _startTime = DateTime.Now;

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

        private static string GetElapsedTime()
        {
            TimeSpan elapsedTime = DateTime.Now - _startTime;
            return $"Tempo decorrido: {elapsedTime.Hours} horas, {elapsedTime.Minutes} minutos e {elapsedTime.Seconds} segundos.";
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
            Console.WriteLine($"Comando recebido no servidor: {message}");

            try
            {
                // Parse the received message (assume it's JSON)
                var command = JsonSerializer.Deserialize<VoiceCommand>(message);
                if (command == null || string.IsNullOrEmpty(command.Intent))
                {
                    Console.WriteLine("Comando inválido ou nulo.");
                    return "Invalid command.";
                }

                Console.WriteLine($"Received Intent: {command.Intent} with additional data: {message}");

                // Garante que a apresentação está em modo de exibição de slides
                EnsureSlideShowView();

                switch (command.Intent.ToLower())
                {
                    case "next_slide":
                        _presentation.SlideShowWindow.View.Next();
                        return "Próximo slide.";

                    case "previous_slide":
                        _presentation.SlideShowWindow.View.Previous();
                        return "Slide anterior.";

                    case "jump_to_slide_by_title":
                        Console.WriteLine("entrou");
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

                    case "highlight_phrase":
                        if (!string.IsNullOrEmpty(command.Phrase))
                        {
                            Console.WriteLine($"Phrase to highlight: {command.Phrase}");
                            string phraseToHighlight = command.Phrase.Trim();
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
                                            return $"Sublinhando a frase: {phraseToHighlight}.";
                                        }
                                    }
                                }
                            }
                            Console.WriteLine($"Phrase '{command.Phrase}' not found.");
                            return $"A frase '{command.Phrase}' não foi encontrada em nenhum slide.";
                        }
                        return "Frase não fornecida.";

                    case "zoom_in":
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

                                return $"Simulando zoom na área principal: {focusShape.Name}.";
                            }

                            return "Nenhuma área principal foi encontrada no slide para aplicar zoom.";
                        }

                    case "zoom_out":
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

                            return "Zoom revertido.";
                        }
                    case "show_elapsed_time":
                        return GetElapsedTime();

                    // Video controls
                    case "play_video":
                        return ControlVideo("play");

                    case "pause_video":
                        return ControlVideo("pause");

                    case "stop_video":
                        return ControlVideo("stop");

                    case "fast_forward_video":
                        // Passe os segundos (use um valor padrão, como 10 segundos, se não for fornecido)
                        int secondsToFastForward = command.Seconds > 0 ? command.Seconds : 10;
                        return ControlVideo("fast_forward", secondsToFastForward);

                    case "rewind_video":
                        return ControlVideo("rewind");

                    case "current_slide":
                        return GetCurrentSlide();

                    case "restart_presentation":
                        if (_presentation?.SlideShowWindow != null)
                        {
                            _presentation.SlideShowWindow.View.GotoSlide(1);
                            return "Apresentação reiniciada no primeiro slide.";
                        }
                        return "Nenhuma apresentação está em execução para reiniciar.";

                    case "start_timer":
                        _startTime = DateTime.Now; // Reinicia o temporizador com o horário atual
                        Console.WriteLine("Temporizador iniciado.");
                        return "Temporizador iniciado.";

                    case "stop_timer":
                        if (_startTime == default(DateTime))
                        {
                            Console.WriteLine("Nenhum temporizador ativo.");
                            return "Nenhum temporizador está ativo.";
                        }

                        TimeSpan elapsed = DateTime.Now - _startTime; // Calcula o tempo decorrido
                        _startTime = default(DateTime); // Reseta o temporizador
                        Console.WriteLine("Temporizador parado.");
                        return $"Temporizador parado. Tempo decorrido: {elapsed.Hours} horas, {elapsed.Minutes} minutos e {elapsed.Seconds} segundos.";

                    case "helper":
                        return "Aqui estão os comandos que pode usar: "
                               + "- Próximo slide para avançar para o próximo slide. "
                               + "- Slide anterior para voltar ao slide anterior. "
                               + "- Reinicia a apresentação para reiniciar desde o primeiro slide. "
                               + "- Em que slide estou? para verificar o slide atual. "
                               + "- Inicia o temporizador para começar a cronometrar. "
                               + "- Para o temporizador para parar o temporizador. "
                               + "- Zoom in para ampliar no slide. "
                               + "- Zoom out para reduzir no slide. "
                               + "- Sublinhar frase para destacar uma frase no slide. "
                               + "- Mostra tempo decorrido para exibir o tempo desde o início da apresentação.";

                    case "greet":
                        return "Olá! Como posso ajudar hoje?";

                    case "ask_how_are_you":
                        return "Estou ótimo, obrigado por perguntar! Como está você?";

                    case "respond_how_am_i":
                        return "Que bom ouvir isso! Estou aqui para o que precisar.";

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

        private static string GetCurrentSlide()
        {
            try
            {
                if (_presentation.SlideShowWindow != null)
                {
                    var currentSlideIndex = _presentation.SlideShowWindow.View.Slide.SlideIndex;
                    Console.WriteLine($"Slide atual: {currentSlideIndex}");
                    return $"Estás no slide número {currentSlideIndex}.";
                }
                else
                {
                    Console.WriteLine("Nenhuma apresentação está em execução.");
                    return "Nenhuma apresentação está em execução.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao verificar o slide atual: {ex.Message}");
                return "Erro ao verificar o slide atual.";
            }
        }


        private static string ControlVideo(string action, int seconds = 0)
        {
            EnsureSlideShowView();

            foreach (Slide slide in _presentation.Slides)
            {
                foreach (Shape shape in slide.Shapes)
                {
                    Console.WriteLine($"Verificando elemento: {shape.Name}, Tipo: {shape.Type}");

                    // Verifique se a forma é de mídia
                    if (shape.Type == Mso.MsoShapeType.msoMedia)
                    {
                        Console.WriteLine($"Elemento de vídeo encontrado: {shape.Name}");

                        switch (action.ToLower())
                        {
                            case "play":
                                shape.AnimationSettings.PlaySettings.PlayOnEntry = Mso.MsoTriState.msoTrue;
                                shape.AnimationSettings.PlaySettings.LoopUntilStopped = Mso.MsoTriState.msoTrue;
                                Console.WriteLine("Iniciando reprodução do vídeo.");
                                return "Reproduzindo o vídeo.";

                            case "pause":
                                Console.WriteLine("Infelizmente, o PowerPoint Interop não suporta pausa diretamente.");
                                return "Não é possível pausar o vídeo neste modo.";

                            case "stop":
                                shape.AnimationSettings.PlaySettings.StopAfterSlides = 1; // Para de reproduzir após o slide
                                Console.WriteLine("Parando o vídeo.");
                                return "Vídeo parado.";

                            default:
                                Console.WriteLine($"Ação '{action}' não reconhecida.");
                                return "Ação não reconhecida.";
                        }
                    }
                }
            }

            Console.WriteLine("Nenhum vídeo encontrado no slide atual.");
            return "Nenhum vídeo encontrado no slide atual.";
        }

        private static void EnsureSlideShowView()
        {
            if (_presentation.SlideShowWindow == null)
            {
                Console.WriteLine("Modo de exibição de slides não encontrado. Iniciando...");
                _presentation.SlideShowSettings.Run();
            }
            else
            {
                Console.WriteLine("Apresentação já está no modo de exibição de slides.");
            }
        }

    }

    public class VoiceCommand
    {
        public string Intent { get; set; }
        public string SlideTitle { get; set; } // For jump_to_slide_by_title
        public string SlideNumber { get; set; }  // For jump_to_slide_by_number
        public string Phrase { get; set; } // For highlight_phrase
        public int Seconds { get; set; }
    }
}

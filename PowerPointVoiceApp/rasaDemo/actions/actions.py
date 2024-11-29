from typing import Any, Text, Dict, List
from rasa_sdk import Action, Tracker
from rasa_sdk.executor import CollectingDispatcher
import win32com.client
import requests
import websocket
import json 
import time

# Temporizador global
timer_start = None

class ActionNextSlide(Action):
    def name(self) -> Text:
        return "action_next_slide"

    def run(self, dispatcher: CollectingDispatcher, tracker, domain):
        # Send POST to PowerPoint server
        response = requests.post(
            "http://localhost:5000/api/voice-command/",
            json={"intent": "next_slide"}
        )

        if response.status_code == 200:
            dispatcher.utter_message(text="Passando para o próximo slide.")
        else:
            dispatcher.utter_message(text="Erro ao comunicar com o servidor PowerPoint.")
        return []



class ActionPreviousSlide(Action):
    def name(self) -> Text:
        return "action_previous_slide"

    def run(self, dispatcher: CollectingDispatcher,
            tracker: Dict[Text, Any],
            domain: Dict[Text, Any]) -> List[Dict[Text, Any]]:
        try:
            pptApp = win32com.client.Dispatch("PowerPoint.Application")
            if pptApp.SlideShowWindows.Count > 0:
                pptApp.SlideShowWindows(1).View.Previous()
                dispatcher.utter_message(text="Voltando para o slide anterior.")
            else:
                dispatcher.utter_message(text="Nenhuma apresentação está em execução.")
        except Exception as e:
            dispatcher.utter_message(text=f"Erro ao voltar para o slide anterior: {e}")
        return []

class ActionJumpToSlideByTitle(Action):
    def name(self) -> Text:
        return "action_jump_to_slide_by_title"

    def run(self, dispatcher: CollectingDispatcher, tracker: Tracker, domain: Dict[Text, Any]) -> List[Dict[Text, Any]]:
        slide_title = tracker.get_slot("slide_title")
        print(f"Slide Title received from slot: {slide_title}")
        if not slide_title:  # Validação
            dispatcher.utter_message(text="Não encontrei um título válido.")
            return []
        if slide_title:
            print(f"Slide Title received: {slide_title}")
            try:
                ws = websocket.create_connection("ws://localhost:5000/")
                command = {"Intent": "jump_to_slide_by_title", "SlideTitle": slide_title}
               
                ws.send(json.dumps(command))
                ws.close()
                dispatcher.utter_message(text=f"Indo para o slide com o título: {slide_title}.")
            except Exception as e:
                print(f"Error sending WebSocket message: {e}")
                dispatcher.utter_message(text=f"Erro ao comunicar com o servidor: {e}")
        else:
            dispatcher.utter_message(text="Não encontrei um título válido.")
        return []

class ActionJumpToSlideByNumber(Action):
    def name(self) -> Text:
        return "action_jump_to_slide_by_number"

    def run(self, dispatcher: CollectingDispatcher, tracker: Tracker, domain: Dict[Text, Any]) -> List[Dict[Text, Any]]:
            slide_number = tracker.get_slot("slide_number")

            if slide_number:
                try:
                    slide_number = int(slide_number)  # Ensure it's a valid number
                    dispatcher.utter_message(text=f"Indo para o slide número {slide_number}.")

                    # Send WebSocket command
                    import websocket
                    ws = websocket.create_connection("ws://localhost:5000/")
                    command = {"Intent": "jump_to_slide_by_number", "SlideNumber": slide_number}
                    ws.send(json.dumps(command))
                    ws.close()
                except ValueError:
                    dispatcher.utter_message(text="O número do slide fornecido não é válido.")
            else:
                dispatcher.utter_message(text="Nenhum número de slide fornecido.")

            return []

class ActionHighlightPhrase(Action):
    def name(self) -> Text:
        return "action_highlight_phrase"

    def run(self, dispatcher: CollectingDispatcher, tracker: Tracker, domain: Dict[Text, Any]) -> List[Dict[Text, Any]]:
        phrase = tracker.get_slot("phrase")
        if not phrase:
            dispatcher.utter_message(text="Não encontrei a frase que pediu para sublinhar.")
            return []

        try:
            # Conectar ao WebSocket para enviar o comando
            ws = websocket.create_connection("ws://localhost:5000/")
            command = {"Intent": "highlight_phrase", "Phrase": phrase}
            print(f"Sending WebSocket payload: {command}")  # Debug log
            ws.send(json.dumps(command))
            ws.close()

            dispatcher.utter_message(text=f"Sublinhando a frase: {phrase}.")
        except Exception as e:
            print(f"Error in highlight phrase WebSocket: {e}")
            dispatcher.utter_message(text=f"Erro ao comunicar com o servidor: {e}")

        return []

class ActionZoomIn(Action):
    def name(self) -> Text:
        return "action_zoom_in"

    def run(self, dispatcher: CollectingDispatcher, tracker: Tracker, domain: Dict[Text, Any]) -> List[Dict[Text, Any]]:
        try:
            ws = websocket.create_connection("ws://localhost:5000/")
            command = {"Intent": "zoom_in"}
            ws.send(json.dumps(command))
            ws.close()

            dispatcher.utter_message(text="Zoom aumentado e focado no slide.")
        except Exception as e:
            dispatcher.utter_message(text=f"Erro ao comunicar com o servidor: {e}")

        return []

class ActionZoomOut(Action):
    def name(self) -> Text:
        return "action_zoom_out"

    def run(self, dispatcher: CollectingDispatcher, tracker: Tracker, domain: Dict[Text, Any]) -> List[Dict[Text, Any]]:
        try:
            ws = websocket.create_connection("ws://localhost:5000/")
            command = {"Intent": "zoom_out"}
            ws.send(json.dumps(command))
            ws.close()

            dispatcher.utter_message(text="Zoom reduzido no slide.")
        except Exception as e:
            dispatcher.utter_message(text=f"Erro ao comunicar com o servidor: {e}")

        return []

class ActionShowElapsedTime(Action):
    def name(self) -> Text:
        return "action_show_elapsed_time"

    def run(self, dispatcher: CollectingDispatcher, tracker: Tracker, domain: Dict[Text, Any]) -> List[Dict[Text, Any]]:
        try:
            # Conectar ao WebSocket para enviar o comando
            ws = websocket.create_connection("ws://localhost:5000/")
            command = {"Intent": "show_elapsed_time"}
            ws.send(json.dumps(command))
            
            # Receber a resposta
            response = ws.recv()
            ws.close()

            dispatcher.utter_message(text=response)
        except Exception as e:
            dispatcher.utter_message(text=f"Erro ao calcular o tempo decorrido: {e}")

        return []

class ActionPlayVideo(Action):
    def name(self) -> Text:
        return "action_play_video"

    def run(self, dispatcher: CollectingDispatcher, tracker: Tracker, domain: Dict) -> List[Dict]:
        try:
            ws = websocket.create_connection("ws://localhost:5000/")
            command = {"Intent": "play_video"}
            ws.send(json.dumps(command))
            ws.close()
            dispatcher.utter_message(text="Reproduzindo o vídeo.")
        except Exception as e:
            dispatcher.utter_message(text=f"Erro ao comunicar com o servidor: {e}")
        return []


class ActionPauseVideo(Action):
    def name(self) -> Text:
        return "action_pause_video"

    def run(self, dispatcher: CollectingDispatcher, tracker: Tracker, domain: Dict) -> List[Dict]:
        try:
            ws = websocket.create_connection("ws://localhost:5000/")
            command = {"Intent": "pause_video"}
            ws.send(json.dumps(command))
            ws.close()
            dispatcher.utter_message(text="Pausando o vídeo.")
        except Exception as e:
            dispatcher.utter_message(text=f"Erro ao comunicar com o servidor: {e}")
        return []


class ActionStopVideo(Action):
    def name(self) -> Text:
        return "action_stop_video"

    def run(self, dispatcher: CollectingDispatcher, tracker: Tracker, domain: Dict) -> List[Dict]:
        try:
            ws = websocket.create_connection("ws://localhost:5000/")
            command = {"Intent": "stop_video"}
            ws.send(json.dumps(command))
            ws.close()
            dispatcher.utter_message(text="Parando o vídeo.")
        except Exception as e:
            dispatcher.utter_message(text=f"Erro ao comunicar com o servidor: {e}")
        return []


class ActionFastForwardVideo(Action):
    def name(self) -> Text:
        return "action_fast_forward_video"

    def run(self, dispatcher: CollectingDispatcher, tracker: Tracker, domain: Dict) -> List[Dict]:
        try:
            ws = websocket.create_connection("ws://localhost:5000/")
            command = {"Intent": "fast_forward_video"}
            ws.send(json.dumps(command))
            ws.close()
            dispatcher.utter_message(text="Avançando 10 segundos no vídeo.")
        except Exception as e:
            dispatcher.utter_message(text=f"Erro ao comunicar com o servidor: {e}")
        return []


class ActionRewindVideo(Action):
    def name(self) -> Text:
        return "action_rewind_video"

    def run(self, dispatcher: CollectingDispatcher, tracker: Tracker, domain: Dict) -> List[Dict]:
        try:
            ws = websocket.create_connection("ws://localhost:5000/")
            command = {"Intent": "rewind_video"}
            ws.send(json.dumps(command))
            ws.close()
            dispatcher.utter_message(text="Retrocedendo 5 segundos no vídeo.")
        except Exception as e:
            dispatcher.utter_message(text=f"Erro ao comunicar com o servidor: {e}")
        return []


class ActionAdjustVolume(Action):
    def name(self) -> Text:
        return "action_adjust_volume"

    def run(self, dispatcher: CollectingDispatcher, tracker: Tracker, domain: Dict) -> List[Dict]:
        volume_direction = tracker.get_slot("volume_direction")
        if volume_direction not in ["increase", "decrease"]:
            dispatcher.utter_message(text="Direção de ajuste de volume não especificada.")
            return []
        try:
            ws = websocket.create_connection("ws://localhost:5000/")
            command = {"Intent": "adjust_volume", "VolumeDirection": volume_direction}
            ws.send(json.dumps(command))
            ws.close()
            if volume_direction == "increase":
                dispatcher.utter_message(text="Aumentando o volume do vídeo.")
            else:
                dispatcher.utter_message(text="Reduzindo o volume do vídeo.")
        except Exception as e:
            dispatcher.utter_message(text=f"Erro ao comunicar com o servidor: {e}")
        return []


class ActionJumpToTime(Action):
    def name(self) -> Text:
        return "action_jump_to_time"

    def run(self, dispatcher: CollectingDispatcher, tracker: Tracker, domain: Dict) -> List[Dict]:
        video_time = tracker.get_slot("video_time")
        if video_time is None:
            dispatcher.utter_message(text="Tempo do vídeo não especificado.")
            return []
        try:
            ws = websocket.create_connection("ws://localhost:5000/")
            command = {"Intent": "jump_to_time", "VideoTime": video_time}
            ws.send(json.dumps(command))
            ws.close()
            dispatcher.utter_message(text=f"Indo para {video_time} segundos no vídeo.")
        except Exception as e:
            dispatcher.utter_message(text=f"Erro ao comunicar com o servidor: {e}")
        return []


class ActionPlayFullscreen(Action):
    def name(self) -> Text:
        return "action_play_fullscreen"

    def run(self, dispatcher: CollectingDispatcher, tracker: Tracker, domain: Dict) -> List[Dict]:
        try:
            ws = websocket.create_connection("ws://localhost:5000/")
            command = {"Intent": "play_fullscreen"}
            ws.send(json.dumps(command))
            ws.close()
            dispatcher.utter_message(text="Reproduzindo o vídeo em tela cheia.")
        except Exception as e:
            dispatcher.utter_message(text=f"Erro ao comunicar com o servidor: {e}")
        return []

class ActionCurrentSlide(Action):
    def name(self) -> Text:
        return "action_current_slide"

    def run(self, dispatcher: CollectingDispatcher, tracker: Tracker, domain: Dict[Text, Any]) -> List[Dict[Text, Any]]:
        try:
            # Inicializa o PowerPoint
            pptApp = win32com.client.Dispatch("PowerPoint.Application")

            # Verifica se a apresentação está em execução
            if pptApp.SlideShowWindows.Count > 0:
                # Obtém o índice do slide atual
                current_slide = pptApp.SlideShowWindows(1).View.Slide.SlideIndex
                dispatcher.utter_message(text=f"Estás no slide número {current_slide}.")
            else:
                dispatcher.utter_message(text="Nenhuma apresentação está em execução.")
        except Exception as e:
            dispatcher.utter_message(text=f"Erro ao verificar o slide atual: {e}")
        return []

class ActionRestartPresentation(Action):
    def name(self) -> Text:
        return "action_restart_presentation"

    def run(self, dispatcher: CollectingDispatcher, tracker: Tracker, domain: Dict[Text, Any]) -> List[Dict[Text, Any]]:
        try:
            pptApp = win32com.client.Dispatch("PowerPoint.Application")
            if pptApp.SlideShowWindows.Count > 0:
                pptApp.SlideShowWindows(1).View.GotoSlide(1)
                dispatcher.utter_message(text="Apresentação reiniciada no primeiro slide.")
            else:
                dispatcher.utter_message(text="Nenhuma apresentação está em execução para reiniciar.")
        except Exception as e:
            dispatcher.utter_message(text=f"Erro ao reiniciar a apresentação: {e}")
        return []


class ActionStartTimer(Action):
    def name(self) -> Text:
        return "action_start_timer"

    def run(self, dispatcher: CollectingDispatcher, tracker: Tracker, domain: Dict[Text, Any]) -> List[Dict[Text, Any]]:
        global timer_start
        timer_start = time.time()
        dispatcher.utter_message(text="Temporizador iniciado.")
        return []


class ActionStopTimer(Action):
    def name(self) -> Text:
        return "action_stop_timer"

    def run(self, dispatcher: CollectingDispatcher, tracker: Tracker, domain: Dict[Text, Any]) -> List[Dict[Text, Any]]:
        global timer_start
        if timer_start is None:
            dispatcher.utter_message(text="Nenhum temporizador está ativo.")
        else:
            elapsed_time = time.time() - timer_start
            minutes, seconds = divmod(int(elapsed_time), 60)
            dispatcher.utter_message(text=f"Temporizador parado. Tempo decorrido: {minutes} minutos e {seconds} segundos.")
            timer_start = None
        return []

class ActionHelper(Action):
    def name(self) -> Text:
        return "action_helper"

    def run(self, dispatcher: CollectingDispatcher, tracker: Tracker, domain: Dict[Text, Any]) -> List[Dict[Text, Any]]:
        helper_message = (
            "Aqui estão os comandos que pode usar:\n"
            "- 'Próximo slide' para avançar para o próximo slide.\n"
            "- 'Slide anterior' para voltar ao slide anterior.\n"
            "- 'Reinicia a apresentação' para reiniciar desde o primeiro slide.\n"
            "- 'Em que slide estou?' para verificar o slide atual.\n"
            "- 'Inicia o temporizador' para começar a cronometrar.\n"
            "- 'Para o temporizador' para parar o temporizador.\n"
            "- 'Zoom in' para ampliar no slide.\n"
            "- 'Zoom out' para reduzir no slide.\n"
            "- 'Sublinhar frase [frase]' para destacar uma frase no slide.\n"
            "- 'Mostra tempo decorrido' para exibir o tempo desde o início da apresentação."
        )
        dispatcher.utter_message(text=helper_message)
        return []

class ActionDynamicGreet(Action):
    def name(self) -> Text:
        return "action_dynamic_greet"

    def run(self, dispatcher: CollectingDispatcher, tracker: Tracker, domain: Dict[Text, Any]) -> List[Dict[Text, Any]]:
        time_of_day = get_time_of_day()
        dispatcher.utter_message(text=f"Bom {time_of_day}! Como posso ajudar hoje?")
        return []

def get_time_of_day():
    from datetime import datetime
    current_hour = datetime.now().hour
    if current_hour < 12:
        return "dia"
    elif current_hour < 18:
        return "tarde"
    else:
        return "noite"

class ActionDebugVoice(Action):
    def name(self) -> str:
        return "action_debug_voice"

    def run(self, dispatcher: CollectingDispatcher, tracker: Tracker, domain: Dict) -> List[Dict]:
        user_message = tracker.latest_message.get("text", "")
        dispatcher.utter_message(text=f"You said: {user_message}")
        return []


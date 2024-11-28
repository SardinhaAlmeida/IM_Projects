from typing import Any, Text, Dict, List
from rasa_sdk import Action, Tracker
from rasa_sdk.executor import CollectingDispatcher
import win32com.client

import requests
import websocket
import json 

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
        print(f"Slide Title received: {slide_title}")
        if slide_title:
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


class ActionDebugVoice(Action):
    def name(self) -> str:
        return "action_debug_voice"

    def run(self, dispatcher: CollectingDispatcher, tracker: Tracker, domain: Dict) -> List[Dict]:
        user_message = tracker.latest_message.get("text", "")
        dispatcher.utter_message(text=f"You said: {user_message}")
        return []


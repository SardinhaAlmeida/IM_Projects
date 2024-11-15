from typing import Any, Text, Dict, List
from rasa_sdk import Action, Tracker
from rasa_sdk.executor import CollectingDispatcher
import win32com.client

import requests

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

class ActionDebugVoice(Action):
    def name(self) -> str:
        return "action_debug_voice"

    def run(self, dispatcher: CollectingDispatcher, tracker: Tracker, domain: Dict) -> List[Dict]:
        user_message = tracker.latest_message.get("text", "")
        dispatcher.utter_message(text=f"You said: {user_message}")
        return []


/* 
  *   Speech.java generated by speechmod 
 */   

package Modalities; 

import scxmlgen.interfaces.IModality; 

public enum Speech implements IModality{  

	// Redundância
    NEXT_SLIDE("[SPEECH][NEXT_SLIDE]", 2000), // Comando de voz para próximo slide
    PREVIOUS_SLIDE("[SPEECH][PREVIOUS_SLIDE]", 2000), // Comando de voz para slide anterior

    // Complementaridade
    ELAPSED_TIME("[SPEECH][ELAPSED_TIME]", 2000), // Saber quanto tempo decorreu
    HELPER("[SPEECH][HELPER]", 2000), // Comando de voz para pedir ajuda

    // Funcionalidades adicionais
    GO_TO_SLIDE_TITLE("[SPEECH][GO_TO_SLIDE_TITLE]", 2000), // Ir para o slide pelo título
    //GO_TO_SLIDE_NUMBER("[SPEECH][GO_TO_SLIDE_NUMBER]", 2000), // Ir para o slide pelo número
    HIGHLIGHT_PHRASE("[SPEECH][HIGHLIGHT_PHRASE]", 2000), // Destacar uma frase
    ZOOM_IN("[SPEECH][ZOOM_IN]", 2000), // Zoom in
    ZOOM_OUT("[SPEECH][ZOOM_OUT]", 2000), // Zoom out
    CURRENT_SLIDE("[SPEECH][CURRENT_SLIDE]", 2000), // Saber qual é o slide atual
    SLIDES_LEFT("[SPEECH][SLIDES_LEFT]", 2000), // Quantos slides faltam para acabar
    RESTART_PRESENTATION("[SPEECH][RESTART_PRESENTATION]", 2000), // Recomeçar apresentação
    START_TIMER("[SPEECH][START_TIMER]", 2000), // Iniciar temporizador
    STOP_TIMER("[SPEECH][STOP_TIMER]", 2000); // Parar temporizador

	//CHANGE_COLOR_AZUL("[SPEECH][CHANGE_COLOR][AZUL]",1500),
	//CHANGE_COLOR_VERDE("[SPEECH][CHANGE_COLOR][VERDE]",1500),
	//CHANGE_COLOR_CINZENTO("[SPEECH][CHANGE_COLOR][CINZENTO]",1500),
	//CHANGE_COLOR_VERMELHO("[SPEECH][CHANGE_COLOR][VERMELHO]",1500),
	//CHANGE_COLOR_BRANCO("[SPEECH][CHANGE_COLOR][BRANC]",1500),
	//CHANGE_COLOR_ROSA("[SPEECH][CHANGE_COLOR][ROSA]",1500),
	//CHANGE_COLOR_AMARELO("[SPEECH][CHANGE_COLOR][AMARELO]",1500),
	//CHANGE_COLOR_PRETO("[SPEECH][CHANGE_COLOR][PRETO]",1500),
	//CHANGE_COLOR_LARANJA("[SPEECH][CHANGE_COLOR][LARANJA]",1500),

	;


private String event; 
private int timeout;
Speech(String m, int time) {
	event=m;
	timeout=time;
}
@Override
public int getTimeOut(){
	return timeout;
}
@Override
public String getEventName(){
	return event;
}
@Override
public String getEvName(){
	return getModalityName().toLowerCase() +event.toLowerCase();
}

}

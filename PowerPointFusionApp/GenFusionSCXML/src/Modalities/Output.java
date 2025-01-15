package Modalities;

import scxmlgen.interfaces.IOutput;

public enum Output implements IOutput{
    
    // Redundância
    NEXT_SLIDE("[FUSION][NEXT_SLIDE]"), // Próximo slide
    PREVIOUS_SLIDE("[FUSION][PREVIOUS_SLIDE]"), // Slide anterior

    // Complementaridade
    ELAPSED_TIME("[FUSION][ELAPSED_TIME]"), // Saber quanto tempo decorreu
    HELPER("[FUSION][HELPER]"), // Pedir ajuda

    // Ações únicas (Single Gesture e Single Speech)
    START_PRESENTATION("[FUSION][START_PRESENTATION]"), // Iniciar apresentação
    END_PRESENTATION("[FUSION][END_PRESENTATION]"), // Terminar apresentação
    REQUEST_SILENCE("[FUSION][REQUEST_SILENCE]"), // Pedir silêncio à audiência
    QUESTIONS("[FUSION][QUESTIONS]"), // Abrir para questões
    GO_TO_SLIDE_TITLE("[FUSION][GO_TO_SLIDE_TITLE]"), // Ir para o slide pelo título
    HIGHLIGHT_PHRASE("[FUSION][HIGHLIGHT_PHRASE]"), // Destacar uma frase
    ZOOM_IN("[FUSION][ZOOM_IN]"), // Zoom in
    ZOOM_OUT("[FUSION][ZOOM_OUT]"), // Zoom out
    CURRENT_SLIDE("[FUSION][CURRENT_SLIDE]"), // Saber o slide atual
    SLIDES_LEFT("[FUSION][SLIDES_LEFT]"), // Quantos slides faltam
    RESTART_PRESENTATION("[FUSION][RESTART_PRESENTATION]"), // Recomeçar apresentação
    START_TIMER("[FUSION][START_TIMER]"), // Iniciar temporizador
    STOP_TIMER("[FUSION][STOP_TIMER]"); // Parar temporizador


    //CHANGE_COLOR_TRIANGULO_AZUL("[FUSION][CHANGE_COLOR][TRIANGULO][AZUL]"),
    //CHANGE_COLOR_TRIANGULO_VERDE("[FUSION][CHANGE_COLOR][TRIANGULO][VERDE]"),
    //CHANGE_COLOR_TRIANGULO_CINZENTO("[FUSION][CHANGE_COLOR][TRIANGULO][CINZENTO]"),
    //CHANGE_COLOR_TRIANGULO_VERMELHO("[FUSION][CHANGE_COLOR][TRIANGULO][VERMELHO]"),
    //CHANGE_COLOR_TRIANGULO_BRANCO("[FUSION][CHANGE_COLOR][TRIANGULO][BRANCO]"),
    //CHANGE_COLOR_TRIANGULO_ROSA("[FUSION][CHANGE_COLOR][TRIANGULO][ROSA]"),
    //CHANGE_COLOR_TRIANGULO_AMARELO("[FUSION][CHANGE_COLOR][TRIANGULO][AMARELO]"),
    //CHANGE_COLOR_TRIANGULO_PRETO("[FUSION][CHANGE_COLOR][TRIANGULO][PRETO]"),
    //CHANGE_COLOR_TRIANGULO_LARANJA("[FUSION][CHANGE_COLOR][TRIANGULO][LARANJA]"),

    //CHANGE_COLOR_QUADRADO_AZUL("[FUSION][CHANGE_COLOR][QUADRADO][AZUL]"),
    //CHANGE_COLOR_QUADRADO_VERDE("[FUSION][CHANGE_COLOR][QUADRADO][VERDE]"),
    //CHANGE_COLOR_QUADRADO_CINZENTO("[FUSION][CHANGE_COLOR][QUADRADO][CINZENTO]"),
    //CHANGE_COLOR_QUADRADO_VERMELHO("[FUSION][CHANGE_COLOR][QUADRADO][VERMELHO]"),
    //CHANGE_COLOR_QUADRADO_BRANCO("[FUSION][CHANGE_COLOR][QUADRADO][BRANCO]"),
    //CHANGE_COLOR_QUADRADO_ROSA("[FUSION][CHANGE_COLOR][QUADRADO][ROSA]"),
    //CHANGE_COLOR_QUADRADO_AMARELO("[FUSION][CHANGE_COLOR][QUADRADO][AMARELO]"),
    //CHANGE_COLOR_QUADRADO_PRETO("[FUSION][CHANGE_COLOR][QUADRADO][PRETO]"),
    //CHANGE_COLOR_QUADRADO_LARANJA("[FUSION][CHANGE_COLOR][QUADRADO][LARANJA]"),

    //CHANGE_COLOR_CIRCULO_AZUL("[FUSION][CHANGE_COLOR][CIRCULO][AZUL]"),
    //CHANGE_COLOR_CIRCULO_VERDE("[FUSION][CHANGE_COLOR][CIRCULO][VERDE]"),
    //CHANGE_COLOR_CIRCULO_CINZENTO("[FUSION][CHANGE_COLOR][CIRCULO][CINZENTO]"),
    //CHANGE_COLOR_CIRCULO_VERMELHO("[FUSION][CHANGE_COLOR][CIRCULO][VERMELHO]"),
    //CHANGE_COLOR_CIRCULO_BRANCO("[FUSION][CHANGE_COLOR][CIRCULO][BRANCO]"),
    //CHANGE_COLOR_CIRCULO_ROSA("[FUSION][CHANGE_COLOR][CIRCULO][ROSA]"),
    //CHANGE_COLOR_CIRCULO_AMARELO("[FUSION][CHANGE_COLOR][CIRCULO][AMARELO]"),
    //CHANGE_COLOR_CIRCULO_PRETO("[FUSION][CHANGE_COLOR][CIRCULO][PRETO]"),
    //CHANGE_COLOR_CIRCULO_LARANJA("[FUSION][CHANGE_COLOR][CIRCULO][LARANJA]"),




    ;
    
    
    
    private String event;

    Output(String m) {
        event=m;
    }
    
    public String getEvent(){
        return this.toString();
    }

    public String getEventName(){
        return event;
    }
}

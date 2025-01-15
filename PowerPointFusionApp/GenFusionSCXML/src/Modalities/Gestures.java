/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package Modalities;

import scxmlgen.interfaces.IModality;

/**
 *
 * @author nunof
 */
public enum Gestures implements IModality{

    // SHAPE_TRIANGULO("[TOUCH][SHAPE][TRIANGULO]",1500),
    // SHAPE_QUADRADO("[TOUCH][SHAPE][QUADRADO]",1500),
    // SHAPE_CIRCULO("[TOUCH][SHAPE][CIRCULO]",1500),

    NEXT_SLIDE("[GESTURES][NEXT_SLIDE]", 2000), // Gesto para próximo slide
    PREVIOUS_SLIDE("[GESTURES][PREVIOUS_SLIDE]", 2000), // Gesto para slide anterior
    START_PRESENTATION("[GESTURES][START_PRESENTATION]", 2000), // Gesto para iniciar apresentação
    END_PRESENTATION("[GESTURES][END_PRESENTATION]", 2000), // Gesto para encerrar apresentação
    ELAPSED_TIME("[GESTURES][ELAPSED_TIME]", 2000), // Gesto para saber tempo decorrido
    REQUEST_SILENCE("[GESTURES][REQUEST_SILENCE]", 2000), // Gesto para pedir silêncio
    QUESTIONS("[GESTURES][QUESTIONS]", 2000), // Gesto para abrir para questões
    HELPER("[GESTURES][HELPER]", 2000), // Gesto de ajuda
  
    ;
    
    private String event;
    private int timeout;


    Gestures(String m, int time) {
        event=m;
        timeout=time;
    }

    @Override
    public int getTimeOut() {
        return timeout;
    }

    @Override
    public String getEventName() {
        //return getModalityName()+"."+event;
        return event;
    }

    @Override
    public String getEvName() {
        return getModalityName().toLowerCase()+event.toLowerCase();
    }
    
}
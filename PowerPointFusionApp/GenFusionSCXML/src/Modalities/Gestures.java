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

    NEXT_SLIDE("[GESTURES][nextslide]", 5000), // Gesto para próximo slide
    PREVIOUS_SLIDE("[GESTURES][previous_slide]", 5000), // Gesto para slide anterior
    START_PRESENTATION("[GESTURES][start]", 5000), // Gesto para iniciar apresentação
    END_PRESENTATION("[GESTURES][stop]", 5000), // Gesto para encerrar apresentação
    ELAPSED_TIME("[GESTURES][timer]", 5000), // Gesto para saber tempo decorrido
    REQUEST_SILENCE("[GESTURES][silence]", 5000), // Gesto para pedir silêncio
    QUESTIONS("[GESTURES][questions]", 5000), // Gesto para abrir para questões
    HELPER("[GESTURES][helper]", 5000), // Gesto de ajuda
  
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
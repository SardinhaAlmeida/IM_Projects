/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */

import java.io.IOException;
import scxmlgen.Fusion.FusionGenerator;
//import FusionGenerator;

import Modalities.Output;
import Modalities.Speech;
import Modalities.Touch;

/**
 *
 * @author nunof
 */
public class GenFusionSCXML {

    /**
     * @param args the command line arguments
     */
    public static void main(String[] args) throws IOException {

    FusionGenerator fg = new FusionGenerator();

    // Redundância
    // Próximo slide: pode ser ativado por gesto ou comando de voz
    fg.Redundancy(Speech.NEXT_SLIDE, Touch.NEXT_SLIDE, Output.NEXT_SLIDE);

    // Slide anterior: pode ser ativado por gesto ou comando de voz
    fg.Redundancy(Speech.PREVIOUS_SLIDE, Touch.PREVIOUS_SLIDE, Output.PREVIOUS_SLIDE);

    // Complementaridade
    // Saber quanto tempo decorreu: precisa de gesto + comando de voz
    fg.Complementary(Speech.ELAPSED_TIME, Touch.ELAPSED_TIME, Output.ELAPSED_TIME);

    // Gesto de ajuda: precisa de gesto + comando de voz
    fg.Complementary(Speech.HELPER, Touch.HELPER, Output.HELPER);

    // Ações únicas (Single Speech)
    fg.Single(Speech.GO_TO_SLIDE_TITLE, Output.GO_TO_SLIDE_TITLE);       // Ir para o slide pelo título
    fg.Single(Speech.GO_TO_SLIDE_NUMBER, Output.GO_TO_SLIDE_NUMBER);     // Ir para o slide pelo número
    fg.Single(Speech.HIGHLIGHT_PHRASE, Output.HIGHLIGHT_PHRASE);         // Destacar uma frase
    fg.Single(Speech.ZOOM_IN, Output.ZOOM_IN);                           // Zoom in
    fg.Single(Speech.ZOOM_OUT, Output.ZOOM_OUT);                         // Zoom out
    fg.Single(Speech.CURRENT_SLIDE, Output.CURRENT_SLIDE);               // Slide atual
    fg.Single(Speech.SLIDES_LEFT, Output.SLIDES_LEFT);                   // Slides restantes
    fg.Single(Speech.RESTART_PRESENTATION, Output.RESTART_PRESENTATION); // Recomeçar apresentação
    fg.Single(Speech.START_TIMER, Output.START_TIMER);                   // Iniciar temporizador
    fg.Single(Speech.STOP_TIMER, Output.STOP_TIMER);                     // Parar temporizador

    // Funcionalidades adicionais (somente gestos)
    fg.Single(Touch.START_PRESENTATION, Output.START_PRESENTATION); // Iniciar apresentação
    fg.Single(Touch.END_PRESENTATION, Output.END_PRESENTATION);     // Encerrar apresentação
    fg.Single(Touch.REQUEST_SILENCE, Output.REQUEST_SILENCE);       // Pedir silêncio
    fg.Single(Touch.QUESTIONS, Output.QUESTIONS);                   // Abrir para questões
  

    //fg.Complementary(Speech.CHANGE_COLOR_AZUL, Touch.SHAPE_TRIANGULO, Output.CHANGE_COLOR_TRIANGULO_AZUL);
    //fg.Complementary(Speech.CHANGE_COLOR_VERDE, Touch.SHAPE_TRIANGULO, Output.CHANGE_COLOR_TRIANGULO_VERDE);
    //fg.Complementary(Speech.CHANGE_COLOR_CINZENTO, Touch.SHAPE_TRIANGULO, Output.CHANGE_COLOR_TRIANGULO_CINZENTO);
    //fg.Complementary(Speech.CHANGE_COLOR_VERMELHO, Touch.SHAPE_TRIANGULO, Output.CHANGE_COLOR_TRIANGULO_VERMELHO);
    //fg.Complementary(Speech.CHANGE_COLOR_BRANCO, Touch.SHAPE_TRIANGULO, Output.CHANGE_COLOR_TRIANGULO_BRANCO);
    //fg.Complementary(Speech.CHANGE_COLOR_ROSA, Touch.SHAPE_TRIANGULO, Output.CHANGE_COLOR_TRIANGULO_ROSA);
    //fg.Complementary(Speech.CHANGE_COLOR_AMARELO, Touch.SHAPE_TRIANGULO, Output.CHANGE_COLOR_TRIANGULO_AMARELO);
    //fg.Complementary(Speech.CHANGE_COLOR_PRETO, Touch.SHAPE_TRIANGULO, Output.CHANGE_COLOR_TRIANGULO_PRETO);
    //fg.Complementary(Speech.CHANGE_COLOR_LARANJA, Touch.SHAPE_TRIANGULO, Output.CHANGE_COLOR_TRIANGULO_LARANJA);

    //fg.Complementary(Speech.CHANGE_COLOR_AZUL, Touch.SHAPE_QUADRADO, Output.CHANGE_COLOR_QUADRADO_AZUL);
    //fg.Complementary(Speech.CHANGE_COLOR_VERDE, Touch.SHAPE_QUADRADO, Output.CHANGE_COLOR_QUADRADO_VERDE);
    //fg.Complementary(Speech.CHANGE_COLOR_CINZENTO, Touch.SHAPE_QUADRADO, Output.CHANGE_COLOR_QUADRADO_CINZENTO);
    //fg.Complementary(Speech.CHANGE_COLOR_VERMELHO, Touch.SHAPE_QUADRADO, Output.CHANGE_COLOR_QUADRADO_VERMELHO);
    //fg.Complementary(Speech.CHANGE_COLOR_BRANCO, Touch.SHAPE_QUADRADO, Output.CHANGE_COLOR_QUADRADO_BRANCO);
    //fg.Complementary(Speech.CHANGE_COLOR_ROSA, Touch.SHAPE_QUADRADO, Output.CHANGE_COLOR_QUADRADO_ROSA);
    //fg.Complementary(Speech.CHANGE_COLOR_AMARELO, Touch.SHAPE_QUADRADO, Output.CHANGE_COLOR_QUADRADO_AMARELO);
    //fg.Complementary(Speech.CHANGE_COLOR_PRETO, Touch.SHAPE_QUADRADO, Output.CHANGE_COLOR_QUADRADO_PRETO);
    //fg.Complementary(Speech.CHANGE_COLOR_LARANJA, Touch.SHAPE_QUADRADO, Output.CHANGE_COLOR_QUADRADO_LARANJA);

    //fg.Complementary(Speech.CHANGE_COLOR_AZUL, Touch.SHAPE_CIRCULO, Output.CHANGE_COLOR_CIRCULO_AZUL);
    //fg.Complementary(Speech.CHANGE_COLOR_VERDE, Touch.SHAPE_CIRCULO, Output.CHANGE_COLOR_CIRCULO_VERDE);
    //fg.Complementary(Speech.CHANGE_COLOR_CINZENTO, Touch.SHAPE_CIRCULO, Output.CHANGE_COLOR_CIRCULO_CINZENTO);
    //fg.Complementary(Speech.CHANGE_COLOR_VERMELHO, Touch.SHAPE_CIRCULO, Output.CHANGE_COLOR_CIRCULO_VERMELHO);
    //fg.Complementary(Speech.CHANGE_COLOR_BRANCO, Touch.SHAPE_CIRCULO, Output.CHANGE_COLOR_CIRCULO_BRANCO);
    //fg.Complementary(Speech.CHANGE_COLOR_ROSA, Touch.SHAPE_CIRCULO, Output.CHANGE_COLOR_CIRCULO_ROSA);
    //fg.Complementary(Speech.CHANGE_COLOR_AMARELO, Touch.SHAPE_CIRCULO, Output.CHANGE_COLOR_CIRCULO_AMARELO);
    //fg.Complementary(Speech.CHANGE_COLOR_PRETO, Touch.SHAPE_CIRCULO, Output.CHANGE_COLOR_CIRCULO_PRETO);
    //fg.Complementary(Speech.CHANGE_COLOR_LARANJA, Touch.SHAPE_CIRCULO, Output.CHANGE_COLOR_CIRCULO_LARANJA);

    





  /*  
    fg.Sequence(Speech.SQUARE, SecondMod.RED, Output.SQUARE_RED);
    fg.Sequence(Speech.SQUARE, SecondMod.BLUE, Output.SQUARE_BLUE);
    fg.Sequence(Speech.SQUARE, SecondMod.YELLOW, Output.SQUARE_YELLOW);
    fg.Sequence(Speech.TRIANGLE, SecondMod.RED, Output.TRIANGLE_RED);
    fg.Sequence(Speech.TRIANGLE, SecondMod.BLUE, Output.TRIANGLE_BLUE);
    fg.Sequence(Speech.TRIANGLE, SecondMod.YELLOW, Output.TRIANGLE_YELLOW);
    fg.Redundancy(Speech.CIRCLE, SecondMod.RED, Output.CIRCLE_RED);
    fg.Redundancy(Speech.CIRCLE, SecondMod.BLUE, Output.CIRCLE_BLUE);
    fg.Redundancy(Speech.CIRCLE, SecondMod.YELLOW, Output.CIRCLE_YELLOW);
    
    fg.Single(Speech.CIRCLE, Output.CIRCLE);
    
    
    fg.Redundancy(Speech.OPEN_SOCIAL, SecondMod.RED, Output.OPEN_SOCIAL);
    fg.Single(Speech.OPEN_SOCIAL, Output.OPEN_SOCIAL);
   
  
    fg.Redundancy(Speech.OPEN_SOCIAL, SecondMod.SOCIAL, Output.OPEN_SOCIAL);
  
    fg.Redundancy(Speech.OPEN_LIXO, SecondMod.LIXO, Output.OPEN_LIXO);
    fg.Single(Speech.OPEN_LIXO, Output.OPEN_LIXO);
    
    
    fg.Build("fusion.scxml");
       
  

    fg.Complementary(Speech.LIGHT_ON, Touch.LOCATION_LIVINGROOM, Output.LIGHT_LIVINGROOM_ON);
    fg.Complementary(Speech.LIGHT_ON, Touch.LOCATION_ROOM, Output.LIGHT_ROOM_ON);
    fg.Complementary(Speech.LIGHT_ON, Touch.LOCATION_KITCHEN, Output.LIGHT_KITCHEN_ON);
    fg.Complementary(Speech.LIGHT_OFF, Touch.LOCATION_LIVINGROOM, Output.LIGHT_LIVINGROOM_OFF);
    fg.Complementary(Speech.LIGHT_OFF, Touch.LOCATION_ROOM, Output.LIGHT_ROOM_OFF);
    fg.Complementary(Speech.LIGHT_OFF, Touch.LOCATION_KITCHEN, Output.LIGHT_KITCHEN_OFF);  
    
    fg.Complementary(Touch.LOCATION_LIVINGROOM, Speech.LIGHT_ON, Output.LIGHT_LIVINGROOM_ON);
    fg.Complementary(Touch.LOCATION_ROOM, Speech.LIGHT_ON, Output.LIGHT_ROOM_ON);
    fg.Complementary(Touch.LOCATION_KITCHEN, Speech.LIGHT_ON, Output.LIGHT_KITCHEN_ON);
    fg.Complementary(Touch.LOCATION_LIVINGROOM, Speech.LIGHT_OFF, Output.LIGHT_LIVINGROOM_OFF);
    fg.Complementary(Touch.LOCATION_ROOM, Speech.LIGHT_OFF, Output.LIGHT_ROOM_OFF);
    fg.Complementary(Touch.LOCATION_KITCHEN, Speech.LIGHT_OFF, Output.LIGHT_KITCHEN_OFF);  
    
    //
    fg.Complementary(Speech.TEMPERATURE_UP, Touch.LOCATION_LIVINGROOM, Output.TEMP_LIVINGROOM_UP);
    fg.Complementary(Speech.TEMPERATURE_UP, Touch.LOCATION_ROOM, Output.TEMP_ROOM_UP);
    fg.Complementary(Speech.TEMPERATURE_UP, Touch.LOCATION_KITCHEN, Output.TEMP_KITCHEN_UP);
    fg.Complementary(Speech.TEMPERATURE_DOWN, Touch.LOCATION_LIVINGROOM, Output.TEMP_LIVINGROOM_DOWN);
    fg.Complementary(Speech.TEMPERATURE_DOWN, Touch.LOCATION_ROOM, Output.TEMP_ROOM_DOWN);
    fg.Complementary(Speech.TEMPERATURE_DOWN, Touch.LOCATION_KITCHEN, Output.TEMP_KITCHEN_DOWN);  
    
    fg.Complementary(Touch.LOCATION_LIVINGROOM, Speech.TEMPERATURE_UP, Output.TEMP_LIVINGROOM_UP);
    fg.Complementary(Touch.LOCATION_ROOM, Speech.TEMPERATURE_UP, Output.TEMP_ROOM_UP);
    fg.Complementary(Touch.LOCATION_KITCHEN, Speech.TEMPERATURE_UP, Output.TEMP_KITCHEN_UP);
    fg.Complementary(Touch.LOCATION_LIVINGROOM, Speech.TEMPERATURE_DOWN, Output.TEMP_LIVINGROOM_DOWN);
    fg.Complementary(Touch.LOCATION_ROOM, Speech.TEMPERATURE_DOWN, Output.TEMP_ROOM_DOWN);
    fg.Complementary(Touch.LOCATION_KITCHEN, Speech.TEMPERATURE_DOWN, Output.TEMP_KITCHEN_DOWN); 
    

    fg.Single(Speech.LIGHT_ON, Output.LIGHT_ON);
    fg.Single(Speech.LIGHT_OFF, Output.LIGHT_OFF);
    fg.Single(Touch.LOCATION_LIVINGROOM, Output.LOCATION_LIVINGROOM);
    fg.Single(Touch.LOCATION_ROOM, Output.LOCATION_ROOM);
    fg.Single(Touch.LOCATION_KITCHEN, Output.LOCATION_KITCHEN);
    
    fg.Single(Speech.TEMPERATURE_UP, Output.TEMP_UP);
    fg.Single(Speech.TEMPERATURE_DOWN, Output.TEMP_DOWN);
    */
    
    
   //fg.Complementary(Touch.OPEN_NEWS_TITLE, Speech.ACTION_NEWS_NIMAGE, Output.OPEN_NEWS_AS_IMAGE);
   // fg.Complementary(Speech.ACTION_NEWS_NTEXT,Touch.OPEN_NEWS_TITLE, Output.OPEN_NEWS_AS_TEXT);
   // fg.Complementary(Speech.ACTION_NEWS_NIMAGE,Touch.OPEN_NEWS_TITLE, Output.OPEN_NEWS_AS_IMAGE);
   // fg.Single(Touch.OPEN_NEWS_TITLE, Output.OPEN_NEWS_AS_TEXT);
    
   // fg.Redundancy(Touch.GO_BACK, Speech.ACTION_GENERICENTITY_BACK, Output.GO_BACK);
    fg.Build("fusion_novo.scxml");
    System.out.println("Ficheiro SCXML gerado com sucesso!");
        
    }
    
}

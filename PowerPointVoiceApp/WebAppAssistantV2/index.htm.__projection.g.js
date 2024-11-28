/* BEGIN EXTERNAL SOURCE */


        const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;
        const recognition = new SpeechRecognition();
        recognition.continuous = false;
        recognition.interimResults = false;
        recognition.lang = 'pt-PT';

        const testUtterance = new SpeechSynthesisUtterance("Teste de síntese de fala");
        testUtterance.lang = 'pt-PT';
        window.speechSynthesis.speak(testUtterance);


        const statusDiv = document.getElementById('status');
        const responseDiv = document.getElementById('response');
        const micCircle = document.querySelector('#mic circle');

        function log(message) {
            console.log(`[${new Date().toLocaleTimeString()}] ${message}`);
        }


        //recognition.onresult = async (event) => {

        //    if (isSpeaking || cooldown) {
        //        log('Ignorando comando devido ao cooldown ou fala em andamento.');
        //        return;
        //    }

        //    const command = event.results[event.results.length - 1][0].transcript.trim().toLowerCase();
        //    log(`Comando reconhecido: ${command}`);
        //    statusDiv.textContent = `Comando reconhecido: ${command}`;

        //    cooldown = true;
        //    setTimeout(() => cooldown = false, 3000);

        //    try {
        //        const response = await fetch('http://localhost:5005/model/parse', {
        //            method: 'POST',
        //            headers: { 'Content-Type': 'application/json' },
        //            body: JSON.stringify({ text: command }),
        //        });

        //        if (response.ok) {
        //            const data = await response.json();
        //            const intent = data.intent.name;
        //            const confidence = data.intent.confidence;
        //            log(`Intent: ${intent}, Confiança: ${confidence}`);

        //            if (intent === 'fallback' || confidence < 0.6) {
        //                responseDiv.textContent = 'Comando não reconhecido. Tente novamente.';
        //                speak('Desculpe, não percebi o que disse. Pode repetir?');
        //                log('Fallback acionado.');
        //            } else {
        //                switch (intent) {
        //                    case 'next_slide':
        //                        sendVoiceCommand('next_slide', 'Passando para o próximo slide.');
        //                        break;
        //                    case 'previous_slide':
        //                        sendVoiceCommand('previous_slide', 'Voltando ao slide anterior.');
        //                        break;
        //                    default:
        //                        responseDiv.textContent = 'Comando não reconhecido.';
        //                        log('Intent não reconhecida.');
        //                        break;
        //                }
        //            }
        //        } else {
        //            responseDiv.textContent = 'Erro ao processar o comando.';
        //            log('Erro no servidor RASA.');
        //        }
        //    } catch (error) {
        //        log(`Erro na comunicação com o RASA: ${error}`);
        //        responseDiv.textContent = 'Erro na comunicação com o servidor.';
        //    }
        //};


        //function sendVoiceCommand(intent, feedback) {
        //    fetch('http://localhost:5000/api/voice-command/', {
        //        method: 'POST',
        //        headers: { 'Content-Type': 'application/json' },
        //        body: JSON.stringify({ Intent: intent }),
        //    }).then(() => {
        //        responseDiv.textContent = feedback;
        //        speak(feedback);
        //        log(`Comando enviado: ${intent} - ${feedback}`);
        //    }).catch(error => {
        //        log(`Erro ao enviar comando: ${error}`);
        //    });
        //}

        let isSpeaking = false;
        let cooldown = false;

        recognition.onstart = () => {
            console.log('Reconhecimento de voz iniciado.');
            statusDiv.textContent = 'Ouvindo comandos...';
            micCircle.setAttribute('fill', '#00e676');
        };

        recognition.onresult = async (event) => {
            if (isSpeaking || cooldown) {
                console.log('Ignorando comando devido ao cooldown ou fala em andamento.');
                return;
            }

            const command = event.results[event.results.length - 1][0].transcript.trim().toLowerCase();
            console.log(`[${new Date().toLocaleTimeString()}] Comando reconhecido: ${command}`);
            statusDiv.textContent = `Comando reconhecido: ${command}`;

            cooldown = true;
            setTimeout(() => (cooldown = false), 2000); // Reduzir cooldown para evitar bloqueios

            try {
                const response = await fetch('http://localhost:5005/model/parse', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ text: command }),
                });

                if (response.ok) {
                    const data = await response.json();
                    const intent = data.intent.name;
                    const confidence = data.intent.confidence;
                    console.log(`[${new Date().toLocaleTimeString()}] Intent: ${intent}, Confiança: ${confidence}`);

                    if (confidence < 0.4) {
                        console.log('Confiança muito baixa. Comando ignorado.');
                        return; 
                    }

                    if (intent === 'fallback' || confidence < 0.6) {
                        responseDiv.textContent = 'Comando não reconhecido. Tente novamente.';
                        speak('Desculpe, não percebi o que disse. Pode repetir?');
                    } else {
                        handleIntent(intent, data.entities || []);
                    }
                } else {
                    console.error('Erro no servidor NLU.');
                    responseDiv.textContent = 'Erro ao processar o comando.';
                }
            } catch (error) {
                console.error(`Erro na comunicação com o servidor NLU: ${error}`);
                responseDiv.textContent = 'Erro na comunicação com o servidor.';
            }
        };

        function handleIntent(intent, entities) {
            switch (intent) {
                case 'next_slide':
                    sendVoiceCommand('next_slide', 'Passando para o próximo slide.');
                    break;
                case 'previous_slide':
                    sendVoiceCommand('previous_slide', 'Voltando ao slide anterior.');
                    break;
                case 'jump_to_slide_by_title':
                    const slideTitle = entities.find((e) => e.entity === 'slide_title')?.value || '';
                    if (slideTitle) {
                        sendVoiceCommand('jump_to_slide_by_title', `Indo para o slide: ${slideTitle}`, { SlideTitle: slideTitle });
                    } else {
                        responseDiv.textContent = 'Título do slide não fornecido.';
                    }
                    break;
                case 'jump_to_slide_by_number':
                    const slideNumberEntity = entities.find((e) => e.entity === 'slide_number')?.value || '';
                    const slideNumber = parseInt(slideNumberEntity, 10);
                    if (slideNumber) {
                        sendVoiceCommand('jump_to_slide_by_number', `Indo para o slide: ${slideNumber}`, { SlideNumber: `${slideNumber}` });
                    } else {
                        responseDiv.textContent = 'Número do slide não fornecido.';
                    }
                case "highlight_phrase":
                    const phrase = entities.find((e) => e.entity === "phrase")?.value || "";
                    if (phrase) {
                        sendVoiceCommand("highlight_phrase", `Sublinhando a frase: ${phrase}`, { Phrase: phrase });
                    } else {
                        responseDiv.textContent = "Frase não fornecida.";
                    }
                    break;
                default:
                    responseDiv.textContent = 'Comando não reconhecido.';
                    console.log('Intent não reconhecida.');
            }
        }

        recognition.onerror = (event) => {
            console.error('Erro no reconhecimento de voz:', event.error);
            statusDiv.textContent = `Erro no reconhecimento de voz: ${event.error}`;
            micCircle.setAttribute('fill', '#ff5252');
        };

        recognition.onend = () => {
            console.log('Reconhecimento de voz finalizado. Reiniciando...');
            statusDiv.textContent = 'Reconhecimento de voz finalizado. Reiniciando...';
            setTimeout(() => recognition.start(), 500); // Pequeno atraso para reiniciar
        };

        // Inicializar WebSocket
        let socket;

        function initializeWebSocket() {
            socket = new WebSocket('ws://localhost:5000/');

            socket.onopen = () => {
                console.log('WebSocket conectado.');
            };

            socket.onmessage = (event) => {
                console.log('Resposta do servidor WebSocket:', event.data);
                responseDiv.textContent = event.data;
            };

            socket.onerror = (error) => {
                console.error('Erro no WebSocket:', error);
            };

            socket.onclose = () => {
                console.log('WebSocket desconectado. Tentando reconectar em 5 segundos...');
                setTimeout(initializeWebSocket, 5000); // Tenta reconectar
            };
        }

        function sendVoiceCommand(intent, feedback, additionalData = {}) {
            if (socket && socket.readyState === WebSocket.OPEN) {
                const message = JSON.stringify({ Intent: intent, ...additionalData });
                console.log(`Payload sent to WebSocket: ${message}`); // Debug log
                //if (SlideNumber) {
                //    console.log(`Payload sent to WebSocket: ${JSON.stringify({ Intent: intent, SlideNumber: additionalData?.slideNumber })}`);
                //}
                //if (SlideTitle) {
                //    console.log(`Sending payload: ${JSON.stringify({ Intent: intent, SlideTitle: slideTitle })}`);
                //}
                socket.send(message);
                console.log(`Comando enviado: ${intent}`);
                responseDiv.textContent = feedback;
                speak(feedback);
            } else {
                console.error('WebSocket não está conectado.');
                responseDiv.textContent = 'Erro: WebSocket desconectado.';
            }
        }

        function speak(text) {
            console.log(`Attempting to speak: ${text}`); // Debug log
            if ('speechSynthesis' in window) {
                const msg = new SpeechSynthesisUtterance(text);
                msg.lang = 'pt-PT'; // Adjust the language code as needed
                msg.onend = () => {
                    console.log('Resposta falada concluída.');
                };
                window.speechSynthesis.speak(msg);
                console.log(`Resposta falada: ${text}`);
            } else {
                console.error('SpeechSynthesis não é suportado neste navegador.');
                responseDiv.textContent = 'Erro ao fornecer feedback auditivo.';
            }
        }


        // Initialize WebSocket
        initializeWebSocket();

        recognition.start(); // Start listening for commands
    
/* END EXTERNAL SOURCE */

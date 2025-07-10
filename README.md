# kamerkompas
Kamerkompas brings innovative insight into the activity of members of parliament, including helpful AI assistance. 

Kamerkompas is to be used in combination with imsennen/kamerkompas_ai_assistant. <br>
Kamerkompas_ai_assistant controls the AI, and connects to kamerkompas whenever it needs assistance.

Project for CIP, House of Representatives of the States General during the minor Innovative Data Visualisation. <br>

## How to use

1. Clone this repository:
   ```bash
   git clone https://github.com/imsennen/kamerkompas.git

2. Set up your .env file for database features. <br>
   Look at .env.example and fill in your MySql credentials.

3. Set up imsennen/kamerkompas_ai_assistant.

4. Set up LMStudio (https://lmstudio.ai/) with a model of your choice. We recommend any of the Gemma 3 models by Google. Pick the right size of the model depending on your system.
    
5. In chat.jsx, lines 45 and 46, you can pick wether to use the kamerkompas_ai_assistant or LMStudio. Make sure to only comment or uncommment the lines, depending on your choice. <br>
   However, if you're a free user or just curious, we recommend using kamerkompas_ai_assistant. 

6. Have a look at our Figma project for hi-fi designs: https://www.figma.com/design/fgsXBa2ntGvaoFgoWL7Hrw/Minor?node-id=1-2&t=eI0KUR7nsMBXknlY-1

7. All set!


# Made possible by: <br>
Hamsa Abou-Ammar – 22075119 <br>
Jesse van den Broek – 24170232 <br>
Sophie de Groot – 22138870 <br>
Sennen Schoonderwaldt – 22049428 <br>

 

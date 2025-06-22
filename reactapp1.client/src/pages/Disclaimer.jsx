import React from 'react';
import "./PageCommon.css";

const Disclaimer = () => (
    <div className="page-container">
        <h1 className="page-title">Disclaimer</h1>
        <p className="page-description">De informatie in deze chat is bedoeld voor algemene informatiedoeleinden en kan onjuistheden bevatten.
            Raadpleeg altijd officiële bronnen voor belangrijke beslissingen.</p>
        <p className="page-description">Het AI-model kan onnauwkeurige antwoorden genereren en is gemaakt door studenten. </p>
        <p className="page-description">Het AI-model is gebaseerd op Gemini 2.0 Flash, een taalmodel van Google, voornamelijk bekend van de gelijknamige dienst. <br /> We gebruiken een System Instruction om de 'persoonlijkheid' van de AI aan te passen, zodat deze over de Tweede Kamer praat en niet over wat anders. </p>
    </div>
);

export default Disclaimer;
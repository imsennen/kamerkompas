import { useEffect, useState } from 'react';
import { BrowserRouter as Router, Routes, Route, useLocation } from 'react-router-dom';

import './App.css';
import Chat from './pages/Chat';
import Menubar from "./components/Menubar";

import Kamerleden from "./pages/Kamerleden";
import Fracties from "./pages/Fracties";
import FractieDetail from "./pages/FractieDetail"; // You need to create this
import OverOns from "./pages/OverOns";
import Disclaimer from './pages/Disclaimer';

function ChatButton() {
    const location = useLocation();
    // Hide button on /chat
    if (location.pathname === "/chat") return null;
    return (
        <a href="/chat" className="chat-button" title="Chat met de AI">
            💬
        </a>
    );
}

function App() {

    const logo_kamerkompas = "\\images\\logo_kamerkompas_transparant.png";

    useEffect(() => {

    }, []);

    return (
        <Router>
            <Menubar />
            <ChatButton />
            <Routes>
                <Route path="/kamerleden" element={<Kamerleden />} />
                <Route path="/fracties" element={<Fracties />} />
                <Route path="/fracties/:id" element={<FractieDetail />} />
                <Route path="/overons" element={<OverOns />} />
                <Route path="/chat" element={<Chat />} />
                <Route path="/disclaimer" element={<Disclaimer />} />
                <Route path="/" element={
                    <div className="app-container">
                        {/* Welcome Page */}
                        <div className="welcome-content">
                            <img src={logo_kamerkompas} className="app-logo" alt="Logo kamerkompas" />
                            <h1 className="app-name">KamerKompas</h1>
                            <p className="welcome-text">
                                KamerKompas helpt je eenvoudig informatie te vinden over de Tweede Kamer der Staten-Generaal van Nederland.
                                Ontdek bijvoorbeeld wie de huidige Kamerleden zijn of welke fracties er actief zijn.<br /><br />
                                Heb je een vraag over de Tweede Kamer? Start een chat met onze AI-assistent voor snelle antwoorden.<br /><br />
                                Gebruik het menu hierboven om te navigeren. Veel succes!
                            </p>
                        </div>
                    </div>
                } />
            </Routes>
        </Router>
    );
}

export default App;
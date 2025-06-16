import { useEffect, useState } from 'react';
import { BrowserRouter as Router, Routes, Route, useLocation } from 'react-router-dom';

import './App.css';
import Chat from './pages/Chat';
import Menubar from "./components/Menubar";

import Kamerleden from "./pages/Kamerleden";
import Fracties from "./pages/Fracties";
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
                                Dit is KamerKompas.
                            </p>
                        </div>
                    </div>
                } />
            </Routes>
        </Router>
    );
}

export default App;
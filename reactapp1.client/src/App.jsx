import { useEffect, useState } from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';

import './App.css';
import Chat from './pages/Chat';
import Menubar from "./components/Menubar";

import Kamerleden from "./pages/Kamerleden";
import Fracties from "./pages/Fracties";
import OverOns from "./pages/OverOns";

function App() {

    const logo_kamerkompas = "\\images\\logo_kamerkompas_transparant.png";

    useEffect(() => {

    }, []);


    return (
        <Router>
            <Menubar />
            <Routes>
                <Route path="/kamerleden" element={<Kamerleden />} />
                <Route path="/fracties" element={<Fracties />} />
                <Route path="/overons" element={<OverOns />} />
                <Route path="/chat" element={<Chat />} />
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

                        {/* Chat Button */}
                        <a href="/chat" className="chat-button" title="Chat met de AI">
                            💬
                        </a>
                    </div>
                } />
            </Routes>
        </Router>
    );
    
}

export default App;
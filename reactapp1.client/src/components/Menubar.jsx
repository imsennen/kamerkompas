import React from 'react';
import { Link } from "react-router-dom";
import "./menubar.css";


const logo_kamerkompas = "\\images\\logo_kamerkompas_transparant_embleem2.png";

const Menubar = () => {
    return (
        <nav className="menubar">
            <ul className="menubar-list">
                <div className="menu-left">
                    <li>
                        <Link to="/">
                            <img src={logo_kamerkompas} className="menu_logo" alt="Logo kamerkompas" />
                        </Link>
                    </li>
                    <li>
                        <Link to="/kamerleden">Kamerleden</Link>
                    </li>
                    <li>
                        <Link to="/fracties">Fracties</Link>
                    </li>
                </div>
                <div className="menu-right">
                    <li>
                        <Link to="/overons">Over KamerKompas</Link>
                    </li>
                    <li>
                        <Link to="/disclaimer">Disclaimer AI</Link>
                    </li>
                </div>
            </ul>
        </nav>
    );
};

export default Menubar;
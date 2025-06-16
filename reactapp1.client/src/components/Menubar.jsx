import React from 'react';
import { Link } from "react-router-dom";
import "./menubar.css";


const logo_kamerkompas = "\\images\\logo_kamerkompas.png";

const Menubar = () => {
  return (
      <nav className="menubar">
          <ul>
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
              <li>
                  <Link to="/overons">Over KamerKompas</Link>
              </li>
          </ul>
      </nav>
  );
};

export default Menubar;
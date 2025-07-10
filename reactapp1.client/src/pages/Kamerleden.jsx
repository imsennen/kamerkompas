import React, { useEffect, useState } from "react";
import "./PageCommon.css";

const Kamerleden = () => {
  const [leden, setLeden] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetch("https://localhost:7059/api/Database/huidige-functies")
      .then((res) => res.json())
      .then((data) => {
        setLeden(data);
        setLoading(false);
      })
      .catch(() => setLoading(false));
  }, []);

  return (
    <div className="page-container">
      <h1 className="page-title">Kamerleden</h1>
      <p className="page-description">Informatie over de Kamerleden.</p>
      {loading ? (
        <p>Laden...</p>
      ) : leden.length === 0 ? (
        <p>Geen kamerleden gevonden.</p>
      ) : (
        <div className="fracties-card-list">
          {leden.map((lid, idx) => (
            <div className="fractie-card" key={idx}>
              <div className="fractie-card-content">
                <h2 className="fractie-card-title">
                  {lid.persoonNaam}
                </h2>
                <p>
                  <strong>Fractie:</strong> {lid.fractieNaam}
                  <br />
                  <strong>Afkorting:</strong> {lid.fractieAfkorting}
                  <br />
                  <strong>Functie:</strong> {lid.functie}
                </p>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default Kamerleden;
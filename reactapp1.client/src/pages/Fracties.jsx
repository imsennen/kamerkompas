import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import "./PageCommon.css";

const Fracties = () => {
  const [fracties, setFracties] = useState([]);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();

  useEffect(() => {
      fetch("https://localhost:7059/api/Database/fracties")
      .then((res) => res.json())
      .then((data) => {
        setFracties(data);
        setLoading(false);
      })
      .catch(() => setLoading(false));
  }, []);

  if (loading) {
    return (
      <div className="page-container">
        <h1 className="page-title">Fracties</h1>
        <p>Loading...</p>
      </div>
    );
  }

  return (
    <div className="page-container">
      <h1 className="page-title">Fracties</h1>
      {fracties.length === 0 ? (
        <p>Geen fracties gevonden.</p>
      ) : (
        <div className="fracties-card-list">
          {fracties.map(({ id, fotoLink, naamNL, aantalZetels }) => (
            <div className="fractie-card" key={id}>
              <img
                src={fotoLink}
                alt={naamNL}
                className="fractie-card-img"
              />
              <div className="fractie-card-content">
                <h2 className="fractie-card-title">{naamNL}</h2>
                <p>
                  <strong>Aantal zetels:</strong> {aantalZetels}
                </p>
                <button
                  id={`meer-informatie-${id}`}
                  className="meer-informatie-button"
                  type="button"
                  onClick={() => navigate(`/fracties/${id}`)}
                >
                  Meer informatie
                </button>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default Fracties;
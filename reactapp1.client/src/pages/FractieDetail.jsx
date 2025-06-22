import React, { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import "./PageCommon.css";

const partyColors = {
  "Partij voor de Vrijheid": "#1d305c",
  "GroenLinks-PvdA": "#13a438",
  "Volkspartij voor Vrijheid en Democratie": "#f28e00",
  "Nieuw Sociaal Contract": "#1c1937",
  "Democraten 66": "#13a538",
  "BoerBurgerBeweging": "#96bc36",
  "Christen-Democratisch Appèl": "#00785f",
  "Socialistische Partij": "#e42313",
  "Staatkundig Gereformeerde Partij": "#ea5b0c",
  "Partij voor de Dieren": "#006430",
  "ChristenUnie": "#009fe3",
  "DENK": "#44bac1",
  "Forum voor Democratie": "#95342e",
  "Volt": "#512777",
  "JA21": "#243465"
};

const partyHeads = {
    "Partij voor de Vrijheid": "Geert Wilders",
    "GroenLinks-PvdA": "Frans Timmermans",
    "Volkspartij voor Vrijheid en Democratie": "Dilan Yeşilgöz-Zegerius",
    "Nieuw Sociaal Contract": "Nicolien van Vroonhoven",
    "Democraten 66": "Rob Jetten",
    "BoerBurgerBeweging": "Caroline van der Plas",
    "Christen-Democratisch Appèl": "Henri Bontenbal",
    "Socialistische Partij": "Jimmy Dijk",
    "Staatkundig Gereformeerde Partij": "Chris Stoffer",
    "Partij voor de Dieren": "Esther Ouwehand",
    "ChristenUnie": "Mirjam Bikker",
    "DENK": "Stephan van Baarle",
    "Forum voor Democratie": "Thierry Baudet",
    "Volt": "Laurens Dassen",
    "JA21": "Joost Eerdmans"
};

const partyBios = {
    "Partij voor de Vrijheid": "De PVV is een nationalistische partij met nadruk op immigratiebeperking, Nederlandse identiteit en veiligheid. De partij is kritisch op de islam en de Europese Unie. Ze wordt geleid door Geert Wilders en heeft sinds 2006 een vaste plek in de Kamer.",
    "GroenLinks-PvdA": "GroenLinks-PvdA is een gezamenlijke fractie van twee progressieve partijen die samenwerken sinds 2023. De combinatie richt zich op duurzaamheid, sociale gelijkheid en rechtvaardigheid. GroenLinks heeft een groene, linkse inslag, terwijl de PvdA historisch geworteld is in de sociaaldemocratie.",
    "Volkspartij voor Vrijheid en Democratie": "De VVD is een liberale partij die zich richt op economische groei, ondernemerschap en veiligheid. Ze streeft naar een kleine, efficiënte overheid en lagere belastingen. De VVD heeft regelmatig deelgenomen aan regeringen en levert vaak de premier.",
    "Nieuw Sociaal Contract": "NSC is een politieke beweging opgericht door Pieter Omtzigt in 2023. De partij richt zich op bestuurlijke vernieuwing, rechtsstaat en sociale zekerheid. NSC benadrukt transparantie, controle op macht en het herstellen van vertrouwen in de overheid.",
    "Democraten 66": "D66 is een sociaal-liberale partij die nadruk legt op individuele vrijheid, onderwijs en democratische vernieuwing. De partij staat voor progressief beleid op thema’s als klimaat, onderwijs en Europa. D66 werd opgericht in 1966 met als doel het vernieuwen van de democratie.",
    "BoerBurgerBeweging": "De BoerBurgerBeweging richt zich op de belangen van het platteland, agrariërs en burgers buiten de Randstad. De partij staat voor een nuchtere aanpak en wil de kloof tussen burger en politiek verkleinen. BBB ontstond in 2019 en kreeg snel aanhang, vooral in landelijke gebieden.",
    "Christen-Democratisch Appèl": "Het CDA is een christendemocratische partij die inzet op gemeenschapszin, solidariteit en rentmeesterschap. De partij richt zich op gezin, veiligheid en een sterke samenleving. Het CDA heeft een lange politieke geschiedenis en heeft meerdere regeringen geleid.",
    "Socialistische Partij": "De SP is een linkse partij die staat voor solidariteit, publieke voorzieningen en economische gelijkheid. De partij wil minder marktwerking in zorg, onderwijs en volkshuisvesting. Ze ontstond uit de marxistisch-socialistische beweging van de jaren '70.",
    "Staatkundig Gereformeerde Partij": "De SGP is de oudste partij van Nederland en baseert haar beleid op Bijbelse principes. De partij staat voor behoud van traditionele waarden, gezag en een terughoudende overheid. Ze hecht veel belang aan christelijke normen in de samenleving.",
    "Partij voor de Dieren": "De PvdD is een ecologische partij die dierenrechten, natuur en klimaat centraal stelt in haar beleid. Ze wil een omslag naar een duurzamere en diervriendelijkere samenleving. De partij bekijkt politiek vanuit het perspectief van de planeet en toekomstige generaties.",
    "ChristenUnie": "De ChristenUnie is een christelijk-sociale partij die christelijke waarden combineert met sociale betrokkenheid. Thema’s als zorg, armoedebestrijding en mensenrechten staan centraal. De partij baseert haar standpunten op de Bijbel en hecht waarde aan ethische kwesties.",
    "DENK": "DENK richt zich op diversiteit, inclusie en gelijke behandeling, met bijzondere aandacht voor mensen met een migratieachtergrond. De partij wil discriminatie bestrijden en pleit voor een rechtvaardige samenleving. Ze werd opgericht in 2015 na een afsplitsing van de PvdA.",
    "Forum voor Democratie": "FVD is een rechts-conservatieve partij met nadruk op nationale soevereiniteit, referenda en cultuurbehoud. De partij is kritisch op de EU en klimaatbeleid. Ze werd bekend door haar uitgesproken standpunten over democratie en identiteit.",
    "Volt": "Volt is een pan-Europese partij die pleit voor samenwerking binnen de EU, duurzame innovatie en burgerrechten. Ze richt zich op grensoverschrijdende oplossingen voor thema’s als klimaat, digitalisering en democratie. Volt is sinds 2021 vertegenwoordigd in de Kamer.",
    "JA21": "JA21 is een conservatief-liberale partij die zich inzet voor streng immigratiebeleid, economische vrijheid en behoud van Nederlandse waarden. De partij is ontstaan in 2020 na een afsplitsing van FVD. JA21 is eurosceptisch en kritisch op klimaatwetgeving."
};


const FractieDetail = () => {
  const { id } = useParams();
  const [fractie, setFractie] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetch(`https://localhost:7059/api/Database/fracties/${id}`)
      .then((res) => res.json())
      .then((data) => {
        setFractie(data);
        setLoading(false);
      })
      .catch(() => setLoading(false));
  }, [id]);

  if (loading) return <p>Loading...</p>;
  if (!fractie) return <p>Fractie niet gevonden.</p>;

  const color = partyColors[fractie.naamNL];

    console.log(fractie);

  return (
      <div className="page-container">
          <div
              className="fractie-detail-header"
              style={{ backgroundColor: color }}
          >
              <h1 className="fractie-title">{fractie.naamNL}</h1>
              <div className="fractie-detail-content">
                  <div className="fractie-detail-left">
                      <img
                          className="fractie-logo"
                          src={fractie.fotoLink}
                          alt={fractie.naamNL}
                      />
                  </div>
                  <div className="fractie-detail-right">
                      <div className="fractie-bio">
                          <strong>Biografie:</strong> <br />
                          {partyBios[fractie.naamNL]}
                      </div>
                  </div>
                  <p><b>Fractievoorzitter: </b>{partyHeads[fractie.naamNL]}
                  <br />  <br /> <b>Aantal zetels: </b>{fractie.aantalZetels}</p> 
              </div>

              
          </div>
      </div>
  );
};

export default FractieDetail;
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EindProjectBusinessModels;
using System.Reflection;
using System.Data.Entity;

namespace EindProjectDAL
{
    public class DalMethodes
    {

        /************** 1. BEHEREN VAN WERKNEMERS *******************/


        /**************************
         *                        *
         * 1.1 Beheren Werknemers *
         *                        *
         **************************/

        /**************************************
         * 1.1.1. Toevoegen van een werknemer *
         **************************************
         * Bernd 17/02/15                     *
         **************************************/
        public void VoegWerknemerToeAanDb(Werknemer werknemer, int teamCode)
        {
            // nodig om duplicatie van team te voorkomen
            using (DbEindproject db = new DbEindproject())
            {
                try
                {
                    werknemer.Team = HaalTeamVoorWerknemerUitDb(werknemer, teamCode, db);
                    db.Werknemers.Add(werknemer);
                    db.SaveChanges();
                }
                catch
                {
                    throw new Exception("Probleempje met het toevoegen van een werknemer.");
                }
            }
        }


        /*****************************************************
         * Hulpmethode Team ophalen voor bestaande werknemer *
         *****************************************************
         * Bernd                                             *
         *****************************************************/
        private Team HaalTeamVoorWerknemerUitDb(Werknemer werknemer, int teamCode, DbEindproject db)
        { 
             Team team = (from t in db.Teams
                                where t.Code == teamCode
                                select t).First<Team>()
                               ;
            return team;
        }



        /**********************************
         * 1.1.2. Opvragen van werknemers *
         **********************************/
        /***************************************************************
         * 1.1.2. a. Opvragen van werknemers volgens bepaalde criteria *
         ***************************************************************
         * Roel 16/02/15                                               *
         ***************************************************************/
        public List<Werknemer> VraagWerknemerOp(string personeelsNr, string naam, string voornaam)
        {
            using (DbEindproject db = new DbEindproject())
            {
                List<Werknemer> wnLijst = (from w in db.Werknemers.Include(w => w.Team).Include(w => w.Verlofaanvragen)
                                           where w.Naam.Contains(naam)
                                           && w.Voornaam.Contains(voornaam)
                                           && w.PersoneelsNr.ToString().Contains(personeelsNr)
                                           orderby w.Naam, w.Voornaam, w.PersoneelsNr
                                           select w).ToList<Werknemer>();

                return wnLijst;
            }
            // throw new Exception("Er liep iets fout tijdens het opvragen van 0, 1 of meerdere werknemers");
        }

        public Werknemer VraagEnkeleWerknemerOp(string personeelsNr)
        {
            using (DbEindproject db = new DbEindproject())
            {
                var wn =  from w in db.Werknemers.Include(w => w.Team).Include(w => w.Verlofaanvragen)
                                where w.PersoneelsNr.ToString() == personeelsNr
                                select w;

                return (Werknemer)wn;
            }
            // throw new Exception("Er liep iets fout tijdens het opvragen van 0, 1 of meerdere werknemers");
        }
        /******************************************
        * 1.1.2. b. Opvragen van alle werknemers *
        ******************************************
        * David 16/02/15                         *
        ******************************************/
        public List<Werknemer> VraagAlleWerknemersOp()
        {
            return VraagWerknemerOp(string.Empty, string.Empty, string.Empty);
        }


        /*********************************
        * 1.1.3. Wijzigen van werknemers *
        **********************************
        * Roel 16/02/15                  *
        **********************************/
        public void WijzigWerknemerProperty(Werknemer werknemer, int teamCode)
        {
           
            using (DbEindproject db = new DbEindproject())
            {
                Werknemer wn = (from w in db.Werknemers
                                where w.PersoneelsNr == werknemer.PersoneelsNr
                                select w).FirstOrDefault();

                werknemer.Team = HaalTeamVoorWerknemerUitDb(werknemer, teamCode, db);
                wn = werknemer;
                db.SaveChanges();
            }
        }

        /**************************
         *                        *
         * 1.2. Beheren van Teams *
         *                        *
        ***************************/

        /******************************
         * 1.2.1. Toevoegen van teams *
         ******************************
         * David 16/02/15             *
         ******************************/
        public void VoegTeamToeAanDb(Team team)
        {
            /*
             * Team wordt bepaald door autogenerated key (Code)
             * zelfde omschrijving kan en dient niet opgevangen (dixit Ann)
            */
            using (DbEindproject db = new DbEindproject())
            {
                try
                {
                    db.Teams.Add(team);
                    db.SaveChanges();
                }
                catch
                {
                    throw new Exception("Het toevoegen van een team is niet gelukt.");
                }
            }
        }


        /********************************************
         * 1.2.2. Beheren van teamverantwoordelijken *
         ********************************************
         * David 16/02/15                           *
         ********************************************/
        public void BeheerTeamVerantwoordelijke(Werknemer werknemer)
        {
            /*
             * Huidige teamleader teamleader af maken en
             * geselecteerde werknemer teamleader maken
            */
            var theTeam = werknemer.Team;
            using (DbEindproject db = new DbEindproject())
            {
                var huidigTL = (from wn in db.Werknemers
                                where wn.Team.Code == theTeam.Code
                                   && wn.TeamLeader == true
                                select wn).FirstOrDefault();
                if (huidigTL != null)
                {
                    huidigTL.TeamLeader = false;
                }
                try
                {
                    werknemer.TeamLeader = true;
                    db.SaveChanges();
                }
                catch
                {
                    throw new Exception("Nieuwe teamleader kon niet worden aangesteld.");
                }
            }

        }

        /**********************************************
         * Hulpmethode Heeft Team al een teamleader ? *
         **********************************************
         * onbekend auteur                            *
         **********************************************/
        public List<Team> OpvragenAlleTeams()
        {
            using (DbEindproject db = new DbEindproject())
            {
                List<Team> teamLijst = (from w in db.Teams
                                        select w).ToList<Team>();

                return teamLijst;
            }
        }



        /**********************************************
         * Hulpmethode Heeft Team al een teamleader ? *
         **********************************************
         * David 18/02/15                             *
        ***********************************************/ 
        public bool IsErAlEenTeamLeader(Team team)
        {
            using (DbEindproject db = new DbEindproject())
            {
                var tl = (from wn in db.Werknemers
                          where wn.Team.Code == team.Code
                             && wn.TeamLeader == true
                          select wn).FirstOrDefault
                          ();
                if (tl != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
 




        /*****************************
         * 1.2.3. Opvragen van teams *
         *****************************
         * David 15/02/15            *
        *****************************/ 
        public List<Team> OpvragenTeams(string code, string teamnaam, string teamleader)
        {
            //  De medewerker geeft 0, 1 of meer van volgende criteria op:
            //   - Gedeelte van teamnaam
            //   - Gedeelte van naam van teamverantwoordelijke
            //   - Code
            // Het systeem toont de gegevens (code; naam; nummer, naam en voornaam teamverantwoordelijke;
            // nummer naam en voornaam van alle werknemers die tot het team behoren ) van de teams die aan
            // alle opgegeven criteria voldoen.  De gegevens zijn gesorteerd op teamnaam.  Binnen een team
            // zijn de gegevens van de werknemers gesorteerd op naam en voornaam van de werknemers.
            return null;
        }




        /**********************************
         * 1.2.4 Verwijderen van een team *
         **********************************/
        public void VerwijderTeam(Team team)
        {
            using (DbEindproject db = new DbEindproject())
            {
                var wn = from w in db.Werknemers
                         where w.Team.Code == team.Code
                         select w;
                // Zijn er nog werknemers die tot dit team behoren?
                // Zo ja, team blijft bestaan
                if (wn != null)
                {
                    throw new Exception("Er bestaan nog werknemers in dit team.");
                }
                else
                {
                    try
                    {
                        // Als er geen werknermers meer tot het team behoren, kan het gerust verwijdert worden
                        db.Teams.Remove(team);
                        db.SaveChanges();
                    }
                    catch
                    {
                        throw new Exception("Het verwijderen van het team is niet succesvol verlopen.");
                    }
                }
            }
        }


        /*********************************
         * 1.2.5. Opvragen van Teamleden *
         *********************************
         * Roel 16/02/15                 *
         *********************************/
        public List<Werknemer> GeefTeamleden(Team team)
        {
            using (DbEindproject db = new DbEindproject())
            {
                List<Werknemer> wnLijst = (from w in db.Werknemers
                                           where w.Team.Code == team.Code
                                           select w).ToList<Werknemer>();
                return wnLijst;
            }
            throw new Exception("Opvragen Teamleden mislukt.");
        }

        /************** 2. BEHEREN VAN JAARLIJKSE VERLOVEN *******************/

        /******************************************
         *                                        *
         * 2.1. Toevoegen van jaarlijkse verloven *
         *                                        *
         ******************************************
         * Roel 16/02/15                          *
         ******************************************/
        public void WijzigJaarlijksVerlof(Werknemer werknemer, JaarlijksVerlof jaarlijksverlof)
        {
            if (jaarlijksverlof.Jaar < DateTime.Now.Year)
                throw new Exception("Het opgegeven jaartal ligt in het verleden.");
            if (jaarlijksverlof.AantalDagen < 0 || jaarlijksverlof.AantalDagen > 50)
                throw new Exception("Het aantal verlofdagen moet tussen 0(incl.) en 50(incl.) liggen.");
            using (DbEindproject db = new DbEindproject())
            {
                Werknemer wn = (from w in db.Werknemers.Include(w => w.JaarlijksVerlof)
                                where w.PersoneelsNr == werknemer.PersoneelsNr
                                select w).FirstOrDefault();

                foreach (JaarlijksVerlof jv in wn.JaarlijksVerlof)
                {
                    if (jv.Jaar == jaarlijksverlof.Jaar)
                    {
                        throw new Exception("De verlofdagen voor dit jaar voor deze werknemer waren al ingevuld.");
                    }
                }

                wn.JaarlijksVerlof.Add(jaarlijksverlof);
                db.SaveChanges();
            }
        }

        /****************************************************
         *                                                  *
         * 2.2. Beheren feestdagen & verplichte verlofdagen *
         *                                                  *
         ****************************************************/

        /************************************
         *                                  *
         * 2.3. Beheren van verlofaanvragen *
         *                                  *
         ************************************/

        /***************************************
         * 2.3.1. Indienen van verlofaanvragen *
         ***************************************/

        /***************************************************************************************
         * 2.3.1.1. Indienen van verlofaanvragen met geldige gegevens en voldoende verlofdagen *
         ***************************************************************************************
         * Roel 16/02/15                                                                       *
         ***************************************************************************************/
        public Aanvraagstatus IndienenVerlofaanvraag(Werknemer werknemer, VerlofAanvraag verlofaanvraag)
        {
            using (DbEindproject db = new DbEindproject())
            {
                Werknemer wn = (from w in db.Werknemers
                                where w.PersoneelsNr == werknemer.PersoneelsNr
                                select w).FirstOrDefault();
                wn.Verlofaanvragen.Add(verlofaanvraag);
                verlofaanvraag.Toestand = Aanvraagstatus.Ingediend;
                db.SaveChanges();
                return Aanvraagstatus.Ingediend;
            }
            throw new Exception("Er liep iets fout in de methode IndienenVerlofaanvraag in DAL");
        }

        /*****************************************************************************************
         * 2.3.1.2. Indienen van verlofaanvragen met geldige gegevens en onvoldoende verlofdagen *
         *****************************************************************************************/

        /****************************************************************
         * 2.3.1.3. Indienen van verlofaanvragen met ongeldige gegevens *
         ****************************************************************/


        /****************************************
         * 2.3.2. Annuleren van verlofaanvragen *
         ****************************************/

        /*********************************************************************
         * 2.3.3. Goedkeuren van verlofaanvragen door teamverantwoordelijken *
         *********************************************************************/

        /**********************************************************
         * 2.3.4. Goedkeuren van verlofaanvragen door het systeem *
         **********************************************************/
        public void WijzigBehandelDatumVerlofaanvraag(VerlofAanvraag verlofaanvraag)
        {
            // nog niet getest
            using (DbEindproject db = new DbEindproject())
            {
                VerlofAanvraag aanvraag = (from v in db.Verlofaanvragen
                                           where v.Id == verlofaanvraag.Id
                                           select v).FirstOrDefault();
                aanvraag.BehandelDatum = DateTime.Now;
                db.SaveChanges();
            }
        }
        public void WijzigBehandeldDoorVerlofaanvraag(VerlofAanvraag verlofaanvraag, Werknemer werknemer)
        {
            // nog niet getest
            using (DbEindproject db = new DbEindproject())
            {
                VerlofAanvraag aanvraag = (from v in db.Verlofaanvragen
                                           where v.Id == verlofaanvraag.Id
                                           select v).FirstOrDefault();

                Werknemer teamleader = (from w in db.Werknemers
                                        where w.PersoneelsNr == werknemer.PersoneelsNr
                                        select w).FirstOrDefault();
                
                aanvraag.BehandeldDoor= teamleader;
                db.SaveChanges();
            }
        }


        /***************************************
         * 2.3.5. Afkeuren van verlofaanvragen *
         ***************************************/
        public void WijzigStatusVerlofaanvraag(VerlofAanvraag verlofaanvraag, Aanvraagstatus status)
        {
            // nog niet getest
            using (DbEindproject db = new DbEindproject())
            {
                VerlofAanvraag aanvraag = (from v in db.Verlofaanvragen
                                           where v.Id == verlofaanvraag.Id
                                           select v).FirstOrDefault();
                aanvraag.Toestand = status;
                db.SaveChanges();
            }
        }

        /*********************************************
         * 2.3.6. Opvragen lijst met verlofaanvragen *
         *********************************************/

        /***************************************************************************
         * 2.3.6.1. Opvragen lijst met verlofaanvragen door teamverantwoordelijken *
         ***************************************************************************/

        /***************************************************************
         * 2.3.6.2. Opvragen lijst met verlofaanvragen door werknemers *
         ***************************************************************/

        /************************************************************
         * 2.3.6.3. Opvragen lijst met verlofaanvragen door systeem *
         ************************************************************/


        /************** 3. BEHEREN VAN ADV dagenJAARLIJKSE VERLOVEN *******************/

        /*********************************
         *                               *
         * 3.1. Registreren van overuren *
         *                               *
         *********************************/


        /********************************
         *                              *
         * 3.2. Aanvragen van ADV dagen *
         *                              *
         ********************************/


        /************** 4. AUTHORISATIE *******************/




        /************** 5. GEBRUIK VAN KALENDER VOOR VISUALISATIE *******************/

        /*********************************************************************
         *                                                                   *
         * 5.1. Gebruik van een kalender bij indienen van een verlofaanvraag *
         *                                                                   *
         *********************************************************************/

        /********************************************************************************************
         *                                                                                          *
         * 5.2. Gebruik van een kalender bij het opvragen van de verlofaanvragen door een werknemer *
         *                                                                                          *
         ********************************************************************************************/

        /*******************************************************************************************************
         *                                                                                                     *
         * 5.3. Gebruik van een kalender bij het opvragen van de verlofaanvragen door de teamverantwoordelijke *
         *                                                                                                     *
         *******************************************************************************************************/



        /************** 6. E-MAIL *******************/

        /*****************************************************************************
         *                                                                           *
         * 6.1. Verzenden van email bij indienen of annuleren van een verlofaanvraag *
         *                                                                           *
         *****************************************************************************/

        /*****************************************************************************
         *                                                                           *
         * 6.2. Verzenden van email bij goekeuren of afkeuren van een verlofaanvraag *
         *                                                                           *
         *****************************************************************************/



    }
}

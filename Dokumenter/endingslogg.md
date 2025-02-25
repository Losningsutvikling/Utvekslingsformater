# Endringslogg for bufdirs meldingsformater

# Versjon 1.0.0 - 20.02.2025

- Generelt:

  - Henvisningene er noe omstrukturert for å tilfredsstille et krav om at alle henvisninger skal kunne lastes inn i en "sum-struktur", dvs. at element som har samme betydning vil hete det samme (og ha samme XPath-uttrykk) uansett hvilken henvisning de forekommer i. Dette er løst delvis gjennom arv (extensions).
  
  - Obligatoriske tekstelement er endret fra <xs:string> til en-eller flerlinje typer med minLength=2. Årsaken er at validering ikke fanger opp et tomt tekstelement når det ikke er satt en minLength-verdi.
  
  - Felt som enables ved verdi i annet felt (typisk "Annet, spesifiser") er gjort ikke-obligatoriske for ikke å feile i XSD-validering
  
  - En del navneendringer som følge av intern gjennomgang
  
  - <kodelistefilter> benyttes ikke lenger
  
- Valideringsregler - tillegg:

Det er lagt ut en ***Bufdir_Melding_v1.0.0_validering.XSD*** som er vårt format for å angi valideringsregler som ikke kan spesifiseres i XSD. Formatet er ment å skulle være maskinlesbart, men funksjonalitet må sannsynligvis delvis mappes manuelt i kode ved implementering.
Som et minimum vil disse reglene representere en oversikt over hvilke feilmeldinger en vil kunne motta fra Bufdir. 

## Bufdir_Generelt_v1.0.0.xsd

- Nye elementtyper:

  - ***EnlinjeObligatorisk***: Brukes ved minOccurs > 0, minLength=2
  
  - ***LandkodeType***: ISO 3166 landkoder
  
  - ***SprakType***: ISO 639-1 språkkoder
  
  - ***VedleggTypeType***: enum type, foreløpig kun to verdier - vil utvides med kjente vedleggtyper

- Endrede elementer/typer:

  - ***Enlinje***: Endret maxLength fra 150 til 255 tegn
  
  - ***FritekstObligatorisk***: endret minLength fra 20 til 2, fått manglende ***id*** (og endret navn)
  
  - ***FulltNavnType***: Fått minLength = 2
  
  - ***VedleggType***:
  
    - ***Filnavn***: Lagt til \<unique\> constraint
	
	- ***VedleggType***: Nytt element (se ny type ***VedleggTypeType***)
	
- Navneendringer:

  - ***FritekstMinLengde***: Nytt navn ***FritekstObligatorisk***

## Bufdir_Melding_v1.0.0.xsd

- Endrede elementer/typer:

  - ***MeldingshodeType***:
  
    - ***MeldingstypeXpath***: Fjernet, ikke nødvendig da alle XSD'er har maks 1 rotelement
	
	- ***SendtTidspunkt***: Type endret til xs:dateTime

  - ***MeldingsForbindelseType***: Nye enum-verdier ***Duplikat*** og ***Feil***
  
  
## Bufdir_Barnevern_Generelt_v1.0.0.xsd

- Nye elementtyper:

  - ***DagtilbudType/DagtilbudAnnet***
  
  - ***KunnskapsmodellBarnetsSituasjonOgBehovType***: Se endring for ***BarnetsSituasjonOgBehovType*** 
  
  - ***KlientBarnevernMedEMAType***: Se endring beskrevet for ***KlientIdentifikatorType*** nedenfor
  
  - ***VedtakOmsorgType***: Se endring beskrevet for ***VedtakType*** nedenfor
  
  - ***VedtakStatusIkkeOmsorgType*** Se endring beskrevet for ***VedtakType*** nedenfor
  

- Endrede elementer/typer:
  
  - ***BarnetsSituasjonOgBehovType***: Elementet er nå en basistype som kun inneholder beskrivelse av samlet behov, arves og utvides med dimensjon-/områdebeskrivelser for henvisninger der disse benyttes. Elementet som nå tilsvarer den tidligere strukturen heter nå ***KunnskapsmodellBarnetsSituasjonOgBehovType***
  
  - ***DagtilbudType/Beskrivelse***: Var tidligere enablet ved "Annet" i dagtilbud, er nå et selvstendig, ikke-obligatorisk beskrivelsesfelt
    
  - ***IndividuellPlanType/Koordinator***: Har fått <enable> - uttrykk, kun relevant når det er angitt at individuell plan er relevant.
  
  - ***SamarbeidMedForeldreType***: Nytt navn ***ForeldreInvolveringType***
  
    - ***SamvaerPersoner***: Nytt navn ***SamvaerOgKontakt***
	
  - ***KlientIdentifikatorType***: Er blitt en basistype, uten EMA/Ufødt - elementene, disse er overført til typen ***KlientBarnevernType***
  
  - ***KlientIdentifikatorType/Fodseldato***: Fjernet, hentes fra FREG
  
  - ***KlientIdentifikatorType/Kjonn***: Fjernet, hentes fra FREG
  
  - ***KlientBarnevernType***: 
    
    - ***Ufodt***, ***TerminDato***: Nye element (se ***KlientIdentifikatorType***)
	  
	- ***Bosted***: Nytt element - beskrivelse av bosted (om avvikende fra FREG)

    - ***Bostedsadresse***: Utgår, hentes fra FREG
	
	- ***UtenlandskBakgrunn***: Navn endret til ***BehovForTolk***
	
  - ***BarnetsMedvirkningType***:
    
	- ***Rammer***: Endret navn til ***RammerForGjennomforing***
	
	- ***BarnetsBetraktning***: Endret navn til ***BarnetsSynspunkter***
	
  - ***UtenlandskBakgrunnType***: Endret navn til ***BehovForTolkType***
  
    - ***Statsborgerskap***: Endret navn til ***Sprak***
	
	- ***BehovForTolkBeskrivelse***: Nytt element
	
  - ***VedtakType***: Er blitt basistype som utvides med aktuelle element for ***Lovhjemmel*** og ***Vedtakstatus***. Dette er element som vil gå igjen i alle arvede typer, men enum-listene vil være ulike.

    - ***NemndsbehandlingStatus*** og ***NemndsbehandlingDato***: Tatt ut, da de kun er aktuelle for omsorgstiltak
	
  - ***MorsmalType***: Utgår, erstattes av ***StatsborgerskapType*** i Bufdir_Generelt
  
  - ***OmsorgType***: Ny kode 9=Annet

- Slettede elementer:

  - ***LovhjemmelType***: Har ikke vært i bruk (tilsvarer struktur i BVR)

## Bufdir_Barnevern_Henvisning_v1.0.0
  
- Nye elementtyper:

  - ***YtterligereBistandOmsorgstiltakType***: Den utvidede varianten av ***YtterligereBistandType***
  
  - ***HenvisningForespurtBistandInstitusjonType***: Utvalg av enum - koder fra ***HenvisningForespurtBistandType***
  
  - ***KanPlasseresINettverkType***: Ja/Nei/Uavklart
  
- Endrede elementer/typer:

  - ***OmsorgspersonType***: 
  
    - ***Etternavn***: Nytt element
	
	- ***KontaktFrekvens***: Har fått <enable> - betingelse til ***Dod***=0
	
    - ***Foreldreansvar***: Har fått <enable> - betingelse til ***Dod***=0

  - ***OnsketBistandsPeriodeType***: ***DatoTil*** fjernet, erstattet med tekstfelt ***Varighet***
  
  - ***PlasseringINettverkType***: Element ***Vurdert*** erstattet med ***KanPlasseresINettverk***
  
  - ***SoskenSamplasseringType: 
  
    - ***Fodselsnummer***: Endret ***id***, var ved en feil et duplikat

    - ***DUFnummer***: Endret ***id***, var ved en feil et duplikat
  
  - ***TilleggstjenesterType***: Tatt vekk elementet ***TverrfagligHelsekartlegging***, dette ligger nå i typen ***YtterligereBistandOmsorgstiltakType***
  
  - ***HenvisningMeldingType***: Flytting av elementer mellom denne typen, ***OmsorgstiltakHenvisningType*** og ***FosterhjemHenvisningType*** for å passe med flere, nye henvisningstyper
  
  - ***ViderePlanType***: enum-verdi "Ikke nye tiltak" tatt ut av liste, løses ved annet element (i <choice>)

- Navneendringer:
  
  - ***TilleggstjenesterType*** => ***YtterligereBistandType***
  
  - ***MedvirkningHenvisningType*** => ***MedvirkningOgInvolveringType***
  
  - ***MedvirkningHenvisningType***:
  
    - ***OmForeldrene*** => ***ForeldreInvolvering***

    - ***MedvirkningAnnet*** => ***MedvirkningOgInvolveringAnnet***
  
  - ***ViderePlanType*** => ***PlanEtterTiltakType***
  
## Bufdir_Barnevern_Henvisning_Fosterhjem_v1.0.0
  
- Nye elementtyper:

  - ***HenvisningFosterhjemVedtakType***: Dette elementet er "trukket ut" av tidligere standard henvisning fordi lovhjemmel-listene er ulike for ulike tiltak. Tidligere var dette løst ved å oppgi filtre på gyldige enum-verdier, nå løses det med enum-definisjoner som er utdrag av basistypen.

- Endrede elementer/typer:

  - ***HenvisningFosterhjemType***: Nytt element ***Vedtak***, flyttet fra tidligere struktur (se ***HenvisningFosterhjemVedtakType***)

 - Navneendringer:

  - ***HenvisningFosterhjem*** => ***Henvisning***

## Bufdir_Barnevern_Henvisning_Institusjon_v1.0.0.xsd

- Nesten 100% identisk med Fosterhjem, avvikende kodelister på vedtak/tjeneste
  
## Bufdir_Barnevern_Henvisning_Familierad_v0.9.0.xsd

- Ny, ikke produksjonsklar

## Bufdir_Barnevern_Henvisning_Hjelpetiltak_v0.9.0.xsd

- Ny, ikke produksjonsklar

## Bufdir_Kvitteringsmelding_v1.0.0

- ingen endringer
  
## Bufdir_TrukjketMelding_v1.0.0

- ingen endringer
  
# Kodelister:

## Generelt

- Tilleggsvariabler ligger nå som element i stedet for tidligere som attributter

- Det er laget en XSD for kodelister: Bufdir_kodelister_v1.0.0.xsd

## Bufdir_Generelt_kodelister_v1.0.0

- Nye kodelister:

  - ***LandkodeType***
  
  - ***SprakType***
  
  - ***VedleggTypeType***

## Bufdir_Melding_kodelister_v1.0.0

- ***MeldingsForbindelseType***: 
    
  - Nye koder "Duplikat" og "Feil"

  - variabel ***retning*** viser om verdien indikerer et svar eller en oppfølging av egen melding 
	
## Bufdir_Barnevern_Generelt_kodelister_v1.0.0
	
- ***KunnskapsmodellOmradeType***: Endringer i tekst og variabler

- ***MorsmalType*** fjernet, erstattet av ***SprakType*** i Bufdir_Generelt

- ***OmsorgsrelasjonType***: Fjernet gyldigFra/gyldigTil-verdier (var tidligere en testverdi) i verdi=1

- ***StatsborgerskapType*** fjernet, erstattet av ***LandkodeType*** i Bufdir_Generelt

- ***VedtakHenvisningParagrafType***: Fjernet attributter, lagt til variabler

- ***VedtakInstansType***: Rettet skrivefeil => "Barneverns- og helsenemnda"

- ***NemndsbehandlingStatusType***: Rettet feil ***id***

- ***VedtakStatusType***: 

  - Rettet skrivefeil => "Avventer nemndsbehandling"
  
  - Ny kode: verdi="4" tekst="Ikke aktuelt"
  
- ***OmsorgType***: Ny kode   

## Bufdir_Barnevern_Henvisning_kodelister_v1.0.0

- ***KanPlasseresINettverkType***: Ny kodeliste

- ***ViderePlanType***: 

  - Nytt navn ***PlanEtterTiltakType***
  
  - Fjernet kode med verdi=1
  
# Veiledningstekster:

## HenvisningVeiledning_v0.10.0.schema.json

- ingen endring
  
## Bufdir_barnevern_henvisning_veiledning_v0.10.0.json

- Oppdatert iht. endringer i XSD-element

    
# Versjon 0.10.0 - 25.11.2024

## Bufdir_Generelt_v0.10.0.xsd

- Nye elementer:

  - ***FritekstMinLengde***: Representerer fritekst som må fylles ut med minium antall tegn for å validere

- Endrede elementer:

  - ***OrganisasjonType/Navn***: Oppdatert minLength/MaxLength

  - ***VedleggType***: Elementnavn begynner med stor bokstav

## Bufdir_Melding_v0.10.0.xsd

- Nye elementer:

  - ***MeldingshodeType/Meldingstype***: Klartekst-versjon av meldingens type, innført for å kunne lese meldingshoder (typisk med en SAX-parser) og kunne liste opp meldinger uten å måtte lese mer data

## Bufdir_Barnevern_Generelt_v0.10.0.xsd

- Nye elementer:

  - ***BarnetsSituasjonOgBehovType/DimensjonVurdering***: 

  - ***KunnskapsmodellDimensjonVurderingType***  
  
  - ***VedtakType/FattetAv***
  
  - ***VedtakInstansType***
  
- Endrede elementer:

  - ***DagtilbudType/Beskrivelse***: Er blitt obligatorisk element
  
  - ***IndividuellPlanType/GyldigFra***: Er ikke lenger obligatorisk

  - ***KlientIdentifikatorType/TerminDato***: Er ikke lenger obligatorisk
  
  - ***KlientIdentifikatorType/Fodselsnummer*** og ***KlientIdentifikatorType/DUFnummer*** er nå lagt inn som et <choice> - element, dvs. én må være tilstede
  
  - ***KunnskapsmodellOmradeBeskrivelseType/BeskrivelseFungering*** endres til et <choice> - element hvor enten ***KunnskapsmodellOmradeBeskrivelseType/Beskrivelse*** eller ***KunnskapsmodellOmradeBeskrivelseType/BegrunnelseForIkkeAngitt*** må være tilstede

  - ***IndividuellPlanStatusType***: Koder endret fra string til tall

  - ***MorsmalType***: Koder endret fra string til tall
  
  - ***StatsborgerskapType***: Koder endret fra string til tall
  
  - ***KunnskapsmodellOmradeType***: Koder endret fra tall 1..11 -> koder i formen 1.1 tom. 3.3 (representerer nå <dimensjon>.<område>), burde gjøre det lettere å se sammenhengen dimensjon->område
    

- Navneendringer:

  - ***BarnetsSituasjonOgBehovType/BistandSamletBehov***: endret til ***SamletBehov***
  
  - ***BarnetsSituasjonOgBehovType/OmradeBeskrivelse***: endret til ***Omrade"***

  - ***BarnetsMedvirkningType/BarnetsMedvirkning***: endret til ***BarnetsBetraktning***
  
  - ***KommunensTiltaksplanType***: endret til ***KommunensPlanForTiltaketType*** for å unngå begrepsuklarhet, 'Tiltaksplan' var tidligere et begrep benyttet i BVL.
  
  - ***NettverkRelasjonType*** endret til ***OmsorgsrelasjonType***
  
## Bufdir_Barnevern_Henvisning_v0.10.0
  
- Nye elementer:

  - ***MedvirkningHenvisningType***: Samler elementene ***BarnetsMedvirkning***, ***OmForeldrene*** og ***MedvirkningAnnet*** i en complexType 

  - ***OmsorgstiltakHenvisningType/Medvirkning + OmForeldrene + MedvirkningAnnet*** er erstattet av et element av type ***MedvirkningHenvisningType***  
  
  - ***OmsorgstiltakHenvisningType/PlanEtterOnsketTiltakIngenPlan*** er et nytt <choice> - element som brukes dersom ingen ***OmsorgstiltakHenvisningType/PlanEtterOnsketTiltak/Plan*** skal angis

  
- Endrede elementer:

  - ***OmsorgspersonType/RelasjonAnnet***: Ikke lenger obligatorisk

  - ***OmsorgspersonType/Omsorg***: Ikke lenger obligatorisk
  
  - ***SoskenSamplasseringType/SoskenRelasjonAnnet***: Ikke lenger obligatorisk 
  
  - ***SoskenSamplasseringType/Fodselsnummer*** og ***SoskenSamplasseringType/DUFnummer*** inngår nå i et <choice> - element
  
  - ***OmsorgstiltakHenvisningType/PlanEtterOnsketTiltak*** er nå lagt om til et element med mulig repeterende element ***Plan*** av samme type som tidligere


- Slettede elementer:

  - ***HenvisningMeldingType/BistandSamletBehov*** var et duplikat av ***BarnetsSituasjonOgBehovType/BistandSamletBehov*** og er slettet

- Navneendringer:
  
  - ***NettverkPersonType*** endret til ***OmsorgspersonType***
  
  - ***BarnetsNettverkType*** endret til ***BarnetsOmsorgspersonerType***

  - ***PlasseringINettverkType/NettverkPlasseringVurdert*** endret til ***Vurdert***
  
  - ***PlasseringINettverkType/NettverkPlasseringVurdering*** endret til ***Vurdering***

  - ***HenvisningMeldingType/Nettverk*** endret til ***ForeldreOgOmsorgspersoner***
  
  - ***OmsorgstiltakHenvisningType/Nettverk*** endret til ***ForeldreOgOmsorgspersoner***
  
  - ***HenvisningMeldingType/TiltaksplanPlan*** endret til ***PlanForTiltaket***
  
## Bufdir_Barnevern_Henvisning_Fosterhjem_v0.10.0
  
- Endrede elementer:

  - ***HenvisningFosterhjem***: Lagt til kodelistefilter i <appInfo>, begrenser gyldige lovparagrafer i henvisning fosterhjem
  
- Navneendringer:

  - ***FosterhjemHenvisningType*** endret til ***HenvisningFosterhjemType***
  
# Kodelister:

## Bufdir_Barnevern_Generelt_kodelister_v0.10.0

- Nye kodelister:

  - ***VedtakInstansType***

- Kodeendringer:

  - ***KunnskapsmodellOmradeType***: Koder lagt om fra tallverdier 1..11 til '1.1' .. '3.3' (Se endring beskrevet under Bufdir_Barnevern_Generelt_v0.10.0.xsd)
  
  - ***MorsmalType***: Kode for "Annet" endret fra 9 -> 999. OBS! Her vil vi sannsynligvis bruke internasjonale språkkoder
  
  - ***StatsborgerskapType***: Samme som for MorsmalType
  
- Navneendringer:

  - ***NettverkRelasjonType*** endret til ***OmsorgsrelasjonType*** 
  
- Andre endringer:

  - ***VedtakHenvisningParagrafType***: Er nå koblet til kodeliste for ***HenvisningForespurtBistandType*** for å filtrere ut lovhjemler som er aktuelle for fosterhjem
  
# Veiledningstekster:

## HenvisningVeiledning_v0.10.0.schema.json

- Nye elementtyper:

  - ***KodelisteTekster*** og ***KodelisteVerdier***
  
## Bufdir_barnevern_henvisning_veiledning_v0.10.0.json

- Nye elementer:
  
  - ***Meldingshode*** 
  
  - ***Medvirkning***: se endring i Bufdir_Barnevern_Henvisning_v0.10.0.xsd
  
  - ***FattetAv***: se endring i Bufdir_Barnevern_Generelt_v0.10.0.xsd
  
  - ***Vurdering***: se endring i Bufdir_Barnevern_Generelt_v0.10.0.xsd (DimensjonVurdering)
  
  - ***Sosken***
  
  - ***Klient***
  
  - ***TilleggstjenesterType***
  
  - ***IngenPlan***
  
  - ***ForeldreOgOmsorgspersoner***
  
  - Kodelistetekster:
    
	- KunnskapsmodellOmradeType benyttet i Fosterhjem - henvisningen
  
- Endrede elementer:

  - ***Vedlegg*** har fått sortering 9999 for å plassere elementet til slutt i skjema  
  
  - ***TiltakHistorikk***: Tidliegere ledetekst flyttet til veiledning, har fått ny ledetekst
  
  - ***BistandSamletBehov*** har fått sortering 9999 for å plassere elementet som siste element
  
  - ***PlanEtterOnsketTiltak***: Hadde feil id, endret
  
  - ***Identifikator***: Ledetekst endret
  
  - ***Beskrivelse*** (id=BUF_02E03054-70DA-4618-BD00-21F99145DF56): ledetekst og veiledning endret
  
  - ***Kommuneinfo*** endret ledetekst

- Slettede elementer:

  - ***BistandSamletBehov*** med id = BUF_A7DEBDE6-0131-489A-831F-D11711B7319F var et duplikat i XSD og er slettet (se endring i Bufdir_Barnevern_Henvisning_v0.10.0.XSD)
  
  - ***OmsorgssituasjonAnnet*** med id = BUF27C8B361-A710-497C-B03A-557E3FC4D29C
  
  - ***BeskrivelseBehov*** med id = BUF_E42B359C-9134-4868-ADB5-19042BBC0942
  
- Navneendringer:

  - Navneendringer som følge av endringer i XSD'er
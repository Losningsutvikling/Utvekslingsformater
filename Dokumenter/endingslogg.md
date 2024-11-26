# Endringslogg for bufdirs meldingsformater

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
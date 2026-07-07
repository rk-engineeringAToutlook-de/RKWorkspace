# iOS/iPad Pilot Runbook - MA017

Status: final
Datum: 2026-07-07

## Windows Vorbereitung

~~~powershell
.\tools\run-ma017-ios-handoff.ps1 -SmokeTest
.\tools\run-pilot-windows-to-ipad-closed-pdf.ps1 -SmokeTest -Policy TrustedPersonalDevices
.\tools\run-pilot-windows-to-ipad-open-pdf.ps1 -SmokeTest -Policy TrustedPersonalDevices
~~~

## Pilotfluss

1. Windows Owner haelt `Rechnung.pdf`.
2. iPad meldet `AblageHello`.
3. Windows sendet Capsule oder OpenFrame.
4. iPad zeigt `liegt hier im Frame`.
5. iPad erzeugt nur subtile Haptics.
6. iPad beweist Sandbox NoFileIngress.
7. iPad sendet `zurueckgeben`.
8. Windows bestaetigt Rueckgabe oder Recovery.

## Owner-Fragen

1. Fuehlt sich iPad wie eine Ablage an?
2. Denke ich an ein Geraet oder an meinen Arbeitsraum?
3. Hat die Haptik geholfen oder gestoert?
4. Glaube ich, dass das Original beim Owner bleibt?

## Abbruch

Abbrechen, wenn:

- eine Original-PDF im App Container liegt.
- Share Sheet oder Files App das Original anbietet.
- Haptics technisch oder alarmierend wirken.
- Return oder Recovery nicht reproduzierbar ist.

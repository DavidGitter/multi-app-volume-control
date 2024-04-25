# Hardware Guide

## About
This guide contains a step-by-step instruction on how to build your own volume mixer hardware.

## Part list
You will need the following parts to build your mixer:
- **10 kOhm Potentiometer (linear)**
  For this project 4x B10K Potentiometer are used

<img src="/docs/readme-pics/poti.png" alt="drawing" width="200"/>

- **Breadboard and Jumper Cable** 
  Just for the development phase, later you might want to switch to a PCB
  
<img src="/docs/readme-pics/bread.png" alt="drawing" width="200"/>

- **A Microcontroller** 
  It should be able to be used out-of-the-box with the Arduino Enviroment like a ESP32 or a Arduino UNO.
  The microcontroller used here is a ESP32 WROOM 32 because its relativly thin and is therefore easy to get in a case.
  You will also need a suitable USB cable to connect your microcontroller to your PC. It must be a USB-!DATA!-cable!
  >[!IMPORTANT]
  >The micro controller you choose should be have atleast 4 ADC Pins where the rotary knobs are connected to. To find out if a microcontroller is usaable please refer to its datasheet.
  
<img src="/docs/readme-pics/microcontroller.png" alt="drawing" width="200"/>

- **Mixer Casing**
  You can let your creativity run wild when building the casing for your volume mixer. Bear in mind that all parts of your project must fit inside and that no short circuits are caused by contacts that are in contact with a metal casing, for example. 
  For those who have a 3D printer, there will be an STL file in the future so that you can print the housing yourself.

## Installation
*To be continued...*

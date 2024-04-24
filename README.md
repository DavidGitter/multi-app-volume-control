# Multi App Volume Control (MAVC)



<!-- TABLE OF CONTENTS -->
<details>
  <summary>Table of Contents</summary>
  <ol>
    <li>
      <a href="#getting-started">Getting Started</a>
      <ul>
        <li><a href="#Knowledge Prerequisites">Knowledge Prerequisites</a></li>
        <li><a href="#Software Prerequisites">Software Prerequisites</a></li>
        <li><a href="#Hardware Prerequisites">Hardware Prerequisites</a></li>
        <li><a href="#installation">Installation</a></li>
      </ul>
    </li>
    <li><a href="#usage">Usage</a></li>
    <li><a href="#roadmap">Roadmap</a></li>
    <li><a href="#contributing">Contributing</a></li>
    <li><a href="#license">License</a></li>
    <li><a href="#contact">Contact</a></li>
    <li><a href="#acknowledgments">Acknowledgments</a></li>
  </ol>
</details>



<!-- ABOUT THE PROJECT -->
## About The Project

![Intro-Picture](/docs/readme-pics/mixer-pic.jpg)

Can you relate? You're playing a relaxed game or concentrating on your work and suddenly your mate comes into the voice channel who installed a soundboard for the first time and is now getting on your nerves with unfunny meme sounds. I have good news for you! 

With this repository you have all the basics to build your own open-source soundboard and mute your mate for all eternity with a single turn of a knob. This means you don't have to go to the Windows volume mixer but can easily adjust the volume for any application with (currently) upto 4 rotary knobs and a User Interface to configurate them.

### Have I piqued your interest?
Then continue reading below under [Getting Started](## Getting Started).

### Do you want to costumise or improve the project?
Then read the [Developer Guide](/docs/Developer.md) to further develope the project.

<p align="right">(<a href="#readme-top">back to top</a>)</p>


## Getting Started

### Architecture
This project consits of the following components:
- The **Microcontroller program (Mixer)** to control your hardware
- The **background service (Agent)** that communicates with you Microcontroller and controls the volume
- The **User Interface (UI)** with which you configurate your mixer

This results in the following structure:
![Archtitecture](./docs/diagrams/architecture.png?raw)

### Knowledge Prerequisites
- A little knowledge about Mirocontroller and electronics
- If you plan on using costum hardware: Some programming knowledge
- Manual dexterity, for example for building the mixer and its casing

### Software Prerequisites
What you need in terms of software in order to use your own mixer:
- Windows 10/11 installed
- Currently VS-Code with the PlatformIO Extension to setup your Microcontroller (may no longer need to be done in the future due to additional software)
- The latest [MavcSetup.zip](https://github.com/DavidGitter/multi-app-volume-control/releases/latest) found in this repository under Release

### Hardware Prerequisites
What you need in to build your own volume controller:
- 4x 10kOhm Potentiometer (linear)

<!--![Poti](/docs/readme-pics/architecture.png)-->

- Breadboard and Jumper Cable (just for the development phase, later you might want to switch to a PCB)
<!--![Breadboard and Jumpercable](/docs/readme-pics/breadboard.png)-->

- A Microcontroller that can be used out-of-the-box with the Arduino Enviroment, e.g. ESP32 like in this project, Arduino UNO...
>[!IMPORTANT]
>The micro controller you choose should be have atleast 4 ADC Pins where the rotary knobs are connected to
<!--(otherwise if you have less you might need to customise the Microcontroller code to do [multiplexing](https://en.wikipedia.org/wiki/Multiplexing) or use an external AD-IC). To find out whether your Microcontroller is suitable, you can usually simply lookup Images with "your-Microcontroller-name pinout" in your browser or refer to the appropriate data sheet.-->

- A breadboard and jumper cables (now or later you might consider getting a PCB online for a small amount of money)
<!-- TODO: provide a working PCB file for the users -->
<!--![Microcontroller](/docs/readme-pics/breadboard.png)-->

- A cool looking case for your mixer (this might be the hardest part where you are on your own ;))

<!-- TODO: add 3D printing files for users owning a 3D printer -->

### Other Prerequisites
- You need patience: This project tries to give you all software assistance as far as possible, but you might need to do some additional stuff on your own to get the project up and running. Plan to work on it for several hours in total, but it will be hopefully worth it, fun and you might even learn some skills along the way.



### Installation
For the build of the mixer hardware in detail please refer to the [Hardware Setup Guide](/docs/Hardware Setup.md) in the docs folder.

To use the mixer the [MavcSetup.zip](https://github.com/DavidGitter/multi-app-volume-control/releases/latest) needs to be installed. It automaticlly looks for updates on every startup of the user-interface.

<!-- USAGE EXAMPLES -->
## Usage
Once the MavcSetup is installed you will get a Short-Cut to the Desktop for the User Interface with which you can configurate your mixer.

*The user interface*
![UI](/docs/readme-pics/gui-v1.png)

The UI consists of four sections. Each secetion belongs to one rotary knob that you can map your devices and applications too. The mapped applications and devices are shown in the list of each section. 
For every section there is a dropbox that provides the available Audio-Outputs like devices and applications you can map, but also higher functions. When selecting a item, it gets added to the list and is ready to be used. Every item has a type as bracket before the name (e.g. (Type) name-of-audio-output)
There are currently four types:
- **App**: A normal application that was found on your pc like a game
- **Device**: A audio output device like a headphone or your speakers where you want to regulate the master volume
- **Funtion**: A complex function of MAVC that allows you to make cool stuff (like "focus" where the application you currently using is choosen as regulation target)
<p align="center">
  <img src="/docs/readme-pics/gui-sections.png" />
</p>

On the right hand side there is a section for settings. Here you can apply specific configurations to your MAVC-system including actions like inverting the rotary knob mapping so that the rotary knobs are used in the reversed order.
![GUI Reverse Order](/docs/readme-pics/gui-reverse-order.png)

If you have everything configurated the way you want you must save the configurations by pressing the *Save*-button in the right down corner. This saves the configurations made to the config.json file in your MAVC folder under documents.
<p align="center">
  <img src="/docs/readme-pics/gui-save.png" />
</p>

For more information on the usage of the UI please refer to the [User-Interface.md](/docs/User-Interface.md)

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- ROADMAP -->
## Roadmap
This roadmap has to be updated in the future.
- [x] Basic software system
- [ ] Burn-Application for μ-Controller
- [ ] Make system more generic (e.g. choosable how much knobs to use and so on)
- [ ] Plugin system

See the [open issues](https://github.com/othneildrew/Best-README-Template/issues) for a full list of proposed features (and known issues).

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- CONTRIBUTING -->
## Contributing
Contributions are what make the open source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

If you have a suggestion that would make this better, please fork the repo and create a pull request. You can also simply open an issue with the tag "enhancement".
Don't forget to give the project a star! Thanks again!

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- LICENSE -->
## License
Distributed under the MIT License. See `LICENSE.txt` for more information.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Disclaimer
This project is provided as is, without warranty of any kind, express or implied, including but not limited to the warranties of merchantability, fitness for a particular purpose, and non-infringement.

In no event shall the authors or copyright holders be liable for any claim, damages, or other liability, whether in an action of contract, tort, or otherwise, arising from, out of, or in connection with the project or the use or other dealings in the project.



<!-- CONTACT -->
## Contact
-   Silas Jung
    GitHub: https://github.com/DavidGitter

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- ACKNOWLEDGMENTS -->
## Acknowledgments

Use this space to list resources you find helpful and would like to give credit to. I've included a few of my favorites to kick things off!

* See [Referende.md](/docs/Reference.md)
* [GitHub Readme Template](https://github.com/othneildrew/Best-README-Template)

<p align="right">(<a href="#readme-top">back to top</a>)</p>

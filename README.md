# MindMapper

MindMapper is the world's most adaptive interface device.

The project is seperated into 2 different solution files, UI and Controller.

## Build Instructions

1. Use your IDE of choice to create a `Publish to folder` action and publish both the UI and Controller project.
2. Run the UI exe and open the configuration server in your browser using the link provided in the console (usually `localhost:5000`).
3. Download the OpenBCI GUI from `https://github.com/OpenBCI/OpenBCI_GUI` and copy the contents of the `lib/` folder to the Controller build directory.
4. Download the ViGEm drivers from `https://github.com/nefarius/ViGEmBus/releases/tag/v1.22.0` and install them
5. Open the `config.json` file and change the parameters to match your OpenBCI unit.
6. Run the controller program. If everything went right, you should see the stream of live controller mappings.
7. Using the UI, test the controller mappings.

## Controller

The controller project is where the magic happens. It connects to the Brainflow device and translates signals to controller mappings using ViGEm.

### config.json
This file is where all configurations are stored, including the controller mappings and threshold values. Use this to fine tune your experience.

## UI

The MindMaper UI is designed with Blazor. It features a config editor and a live controller mapping viewer.

### Controller Tester
This page allows you to view all controller inputs, enabling you to fine tune your experience.

### Config Editor
Due to time constraints, the config editor is not functional. In a commercial release, this would be functional and allow for the user to make real time edits to the config file.

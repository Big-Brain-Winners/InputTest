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

### Configuration

The highly flexible configuration capabilities of MindMap makes it a truly capable tool. The tool will automatically detect all configuration files in the root directory, searching for JSON files with names containing the phrase "cconfig", which stands for controller config. Configurations can be rotated through at any time by inputting A or D into the console to move to the previous and next configuration file respectively. Configuration files are ordered lexigraphically.

Every configuration file must contain a root attribute "BrainflowSettings". this attribute contains the information required to find and connect to your OpenBCI peripheral.
The Required BrainflowSettings Attributes are: 
"BoardId": Integer, the Identification number of your model of OpenBCI peripheral. For ganglion boards this is 1
"SerialPort": String, the com port of your serial connection, for example, our com port was "COM10". this ca be found in device manager
"MacAddress": the MAC address of your OpenBCI peripheral

Every configuration file must also contain a root attribute "AdjustmentSettings". this attribute contains default information for runtime variables.
The Required BrainflowSettings Attributes are: 
"PollingTime": Integer, the time, in miliseconds, to wait between every update

Finally, each configuration file must contain a Root Element "Bindings", which is a list of instruction objects containing information on how to handle each channel of output from the OpenBCI peripheral. The index of the instruction object correlates to the channel it governs, any channels with indexes outside of the "Bindings" list will be ignored.

Each instruction object will have the following attributes:
"ControlType": Integer. This indicates the type of XInput output you wish the channel to be routed to. It's options are:
  -1: Null, this indicates you wish to ignore the channel. if ControlType is set to this, all other attributes are optional and ignored
  0: Button. See "Button options" for list of available buttons
  1: Axis See "Axis Options" for list of available Axies
  2: Slider See "Slider Options" for list of available sliders

"ChannelType": Integer. This indicates the type of the input channel, which influences how incoming data is processed.  Currently supported options are:
0: EMG channel
1: Accelerometer Channel

"ControlIndex": Integer. This indicates which output of type "ControlType" the output should be routed to, indexed into the correlated "options" list.

"Inverted": Boolean, optional, defaullt False. This flips the typical behaviour of the channel output. For buttons, this flips the boolean value sent to the button. For Axies this multiplies the output by -1, changing one extreme to another. For sliders this sets the output value to be Max_value - outputValue

"Analog": Boolean, optional, default false. This indicates if the output shoudl be sent as an analog signal rather than a boolean maximum/minimum value based on threshold. Not supported for button outputs.

"Threshold": Integer, required for non-analog outputs. This sets the channels threshold where the output value changes. If output activates too easily, raise this value. If output is too difficult to activate, lower this signal.

"RollingAvgSize": Integer, optional, Default 1. This indicates how large the temporal smoothing for the channel should be. Higher values reduce "jittery" outputs at the cost of response time.

Button Options:
```
Xbox360Button.A
Xbox360Button.X
Xbox360Button.Y
Xbox360Button.B
Xbox360Button.Left (DPAD)
Xbox360Button.Right
Xbox360Button.Up
Xbox360Button.Down
Xbox360Button.Back (Menu Buttons)
Xbox360Button.Start
Xbox360Button.LeftShoulder
Xbox360Button.RightShoulder
Xbox360Button.LeftThumb
Xbox360Button.RightThumb
Xbox360Button.Guide (Unknown)
```
Axis Options:
```
Xbox360Axis.LeftThumbX
Xbox360Axis.LeftThumbY
Xbox360Axis.RightThumbX
Xbox360Axis.RightThumbY
```
Slider Options:
```
Xbox360Slider.LeftTrigger
Xbox360Slider.RightTrigger
```

## UI

The MindMaper UI is designed with Blazor. It features a config editor and a live controller mapping viewer.

### Controller Tester
This page allows you to view all controller inputs, enabling you to fine tune your experience.

### Config Editor
Due to time constraints, the config editor is not functional. In a commercial release, this would be functional and allow for the user to make real time edits to the config file.

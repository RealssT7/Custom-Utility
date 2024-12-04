# Custom-Utility

## Overview

Custom-Utility is a Unity editor extension that provides various utility tools to enhance game developers' workflows. This toolset includes a "Selection Utility" and a "Play from Here Utility".

## Features

- **Selection Utility**: Select GameObjects based on name, tag, layer, or component.
- **Play from Here Utility**: Start play mode from the current camera position in the editor.

## Installation

1. Clone or download the repository.
2. Copy the `Assets/CustomUtility` folder into your Unity project's `Assets` directory.

## Usage

### Selection Utility
Inspire from this https://assetstore.unity.com/packages/tools/utilities/free-ui-utility-206817#description unity asset.

1. Open the Custom Utility window from the Unity Editor menu.
2. Use the selection options to filter GameObjects by name, tag, layer, or component.
3. The selected GameObjects will be highlighted in the scene.

### Play from Here Utility
Similar to the Unreal Play from here function, this will use the camera's current position from the scene view instead.

1. Ensure your player GameObject is named "PlayerCharacter", tagged as "Player", or has the `PlayerCustomPlay` component.
2. click the "Play from Here" button in the Scene View toolbar.
3. The game will start from the current camera position in the editor.

(If the overlay button does not appear, go to the overlay menu on the scene view window.)

## License

This project is licensed under the MIT License.

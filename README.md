# Custom-Utility

## Overview

Custom-Utility is a Unity editor extension that provides various utility tools to enhance the workflow of game developers. This toolset includes selection utilities and a "Play from Here" utility.

## Features

- **Selection Utility**: Select GameObjects based on name, tag, layer, or component.
- **Play from Here Utility**: Start play mode from the current camera position in the editor.

## Installation

1. Clone or download the repository.
2. Copy the `Assets/CustomUtility` folder into your Unity project's `Assets` directory.

## Usage

### Selection Utility

1. Open the Custom Utility window from the Unity Editor menu.
2. Use the selection options to filter GameObjects by name, tag, layer, or component.
3. The selected GameObjects will be highlighted in the scene.

### Play from Here Utility

1. Ensure your player GameObject is named "PlayerCharacter", tagged as "Player", or has the `PlayerCustomPlay` component.
2. In the Scene View toolbar, click the "Play from Here" button.
3. The game will start from the current camera position in the editor.

(If the overlay button not appear, go to the overlay menu on the scene view window.)

## License

This project is licensed under the MIT License.